using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Tests.Models;

namespace Pooshit.Ocelot.Tests.Clients;

/// <summary>
/// Tests that the SQLite semaphore inside LockedDBClient is correctly released when a
/// CancellationToken fires during WaitAsync, and that subsequent operations succeed.
/// </summary>
[TestFixture, Parallelizable]
public class LockedDBClientCancellationTests {

    /// <summary>
    /// Creates a fresh in-memory SQLite client (LockedDBClient) backed by a persistent
    /// connection so multiple operations on the same EntityManager share the semaphore.
    /// </summary>
    static (IDBClient client, IEntityManager em) CreateShared() {
        string dbName = Guid.NewGuid().ToString("N");
        SqliteConnection connection = new($"Data Source={dbName};Mode=Memory;Cache=Shared");
        connection.Open();
        IDBClient client = ClientFactory.Create(connection, new SQLiteInfo());
        IEntityManager em = new EntityManager(client);
        return (client, em);
    }

    // -------------------------------------------------------------------------
    // Basic: pre-canceled CT on LockedDBClient methods.
    // Use Assert.CatchAsync (catches subclasses) — TaskCanceledException is a
    // subclass of OperationCanceledException and both are acceptable.
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public void LockedClient_QueryAsync_PreCanceledToken_ThrowsOperationCanceled() {
        (IDBClient client, _) = CreateShared();
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await client.QueryAsync(null, "SELECT 1", Array.Empty<object>(), cts.Token));
    }

    [Test, Parallelizable]
    public void LockedClient_ScalarAsync_PreCanceledToken_ThrowsOperationCanceled() {
        (IDBClient client, _) = CreateShared();
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await client.ScalarAsync(null, "SELECT 1", Array.Empty<object>(), cts.Token));
    }

    [Test, Parallelizable]
    public void LockedClient_ReaderAsync_PreCanceledToken_ThrowsOperationCanceled() {
        (IDBClient client, _) = CreateShared();
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await client.ReaderAsync(null, "SELECT 1", Array.Empty<object>(), cts.Token));
    }

    // -------------------------------------------------------------------------
    // Core: semaphore is released after cancel-during-wait so a third op succeeds
    // -------------------------------------------------------------------------

    [Test]
    public async Task SemaphoreReleasedOnCancelDuringWait_ThirdOperationSucceeds() {
        string dbName = Guid.NewGuid().ToString("N");
        SqliteConnection connection = new($"Data Source={dbName};Mode=Memory;Cache=Shared");
        connection.Open();
        IDBClient client = ClientFactory.Create(connection, new SQLiteInfo());
        IEntityManager em = new EntityManager(client);

        em.UpdateSchema<ValueModel>();
        await em.Insert<ValueModel>().Columns(v => v.Integer).ExecuteAsync(1);

        // Acquire the semaphore via a held reader — this locks the single connection
        Reader heldReader = client.Reader(null, "SELECT integer FROM valuemodel", Array.Empty<object>());

        // Second operation blocks on WaitAsync — cancel it while waiting
        using CancellationTokenSource cts = new();
        Task<Reader> blockedTask = client.ReaderAsync(null, "SELECT 1", Array.Empty<object>(), cts.Token);

        await Task.Delay(50);
        cts.Cancel();

        OperationCanceledException caught = null;
        try {
            await blockedTask;
        }
        catch (OperationCanceledException ex) {
            caught = ex;
        }

        Assert.IsNotNull(caught, "Blocked ReaderAsync must throw OperationCanceledException when CT fires");

        // Release the semaphore by disposing the held reader
        heldReader.Dispose();

        // Third operation must succeed
        object scalar = await client.ScalarAsync(null, "SELECT 42", Array.Empty<object>(), CancellationToken.None);
        Assert.IsNotNull(scalar, "Third operation must succeed after cancel-during-wait releases the semaphore");
    }

    // -------------------------------------------------------------------------
    // ExecutePagedAsync: semaphore correctly released after cancellation
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public async Task ExecutePagedAsync_AfterCancellation_SemaphoreReleasedForNextCall() {
        (IDBClient client, IEntityManager em) = CreateShared();
        em.UpdateSchema<ValueModel>();

        for (int i = 0; i < 5; i++)
            await em.Insert<ValueModel>().Columns(v => v.Integer).ExecuteAsync(i);

        using CancellationTokenSource cts = new();
        cts.Cancel();

        try {
            await em.Load<ValueModel>().ExecutePagedAsync(10, 0, cts.Token);
        }
        catch (OperationCanceledException) {
            // expected
        }

        // Semaphore must be free — normal call must complete without deadlock
        WindowResult<ValueModel, long> result = await em.Load<ValueModel>().ExecutePagedAsync(10, 0);
        long total = await result.WindowValue;
        Assert.AreEqual(5L, total, "Operation after canceled ExecutePagedAsync must succeed");
    }

    // -------------------------------------------------------------------------
    // CT propagates through WaitAsync — not just to inner call
    // -------------------------------------------------------------------------

    [Test]
    public async Task ReaderAsync_CancelWhileWaiting_SemaphoreStillReleasable() {
        string dbName = Guid.NewGuid().ToString("N");
        SqliteConnection connection = new($"Data Source={dbName};Mode=Memory;Cache=Shared");
        connection.Open();
        IDBClient client = ClientFactory.Create(connection, new SQLiteInfo());

        // Hold semaphore
        Reader heldReader = client.Reader(null, "SELECT 1", Array.Empty<object>());

        using CancellationTokenSource cts = new();
        Task<Reader> waiting = client.ReaderAsync(null, "SELECT 1", Array.Empty<object>(), cts.Token);
        await Task.Delay(30);
        cts.Cancel();

        try { await waiting; }
        catch (OperationCanceledException) { }

        heldReader.Dispose();

        // Must not deadlock
        Reader r = await client.ReaderAsync(null, "SELECT 1", Array.Empty<object>(), CancellationToken.None);
        r.Dispose();
        Assert.Pass("Third reader acquired successfully after cancel-during-wait");
    }
}
