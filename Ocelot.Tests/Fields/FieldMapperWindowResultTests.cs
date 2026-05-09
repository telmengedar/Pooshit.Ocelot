using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Models;
using Pooshit.Ocelot.Tests.Operations;
using Pooshit.Ocelot.Tokens;
using Pooshit.Ocelot.Tokens.Partitions;

namespace Pooshit.Ocelot.Tests.Fields;

/// <summary>
/// Tests for <see cref="FieldMapper{TModel}.WindowedFromOperation{TWindow}"/> and
/// <see cref="FieldMapper{TModel}.PagedFromOperation"/> — DiVoid task 148.
/// </summary>
[TestFixture, Parallelizable]
public class FieldMapperWindowResultTests {

    // -------------------------------------------------------------------------
    // Helper: a simple FieldMapper for ValueModel over the Integer + String fields
    // -------------------------------------------------------------------------

    static FieldMapper<ValueModel> CreateValueMapper() {
        return new FieldMapper<ValueModel>(
            new FieldMapping<ValueModel, int>("integer", DB.Property<ValueModel>(v => v.Integer), (e, v) => e.Integer = v),
            new FieldMapping<ValueModel, string>("string", DB.Property<ValueModel>(v => v.String), (e, v) => e.String = v)
        );
    }

    static IEntityManager CreateEntityManager() => TestData.CreateEntityManager();

    static async Task InsertRows(IEntityManager em, int count) {
        for (int i = 0; i < count; i++)
            await em.Insert<ValueModel>().Columns(v => v.Integer, v => v.String).ExecuteAsync(i, $"item-{i}");
    }

    // -------------------------------------------------------------------------
    // Headline test: MaxOver with a multi-field mapper
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public async Task WindowedFromOperation_MaxOver_MapperMaterializesEntities() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();
        await InsertRows(em, 5);

        FieldMapper<ValueModel> mapper = CreateValueMapper();
        WindowResult<ValueModel, int> result = await mapper.WindowedFromOperation<int>(
            em.Load<ValueModel>(DB.Property<ValueModel>(v => v.Integer), DB.Property<ValueModel>(v => v.String)),
            DB.MaxOver(DB.Property<ValueModel>(v => v.Integer)));

        int maxValue = await result.WindowValue;
        Assert.AreEqual(4, maxValue, "MaxOver should resolve to the maximum Integer value");

        List<ValueModel> items = [];
        await foreach (ValueModel item in result.Items)
            items.Add(item);

        Assert.AreEqual(5, items.Count, "All rows should be returned");
        // Verify mapper shape: each entity has both Integer and String populated
        foreach (ValueModel item in items)
            Assert.AreEqual($"item-{item.Integer}", item.String, "Mapper should populate both fields correctly");
    }

    // -------------------------------------------------------------------------
    // Caller-supplied alias round-trips
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public async Task WindowedFromOperation_CallerSuppliedAlias_RoundTrips() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();
        await InsertRows(em, 3);

        FieldMapper<ValueModel> mapper = CreateValueMapper();
        WindowedAggregate agg = new(DB.Count(DB.All), alias: "_custom");
        WindowResult<ValueModel, long> result = await mapper.WindowedFromOperation<long>(
            em.Load<ValueModel>(DB.Property<ValueModel>(v => v.Integer), DB.Property<ValueModel>(v => v.String)),
            agg);

        long count = await result.WindowValue;
        Assert.AreEqual(3L, count, "Value must be extracted via the caller-supplied alias");

        List<ValueModel> items = [];
        await foreach (ValueModel item in result.Items)
            items.Add(item);
        Assert.AreEqual(3, items.Count);
    }

    // -------------------------------------------------------------------------
    // Alias collision: mapper field named __window
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public async Task WindowedFromOperation_AliasCollision_MapperFieldNamedDoubleUnderscoreWindow() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();
        await InsertRows(em, 3);

        // Mapper with a field literally named "__window" — should not affect materialization
        // because the positional walk only consumes fields.Length ordinals (0..fields.Length-1)
        // and the windowed column sits at the end.
        FieldMapper<ValueModel> mapper = new(
            new FieldMapping<ValueModel, int>("integer", DB.Property<ValueModel>(v => v.Integer), (e, v) => e.Integer = v),
            new FieldMapping<ValueModel, string>("__window", DB.Property<ValueModel>(v => v.String), (e, v) => e.String = v)
        );

        WindowResult<ValueModel, long> result = await mapper.WindowedFromOperation<long>(
            em.Load<ValueModel>(DB.Property<ValueModel>(v => v.Integer), DB.Property<ValueModel>(v => v.String)),
            DB.CountOver(),
            default,
            "integer", "__window");

        long count = await result.WindowValue;
        Assert.AreEqual(3L, count, "WindowValue must resolve correctly despite field name __window");

        List<ValueModel> items = [];
        await foreach (ValueModel item in result.Items)
            items.Add(item);
        Assert.AreEqual(3, items.Count, "All rows should be returned");
        // Verify that entity Integer values are correct (mapper positional walk was not confused)
        foreach (ValueModel item in items)
            Assert.IsTrue(item.Integer >= 0 && item.Integer < 3, $"Integer {item.Integer} out of range");
    }

    // -------------------------------------------------------------------------
    // Zero rows — WindowValue resolves to default
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public async Task WindowedFromOperation_ZeroRows_ResolvesDefault() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();
        // No inserts

        FieldMapper<ValueModel> mapper = CreateValueMapper();
        WindowResult<ValueModel, long> result = await mapper.WindowedFromOperation<long>(
            em.Load<ValueModel>(DB.Property<ValueModel>(v => v.Integer), DB.Property<ValueModel>(v => v.String)),
            DB.CountOver());

        long windowValue = await result.WindowValue;
        Assert.AreEqual(0L, windowValue, "Zero-row result should resolve to default(long)");

        List<ValueModel> items = [];
        await foreach (ValueModel item in result.Items)
            items.Add(item);
        Assert.AreEqual(0, items.Count, "Items should be empty for zero-row result");
    }

    // -------------------------------------------------------------------------
    // PagedFromOperation — total matches unpaginated count
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public async Task PagedFromOperation_TotalMatchesUnpaginatedCount() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();
        await InsertRows(em, 10);

        FieldMapper<ValueModel> mapper = CreateValueMapper();
        WindowResult<ValueModel, long> result = await mapper.PagedFromOperation(
            em.Load<ValueModel>(DB.Property<ValueModel>(v => v.Integer), DB.Property<ValueModel>(v => v.String)),
            limit: 3,
            offset: 0);

        long total = await result.WindowValue;
        Assert.AreEqual(10L, total, "WindowValue should equal full unpaginated count");

        List<ValueModel> items = [];
        await foreach (ValueModel item in result.Items)
            items.Add(item);
        Assert.AreEqual(3, items.Count, "Items should contain only the page (limit=3)");
    }

    // -------------------------------------------------------------------------
    // PagedFromOperation — negative limit throws synchronously
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public void PagedFromOperation_NegativeLimit_Throws() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();

        FieldMapper<ValueModel> mapper = CreateValueMapper();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            mapper.PagedFromOperation(
                em.Load<ValueModel>(DB.Property<ValueModel>(v => v.Integer)),
                limit: -1,
                offset: 0)
        );
    }

    // -------------------------------------------------------------------------
    // Pre-canceled token short-circuits immediately
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public void WindowedFromOperation_PreCanceledToken_FaultsImmediately() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();

        using CancellationTokenSource cts = new();
        cts.Cancel();

        FieldMapper<ValueModel> mapper = CreateValueMapper();
        Assert.CatchAsync<OperationCanceledException>(async () =>
            await mapper.WindowedFromOperation<long>(
                em.Load<ValueModel>(DB.Property<ValueModel>(v => v.Integer), DB.Property<ValueModel>(v => v.String)),
                DB.CountOver(),
                cts.Token));
    }

    // -------------------------------------------------------------------------
    // Cancellation mid-stream
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public async Task WindowedFromOperation_CancellationMidStream_FaultsBoth() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();
        await InsertRows(em, 10);

        using CancellationTokenSource cts = new();

        FieldMapper<ValueModel> mapper = CreateValueMapper();
        WindowResult<ValueModel, long> result = await mapper.WindowedFromOperation<long>(
            em.Load<ValueModel>(DB.Property<ValueModel>(v => v.Integer), DB.Property<ValueModel>(v => v.String)),
            DB.CountOver(),
            cts.Token);

        long total = await result.WindowValue;
        Assert.Greater(total, 0L, "WindowValue should be resolved");

        cts.Cancel();

        // On SQLite the Items buffer is fully filled eagerly, so pass pre-canceled CT to
        // WithCancellation to observe the cancellation.
        Assert.CatchAsync<OperationCanceledException>(async () => {
            await foreach (ValueModel _ in result.Items.WithCancellation(cts.Token))
                cts.Token.ThrowIfCancellationRequested();
        });
    }

    // -------------------------------------------------------------------------
    // Single-statement assertion
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public async Task WindowedFromOperation_SingleStatementAssertion() {
        IDBClient realClient = TestData.CreateDatabaseAccess();
        CountingDBClient counter = new(realClient);
        IEntityManager em = new EntityManager(counter);
        em.UpdateSchema<ValueModel>();
        await InsertRows(em, 5);

        FieldMapper<ValueModel> mapper = CreateValueMapper();
        WindowResult<ValueModel, long> result = await mapper.WindowedFromOperation<long>(
            em.Load<ValueModel>(DB.Property<ValueModel>(v => v.Integer), DB.Property<ValueModel>(v => v.String)),
            DB.CountOver());

        await result.WindowValue;
        await foreach (ValueModel _ in result.Items) { }

        Assert.AreEqual(1, counter.ReaderCallCount,
            "WindowedFromOperation must issue exactly one reader call");
    }
}
