using System;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Npgsql;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Tests.Data;

/// <summary>
/// Isolated disposable database for binary parameter tests.
/// </summary>
public sealed class StreamTestEnvironment : IDisposable {
    readonly DbConnection connection;
    readonly string administrationConnectionString;
    readonly string schema;

    /// <summary>
    /// Creates isolated source and target tables.
    /// </summary>
    /// <param name="postgres">Whether to use PostgreSQL instead of SQLite.</param>
    public StreamTestEnvironment(bool postgres) {
        if (postgres) {
            administrationConnectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");
            if (string.IsNullOrEmpty(administrationConnectionString))
                Assert.Inconclusive("POSTGRES_CONNECTION is required for real PostgreSQL stream tests.");
            schema = "stream_" + Guid.NewGuid().ToString("N");
            using NpgsqlConnection administration = new(administrationConnectionString);
            administration.Open();
            using DbCommand command = administration.CreateCommand();
            command.CommandText = $"CREATE SCHEMA {schema}";
            command.ExecuteNonQuery();
            connection = new NpgsqlConnection(new NpgsqlConnectionStringBuilder(administrationConnectionString) { SearchPath = schema }.ToString());
            Client = ClientFactory.Create(connection, new PostgreInfo());
        }
        else {
            connection = new SqliteConnection("Data Source=:memory:");
            Client = ClientFactory.Create(connection, new SQLiteInfo());
        }
        try {
            Entities = new EntityManager(Client);
            Client.NonQuery($"CREATE TABLE streamsource (id INTEGER, content {(postgres ? "BYTEA" : "BLOB")})");
            Client.NonQuery($"CREATE TABLE streamtarget (id INTEGER, content {(postgres ? "BYTEA" : "BLOB")})");
        }
        catch {
            Dispose();
            throw;
        }
    }

    /// <summary>
    /// Database client for this test.
    /// </summary>
    public IDBClient Client { get; }

    /// <summary>
    /// Entity manager for this test.
    /// </summary>
    public EntityManager Entities { get; }

    /// <summary>
    /// Connection string for independently owned connections to this test database.
    /// </summary>
    public string ConnectionString => connection.ConnectionString;

    /// <inheritdoc />
    public void Dispose() {
        connection.Dispose();
        if (schema == null)
            return;
        using NpgsqlConnection administration = new(administrationConnectionString);
        administration.Open();
        using DbCommand command = administration.CreateCommand();
        command.CommandText = $"DROP SCHEMA IF EXISTS {schema} CASCADE";
        command.ExecuteNonQuery();
    }
}
