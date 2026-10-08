using System;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Schemas;

namespace Pooshit.Ocelot.Tests.Postgres;

[TestFixture, Parallelizable]
public class PostgresSchemaTransactionTests {

    static IDBClient CreateClient() {
        string connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");
        if (string.IsNullOrEmpty(connectionString))
            Assert.Inconclusive("POSTGRES_CONNECTION not set");

        return ClientFactory.Create(() => new NpgsqlConnection(connectionString), new PostgreInfo(), true);
    }

    static string CreateTableName() {
        return "stx_" + Guid.NewGuid().ToString("N");
    }

    [Test, Parallelizable, Description("GetSchemaAsync reports an uncommitted column type change made in the supplied transaction")]
    public async Task GetSchemaAsync_ColumnTypeChangedInTransaction_ReportsNewType() {
        IDBClient client = CreateClient();
        string table = CreateTableName();
        await client.NonQueryAsync($"CREATE TABLE {table} (id integer, value integer)");
        try {
            using Transaction transaction = client.Transaction();
            await client.NonQueryAsync(transaction, $"ALTER TABLE {table} ALTER COLUMN value TYPE text");

            TableSchema schema = (TableSchema)await client.DBInfo.GetSchemaAsync(client, table, transaction);

            Assert.That(schema.Columns.Single(c => c.Name == "value").Type, Is.EqualTo("text"));
        }
        finally {
            await client.NonQueryAsync($"DROP TABLE IF EXISTS {table}");
        }
    }

    [Test, Parallelizable, Description("GetSchemaAsync reports a table created in the supplied transaction before it is committed")]
    public async Task GetSchemaAsync_TableCreatedInTransaction_ReportsColumns() {
        IDBClient client = CreateClient();
        string table = CreateTableName();
        try {
            using Transaction transaction = client.Transaction();
            await client.NonQueryAsync(transaction, $"CREATE TABLE {table} (id integer)");

            TableSchema schema = (TableSchema)await client.DBInfo.GetSchemaAsync(client, table, transaction);

            Assert.That(schema.Columns.Select(c => c.Name).ToArray(), Is.EqualTo(new[] { "id" }));
        }
        finally {
            await client.NonQueryAsync($"DROP TABLE IF EXISTS {table}");
        }
    }

    [Test, Parallelizable, Description("GetSchemaAsync reports a view created in the supplied transaction before it is committed")]
    public async Task GetSchemaAsync_ViewCreatedInTransaction_ReportsViewSchema() {
        IDBClient client = CreateClient();
        string table = CreateTableName();
        string view = table + "_v";
        await client.NonQueryAsync($"CREATE TABLE {table} (id integer)");
        try {
            using Transaction transaction = client.Transaction();
            await client.NonQueryAsync(transaction, $"CREATE VIEW {view} AS SELECT id FROM {table}");

            Schema schema = await client.DBInfo.GetSchemaAsync(client, view, transaction);

            Assert.That(schema, Is.InstanceOf<ViewSchema>());
        }
        finally {
            await client.NonQueryAsync($"DROP TABLE IF EXISTS {table} CASCADE");
        }
    }

    [Test, Parallelizable, Description("GetSchemaAsync reports an index created in the supplied transaction before it is committed")]
    public async Task GetSchemaAsync_IndexCreatedInTransaction_ReportsIndex() {
        IDBClient client = CreateClient();
        string table = CreateTableName();
        await client.NonQueryAsync($"CREATE TABLE {table} (id integer, value integer)");
        try {
            using Transaction transaction = client.Transaction();
            await client.NonQueryAsync(transaction, $"CREATE INDEX idx_{table}_value ON {table} (value)");

            TableSchema schema = (TableSchema)await client.DBInfo.GetSchemaAsync(client, table, transaction);

            Assert.That(schema.Index.Select(i => i.Name).ToArray(), Is.EqualTo(new[] { "value" }));
        }
        finally {
            await client.NonQueryAsync($"DROP TABLE IF EXISTS {table}");
        }
    }

    [Test, Parallelizable, Description("GetSchemaAsync without a transaction reports committed state only")]
    public async Task GetSchemaAsync_NoTransactionWhileColumnChangedInTransaction_ReportsCommittedType() {
        IDBClient client = CreateClient();
        string table = CreateTableName();
        await client.NonQueryAsync($"CREATE TABLE {table} (id integer, value integer)");
        try {
            using Transaction transaction = client.Transaction();
            await client.NonQueryAsync(transaction, $"ALTER TABLE {table} ALTER COLUMN value TYPE text");

            TableSchema schema = (TableSchema)await client.DBInfo.GetSchemaAsync(client, table, null);

            Assert.That(schema.Columns.Single(c => c.Name == "value").Type, Is.EqualTo("integer"));
        }
        finally {
            await client.NonQueryAsync($"DROP TABLE IF EXISTS {table}");
        }
    }
}
