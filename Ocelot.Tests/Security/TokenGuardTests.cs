using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Errors;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Security.Models;
using Pooshit.Ocelot.Tokens;
using Pooshit.Ocelot.Tokens.Partitions;

namespace Pooshit.Ocelot.Tests.Security;

[TestFixture, Parallelizable]
public class TokenGuardTests {

    [Test, Parallelizable, Description("DB.Column(table, name) rejects an injected table qualifier")]
    public void ColumnTokenRejectsInjectedTableQualifier() {
        IDBClient client = TestData.CreateDatabaseAccess();
        Assert.Throws<InvalidIdentifierException>(() => new LoadOperation<InjModel>(client, EntityDescriptor.Create, DB.Column("ss; DROP TABLE x; --", "string"))
                                                         .Prepare());
    }

    [Test, Parallelizable, Description("DB.Property<T>(expr, alias) rejects an injected alias")]
    public void PropertyAliasRejectsInjectedAlias() {
        IDBClient client = TestData.CreateDatabaseAccess();
        Assert.Throws<InvalidIdentifierException>(() => new LoadOperation<InjModel>(client, EntityDescriptor.Create, DB.Property<InjModel>(m => m.A, "x; DROP TABLE t; --"))
                                                         .Prepare());
    }

    [Test, Parallelizable, Description("LoadOperation<T>.Alias(string) rejects an injected alias")]
    public void LoadAliasRejectsInjectedAlias() {
        IDBClient client = TestData.CreateDatabaseAccess();
        Assert.Throws<InvalidIdentifierException>(() => new LoadOperation<InjModel>(client, EntityDescriptor.Create, DB.All)
                                                         .Alias("x; DROP TABLE t; --")
                                                         .Prepare());
    }

    [Test, Parallelizable, Description("LoadData().Columns(string[]) rejects an injected column name")]
    public void LoadDataColumnsRejectsInjectedColumnName() {
        EntityManager em = new(TestData.CreateDatabaseAccess());
        Assert.Throws<InvalidIdentifierException>(() => em.LoadData("victim").Columns("a; DROP TABLE x; --"));
    }

    [Test, Parallelizable, Description("InsertData().Columns(string[]) rejects an injected column name")]
    public void InsertDataColumnsRejectsInjectedColumnName() {
        EntityManager em = new(TestData.CreateDatabaseAccess());
        Assert.Throws<InvalidIdentifierException>(() => em.InsertData("victim").Columns("a; DROP TABLE x; --").Prepare());
    }

    [Test, Parallelizable, Description("DB.CountOver(alias:) rejects an injected alias")]
    public void WindowedAggregateRejectsInjectedAlias() {
        IDBClient client = TestData.CreateDatabaseAccess();
        Assert.Throws<InvalidIdentifierException>(() => new LoadOperation<InjModel>(client, EntityDescriptor.Create, DB.CountOver(alias: "x; DROP TABLE t; --"))
                                                         .Prepare());
    }

    [Test, Parallelizable, Description("DB.Field(string) rejects an injected fragment")]
    public void FieldTokenRejectsInjectedName() {
        IDBClient client = TestData.CreateDatabaseAccess();
        Assert.Throws<InvalidIdentifierException>(() => new LoadOperation<InjModel>(client, EntityDescriptor.Create, DB.Field("x; DROP TABLE t; --"))
                                                         .Prepare());
    }

    [Test, Parallelizable, Description("a join alias carrying an injected fragment throws before the join is rendered")]
    public void JoinAliasRejectsInjectedAlias() {
        IDBClient client = TestData.CreateDatabaseAccess();
        Assert.Throws<InvalidIdentifierException>(() => new LoadOperation<InjModel>(client, EntityDescriptor.Create, DB.All)
                                                         .Join<InjModel>((a, b) => a.A == b.A, "x; DROP TABLE t; --")
                                                         .Prepare());
    }
}
