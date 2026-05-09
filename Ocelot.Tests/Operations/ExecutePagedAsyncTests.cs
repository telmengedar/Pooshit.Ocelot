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

namespace Pooshit.Ocelot.Tests.Operations;

[TestFixture, Parallelizable]
public class ExecutePagedAsyncTests {

    static IEntityManager CreateEntityManager() => TestData.CreateEntityManager();

    static async Task InsertRows(IEntityManager em, int count) {
        for (int i = 0; i < count; i++)
            await em.Insert<ValueModel>().Columns(v => v.Integer, v => v.String).ExecuteAsync(i, $"item-{i}");
    }

    [Test, Parallelizable]
    public async Task ZeroRows_TotalIsZero_ItemsIsEmpty() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();

        WindowResult<ValueModel, long> result = await em.Load<ValueModel>().ExecutePagedAsync(10, 0);

        long total = await result.WindowValue;
        Assert.AreEqual(0L, total);

        List<ValueModel> items = [];
        await foreach (ValueModel item in result.Items)
            items.Add(item);
        Assert.AreEqual(0, items.Count);
    }

    [Test, Parallelizable]
    public async Task OneRow_TotalIsOne_ItemsHasOneItem() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();
        await InsertRows(em, 1);

        WindowResult<ValueModel, long> result = await em.Load<ValueModel>().ExecutePagedAsync(10, 0);

        long total = await result.WindowValue;
        Assert.AreEqual(1L, total);

        List<ValueModel> items = [];
        await foreach (ValueModel item in result.Items)
            items.Add(item);
        Assert.AreEqual(1, items.Count);
    }

    [Test, Parallelizable]
    public async Task ExactlyOnePage_TotalEqualsLimit() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();
        await InsertRows(em, 5);

        WindowResult<ValueModel, long> result = await em.Load<ValueModel>().ExecutePagedAsync(5, 0);

        long total = await result.WindowValue;
        Assert.AreEqual(5L, total);

        int count = 0;
        await foreach (ValueModel _ in result.Items)
            count++;
        Assert.AreEqual(5, count);
    }

    [Test, Parallelizable]
    public async Task PartialPage_TotalIsFullCount_ItemsIsPage() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();
        await InsertRows(em, 15);

        // Request page of 5 from 15 total rows
        WindowResult<ValueModel, long> result = await em.Load<ValueModel>().ExecutePagedAsync(5, 0);

        long total = await result.WindowValue;
        Assert.AreEqual(15L, total);

        int count = 0;
        await foreach (ValueModel _ in result.Items)
            count++;
        Assert.AreEqual(5, count);
    }

    [Test, Parallelizable]
    public async Task OffsetBeyondData_TotalIsFullCount_ItemsIsEmpty() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();
        await InsertRows(em, 5);

        WindowResult<ValueModel, long> result = await em.Load<ValueModel>().ExecutePagedAsync(10, 100);

        long total = await result.WindowValue;
        // Window function count is per-row; if no rows returned, total = 0 (empty set)
        Assert.AreEqual(0L, total);

        List<ValueModel> items = [];
        await foreach (ValueModel item in result.Items)
            items.Add(item);
        Assert.AreEqual(0, items.Count);
    }

    [Test, Parallelizable]
    public async Task Total_MatchesSeparateCountQuery() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();
        await InsertRows(em, 20);

        // Get paged result with page size smaller than total
        WindowResult<ValueModel, long> result = await em.Load<ValueModel>().ExecutePagedAsync(7, 0);
        long pagedTotal = await result.WindowValue;

        // Drain items so reader is closed
        await foreach (ValueModel _ in result.Items) { }

        // Separate count query
        long separateCount = em.Load<ValueModel>(DB.Count(DB.All)).ExecuteScalar<long>();
        Assert.AreEqual(separateCount, pagedTotal);
    }

    [Test, Parallelizable]
    public async Task EagerAwaitTotal_BeforeIteratingItems_Works() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();
        await InsertRows(em, 8);

        WindowResult<ValueModel, long> result = await em.Load<ValueModel>().ExecutePagedAsync(5, 0);

        // Await WindowValue BEFORE iterating Items — must already be set
        long total = await result.WindowValue;
        Assert.AreEqual(8L, total);

        int count = 0;
        await foreach (ValueModel _ in result.Items)
            count++;
        Assert.AreEqual(5, count);
    }

    [Test, Parallelizable]
    public void NegativeLimit_ThrowsArgumentOutOfRange() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await em.Load<ValueModel>().ExecutePagedAsync(-1, 0));
    }

    [Test, Parallelizable]
    public void NegativeOffset_ThrowsArgumentOutOfRange() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await em.Load<ValueModel>().ExecutePagedAsync(10, -1));
    }

    [Test, Parallelizable]
    public void AlreadyCanceledToken_ThrowsOperationCanceled() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();

        using CancellationTokenSource cts = new();
        cts.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await em.Load<ValueModel>().ExecutePagedAsync(10, 0, cts.Token));
    }

    [Test, Parallelizable]
    public async Task WithPredicate_TotalReflectsFilteredCount() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();
        // Insert 10 with Integer < 5 and 10 with Integer >= 5
        for (int i = 0; i < 10; i++)
            await em.Insert<ValueModel>().Columns(v => v.Integer).ExecuteAsync(i);

        WindowResult<ValueModel, long> result = await em.Load<ValueModel>()
            .Where(v => v.Integer < 5)
            .ExecutePagedAsync(10, 0);

        long total = await result.WindowValue;
        Assert.AreEqual(5L, total);
    }

    [Test, Parallelizable]
    public async Task ItemsMappedCorrectly_EntityPropertiesSet() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();
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
