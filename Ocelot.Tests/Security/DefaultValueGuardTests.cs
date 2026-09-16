using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Errors;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Schemas;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Security.Models;

namespace Pooshit.Ocelot.Tests.Security;


[TestFixture, Parallelizable]
public class DefaultValueGuardTests {

    [Test, Parallelizable, Description("a string DEFAULT containing a quote breaks out of the ddl literal on CREATE TABLE")]
    public void SqliteCreateColumnRejectsQuoteInStringDefault() {
        IDBClient client = TestData.CreateDatabaseAccess();
        EntityManager em = new(client);
        Assert.Throws<InvalidIdentifierException>(() => em.CreateTable("victim").Column("first", typeof(string), defaultvalue: "a'; DROP TABLE x; --").Execute());
    }

    [Test, Parallelizable, Description("a string DEFAULT containing a quote breaks out of the ddl literal on ADD COLUMN")]
    public void SqliteAddColumnRejectsQuoteInStringDefault() {
        SQLiteInfo dbInfo = new();
        OperationPreparator preparator = new();
        Assert.Throws<InvalidIdentifierException>(() => dbInfo.AddColumn(preparator, new ColumnDescriptor("first", "TEXT") { DefaultValue = "a'; DROP TABLE x; --" }));
    }

    [Test, Parallelizable, Description("a bare (non-string) DEFAULT whose rendered text is not a plain token breaks out of the ddl literal")]
    public void SqliteCreateColumnRejectsBareDefaultWithBreakout() {
        SQLiteInfo dbInfo = new();
        OperationPreparator preparator = new();
        Assert.Throws<InvalidIdentifierException>(() => dbInfo.CreateColumn(preparator, new ColumnDescriptor("first", "INTEGER") { DefaultValue = new BreakoutToString() }));
    }

    [Test, Parallelizable, Description("Postgres column DEFAULT rejects a quote breakout (rendered-only, no live connection)")]
    public void PostgresColumnAttributesRejectsQuoteInStringDefault() {
        PostgreInfo dbInfo = new();
        OperationPreparator preparator = new();
        Assert.Throws<InvalidIdentifierException>(() => dbInfo.CreateColumn(preparator, new ColumnDescriptor("first", Types.String) { DefaultValue = "a'; DROP TABLE x; --" }));
    }

    [Test, Parallelizable, Description("a fluent CreateTable() DEFAULT carrying a quote is rejected")]
    public void CreateTableFluentRejectsQuoteInDefault() {
        IDBClient client = TestData.CreateDatabaseAccess();
        EntityManager em = new(client);
        Assert.Throws<InvalidIdentifierException>(() => em.CreateTable("victim").Column("first", typeof(string), defaultvalue: "a'b").Execute());
    }

    [TestCase("")]
    [TestCase("none")]
    [TestCase("0")]
    [TestCase("-1.5")]
    [TestCase("true")]
    [Parallelizable]
    public void DefaultAcceptsPlainValues(string value) {
        IDBClient client = TestData.CreateDatabaseAccess();
        EntityManager em = new(client);
        Assert.DoesNotThrow(() => em.CreateTable("victim_" + System.Guid.NewGuid().ToString("N")).Column("first", typeof(string), defaultvalue: value).Execute());
    }
}
