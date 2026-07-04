using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Npgsql;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.CustomTypes;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Entities.Operations.Tables;
using Pooshit.Ocelot.Extensions;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Schemas;
using Pooshit.Ocelot.Tests.Entities;
using Pooshit.Ocelot.Tests.Models;

namespace Pooshit.Ocelot.Tests.Postgres;

/// <summary>
/// only used for local tests to test against a real postgres
/// deactivated in repository
/// </summary>
[TestFixture, Parallelizable]
public class PostgresLocalTests {

    [Test]
    public async Task BigInteger() {
        
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")))
            Assert.Inconclusive("Test only active on local dev machine");

        IDBClient dbclient = ClientFactory.Create(() => new NpgsqlConnection(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")), new PostgreInfo(), true);
        EntityManager entitymanager = new(dbclient);

        BigIntData entity = new() {
                                      Data = System.Numerics.BigInteger.Parse("92347657832742983982365723472321")
                                  };
        SchemaService schemaService = new(dbclient);
        await schemaService.CreateOrUpdateSchema<BigIntData>();

        PreparedOperation insertoperation = entitymanager.Insert<BigIntData>()
                                                         .Columns(d => d.Data)
                                                         .Prepare();

        await insertoperation.ExecuteAsync(entity.Data);

        BigIntData loadedData = await entitymanager.Load<BigIntData>()
                                                   .ExecuteEntityAsync();

        Assert.NotNull(loadedData);
        Assert.AreEqual(entity.Data, loadedData.Data);
    }

    [Test]
    public async Task BigIntegerRange() {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")))
            Assert.Inconclusive("Test only active on local dev machine");
            
        IDBClient dbclient = ClientFactory.Create(() => new NpgsqlConnection(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")), new PostgreInfo(), true);
        EntityManager entitymanager = new(dbclient);

        BigIntData entity = new() {
                                      Range = new Range<BigInteger>(new BigInteger(5), new BigInteger(120)) {
                                                                                                                LowerInclusive = true,
                                                                                                                UpperInclusive = false
                                                                                                            }
                                  };
        SchemaService schemaService = new(dbclient);
        await schemaService.CreateOrUpdateSchema<BigIntData>();
            
        await entitymanager.Truncate<BigIntData>();
            
        PreparedOperation insertoperation = entitymanager.Insert<BigIntData>()
                                                         .Columns(d=>d.Data, d => d.Range)
                                                         .Prepare();

        await insertoperation.ExecuteAsync(0, entity.Range);
        await insertoperation.ExecuteAsync(0, new Range<BigInteger>(6, 9));

        BigIntData[] loadedData = (await entitymanager.Load<BigIntData>()
                                                      .Where(d=>d.Range.Contains(11m))
                                                      .ExecuteEntitiesAsync().ToArray());

        Assert.AreEqual(1, loadedData.Length);
            
        Assert.NotNull(loadedData);
        Assert.AreEqual(entity.Range.Lower, loadedData[0].Range.Lower);
        Assert.AreEqual(entity.Range.Upper, loadedData[0].Range.Upper);
        Assert.AreEqual(entity.Range.LowerInclusive, loadedData[0].Range.LowerInclusive);
        Assert.AreEqual(entity.Range.UpperInclusive, loadedData[0].Range.UpperInclusive);

    }

    [Test, Parallelizable]
    public async Task LoadArrayPrepared() {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")))
            Assert.Inconclusive("Test only active on local dev machine");

        IDBClient dbclient = ClientFactory.Create(() => new NpgsqlConnection(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")), new PostgreInfo(), true);
        EntityManager entitymanager = new(dbclient);

        string[] terms = ["motherfucker", "retard", "asshole", "bitch", "fuck"];
        Word[] result = await entitymanager.Load<Word>()
                                           .Where(w => w.Text.In(DBParameter<string[]>.Value))
                                           .Prepare()
                                           .ExecuteEntitiesAsync([terms]).ToArray();
        Assert.AreEqual(5, result.Length);
    }

    [Test, Parallelizable]
    public async Task LoadArrayUnprepared() {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")))
            Assert.Inconclusive("Test only active on local dev machine");

        IDBClient dbclient = ClientFactory.Create(() => new NpgsqlConnection(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")), new PostgreInfo(), true);
        EntityManager entitymanager = new(dbclient);

        string[] terms = ["motherfucker", "retard", "asshole", "bitch", "fuck"];
        Word[] result = await entitymanager.Load<Word>()
                                           .Where(w => w.Text.In(DBParameter<string[]>.Value))
                                           .ExecuteEntitiesAsync([terms]).ToArray();
        Assert.AreEqual(5, result.Length);
    }

    [Test, Parallelizable]
    public async Task LoadArrayUnpreparedNewArray() {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")))
            Assert.Inconclusive("Test only active on local dev machine");

        IDBClient dbclient = ClientFactory.Create(() => new NpgsqlConnection(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")), new PostgreInfo(), true);
        EntityManager entitymanager = new(dbclient);

        Word[] result = await entitymanager.Load<Word>()
                                           .Where(w => w.Text.In(new[] { "motherfucker", "retard", "asshole", "bitch", "fuck" }))
                                           .ExecuteEntitiesAsync().ToArray();
        Assert.AreEqual(5, result.Length);
    }

    [Test, Parallelizable]
    public async Task LoadArrayWithoutParameterPrepared() {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")))
            Assert.Inconclusive("Test only active on local dev machine");

        IDBClient dbclient = ClientFactory.Create(() => new NpgsqlConnection(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")), new PostgreInfo(), true);
        EntityManager entitymanager = new(dbclient);

        string[] terms = ["motherfucker", "retard", "asshole", "bitch", "fuck"];
        Word[] result = await entitymanager.Load<Word>()
                                           .Where(w => w.Text.In(terms))
                                           .Prepare()
                                           .ExecuteEntitiesAsync().ToArray();
        Assert.AreEqual(5, result.Length);
    }

    [Test, Parallelizable]
    public async Task LoadArrayWithoutParameterUnprepared() {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")))
            Assert.Inconclusive("Test only active on local dev machine");

        IDBClient dbclient = ClientFactory.Create(() => new NpgsqlConnection(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")), new PostgreInfo(), true);
        EntityManager entitymanager = new(dbclient);

        string[] terms = ["motherfucker", "retard", "asshole", "bitch", "fuck"];
        Word[] result = await entitymanager.Load<Word>()
                                           .Where(w => w.Text.In(terms))
                                           .ExecuteEntitiesAsync().ToArray();
        Assert.AreEqual(5, result.Length);
    }

    [Test, Parallelizable]
    public async Task LoadNotInArrayPrepared() {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")))
            Assert.Inconclusive("Test only active on local dev machine");

        IDBClient dbclient = ClientFactory.Create(() => new NpgsqlConnection(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")), new PostgreInfo(), true);
        EntityManager entitymanager = new(dbclient);

        string[] terms = ["motherfucker", "retard", "asshole", "bitch", "fuck"];
        Word[] result = await entitymanager.Load<Word>()
                                           .Where(w => !w.Text.In(DBParameter<string[]>.Value))
                                           .Limit(10)
                                           .ExecuteEntitiesAsync([terms]).ToArray();
        Assert.AreEqual(10, result.Length);
    }

    [Test, Parallelizable]
    public async Task TransferDoesntHang() {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")))
            Assert.Inconclusive("Test only active on local dev machine");

        IEntityManager sourceDatabase=new EntityManager(ClientFactory.Create(new SqliteConnection("Data Source=:memory:"), new SQLiteInfo()));
        sourceDatabase.Create<ValueModel>();
            
        IDBClient targetClient = ClientFactory.Create(() => new NpgsqlConnection(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")), new PostgreInfo(), true);
        IEntityManager targetDatabase = new EntityManager(targetClient);

        if (targetDatabase.Exists("valuemodel"))
            targetDatabase.Drop<ValueModel>();

        targetDatabase.Create<ValueModel>();
        await targetDatabase.Truncate<ValueModel>();

        PreparedOperation insert=sourceDatabase.Insert<ValueModel>().Columns(v => v.Integer).Prepare();
        for (int i = 0; i < 800; ++i)
            await insert.ExecuteAsync(i);

        long objectCount = sourceDatabase.LoadData("valuemodel").Columns(new DataField("COUNT(*)")).ExecuteScalar<long>();
            
        List<object> parameters = new();

        PreparedOperation insertOperation=null;
        int offset = 0;
        while (offset<objectCount) {
            using IDataReader reader = await sourceDatabase.LoadData("valuemodel").Offset(offset).Limit(500).ExecuteReaderAsync();
            if (insertOperation == null) {
                List<string> columns = new();
                for (int i = 0; i < reader.FieldCount; ++i)
                    columns.Add(reader.GetName(i));

                insertOperation = targetDatabase.InsertData("valuemodel").Columns(columns.ToArray()).Prepare();
            }

            while (reader.Read()) {
                parameters.Clear();
                for (int i = 0; i < reader.FieldCount; ++i)
                    parameters.Add(reader.GetValue(i));
                await insertOperation.ExecuteAsync(parameters.ToArray());
            }

            offset += 500;
        }

        Assert.Pass();
    }

    /// <summary>
    /// verifies that a prepared READ transparently survives a forced backend connection loss
    /// when Increment 2 (connection-loss retry on read paths) is active.
    /// Without Increment 2 this test should fail with a StatementException.
    /// </summary>
    [Test]
    public async Task PreparedReadSurvivesConnectionLoss() {
        string connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");
        if (string.IsNullOrEmpty(connectionString))
            Assert.Inconclusive("Test only active on local dev machine");

        // Set up the table using a factory-backed client.
        // Use raw SQL DROP IF EXISTS to avoid the pre-existing 'NO'→bool GetSchema bug (DiVoid #3287).
        IDBClient setupClient = ClientFactory.Create(() => new NpgsqlConnection(connectionString), new PostgreInfo(), true);
        EntityManager setupManager = new(setupClient);
        await setupClient.NonQueryAsync("DROP TABLE IF EXISTS valuemodel CASCADE");
        setupManager.Create<ValueModel>();
        await setupClient.NonQueryAsync("INSERT INTO valuemodel (integer) VALUES (7)");

        // Open a single held connection — this is the specific connection we will terminate
        NpgsqlConnection conn = new(connectionString);
        await conn.OpenAsync();
        IDBClient dbclient = ClientFactory.Create(conn, new PostgreInfo());
        EntityManager em = new(dbclient);

        // Prepare the read — with PreparationSupported=true this routes through ReaderPreparedAsync
        PreparedLoadOperation<ValueModel> readOp = em.Load<ValueModel>().Prepare();

        // Baseline: confirm the prepared read works on a live connection
        ValueModel baseline = await readOp.ExecuteEntityAsync();
        Assert.IsNotNull(baseline, "Baseline read should succeed before connection loss");

        // Get the backend PID from the single-connection client while it is alive
        long backendPid = Convert.ToInt64(await dbclient.ScalarAsync("SELECT pg_backend_pid()"));

        // Kill the backend from a separate connection
        await using (NpgsqlConnection killer = new(connectionString)) {
            await killer.OpenAsync();
            await using NpgsqlCommand killCmd = new($"SELECT pg_terminate_backend({backendPid})", killer);
            await killCmd.ExecuteScalarAsync();
        }

        // With Increment 2: the connection-loss is classified by PostgreInfo.IsConnectionLost,
        // OpenReaderWithRetry reconnects and re-prepares, and the read succeeds transparently
        ValueModel recovered = await readOp.ExecuteEntityAsync();
        Assert.IsNotNull(recovered, "Read should succeed after connection loss via transparent retry");
        Assert.AreEqual(7, recovered.Integer, "Recovered data should match the inserted value");
    }

    /// <summary>
    /// verifies that a prepared WRITE after a forced backend connection loss surfaces the error
    /// rather than silently retrying — write paths are structurally excluded from retry.
    /// </summary>
    [Test]
    public async Task PreparedWriteAfterConnectionLossThrows() {
        string connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");
        if (string.IsNullOrEmpty(connectionString))
            Assert.Inconclusive("Test only active on local dev machine");

        // Set up the table using a factory-backed client.
        // Use raw SQL DROP IF EXISTS to avoid the pre-existing 'NO'→bool GetSchema bug (DiVoid #3287).
        IDBClient setupClient = ClientFactory.Create(() => new NpgsqlConnection(connectionString), new PostgreInfo(), true);
        EntityManager setupManager = new(setupClient);
        await setupClient.NonQueryAsync("DROP TABLE IF EXISTS valuemodel CASCADE");
        setupManager.Create<ValueModel>();

        // Open a single held connection
        NpgsqlConnection conn = new(connectionString);
        await conn.OpenAsync();
        IDBClient dbclient = ClientFactory.Create(conn, new PostgreInfo());
        EntityManager em = new(dbclient);

        // Prepare the write — with PreparationSupported=true this routes through NonQueryPreparedAsync
        PreparedOperation insertOp = em.Insert<ValueModel>().Columns(v => v.Integer).Prepare();

        // Baseline write: should succeed on the live connection
        await insertOp.ExecuteAsync(42);

        // Get the backend PID while the connection is alive
        long backendPid = Convert.ToInt64(await dbclient.ScalarAsync("SELECT pg_backend_pid()"));

        // Kill the backend
        await using (NpgsqlConnection killer = new(connectionString)) {
            await killer.OpenAsync();
            await using NpgsqlCommand killCmd = new($"SELECT pg_terminate_backend({backendPid})", killer);
            await killCmd.ExecuteScalarAsync();
        }

        // Write path has NO retry — NonQueryPreparedAsync does not call IsConnectionLost.
        // The error must be surfaced (thrown) rather than silently suppressed or retried.
        Assert.CatchAsync<Exception>(() => insertOp.ExecuteAsync(99));
    }
}