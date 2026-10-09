using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Attributes;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Entities.Schema;
using SchemaCreator = Pooshit.Ocelot.Schemas.SchemaCreator;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Schemas;
using Pooshit.Ocelot.Tests.Entities;
using Pooshit.Ocelot.Tests.Mocks;

namespace Pooshit.Ocelot.Tests.Postgres;

/// <summary>
/// tests for native pgvector columns which need no database (statement text and introspection against canned readers)
/// </summary>
[TestFixture, Parallelizable]
public class PostgreVectorColumnTests {

    class NotAnArray {
        [Vector(3)]
        public string Embedding { get; set; }
    }

    static readonly string[] ColumnNames = ["table_catalog", "table_schema", "table_name", "column_name", "data_type", "is_nullable", "column_default", "is_identity"];

    static object[][] VectorTableColumns() => [
        ["xx.io", "public", "vectorentity", "id", "bigint", "NO", "nextval('vectorentity_id_seq'::regclass)", "NO"],
        ["xx.io", "public", "vectorentity", "label", "character varying", "YES", DBNull.Value, "NO"],
        ["xx.io", "public", "vectorentity", "embedding", "USER-DEFINED", "YES", DBNull.Value, "NO"]
    ];

    static object[][] VectorTableFormattedTypes() => [["id", "bigint"], ["label", "character varying(50)"], ["embedding", "vector(3)"]];

    static object[][] VectorEntityColumns() => [VectorTableColumns()[0], VectorTableColumns()[2]];

    static object[][] VectorEntityFormattedTypes() => [VectorTableFormattedTypes()[0], VectorTableFormattedTypes()[2]];

    static object[][] VectorEntityIndexes() => [["public", "vectorentity", "vectorentity_pkey", "CREATE UNIQUE INDEX vectorentity_pkey ON public.vectorentity USING btree (id)"]];

    static Mock<IDBClient> CreateSchemaClient(PostgreInfo info, object[][] columns, object[][] formattedTypes, Action onFormattedTypes = null, object[][] indexes = null, List<string> statements = null, long tableCount = 1) {
        Mock<IDBClient> client = new();
        client.SetupGet(c => c.DBInfo).Returns(info);

        Reader Answer(string text) {
            if (text.Contains(" information_schema.columns "))
                return new(new FakeReader(ColumnNames, columns), null, info);
            if (text.Contains("format_type")) {
                onFormattedTypes?.Invoke();
                return new(new FakeReader(["attname", "format_type"], formattedTypes), null, info);
            }
            if (text.Contains(" pg_indexes "))
                return new(new FakeReader(["schemaname", "tablename", "indexname", "indexdef"], indexes ?? []), null, info);
            return new(new FakeReader([], []), null, info);
        }

        int Record(string text) {
            statements?.Add(text);
            return 0;
        }

        client.Setup(c => c.Reader(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<IEnumerable<object>>()))
              .Returns<Transaction, string, IEnumerable<object>>((_, text, _) => Answer(text));
        client.Setup(c => c.ReaderAsync(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<IEnumerable<object>>()))
              .Returns<Transaction, string, IEnumerable<object>>((_, text, _) => Task.FromResult(Answer(text)));
        client.Setup(c => c.NonQuery(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<object[]>())).Returns<Transaction, string, object[]>((_, text, _) => Record(text));
        client.Setup(c => c.NonQuery(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<IEnumerable<object>>())).Returns<Transaction, string, IEnumerable<object>>((_, text, _) => Record(text));
        client.Setup(c => c.NonQueryPrepared(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<object[]>())).Returns<Transaction, string, object[]>((_, text, _) => Record(text));
        client.Setup(c => c.NonQueryPrepared(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<IEnumerable<object>>())).Returns<Transaction, string, IEnumerable<object>>((_, text, _) => Record(text));
        client.Setup(c => c.Scalar(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<object[]>())).Returns(tableCount);
        return client;
    }

    [Test, Parallelizable]
    public void DeclaredVectorProperty_CreateRendersVectorN() {
        PostgreInfo info = new();
        Mock<IDBClient> client = new();
        client.SetupGet(c => c.DBInfo).Returns(info);

        AlterTableOperation entityStack = new(client.Object, "test");
        entityStack.Add(new EntityColumnDescriptor("embedding", typeof(VectorEntity).GetProperty(nameof(VectorEntity.Embedding))));
        Assert.That(entityStack.Prepare().CommandText, Is.EqualTo("ALTER TABLE test ADD COLUMN \"embedding\" vector(3) "));

        TableSchema schema = (TableSchema)new SchemaCreator().Create<VectorEntity>(info);
        AlterTableOperation schemaStack = new(client.Object, "test");
        schemaStack.Add(schema.Columns.Single(c => c.Name == "embedding"));
        Assert.That(schemaStack.Prepare().CommandText, Is.EqualTo("ALTER TABLE test ADD COLUMN \"embedding\" vector(3) "));
    }

    [Test, Parallelizable]
    public void DeclaredVectorProperty_SchemaDescriptorCarriesVectorType() {
        TableSchema schema = (TableSchema)new SchemaCreator().Create<VectorEntity>(new PostgreInfo());

        Assert.That(schema.Columns.Single(c => c.Name == "embedding").Type, Is.EqualTo("vector(3)"));
    }

    [Test, Parallelizable]
    public void IsTypeEqual_VectorSameDim_True() {
        Assert.That(new PostgreInfo().IsTypeEqual("vector(768)", "vector(768)"), Is.True);
    }

    [Test, Parallelizable]
    public void IsTypeEqual_VectorDifferentDim_False() {
        Assert.That(new PostgreInfo().IsTypeEqual("vector(768)", "vector(1536)"), Is.False);
    }

    [Test, Parallelizable]
    public void IsTypeEqual_VectorVsRealArray_False() {
        PostgreInfo info = new();
        Assert.That(info.IsTypeEqual("vector(768)", "real[3072]"), Is.False);
        Assert.That(info.IsTypeEqual("float4[]", "vector(768)"), Is.False);
    }

    [Test, Parallelizable]
    public void IsTypeEqual_UnconstrainedVectorVsDimensioned_False() {
        Assert.That(new PostgreInfo().IsTypeEqual("vector", "vector(768)"), Is.False);
    }

    [Test, Parallelizable]
    public void GetDBType_MalformedVector_Throws() {
        Assert.Throws<InvalidOperationException>(() => new PostgreInfo().GetDBType("vector(abc)"));
        Assert.Throws<InvalidOperationException>(() => new PostgreInfo().GetDBType("vectorish"));
    }

    [Test, Parallelizable]
    public void GetSchema_UserDefinedVectorColumn_ReportsVectorN() {
        PostgreInfo info = new();
        Mock<IDBClient> client = CreateSchemaClient(info, VectorTableColumns(), VectorTableFormattedTypes());

        TableDescriptor descriptor = (TableDescriptor)info.GetSchema(client.Object, "vectorentity");

        Assert.That(descriptor.Columns.Single(c => c.Name == "embedding").Type, Is.EqualTo("vector(3)"));
        Assert.That(descriptor.Columns.Single(c => c.Name == "label").Type, Is.EqualTo("character varying"));
    }

    [Test, Parallelizable]
    public async Task GetSchemaAsync_UserDefinedVectorColumn_ReportsVectorN() {
        PostgreInfo info = new();
        Mock<IDBClient> client = CreateSchemaClient(info, VectorTableColumns(), VectorTableFormattedTypes());

        TableSchema schema = (TableSchema)await info.GetSchemaAsync(client.Object, "vectorentity");

        Assert.That(schema.Columns.Single(c => c.Name == "embedding").Type, Is.EqualTo("vector(3)"));
        Assert.That(schema.Columns.Single(c => c.Name == "label").Type, Is.EqualTo("character varying"));
    }

    [Test, Parallelizable]
    public async Task GetSchemaAsync_TableWithoutUserDefinedColumn_DoesNotQueryCatalogTypes() {
        PostgreInfo info = new();
        int formattedTypeQueries = 0;
        Mock<IDBClient> client = CreateSchemaClient(info, [VectorTableColumns()[0]], [], () => formattedTypeQueries++);

        TableSchema asyncSchema = (TableSchema)await info.GetSchemaAsync(client.Object, "vectorentity");
        TableDescriptor syncSchema = (TableDescriptor)info.GetSchema(client.Object, "vectorentity");

        Assert.That(asyncSchema.Columns.Single().Type, Is.EqualTo("bigint"));
        Assert.That(syncSchema.Columns.Single().Type, Is.EqualTo("bigint"));
        Assert.That(formattedTypeQueries, Is.EqualTo(0));
    }

    [Test, Parallelizable]
    public void UpdateSchema_UnchangedVectorColumn_IssuesNoStatement() {
        PostgreInfo info = new();
        List<string> statements = [];
        Mock<IDBClient> client = CreateSchemaClient(info, VectorEntityColumns(), VectorEntityFormattedTypes(), indexes: VectorEntityIndexes(), statements: statements);

        new SchemaUpdater(new EntityDescriptorCache()).Update<VectorEntity>(client.Object);

        Assert.That(statements, Is.Empty);
    }

    [Test, Parallelizable]
    public void EntityManagerCreate_DeclaredVectorProperty_IssuesVectorNColumn() {
        PostgreInfo info = new();
        List<string> statements = [];
        Mock<IDBClient> client = CreateSchemaClient(info, [], [], statements: statements, tableCount: 0);

        new EntityManager(client.Object).Create<VectorEntity>();

        Assert.That(statements.Single(s => s.StartsWith("CREATE TABLE")), Does.Contain("\"embedding\" vector(3)"));
    }

    [Test, Parallelizable]
    public void Vector_NonFloatArrayProperty_Throws() {
        Assert.Throws<InvalidOperationException>(() => VectorAttribute.GetColumnType(typeof(NotAnArray).GetProperty(nameof(NotAnArray.Embedding)), new PostgreInfo()));
    }

    [Test, Parallelizable]
    public void Vector_DimensionsOutOfRange_Throws() {
        Assert.Throws<ArgumentOutOfRangeException>(() => new VectorAttribute(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new VectorAttribute(VectorAttribute.MaxDimensions + 1));
    }
}
