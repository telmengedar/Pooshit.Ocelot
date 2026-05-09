using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Tests.Models;
using Pooshit.Ocelot.Tokens;

namespace Pooshit.Ocelot.Tests.Postgres;

/// <summary>
/// Integration tests for ExecutePagedAsync against a real PostgreSQL instance.
/// Gated on the POSTGRES_CONNECTION environment variable — skips with Assert.Inconclusive when missing.
/// These tests exercise the multi-connection live-streaming path that the SQLite tests cannot cover.
/// </summary>
[TestFixture]
public class PostgresExecutePagedAsyncTests {

    IEntityManager em;

    static IEntityManager CreateEntityManager() {
        string connString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");
        if (string.IsNullOrEmpty(connString))
            Assert.Inconclusive("POSTGRES_CONNECTION not set — Postgres tests skipped");
        IDBClient client = ClientFactory.Create(() => new NpgsqlConnection(connString), new PostgreInfo(), true);
        return new EntityManager(client);
    }

    [SetUp]
    public void SetUp() {
        em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();
        // Delete all rows so each test starts clean (Postgres uses a shared schema)
        em.Delete<ValueModel>().Execute();
    }

    static async Task InsertRows(IEntityManager em, int count) {
        for (int i = 0; i < count; i++)
            await em.Insert<ValueModel>().Columns(v => v.Integer, v => v.String).ExecuteAsync(i, $"item-{i}");
    }

    // -------------------------------------------------------------------------
    // Basic correctness
    // -------------------------------------------------------------------------

    [Test]
    public async Task ZeroRows_TotalIsZero_ItemsIsEmpty() {
        WindowResult<ValueModel, long> result = await em.Load<ValueModel>().ExecutePagedAsync(10, 0);

        long total = await result.WindowValue;
        Assert.AreEqual(0L, total);

        List<ValueModel> items = [];
        await foreach (ValueModel item in result.Items)
            items.Add(item);
        Assert.AreEqual(0, items.Count);
    }

    [Test]
    public async Task OneRow_TotalIsOne_ItemsHasOneItem() {
        await InsertRows(em, 1);

        WindowResult<ValueModel, long> result = await em.Load<ValueModel>().ExecutePagedAsync(10, 0);

        long total = await result.WindowValue;
        Assert.AreEqual(1L, total);

        List<ValueModel> items = [];
        await foreach (ValueModel item in result.Items)
            items.Add(item);
        Assert.AreEqual(1, items.Count);
    }

    [Test]
    public async Task PartialPage_TotalIsFullCount_ItemsIsPage() {
        await InsertRows(em, 15);

        WindowResult<ValueModel, long> result = await em.Load<ValueModel>().ExecutePagedAsync(5, 0);

        long total = await result.WindowValue;
        Assert.AreEqual(15L, total);

        int count = 0;
        await foreach (ValueModel _ in result.Items)
            count++;
        Assert.AreEqual(5, count);
    }

    [Test]
    public async Task OffsetBeyondData_TotalIsZero_ItemsIsEmpty() {
        await InsertRows(em, 5);

        WindowResult<ValueModel, long> result = await em.Load<ValueModel>().ExecutePagedAsync(10, 100);

        long total = await result.WindowValue;
        // Window function over empty result — zero rows returned
        Assert.AreEqual(0L, total);

        List<ValueModel> items = [];
        await foreach (ValueModel item in result.Items)
            items.Add(item);
        Assert.AreEqual(0, items.Count);
    }

    [Test]
    public async Task Total_MatchesSeparateCountQuery() {
        await InsertRows(em, 20);

        WindowResult<ValueModel, long> result = await em.Load<ValueModel>().ExecutePagedAsync(7, 0);
        long pagedTotal = await result.WindowValue;

        await foreach (ValueModel _ in result.Items) { }

        long separateCount = em.Load<ValueModel>(DB.Count(DB.All)).ExecuteScalar<long>();
        Assert.AreEqual(separateCount, pagedTotal);
    }

    [Test]
    public async Task EagerAwaitTotal_BeforeIteratingItems_Works() {
        await InsertRows(em, 8);

        WindowResult<ValueModel, long> result = await em.Load<ValueModel>().ExecutePagedAsync(5, 0);

        // On Postgres (multi-connection) WindowValue is resolved from row 1 during ExecutePagedAsync
        long total = await result.WindowValue;
        Assert.AreEqual(8L, total);

        int count = 0;
        await foreach (ValueModel _ in result.Items)
            count++;
        Assert.AreEqual(5, count);
    }

    // -------------------------------------------------------------------------
    // Streaming: items arrive live (Postgres multi-connection path)
    // -------------------------------------------------------------------------

    [Test]
    public async Task Streaming_ItemsArrivedInOrder() {
        await InsertRows(em, 6);

        WindowResult<ValueModel, long> result = await em.Load<ValueModel>()
            .OrderBy(v => v.Integer)
            .ExecutePagedAsync(6, 0);

        await result.WindowValue; // already resolved
        List<int> values = [];
        await foreach (ValueModel item in result.Items)
            values.Add(item.Integer);

        Assert.AreEqual(6, values.Count);
        for (int i = 0; i < values.Count - 1; i++)
            Assert.Less(values[i], values[i + 1], "Items must arrive in ascending Integer order");
    }

    // -------------------------------------------------------------------------
    // Predicate filtering
    // -------------------------------------------------------------------------

    [Test]
    public async Task WithPredicate_TotalReflectsFilteredCount() {
        await InsertRows(em, 10);

        WindowResult<ValueModel, long> result = await em.Load<ValueModel>()
            .Where(v => v.Integer < 5)
            .ExecutePagedAsync(10, 0);

        long total = await result.WindowValue;
        Assert.AreEqual(5L, total);
    }

    // -------------------------------------------------------------------------
    // Validation: invalid args throw synchronously (same as SQLite path)
    // -------------------------------------------------------------------------

    [Test]
    public void NegativeLimit_ThrowsArgumentOutOfRange() {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await em.Load<ValueModel>().ExecutePagedAsync(-1, 0));
    }

    [Test]
    public void NegativeOffset_ThrowsArgumentOutOfRange() {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await em.Load<ValueModel>().ExecutePagedAsync(10, -1));
    }

    // -------------------------------------------------------------------------
    // Cancellation
    // -------------------------------------------------------------------------

    [Test]
    public void PreCanceledToken_ThrowsOperationCanceled() {
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await em.Load<ValueModel>().ExecutePagedAsync(10, 0, cts.Token));
    }

    // -------------------------------------------------------------------------
    // Entity mapping
    // -------------------------------------------------------------------------

    [Test]
    public async Task ItemsMappedCorrectly_EntityPropertiesSet() {
        await em.Insert<ValueModel>().Columns(v => v.Integer, v => v.String).ExecuteAsync(42, "hello");

        WindowResult<ValueModel, long> result = await em.Load<ValueModel>().ExecutePagedAsync(10, 0);
        await result.WindowValue;

        ValueModel item = null;
        await foreach (ValueModel v in result.Items)
            item = v;

        Assert.IsNotNull(item);
        Assert.AreEqual(42, item.Integer);
        Assert.AreEqual("hello", item.String);
    }
}
