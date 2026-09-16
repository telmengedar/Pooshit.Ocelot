using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Info.Postgre;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Security.Models;
using Pooshit.Ocelot.Tokens;

namespace Pooshit.Ocelot.Tests.Security;

[TestFixture, Parallelizable]
public class LegitimateIdentifierShapesTests {

    [Test, Parallelizable, Description("a dotted schema-qualified table name still renders raw, unsplit")]
    public void SchemaQualifiedTableNameStillRendersRaw() {
        EntityManager em = new(TestData.CreateDatabaseAccess());
        string sql = em.LoadData("information_schema.columns").Prepare().CommandText;
        Assert.That(sql, Does.Contain("FROM information_schema.columns"));
    }

    [Test, Parallelizable, Description("the two-arg DB.Column(table, name) table argument is an alias qualifier and still renders unquoted")]
    public void AliasQualifiedColumnStillRenders() {
        IDBClient client = TestData.CreateDatabaseAccess();
        string sql = new LoadOperation<InjModel>(client, EntityDescriptor.Create, DB.Column("ss", "string")).Prepare().CommandText;
        Assert.That(sql, Does.Contain("ss.[string]"));
    }

    [TestCase("__total")]
    [TestCase("sq1")]
    [TestCase("o666")]
    [Parallelizable]
    public void UnderscoreAndDigitAliasesStillRender(string alias) {
        IDBClient client = TestData.CreateDatabaseAccess();
        string sql = new LoadOperation<InjModel>(client, EntityDescriptor.Create, DB.All).Alias(alias).Prepare().CommandText;
        Assert.That(sql, Does.Contain($"AS {alias}"));
    }

    [Test, Parallelizable, Description("a composed index name idx_<table>_<name> still creates successfully once the model is validated")]
    public void ComposedIndexNameStillCreates() {
        IDBClient client = TestData.CreateDatabaseAccess();
        EntityManager em = new(client);
        em.UpdateSchema<ComposedIndexEntity>();

        bool indexExists = client.Query("SELECT name FROM sqlite_master WHERE type='index' AND name=@1", "idx_composedindexentity_time").Rows.Length > 0;
        Assert.That(indexExists, Is.True);
    }

    [Test, Parallelizable, Description("the library's own postgres introspection entities still describe without throwing")]
    public void PostgresIntrospectionEntitiesStillDescribe() {
        Assert.DoesNotThrow(() => EntityDescriptor.Create(typeof(PgColumn)));
        Assert.DoesNotThrow(() => EntityDescriptor.Create(typeof(PgView)));
        Assert.DoesNotThrow(() => EntityDescriptor.Create(typeof(PgIndex)));
    }

    [Test, Parallelizable, Description("a mixed-case [Table] name still renders unquoted, preserving the postgres unquoted-lowercase fold")]
    public void MixedCaseTableNameStillRendersUnquoted() {
        IDBClient client = TestData.CreateDatabaseAccess();
        string sql = new LoadOperation<MixedCaseTableEntity>(client, EntityDescriptor.Create, DB.All).Prepare().CommandText;
        Assert.That(sql, Does.Contain("FROM activeData"));
    }
}
