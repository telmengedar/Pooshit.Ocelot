using Moq;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Info;
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
    }
}