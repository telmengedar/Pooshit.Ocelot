using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Errors;
using Pooshit.Ocelot.Extern;
using Pooshit.Ocelot.Tests.Data;

namespace Pooshit.Ocelot.Tests.Security;

[TestFixture, Parallelizable]
public class GenerateCreateStatementTests {

    [Test, Parallelizable, Description("the table name reaches sqlite_master as a bound parameter; a valid name still returns the create statement")]
    public void GenerateCreateStatementBindsTableName() {
        IDBClient client = TestData.CreateDatabaseAccess();
        EntityManager em = new(client);
        em.CreateTable("victim").Column("first").Execute();
        em.CreateTable("other").Column("second").Execute();

        string statement = client.DBInfo.GenerateCreateStatement(client, "victim").GetAwaiter().GetResult();
        Assert.That(statement, Does.Contain("victim"));
        Assert.That(statement, Does.Not.Contain("other"));
    }

    [Test, Parallelizable, Description("an identifier the guard rejects never reaches the parameterized value position")]
    public void GenerateCreateStatementRejectsInjectedName() {
        IDBClient client = TestData.CreateDatabaseAccess();
        Assert.Throws<InvalidIdentifierException>(() => client.DBInfo.GenerateCreateStatement(client, "victim; DROP TABLE x; --").GetAwaiter().GetResult());
    }

    [Test, Parallelizable, Description("truncate with reset-identity still resets the autoincrement sequence for a valid table name")]
    public void TruncateResetIdentityStillResetsSequence() {
        IDBClient client = TestData.CreateDatabaseAccess();
        EntityManager em = new(client);
        em.CreateTable("victim").Column("id", typeof(long), true, true).Column("first").Execute();
        em.InsertData("victim").Columns("first").Values("a").Execute();
        em.InsertData("victim").Columns("first").Values("b").Execute();

        client.DBInfo.Truncate(client, "victim", new() { ResetIdentity = true }).GetAwaiter().GetResult();
        em.InsertData("victim").Columns("first").Values("c").Execute();

        long id = Converter.Convert<long>(client.Scalar("SELECT id FROM victim WHERE first='c'"));
        Assert.That(id, Is.EqualTo(1));
    }
}
