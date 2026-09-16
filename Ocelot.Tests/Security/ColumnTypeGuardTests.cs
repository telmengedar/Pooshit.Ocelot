using System.Threading.Tasks;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Errors;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Schemas;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Models;

namespace Pooshit.Ocelot.Tests.Security;

[TestFixture, Parallelizable]
public class ColumnTypeGuardTests {

    [Test, Parallelizable, Description("SQLiteInfo.CreateColumn rejects an injected column type before any statement executes")]
    public void SqliteCreateColumnRejectsInjectedTypeName() {
        SQLiteInfo dbInfo = new();
        OperationPreparator preparator = new();
        Assert.Throws<InvalidIdentifierException>(() => dbInfo.CreateColumn(preparator, new ColumnDescriptor("first") { Type = "TEXT); CREATE TABLE pwned_type(x); --" }));
    }

    [Test, Parallelizable, Description("SQLiteInfo.AddColumn rejects an injected column type before any statement executes")]
    public void SqliteAddColumnRejectsInjectedTypeName() {
        SQLiteInfo dbInfo = new();
        OperationPreparator preparator = new();
        Assert.Throws<InvalidIdentifierException>(() => dbInfo.AddColumn(preparator, new ColumnDescriptor("first") { Type = "TEXT); CREATE TABLE pwned_type(x); --" }));
    }

    [Test, Parallelizable, Description("a null column type renders no type token and does not throw")]
    public void SqliteCreateColumnAcceptsTypelessColumn() {
        IDBClient client = TestData.CreateDatabaseAccess();
        OperationPreparator preparator = new();
        Assert.DoesNotThrow(() => client.DBInfo.CreateColumn(preparator, new ColumnDescriptor("first") { Type = null }));
        string commandText = preparator.GetOperation(client, false).CommandText;
        Assert.That(commandText, Does.Contain("[first]"));
    }

    [Test, Parallelizable, Description("CreateSchema rejects an injected column type and creates nothing")]
    public async Task SchemaServiceCreateRejectsInjectedTypeNameAndCreatesNothing() {
        IDBClient client = TestData.CreateDatabaseAccess();
        SchemaService service = new(client);

        Assert.ThrowsAsync<InvalidIdentifierException>(() => service.CreateSchema(new TableSchema {
            Name = "victim",
            Columns = [new("first") { Type = "TEXT); CREATE TABLE pwned_type(x); --" }]
        }));

        Assert.That(client.Query("SELECT name FROM sqlite_master WHERE type='table' AND name='victim'").Rows.Length, Is.EqualTo(0));
        Assert.That(client.Query("SELECT name FROM sqlite_master WHERE type='table' AND name='pwned_type'").Rows.Length, Is.EqualTo(0));
        await Task.CompletedTask;
    }

    [Test, Parallelizable, Description("the UpdateSchema recreate path rejects an injected column type before the rename executes; victim survives and victim_original is never created")]
    public async Task SchemaServiceUpdateRecreateRejectsInjectedTypeAndLeavesTableIntact() {
        IDBClient client = TestData.CreateDatabaseAccess();
        SchemaService service = new(client);
        await service.CreateSchema(new TableSchema {
            Name = "victim",
            Columns = [new("first") { Type = "TEXT" }, new("second") { Type = "TEXT" }]
        });

        Assert.ThrowsAsync<InvalidIdentifierException>(() => service.UpdateSchema("victim", new TableSchema {
            Name = "victim",
            Columns = [new("first") { Type = "TEXT" }, new("third") { Type = "TEXT); DROP TABLE x; --" }]
        }));

        Assert.That(client.Query("SELECT name FROM sqlite_master WHERE type='table' AND name='victim'").Rows.Length, Is.EqualTo(1));
        Assert.That(client.Query("SELECT name FROM sqlite_master WHERE type='table' AND name='victim_original'").Rows.Length, Is.EqualTo(0));
    }

    [Test, Parallelizable, Description("a fluent CreateTable() column type carrying an injected fragment is rejected and creates nothing")]
    public void CreateTableFluentRejectsInjectedTypeName() {
        IDBClient client = TestData.CreateDatabaseAccess();
        EntityManager em = new(client);
        Assert.Throws<InvalidIdentifierException>(() => em.CreateTable("victim").Column(new ColumnDescriptor("first") { Type = "TEXT); CREATE TABLE pwned_type(x); --" }).Execute());
        Assert.That(client.Query("SELECT name FROM sqlite_master WHERE type='table' AND name='victim'").Rows.Length, Is.EqualTo(0));
    }

    [Test, Parallelizable, Description("the generated GetDBType(Type) output on the entity path still passes the type-token grammar")]
    public void EntityColumnTypeStillRendersOnSqlite() {
        IDBClient client = TestData.CreateDatabaseAccess();
        EntityManager em = new(client);
        Assert.DoesNotThrow(() => em.UpdateSchema<ValueModel>());
    }
}
