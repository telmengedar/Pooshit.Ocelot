using System.Linq;
using Moq;
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

namespace Pooshit.Ocelot.Tests.Security;

[TestFixture, Parallelizable]
public class IdentifierInjectionTests {

    static bool PwnedExists(IDBClient client, string marker)
        => client.Query($"SELECT count(*) c FROM sqlite_master WHERE type='table' AND name='{marker}'").Rows[0]["c"].ToString() != "0";

    [Test, Parallelizable, Description("DeleteOperation(string table) rejects an injected table name and the seeded victim row survives untouched")]
    public void DeleteRejectsInjectedTableNameAndVictimSurvives() {
        IDBClient client = TestData.CreateDatabaseAccess();
        EntityManager em = new(client);
        em.CreateTable("victim").Column("first").Execute();
        em.InsertData("victim").Columns("first").Values("keeper").Execute();

        Assert.Throws<InvalidIdentifierException>(() => em.Delete("victim; CREATE TABLE pwned_delete(x); --").Execute());

        Assert.That(PwnedExists(client, "pwned_delete"), Is.False);
        Assert.That(client.Query("SELECT * FROM victim").Rows.Length, Is.EqualTo(1));
    }

    [Test, Parallelizable, Description("SQLiteInfo.Truncate rejects an injected table name; the seeded victim row survives")]
    public void TruncateRejectsInjectedTableNameAndVictimSurvives() {
        IDBClient client = TestData.CreateDatabaseAccess();
        EntityManager em = new(client);
        em.CreateTable("victim").Column("first").Execute();
        em.InsertData("victim").Columns("first").Values("keeper").Execute();

        Assert.Throws<InvalidIdentifierException>(() => client.DBInfo.Truncate(client, "victim]; CREATE TABLE pwned_truncate(x); --").GetAwaiter().GetResult());

        Assert.That(PwnedExists(client, "pwned_truncate"), Is.False);
        Assert.That(client.Query("SELECT * FROM victim").Rows.Length, Is.EqualTo(1));
    }

    [Test, Parallelizable, Description("SQLiteInfo.Truncate rejects an injected table name before opening a transaction")]
    public void SqliteTruncateRejectsInjectedTableBeforeTransaction() {
        SQLiteInfo dbInfo = new();
        Mock<IDBClient> client = new();
        client.Setup(c => c.DBInfo).Returns(dbInfo);

        Assert.ThrowsAsync<InvalidIdentifierException>(() => dbInfo.Truncate(client.Object, "victim]; CREATE TABLE pwned_truncate(x); --"));

        client.Verify(c => c.Transaction(), Times.Never);
    }

    [Test, Parallelizable, Description("InsertData(string table) rejects an injected table name at Prepare")]
    public void InsertDataRejectsInjectedTableName() {
        IDBClient client = TestData.CreateDatabaseAccess();
        EntityManager em = new(client);
        Assert.Throws<InvalidIdentifierException>(() => em.InsertData("t; DROP TABLE x; --").Columns("a").Prepare());
    }

    [Test, Parallelizable, Description("LoadData(string table) rejects an injected table name at Prepare")]
    public void LoadDataRejectsInjectedTableName() {
        IDBClient client = TestData.CreateDatabaseAccess();
        EntityManager em = new(client);
        Assert.Throws<InvalidIdentifierException>(() => em.LoadData("t; DROP TABLE x; --").Prepare());
    }

    [Test, Parallelizable, Description("DB.Column(string) rejects an injected column name")]
    public void ColumnTokenRejectsInjectedColumnName() {
        IDBClient client = TestData.CreateDatabaseAccess();
        Assert.Throws<InvalidIdentifierException>(() => new LoadOperation<InjModel>(client, EntityDescriptor.Create, DB.All)
                                                         .OrderBy(new OrderByCriteria(DB.Column("a]; DROP TABLE x; --")))
                                                         .Prepare());
    }

    [Test, Parallelizable, Description("DB.As alias is rejected when it carries an injected fragment")]
    public void AliasRejectsInjectedAlias() {
        IDBClient client = TestData.CreateDatabaseAccess();
        Assert.Throws<InvalidIdentifierException>(() => new LoadOperation<InjModel>(client, EntityDescriptor.Create, DB.As(DB.Column("a"), "x FROM sqlite_master; --"))
                                                         .Prepare());
    }

    [Test, Parallelizable, Description("DB.CustomFunction function name is rejected when it carries an injected fragment")]
    public void CustomFunctionNameRejectsInjectedName() {
        IDBClient client = TestData.CreateDatabaseAccess();
        Assert.Throws<InvalidIdentifierException>(() => new LoadOperation<InjModel>(client, EntityDescriptor.Create,
                                                             DB.CustomFunction("evil(1); DROP TABLE x; --", DB.Column("a")))
                                                         .Prepare());
    }

    [Test, Parallelizable, Description("UpdateData column name is rejected when it carries an injected fragment (Set without Values)")]
    public void UpdateDataSetRejectsInjectedColumnName() {
        IDBClient client = TestData.CreateDatabaseAccess();
        EntityManager em = new(client);
        Assert.Throws<InvalidIdentifierException>(() => em.UpdateData("victim").Set("a\"=1; DROP TABLE x; --").Prepare());
    }

    [Test, Parallelizable, Description("UpdateData column name is rejected when it carries an injected fragment (Set with Values)")]
    public void UpdateDataSetWithValuesRejectsInjectedColumnName() {
        IDBClient client = TestData.CreateDatabaseAccess();
        EntityManager em = new(client);
        Assert.Throws<InvalidIdentifierException>(() => em.UpdateData("victim").Set("a", "b\"=1; DROP TABLE x; --").Values(5, 6).Prepare());
    }

    [Test, Parallelizable, Description("UpdateData(string table) rejects an injected table name at Prepare")]
    public void UpdateDataRejectsInjectedTableName() {
        IDBClient client = TestData.CreateDatabaseAccess();
        EntityManager em = new(client);
        Assert.Throws<InvalidIdentifierException>(() => em.UpdateData("t; DROP TABLE x; --").Set("a").Prepare());
    }

    [Test, Parallelizable, Description("CreateTable(string table) rejects an injected table name at Prepare")]
    public void CreateTableRejectsInjectedTableName() {
        IDBClient client = TestData.CreateDatabaseAccess();
        EntityManager em = new(client);
        Assert.Throws<InvalidIdentifierException>(() => em.CreateTable("t; DROP TABLE x; --").Column("a").Prepare());
    }

    [Test, Parallelizable, Description("AlterTableOperation(string table) rejects an injected table name at Prepare")]
    public void AlterTableRejectsInjectedTableName() {
        IDBClient client = TestData.CreateDatabaseAccess();
        Assert.Throws<InvalidIdentifierException>(() => new AlterTableOperation(client, "t; DROP TABLE x; --").Drop("a").Prepare());
    }
}
