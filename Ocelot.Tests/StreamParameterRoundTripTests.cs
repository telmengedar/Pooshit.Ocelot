using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.CustomTypes;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Errors;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Models;
using Pooshit.Ocelot.Tokens;

namespace Pooshit.Ocelot.Tests;

/// <summary>
/// Binary parameter and scalar subquery execution on SQLite and PostgreSQL.
/// </summary>
[TestFixture(false), TestFixture(true)]
public class StreamParameterRoundTripTests {
    readonly bool postgres;

    /// <summary>
    /// Creates a provider-specific fixture.
    /// </summary>
    /// <param name="postgres">Whether to use the real PostgreSQL server.</param>
    public StreamParameterRoundTripTests(bool postgres) => this.postgres = postgres;

    [TestCase(false)]
    [TestCase(true)]
    public async Task InsertUpdatePreparedAndConstantRoundTrip(bool empty) {
        using StreamTestEnvironment database = new(postgres);
        IDBClient client = database.Client;
        EntityManager entities = database.Entities;
        byte[] bytes = empty ? Array.Empty<byte>() : new byte[] { 0, 128, 255, 0, 7 };
        using MemoryStream source = new(new byte[] { 99, 0, 128, 255, 0, 7 });
        source.Position = empty ? source.Length : 1;
        StreamParameter parameter = new(source, bytes.Length);
        PreparedOperation insert = entities.Insert<StreamSource>().Columns(s => s.Id, s => s.Content).Prepare();
        Assert.AreEqual(empty ? 6 : 1, source.Position);
        await insert.ExecuteAsync(1, parameter);
        CollectionAssert.AreEqual(bytes, (byte[])await client.ScalarAsync("SELECT content FROM streamsource WHERE id=1"));
        Assert.IsTrue(source.CanRead);
        Assert.AreEqual(postgres ? "bytea" : "blob", await client.ScalarAsync(postgres
            ? "SELECT pg_typeof(content)::text FROM streamsource WHERE id=1"
            : "SELECT typeof(content) FROM streamsource WHERE id=1"));

        source.Position = empty ? source.Length : 1;
        await entities.Update<StreamSource>().Set(s => s.Content == DB.Constant(parameter).Blob).Where(s => s.Id == 1).ExecuteAsync();
        CollectionAssert.AreEqual(bytes, (byte[])await client.ScalarAsync("SELECT content FROM streamsource WHERE id=1"));

        source.Position = empty ? source.Length : 1;
        await entities.Update<StreamSource>().Set(s => s.Content == DBParameter.Blob).Where(s => s.Id == 1).Prepare().ExecuteAsync(parameter);
        CollectionAssert.AreEqual(bytes, (byte[])await client.ScalarAsync("SELECT content FROM streamsource WHERE id=1"));
        Assert.IsTrue(source.CanRead);
    }

    [Test]
    public async Task InsertValuesAndRawParametersRollbackAndLeaveSourcesOpenAfterFailure() {
        using StreamTestEnvironment database = new(postgres);
        IDBClient client = database.Client;
        EntityManager entities = database.Entities;
        byte[] bytes = { 0, 255, 3 };
        using MemoryStream source = new(bytes);
        using (Transaction transaction = client.Transaction()) {
            await entities.Insert<StreamSource>().Columns(s => s.Id, s => s.Content)
                .Values(1, new StreamParameter(source, bytes.Length)).ExecuteAsync(transaction);
            CollectionAssert.AreEqual(bytes, (byte[])await client.ScalarAsync(transaction, "SELECT content FROM streamsource WHERE id=1"));
            transaction.Rollback();
        }
        Assert.AreEqual(0L, Convert.ToInt64(await client.ScalarAsync("SELECT count(*) FROM streamsource")));
        Assert.IsTrue(source.CanRead);
        source.Position = 0;
        await client.NonQueryAsync("INSERT INTO streamsource VALUES (1, @1)", new StreamParameter(source, bytes.Length));
        CollectionAssert.AreEqual(bytes, (byte[])await client.ScalarAsync("SELECT content FROM streamsource WHERE id=1"));
        source.Position = 0;
        Assert.ThrowsAsync<StatementException>(async () => await client.NonQueryAsync("INSERT INTO missing_stream_table VALUES (@1)", new StreamParameter(source, bytes.Length)));
        Assert.IsTrue(source.CanRead);
        Assert.AreEqual(1, Convert.ToInt32(await client.ScalarAsync("SELECT 1")));
    }

    [Test]
    public async Task UpdateSetBlobScalarSubqueryCopiesSelectedBytesAndRollsBack() {
        using StreamTestEnvironment database = new(postgres);
        IDBClient client = database.Client;
        EntityManager entities = database.Entities;
        byte[] selected = { 0, 255, 128, 5 };
        byte[] untouched = { 7, 8 };
        await entities.Insert<StreamSource>().Columns(s => s.Id, s => s.Content).Values(1, untouched).ExecuteAsync();
        await entities.Insert<StreamSource>().Columns(s => s.Id, s => s.Content).Values(2, selected).ExecuteAsync();
        await entities.Insert<StreamTarget>().Columns(s => s.Id, s => s.Content).Values(1, untouched).ExecuteAsync();
        await entities.Insert<StreamTarget>().Columns(s => s.Id, s => s.Content).Values(2, untouched).ExecuteAsync();
        LoadOperation<StreamSource> query = entities.Load<StreamSource>(s => s.Content).Where(s => s.Id == 2);
        PreparedOperation copy = entities.Update<StreamTarget>().Set(t => t.Content == DB.Operation(query).Blob).Where(t => t.Id == 1).Prepare();
        StringAssert.Contains("SELECT", copy.CommandText.ToUpperInvariant());
        Assert.IsFalse(Array.Exists(copy.ConstantParameters, p => p is byte[]));
        using (Transaction transaction = client.Transaction()) {
            await copy.ExecuteAsync(transaction);
            CollectionAssert.AreEqual(selected, (byte[])await client.ScalarAsync(transaction, "SELECT content FROM streamtarget WHERE id=1"));
            CollectionAssert.AreEqual(untouched, (byte[])await client.ScalarAsync(transaction, "SELECT content FROM streamtarget WHERE id=2"));
            transaction.Rollback();
        }
        CollectionAssert.AreEqual(untouched, (byte[])await client.ScalarAsync("SELECT content FROM streamtarget WHERE id=1"));
        await copy.ExecuteAsync();
        CollectionAssert.AreEqual(selected, (byte[])await client.ScalarAsync("SELECT content FROM streamtarget WHERE id=1"));
    }

    [Test]
    public async Task DirectPreparedAsyncKeepsCommandAliveUntilServerCompletes() {
        if (!postgres)
            Assert.Inconclusive("A real asynchronous provider is required for the command lifetime guard.");
        using StreamTestEnvironment database = new(postgres);
        IDBClient client = database.Client;
        using MemoryStream source = new(new byte[] { 0, 128, 255 });
        IDBClient ownedClient = ClientFactory.Create(() => new NpgsqlConnection(database.ConnectionString), new PostgreInfo(), true);
        Assert.AreEqual(1, await ownedClient.NonQueryPreparedAsync(null,
            "INSERT INTO streamsource SELECT 1, @1 FROM pg_sleep(0.1)",
            new object[] { new StreamParameter(source, 3) }, CancellationToken.None));
        CollectionAssert.AreEqual(new byte[] { 0, 128, 255 }, (byte[])await client.ScalarAsync("SELECT content FROM streamsource"));
        Assert.IsTrue(source.CanRead);
    }

    [Test]
    public async Task PostgreSqlRawSeekableStreamStillRoundTrips() {
        if (!postgres)
            Assert.Inconclusive("Raw stream support is supplied by Npgsql.");
        using StreamTestEnvironment database = new(postgres);
        IDBClient client = database.Client;
        using MemoryStream source = new(new byte[] { 0, 255, 4 });
        await client.NonQueryAsync("INSERT INTO streamsource VALUES (1, @1)", source);
        CollectionAssert.AreEqual(new byte[] { 0, 255, 4 }, (byte[])await client.ScalarAsync("SELECT content FROM streamsource"));
        Assert.IsTrue(source.CanRead);
    }

    [Test]
    public async Task PostgreSqlCancellationDuringSourceReadLeavesSourceOpenAndRollsBack() {
        if (!postgres)
            Assert.Inconclusive("Npgsql supplies cancellable asynchronous source reads.");
        using StreamTestEnvironment database = new(postgres);
        IDBClient client = database.Client;
        using SlowSeekableStream source = new(2 * 1024 * 1024);
        using CancellationTokenSource cancellation = new();
        Task<int> write = client.NonQueryAsync(null, "INSERT INTO streamsource VALUES (1, @1)",
            new object[] { new StreamParameter(source, (int)source.Length) }, cancellation.Token);
        await source.FirstRead.WaitAsync(TimeSpan.FromSeconds(10));
        cancellation.Cancel();
        Assert.CatchAsync<OperationCanceledException>(async () => await write.WaitAsync(TimeSpan.FromSeconds(10)));
        Assert.IsTrue(source.CanRead);
        Assert.Greater(source.Position, 0);
        Assert.Less(source.Position, source.Length);
        Assert.AreEqual(0L, Convert.ToInt64(await client.ScalarAsync("SELECT count(*) FROM streamsource")));
        Assert.AreEqual(1, Convert.ToInt32(await client.ScalarAsync("SELECT 1")));
    }
}
