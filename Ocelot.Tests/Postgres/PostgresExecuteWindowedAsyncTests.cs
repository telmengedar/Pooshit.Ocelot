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
using Pooshit.Ocelot.Tests.Operations;
using Pooshit.Ocelot.Tokens;
using Pooshit.Ocelot.Tokens.Partitions;

namespace Pooshit.Ocelot.Tests.Postgres;

/// <summary>
/// Integration tests for ExecuteWindowedAsync against a real PostgreSQL instance.
/// Gated on the POSTGRES_CONNECTION environment variable — skips with Assert.Inconclusive when missing.
/// These tests exercise the multi-connection live-streaming path and non-count aggregate types
/// that the SQLite tests cannot meaningfully differ on.
/// </summary>
[TestFixture]
public class PostgresExecuteWindowedAsyncTests {

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
        em.Delete<ValueModel>().Execute();
    }

    static async Task InsertRows(IEntityManager em, int count) {
        for (int i = 0; i < count; i++)
            await em.Insert<ValueModel>().Columns(v => v.Integer, v => v.String).ExecuteAsync(i, $"item-{i}");
    }

    // -------------------------------------------------------------------------
    // MaxOver — headline generic aggregate test
    // -------------------------------------------------------------------------

    [Test]
    public async Task ExecuteWindowedAsync_MaxOver_ResolvesMaxValue() {
        await InsertRows(em, 5);

        WindowResult<ValueModel, int> result = await em.Load<ValueModel>()
            .ExecuteWindowedAsync<int>(DB.MaxOver(DB.Property<ValueModel>(v => v.Integer)));

        int maxValue = await result.WindowValue;
        Assert.AreEqual(4, maxValue, "MaxOver should resolve to the maximum Integer value across all rows");

        List<ValueModel> items = [];
        await foreach (ValueModel item in result.Items)
            items.Add(item);
        Assert.AreEqual(5, items.Count, "All rows should be returned in Items");
    }

    // -------------------------------------------------------------------------
    // SumOver
    // -------------------------------------------------------------------------

    [Test]
    public async Task ExecuteWindowedAsync_SumOver_ResolvesSumValue() {
        await InsertRows(em, 5);

        WindowResult<ValueModel, long> result = await em.Load<ValueModel>()
            .ExecuteWindowedAsync<long>(DB.SumOver(DB.Property<ValueModel>(v => v.Integer)));

        long sum = await result.WindowValue;
        Assert.AreEqual(10L, sum, "SumOver should resolve to 0+1+2+3+4 = 10");

        List<ValueModel> items = [];
        await foreach (ValueModel item in result.Items)
            items.Add(item);
        Assert.AreEqual(5, items.Count);
    }

    // -------------------------------------------------------------------------
    // Caller-supplied alias round-trips
    // -------------------------------------------------------------------------

    [Test]
    public async Task ExecuteWindowedAsync_CallerSuppliedAlias_RoundTrips() {
        await InsertRows(em, 3);

        WindowedAggregate agg = new(DB.Count(DB.All), alias: "_custom");
        WindowResult<ValueModel, long> result = await em.Load<ValueModel>()
            .ExecuteWindowedAsync<long>(agg);

        long count = await result.WindowValue;
        Assert.AreEqual(3L, count, "Value must be correctly extracted via the caller-supplied alias");

        List<ValueModel> items = [];
        await foreach (ValueModel item in result.Items)
            items.Add(item);
        Assert.AreEqual(3, items.Count);
    }

    // -------------------------------------------------------------------------
    // Null aggregate throws synchronously
    // -------------------------------------------------------------------------

    [Test]
    public void ExecuteWindowedAsync_NullAggregate_Throws() {
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await em.Load<ValueModel>().ExecuteWindowedAsync<long>(null));
    }

    // -------------------------------------------------------------------------
    // Zero rows resolves to default(TWindow)
    // -------------------------------------------------------------------------

    [Test]
    public async Task ExecuteWindowedAsync_ZeroRows_ResolvesDefault() {
        WindowResult<ValueModel, long> result = await em.Load<ValueModel>()
            .ExecuteWindowedAsync<long>(DB.CountOver());

        long windowValue = await result.WindowValue;
        Assert.AreEqual(default(long), windowValue, "Zero rows should resolve WindowValue to default(TWindow)");

        List<ValueModel> items = [];
        await foreach (ValueModel item in result.Items)
            items.Add(item);
        Assert.AreEqual(0, items.Count);
    }

    // -------------------------------------------------------------------------
    // Single-statement assertion
    // -------------------------------------------------------------------------

    [Test]
    public async Task ExecuteWindowedAsync_SingleStatementAssertion() {
        string connString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");
        if (string.IsNullOrEmpty(connString))
            Assert.Inconclusive("POSTGRES_CONNECTION not set — Postgres tests skipped");

        IDBClient realClient = ClientFactory.Create(() => new NpgsqlConnection(connString), new PostgreInfo(), true);
        CountingDBClient counter = new(realClient);
        IEntityManager countingEm = new EntityManager(counter);
        countingEm.UpdateSchema<ValueModel>();
        await InsertRows(countingEm, 5);

        WindowResult<ValueModel, long> result = await countingEm.Load<ValueModel>()
            .ExecuteWindowedAsync<long>(DB.CountOver());

        await result.WindowValue;
        await foreach (ValueModel _ in result.Items) { }

        Assert.AreEqual(1, counter.ReaderCallCount,
            "ExecuteWindowedAsync must issue exactly one reader call on Postgres");
    }

    // -------------------------------------------------------------------------
    // Respects Limit/Offset set fluently
    // -------------------------------------------------------------------------

    [Test]
    public async Task ExecuteWindowedAsync_RespectsLimitOffset() {
        await InsertRows(em, 10);

        WindowResult<ValueModel, long> result = await em.Load<ValueModel>()
            .Limit(3)
            .Offset(0)
            .ExecuteWindowedAsync<long>(DB.CountOver());

        long total = await result.WindowValue;
        Assert.AreEqual(10L, total, "WindowValue (COUNT(*) OVER()) reflects full unfiltered count");

        List<ValueModel> items = [];
        await foreach (ValueModel item in result.Items)
            items.Add(item);
        Assert.AreEqual(3, items.Count, "Only the limited number of rows should be returned");
    }

    // -------------------------------------------------------------------------
    // Pre-canceled token faults immediately
    // -------------------------------------------------------------------------

    [Test]
    public void ExecuteWindowedAsync_PreCanceledToken_FaultsImmediately() {
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await em.Load<ValueModel>().ExecuteWindowedAsync<long>(DB.CountOver(), cts.Token));
    }

    // -------------------------------------------------------------------------
    // Streaming: items arrive live on Postgres multi-connection path
    // -------------------------------------------------------------------------

    [Test]
    public async Task ExecuteWindowedAsync_Streaming_ItemsArrivedInOrder() {
        await InsertRows(em, 6);

        WindowResult<ValueModel, long> result = await em.Load<ValueModel>()
            .OrderBy(v => v.Integer)
            .ExecuteWindowedAsync<long>(DB.CountOver());

        await result.WindowValue;
        List<int> values = [];
        await foreach (ValueModel item in result.Items)
            values.Add(item.Integer);

        Assert.AreEqual(6, values.Count);
        for (int i = 0; i < values.Count - 1; i++)
            Assert.Less(values[i], values[i + 1], "Items must arrive in ascending Integer order");
    }
}
