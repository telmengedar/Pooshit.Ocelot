using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Npgsql;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Schema;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Schemas;
using Pooshit.Ocelot.Tests.Entities;
using Pooshit.Ocelot.Tests.Models;

namespace Pooshit.Ocelot.Tests.Postgres {

    [TestFixture, Parallelizable]
    public class PostgresSchemaUpdateTests {

        [Test, Parallelizable]
        public void DropColumn() {
            PostgreInfo info = new PostgreInfo();
            Mock<IDBClient> client = new Mock<IDBClient>();
            client.SetupGet(c => c.DBInfo).Returns(info);

            AlterTableOperation operation = new AlterTableOperation(client.Object, "test");
            operation.Drop("first");

            PreparedOperation statement = operation.Prepare();
            Assert.AreEqual("ALTER TABLE test DROP COLUMN \"first\" CASCADE", statement.CommandText);
        }

        [Test, Parallelizable]
        public void DropColumns() {
            PostgreInfo info = new PostgreInfo();
            Mock<IDBClient> client = new Mock<IDBClient>();
            client.SetupGet(c => c.DBInfo).Returns(info);

            AlterTableOperation operation = new AlterTableOperation(client.Object, "test");
            operation.Drop("first", "second");

            PreparedOperation statement = operation.Prepare();
            Assert.AreEqual("ALTER TABLE test DROP COLUMN \"first\" CASCADE , DROP COLUMN \"second\" CASCADE", statement.CommandText);
        }

        [Test, Parallelizable]
        public void AddColumn() {
            PostgreInfo info = new PostgreInfo();
            Mock<IDBClient> client = new Mock<IDBClient>();
            client.SetupGet(c => c.DBInfo).Returns(info);

            AlterTableOperation operation = new AlterTableOperation(client.Object, "test");
            operation.Add(new EntityColumnDescriptor("url", typeof(Company).GetProperty("Url")));

            PreparedOperation statement = operation.Prepare();
            Assert.AreEqual("ALTER TABLE test ADD COLUMN \"url\" character varying ", statement.CommandText);
        }

        [Test, Parallelizable]
        public void AddColumnFloatArray() {
            PostgreInfo info = new();
            Mock<IDBClient> client = new();
            client.SetupGet(c => c.DBInfo)
                  .Returns(info);

            AlterTableOperation operation = new(client.Object, "test");
            operation.Add(new EntityColumnDescriptor("data", typeof(ValueModel).GetProperty("FloatArray")));

            PreparedOperation statement = operation.Prepare();
            Assert.AreEqual("ALTER TABLE test ADD COLUMN \"data\" real[16] ", statement.CommandText);
        }

        [Test, Parallelizable]
        public void AddColumns() {
            PostgreInfo info = new();
            Mock<IDBClient> client = new Mock<IDBClient>();
            client.SetupGet(c => c.DBInfo).Returns(info);

            AlterTableOperation operation = new AlterTableOperation(client.Object, "test");
            operation.Add(new EntityColumnDescriptor("url", typeof(Company).GetProperty("Url")), new EntityColumnDescriptor("name", typeof(Company).GetProperty("Name")));

            PreparedOperation statement = operation.Prepare();
            Assert.AreEqual("ALTER TABLE test ADD COLUMN \"url\" character varying  , ADD COLUMN \"name\" character varying ", statement.CommandText);
        }

        static async Task<IDBClient> CreateVectorClient() {
            string connection = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");
            if (string.IsNullOrEmpty(connection))
                Assert.Inconclusive("Test only active on local dev machine — set POSTGRES_CONNECTION to run");

            IDBClient dbclient = ClientFactory.Create(() => new NpgsqlConnection(connection), new PostgreInfo(), true);
            try {
                await dbclient.NonQueryAsync(null, "CREATE EXTENSION IF NOT EXISTS vector");
            }
            catch (Exception e) {
                Assert.Inconclusive($"pgvector extension not available: {e.Message}");
            }

            await dbclient.NonQueryAsync(null, "DROP TABLE IF EXISTS vectorentity CASCADE");
            return dbclient;
        }

        static async Task<string> FormattedType(IDBClient dbclient) {
            return (string)await dbclient.ScalarAsync(null, "SELECT format_type(atttypid, atttypmod) FROM pg_attribute WHERE attrelid = 'vectorentity'::regclass AND attname = 'embedding'");
        }

        static async Task<string> ColumnVersion(IDBClient dbclient) {
            return (await dbclient.ScalarAsync(null, "SELECT xmin::text FROM pg_attribute WHERE attrelid = 'vectorentity'::regclass AND attname = 'embedding'")).ToString();
        }

        [Test, NonParallelizable]
        public async Task VectorColumn_GetSchemaAsync_ReportsVectorN() {
            IDBClient dbclient = await CreateVectorClient();
            await new SchemaService(dbclient).CreateOrUpdateSchema<VectorEntity>();

            Assert.That(await FormattedType(dbclient), Is.EqualTo("vector(3)"));

            TableSchema asyncSchema = (TableSchema)await dbclient.DBInfo.GetSchemaAsync(dbclient, "vectorentity");
            TableDescriptor syncSchema = (TableDescriptor)dbclient.DBInfo.GetSchema(dbclient, "vectorentity");
            Assert.That(asyncSchema.Columns.Single(c => c.Name == "embedding").Type, Is.EqualTo("vector(3)"));
            Assert.That(syncSchema.Columns.Single(c => c.Name == "embedding").Type, Is.EqualTo("vector(3)"));
        }

        [Test, NonParallelizable]
        public async Task VectorColumn_UpdateTwice_EmitsNoAlter() {
            IDBClient dbclient = await CreateVectorClient();
            SchemaService service = new(dbclient);
            EntityManager em = new(dbclient);
            await service.CreateOrUpdateSchema<VectorEntity>();
            string version = await ColumnVersion(dbclient);

            await service.UpdateSchema<VectorEntity>();
            await service.UpdateSchema<VectorEntity>();
            em.UpdateSchema<VectorEntity>();
            em.UpdateSchema<VectorEntity>();

            Assert.That(await FormattedType(dbclient), Is.EqualTo("vector(3)"));
            Assert.That(await ColumnVersion(dbclient), Is.EqualTo(version), "an ALTER COLUMN was issued for an unchanged vector column");
        }

        [Test, NonParallelizable]
        public async Task VectorColumn_EntityStackCreate_CreatesVectorN() {
            IDBClient dbclient = await CreateVectorClient();

            new EntityManager(dbclient).Create<VectorEntity>();

            Assert.That(await FormattedType(dbclient), Is.EqualTo("vector(3)"));
        }

        [TestCase("real[]", false)]
        [TestCase("real[]", true)]
        [TestCase("vector(4)", false)]
        [TestCase("vector(4)", true)]
        [NonParallelizable]
        public async Task VectorColumn_DifferentLiveType_IsAlteredToDeclaredVector(string liveType, bool entityStack) {
            IDBClient dbclient = await CreateVectorClient();
            await dbclient.NonQueryAsync(null, $"CREATE TABLE vectorentity (id bigserial PRIMARY KEY, embedding {liveType})");

            if (entityStack)
                new EntityManager(dbclient).UpdateSchema<VectorEntity>();
            else
                await new SchemaService(dbclient).UpdateSchema<VectorEntity>();

            Assert.That(await FormattedType(dbclient), Is.EqualTo("vector(3)"));
        }

        [Test, NonParallelizable]
        public async Task VectorColumn_FloatArrayInsert_StoresVector() {
            IDBClient dbclient = await CreateVectorClient();
            EntityManager em = new(dbclient);
            await new SchemaService(dbclient).CreateOrUpdateSchema<VectorEntity>();

            await em.Insert<VectorEntity>().Columns(e => e.Embedding).ExecuteAsync(new[] { 1f, 2f, 3f });

            Assert.That(await dbclient.ScalarAsync(null, "SELECT embedding::text FROM vectorentity"), Is.EqualTo("[1,2,3]"));
        }

        [Test, Parallelizable]
        public void ModifyColumn() {
            PostgreInfo info = new PostgreInfo();
            Mock<IDBClient> client = new Mock<IDBClient>();
            client.SetupGet(c => c.DBInfo).Returns(info);

            AlterTableOperation operation = new AlterTableOperation(client.Object, "test");
            operation.Modify(new EntityColumnDescriptor("url", typeof(Company).GetProperty("Url")));

            PreparedOperation statement = operation.Prepare();
            Assert.AreEqual("ALTER TABLE test ALTER COLUMN \"url\" TYPE character varying", statement.CommandText);
        }

        /// <summary>
        /// Verifies that calling UpdateSchema multiple times does not accumulate duplicate UNIQUE
        /// constraints for a column decorated with [Unique("name")] (named single-column unique).
        /// </summary>
        [Test, Parallelizable]
        public async Task NamedSingleColumnUniqueNotDuplicatedOnRepeatedUpdateSchema() {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")))
                Assert.Inconclusive("Test only active on local dev machine — set POSTGRES_CONNECTION to run");

            IDBClient dbclient = ClientFactory.Create(
                () => new NpgsqlConnection(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")),
                new PostgreInfo(),
                true);

            SchemaService schemaService = new(dbclient);

            // ensure a clean slate
            if (await schemaService.ExistsSchema<UniqueConstraintEntity>())
                await dbclient.NonQueryAsync(null, "DROP TABLE uniqueconstraintentity CASCADE");

            // first creation
            await schemaService.CreateOrUpdateSchema<UniqueConstraintEntity>();

            // UpdateSchema called twice more — must not add new UNIQUE constraints each time
            await schemaService.UpdateSchema<UniqueConstraintEntity>();
            await schemaService.UpdateSchema<UniqueConstraintEntity>();

            // count unique indexes on the 'url' column (the named-unique column)
            long urlUniqueCount = (long)await dbclient.ScalarAsync(
                null,
                "SELECT COUNT(*) FROM pg_indexes WHERE tablename = 'uniqueconstraintentity' AND indexdef LIKE '%UNIQUE%' AND indexdef LIKE '%(url)%'");

            Assert.AreEqual(1, urlUniqueCount,
                "Expected exactly one UNIQUE index on 'url' after repeated UpdateSchema calls, but found multiple — constraint is being duplicated.");
        }

        /// <summary>
        /// Verifies that calling UpdateSchema multiple times does not accumulate duplicate UNIQUE
        /// constraints for a column decorated with [Unique] (unnamed single-column unique).
        /// </summary>
        [Test, Parallelizable]
        public async Task UnnamedSingleColumnUniqueNotDuplicatedOnRepeatedUpdateSchema() {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")))
                Assert.Inconclusive("Test only active on local dev machine — set POSTGRES_CONNECTION to run");

            IDBClient dbclient = ClientFactory.Create(
                () => new NpgsqlConnection(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")),
                new PostgreInfo(),
                true);

            SchemaService schemaService = new(dbclient);

            // ensure a clean slate
            if (await schemaService.ExistsSchema<UniqueConstraintEntity>())
                await dbclient.NonQueryAsync(null, "DROP TABLE uniqueconstraintentity CASCADE");

            // first creation
            await schemaService.CreateOrUpdateSchema<UniqueConstraintEntity>();

            // UpdateSchema called twice more
            await schemaService.UpdateSchema<UniqueConstraintEntity>();
            await schemaService.UpdateSchema<UniqueConstraintEntity>();

            // count unique indexes on the 'slug' column (the unnamed-unique column)
            long slugUniqueCount = (long)await dbclient.ScalarAsync(
                null,
                "SELECT COUNT(*) FROM pg_indexes WHERE tablename = 'uniqueconstraintentity' AND indexdef LIKE '%UNIQUE%' AND indexdef LIKE '%(slug)%'");

            Assert.AreEqual(1, slugUniqueCount,
                "Expected exactly one UNIQUE index on 'slug' after repeated UpdateSchema calls, but found multiple — constraint is being duplicated.");
        }
    }
}