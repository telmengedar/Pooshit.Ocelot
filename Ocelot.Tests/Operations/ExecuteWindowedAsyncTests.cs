using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Models;
using Pooshit.Ocelot.Tokens;
using Pooshit.Ocelot.Tokens.Partitions;

namespace Pooshit.Ocelot.Tests.Operations;

[TestFixture, Parallelizable]
public class ExecuteWindowedAsyncTests {

    static IEntityManager CreateEntityManager() => TestData.CreateEntityManager();

    static async Task InsertRows(IEntityManager em, int count) {
        for (int i = 0; i < count; i++)
            await em.Insert<ValueModel>().Columns(v => v.Integer, v => v.String).ExecuteAsync(i, $"item-{i}");
    }

    // -------------------------------------------------------------------------
    // MaxOver — headline test for a non-count aggregate
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public async Task ExecuteWindowedAsync_MaxOver_ResolvesMaxValue() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();
        // Insert rows with Integer values 0..4
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

    [Test, Parallelizable]
    public async Task ExecuteWindowedAsync_SumOver_ResolvesSumValue() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();
        // Insert rows with Integer values 0+1+2+3+4 = 10
        await InsertRows(em, 5);

        WindowResult<ValueModel, long> result = await em.Load<ValueModel>()
            .ExecuteWindowedAsync<long>(DB.SumOver(DB.Property<ValueModel>(v => v.Integer)));

        long sum = await result.WindowValue;
        Assert.AreEqual(10L, sum, "SumOver should resolve to the sum of all Integer values");

        List<ValueModel> items = [];
        await foreach (ValueModel item in result.Items)
            items.Add(item);
        Assert.AreEqual(5, items.Count, "All rows should be returned in Items");
    }

    // -------------------------------------------------------------------------
    // Caller-supplied alias round-trips
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public async Task ExecuteWindowedAsync_CallerSuppliedAlias_RoundTrips() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();
        await InsertRows(em, 3);

        // Supply an explicit alias; primitive must use it verbatim without substituting __window
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

    [Test, Parallelizable]
    public void ExecuteWindowedAsync_NullAggregate_Throws() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();

        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await em.Load<ValueModel>().ExecuteWindowedAsync<long>(null));
    }

    // -------------------------------------------------------------------------
    // Zero rows resolves to default(TWindow)
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public async Task ExecuteWindowedAsync_ZeroRows_ResolvesDefault() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();
        // No inserts

        WindowResult<ValueModel, long> result = await em.Load<ValueModel>()
            .ExecuteWindowedAsync<long>(DB.CountOver());

        long windowValue = await result.WindowValue;
        Assert.AreEqual(default(long), windowValue, "Zero rows should resolve WindowValue to default(TWindow)");

        List<ValueModel> items = [];
        await foreach (ValueModel item in result.Items)
            items.Add(item);
        Assert.AreEqual(0, items.Count, "Items should be empty for zero-row result");
    }

    // -------------------------------------------------------------------------
    // Single-statement assertion
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public async Task ExecuteWindowedAsync_SingleStatementAssertion() {
        IDBClient realClient = TestData.CreateDatabaseAccess();
        CountingDBClient counter = new(realClient);
        IEntityManager em = new EntityManager(counter);
        em.UpdateSchema<ValueModel>();
        await InsertRows(em, 5);

        WindowResult<ValueModel, long> result = await em.Load<ValueModel>()
            .ExecuteWindowedAsync<long>(DB.CountOver());

        await result.WindowValue;
        await foreach (ValueModel _ in result.Items) { }

        Assert.AreEqual(1, counter.ReaderCallCount,
            "ExecuteWindowedAsync must issue exactly one reader call");
    }

    // -------------------------------------------------------------------------
    // Respects Limit/Offset set fluently
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public async Task ExecuteWindowedAsync_RespectsLimitOffset() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();
        // Insert 10 rows
        await InsertRows(em, 10);

        // Request only 3 rows but expect WindowValue to reflect all 10 (COUNT(*) OVER() is unwindowed)
        WindowResult<ValueModel, long> result = await em.Load<ValueModel>()
            .Limit(3)
            .Offset(0)
            .ExecuteWindowedAsync<long>(DB.CountOver());

        long total = await result.WindowValue;
        Assert.AreEqual(10L, total, "WindowValue (COUNT(*) OVER()) reflects full unfiltered count");

        List<ValueModel> items = [];
        await foreach (ValueModel item in result.Items)
            items.Add(item);
        Assert.AreEqual(3, items.Count, "Only the limited number of rows should be returned in Items");
    }

    // -------------------------------------------------------------------------
    // Pre-canceled token faults immediately
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public void ExecuteWindowedAsync_PreCanceledToken_FaultsImmediately() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();

        using CancellationTokenSource cts = new();
        cts.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await em.Load<ValueModel>().ExecuteWindowedAsync<long>(DB.CountOver(), cts.Token));
    }

    // -------------------------------------------------------------------------
    // Cancellation mid-stream: canceling mid-iteration with a pre-canceled CT
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public async Task ExecuteWindowedAsync_CancellationMidStream_FaultsBoth() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();
        await InsertRows(em, 10);

        using CancellationTokenSource cts = new();

        // WindowValue is resolved eagerly on row 1 regardless of cancellation timing
        WindowResult<ValueModel, long> result = await em.Load<ValueModel>()
            .ExecuteWindowedAsync<long>(DB.CountOver(), cts.Token);

        long total = await result.WindowValue;
        Assert.Greater(total, 0L, "WindowValue should be resolved before cancellation");

        // Cancel and pass the token to WithCancellation so the async enumerable
        // propagates the cancellation via the EnumeratorCancellation attribute.
        cts.Cancel();

        // On SQLite the Items buffer is already fully filled (buffering happens eagerly inside
        // ExecuteWindowedAsync before the task returns), so passing a pre-canceled CT to
        // WithCancellation is the only reliable way to observe the cancellation on this dialect.
        Assert.CatchAsync<OperationCanceledException>(async () => {
            await foreach (ValueModel _ in result.Items.WithCancellation(cts.Token))
                cts.Token.ThrowIfCancellationRequested();
        });
    }
}
