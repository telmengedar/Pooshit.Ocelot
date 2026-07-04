using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Tests.Data;

namespace Pooshit.Ocelot.Tests.Clients;

/// <summary>
/// covers the connection-loss-resilience salvage increment (DiVoid #3270): the genuinely
/// async <see cref="DBClient.NonQueryPreparedAsync" /> and explicit parameter typing
/// </summary>
[TestFixture, Parallelizable]
public class PreparedStatementSalvageTests {

    [Test]
    public async Task NonQueryPreparedAsync_RepeatedInserts_AllRowsPersist() {
        IDBClient client = TestData.CreateDatabaseAccess();
        await client.NonQueryAsync(null, "CREATE TABLE salvage_async (id INTEGER)");

        const int rowCount = 25;
        for (int i = 0; i < rowCount; i++)
            await client.NonQueryPreparedAsync(null, "INSERT INTO salvage_async (id) VALUES (@1)", [i]);

        long stored = (long)await client.ScalarAsync(null, "SELECT COUNT(*) FROM salvage_async");
        Assert.AreEqual(rowCount, stored);
    }

    [Test]
    public void CreateParameter_IntValue_SetsInt32DbType() {
        using SqliteConnection connection = new("Data Source=:memory:");
        connection.Open();
        using IDbCommand command = connection.CreateCommand();

        new SQLiteInfo().CreateParameter(command, 42);

        IDbDataParameter parameter = (IDbDataParameter)command.Parameters[0];
        Assert.AreEqual(DbType.Int32, parameter.DbType);
        Assert.AreEqual(42, parameter.Value);
    }

    [Test]
    public void CreateParameter_StringValue_SetsStringDbType() {
        using SqliteConnection connection = new("Data Source=:memory:");
        connection.Open();
        using IDbCommand command = connection.CreateCommand();

        new SQLiteInfo().CreateParameter(command, "label");

        IDbDataParameter parameter = (IDbDataParameter)command.Parameters[0];
        Assert.AreEqual(DbType.String, parameter.DbType);
    }

    [Test]
    public void CreateParameter_NullValue_BindsDbNull() {
        using SqliteConnection connection = new("Data Source=:memory:");
        connection.Open();
        using IDbCommand command = connection.CreateCommand();

        new SQLiteInfo().CreateParameter(command, null);

        IDbDataParameter parameter = (IDbDataParameter)command.Parameters[0];
        Assert.AreEqual(DBNull.Value, parameter.Value);
    }
}
