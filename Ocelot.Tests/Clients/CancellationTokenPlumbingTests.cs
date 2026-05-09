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

namespace Pooshit.Ocelot.Tests.Clients;

[TestFixture, Parallelizable]
public class CancellationTokenPlumbingTests {

    static IDBClient CreateClient() => TestData.CreateDatabaseAccess();

    static IEntityManager CreateEntityManager() => TestData.CreateEntityManager();

    static async Task InsertRows(IEntityManager em, int count) {
        for (int i = 0; i < count; i++)
            await em.Insert<ValueModel>().Columns(v => v.Integer).ExecuteAsync(i);
    }

    // -------------------------------------------------------------------------
    // Pre-canceled token tests: verify OperationCanceledException is NOT wrapped
    // in StatementException. TaskCanceledException is a subclass of
    // OperationCanceledException — both are acceptable; neither must become
    // a StatementException. Use Assert.CatchAsync (catches subclasses) rather
    // than Assert.ThrowsAsync (exact type only).
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public void ReaderAsync_PreCanceledToken_ThrowsOperationCanceled_NotStatementException() {
        IDBClient client = CreateClient();
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await client.ReaderAsync(null, "SELECT 1", Array.Empty<object>(), cts.Token));
    }

    [Test, Parallelizable]
    public void ReaderPreparedAsync_PreCanceledToken_ThrowsOperationCanceled_NotStatementException() {
        IDBClient client = CreateClient();
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await client.ReaderPreparedAsync(null, "SELECT 1", Array.Empty<object>(), cts.Token));
    }

    [Test, Parallelizable]
    public void QueryAsync_PreCanceledToken_ThrowsOperationCanceled_NotStatementException() {
        IDBClient client = CreateClient();
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await client.QueryAsync(null, "SELECT 1", Array.Empty<object>(), cts.Token));
    }

    [Test, Parallelizable]
    public void ScalarAsync_PreCanceledToken_ThrowsOperationCanceled_NotStatementException() {
        IDBClient client = CreateClient();
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await client.ScalarAsync(null, "SELECT 1", Array.Empty<object>(), cts.Token));
    }

    [Test, Parallelizable]
    public void NonQueryAsync_PreCanceledToken_ThrowsOperationCanceled_NotStatementException() {
        IDBClient client = CreateClient();
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await client.NonQueryAsync(null, "SELECT 1", Array.Empty<object>(), cts.Token));
    }

    // -------------------------------------------------------------------------
    // CT default === no cancellation: CT-bearing overloads return same data
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public async Task QueryAsync_DefaultCT_ReturnsCorrectData() {
        IDBClient client = CreateClient();
        var result = await client.QueryAsync(null, "SELECT 42", Array.Empty<object>(), CancellationToken.None);
        Assert.IsNotNull(result);
        Assert.Greater(result.Rows.Length, 0);
    }

    [Test, Parallelizable]
    public async Task ScalarAsync_DefaultCT_ReturnsCorrectData() {
        IDBClient client = CreateClient();
        object result = await client.ScalarAsync(null, "SELECT 123", Array.Empty<object>(), CancellationToken.None);
        Assert.IsNotNull(result);
    }

    // -------------------------------------------------------------------------
    // ExecutePagedAsync: pre-canceled token surfaces via Task<PagedResult<T>>
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public void ExecutePagedAsync_PreCanceledToken_ThrowsOperationCanceled() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();

        using CancellationTokenSource cts = new();
        cts.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await em.Load<ValueModel>().ExecutePagedAsync(10, 0, cts.Token));
    }

    [Test, Parallelizable]
    public async Task ExecutePagedAsync_PreCanceledToken_Total_AlsoFaults() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();

        using CancellationTokenSource cts = new();
        cts.Cancel();

        OperationCanceledException caught = null;
        try {
            await em.Load<ValueModel>().ExecutePagedAsync(10, 0, cts.Token);
        }
        catch (OperationCanceledException ex) {
            caught = ex;
        }

        Assert.IsNotNull(caught, "Expected OperationCanceledException from ExecutePagedAsync");
    }

    // -------------------------------------------------------------------------
    // CT-bearing overloads with default CT produce same results as CT-less ones
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public async Task ExecutePagedAsync_DefaultCT_ProducesSameResultsAsWithoutCT() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();
        await InsertRows(em, 8);

        WindowResult<ValueModel, long> result = await em.Load<ValueModel>().ExecutePagedAsync(5, 0, CancellationToken.None);

        long total = await result.WindowValue;
        Assert.AreEqual(8L, total);

        int count = 0;
        await foreach (ValueModel _ in result.Items)
            count++;
        Assert.AreEqual(5, count);
    }

    // -------------------------------------------------------------------------
    // PreparedAsync CT overloads: verify wiring through ADbClient default delegation
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public void NonQueryPreparedAsync_PreCanceledToken_ThrowsOperationCanceled() {
        IDBClient client = CreateClient();
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await client.NonQueryPreparedAsync(null, "SELECT 1", Array.Empty<object>(), cts.Token));
    }

    [Test, Parallelizable]
    public void QueryPreparedAsync_PreCanceledToken_ThrowsOperationCanceled() {
        IDBClient client = CreateClient();
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await client.QueryPreparedAsync(null, "SELECT 1", Array.Empty<object>(), cts.Token));
    }

    [Test, Parallelizable]
    public void ScalarPreparedAsync_PreCanceledToken_ThrowsOperationCanceled() {
        IDBClient client = CreateClient();
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await client.ScalarPreparedAsync(null, "SELECT 1", Array.Empty<object>(), cts.Token));
    }
}
