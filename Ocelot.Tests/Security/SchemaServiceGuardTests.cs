using System.Threading.Tasks;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Errors;
using Pooshit.Ocelot.Schemas;
using Pooshit.Ocelot.Tests.Data;

namespace Pooshit.Ocelot.Tests.Security;

[TestFixture, Parallelizable]
public class SchemaServiceGuardTests {

    [Test, Parallelizable, Description("CreateSchema rejects an injected table name before any statement executes")]
    public void SchemaServiceCreateRejectsInjectedTableName() {
        IDBClient client = TestData.CreateDatabaseAccess();
        SchemaService service = new(client);

        Assert.ThrowsAsync<InvalidIdentifierException>(() => service.CreateSchema(new TableSchema {
            Name = "victim; DROP TABLE x; --",
            Columns = [new("first") { Type = "TEXT" }]
        }));
    }

    [Test, Parallelizable, Description("CreateSchema rejects an injected column name before any statement executes")]
    public void SchemaServiceCreateRejectsInjectedColumnName() {
        IDBClient client = TestData.CreateDatabaseAccess();
        SchemaService service = new(client);

        Assert.ThrowsAsync<InvalidIdentifierException>(() => service.CreateSchema(new TableSchema {
            Name = "victim",
            Columns = [new("first; DROP TABLE x; --") { Type = "TEXT" }]
        }));
    }

    [Test, Parallelizable, Description("CreateSchema rejects an injected index column and creates no table")]
    public async Task SchemaServiceCreateRejectsInjectedIndexColumnAndCreatesNothing() {
        IDBClient client = TestData.CreateDatabaseAccess();
        SchemaService service = new(client);

        Assert.ThrowsAsync<InvalidIdentifierException>(() => service.CreateSchema(new TableSchema {
            Name = "victim",
            Columns = [new("first") { Type = "TEXT" }],
            Index = [new("idx", ["bad; --"], null)]
        }));

        Assert.That(client.Query("SELECT name FROM sqlite_master WHERE type='table' AND name='victim'").Rows.Length, Is.EqualTo(0));
        await Task.CompletedTask;
    }

    [Test, Parallelizable, Description("UpdateSchema rejects an injected column name before the live table is touched")]
    public async Task SchemaServiceUpdateRejectsInjectedColumnNameBeforeAnyStatement() {
        IDBClient client = TestData.CreateDatabaseAccess();
        SchemaService service = new(client);

        await service.CreateSchema(new TableSchema {
            Name = "victim",
            Columns = [new("first") { Type = "TEXT" }]
        });

        Assert.ThrowsAsync<InvalidIdentifierException>(() => service.UpdateSchema("victim", new TableSchema {
            Name = "victim",
            Columns = [new("first") { Type = "TEXT" }, new("second; DROP TABLE x; --") { Type = "TEXT" }]
        }));

        TableSchema unchanged = await service.GetSchema("victim") as TableSchema;
        Assert.That(unchanged, Is.Not.Null);
        Assert.That(unchanged.Columns.Length, Is.EqualTo(1));
    }

    [Test, Parallelizable, Description("the UpdateSchema recreate path rejects an injected column name before the rename executes; victim survives and victim_original is never created")]
    public async Task SchemaServiceUpdateRecreateRejectsInjectedColumnAndLeavesTableIntact() {
        IDBClient client = TestData.CreateDatabaseAccess();
        SchemaService service = new(client);
        await service.CreateSchema(new TableSchema {
            Name = "victim",
            Columns = [new("first") { Type = "TEXT" }, new("second") { Type = "TEXT" }]
        });

        Assert.ThrowsAsync<InvalidIdentifierException>(() => service.UpdateSchema("victim", new TableSchema {
            Name = "victim",
            Columns = [new("first") { Type = "TEXT" }, new("bad; DROP TABLE x; --") { Type = "TEXT" }]
        }));

        Assert.That(client.Query("SELECT name FROM sqlite_master WHERE type='table' AND name='victim'").Rows.Length, Is.EqualTo(1));
        Assert.That(client.Query("SELECT name FROM sqlite_master WHERE type='table' AND name='victim_original'").Rows.Length, Is.EqualTo(0));
        Assert.That(client.Query("SELECT * FROM victim").Columns.Names, Has.Exactly(2).Items);
    }

    [Test, Parallelizable, Description("the UpdateSchema recreate path rejects an injected target schema name before the rename executes; victim survives and victim_original is never created")]
    public async Task SchemaServiceUpdateRejectsInjectedTargetNameBeforeAnyStatement() {
        IDBClient client = TestData.CreateDatabaseAccess();
        SchemaService service = new(client);
        await service.CreateSchema(new TableSchema {
            Name = "victim",
            Columns = [new("first") { Type = "TEXT" }, new("second") { Type = "TEXT" }]
        });

        Assert.ThrowsAsync<InvalidIdentifierException>(() => service.UpdateSchema("victim", new TableSchema {
            Name = "victim; --",
            Columns = [new("first") { Type = "TEXT" }]
        }));

        Assert.That(client.Query("SELECT name FROM sqlite_master WHERE type='table' AND name='victim'").Rows.Length, Is.EqualTo(1));
        Assert.That(client.Query("SELECT name FROM sqlite_master WHERE type='table' AND name='victim_original'").Rows.Length, Is.EqualTo(0));
    }

    [Test, Parallelizable, Description("RemoveSchema rejects an injected name")]
    public void RemoveSchemaRejectsInjectedName() {
        IDBClient client = TestData.CreateDatabaseAccess();
        SchemaService service = new(client);
        Assert.ThrowsAsync<InvalidIdentifierException>(() => service.RemoveSchema("victim; DROP TABLE x; --"));
    }
}
