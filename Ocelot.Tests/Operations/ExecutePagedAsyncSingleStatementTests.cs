using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Clients.Tables;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Models;
using DataTable = Pooshit.Ocelot.Clients.Tables.DataTable;

namespace Pooshit.Ocelot.Tests.Operations;

/// <summary>
/// wraps an IDBClient, counting every ReaderAsync / ReaderPreparedAsync call (the paths ExecutePagedAsync uses)
/// </summary>
class CountingDBClient : IDBClient {
    readonly IDBClient inner;
    int readerCallCount;

    public CountingDBClient(IDBClient inner) {
        this.inner = inner;
    }

    public int ReaderCallCount => readerCallCount;

    public IDBInfo DBInfo => inner.DBInfo;
    public IConnectionProvider Connection => inner.Connection;

    // Counting overrides for reader methods
    public Task<Reader> ReaderAsync(Transaction transaction, string command, IEnumerable<object> parameters, CancellationToken cancellationToken) {
        Interlocked.Increment(ref readerCallCount);
        return inner.ReaderAsync(transaction, command, parameters, cancellationToken);
    }

    public Task<Reader> ReaderPreparedAsync(Transaction transaction, string command, IEnumerable<object> parameters, CancellationToken cancellationToken) {
        Interlocked.Increment(ref readerCallCount);
        return inner.ReaderPreparedAsync(transaction, command, parameters, cancellationToken);
    }

    // Pass-throughs for all remaining interface members
    public int NonQuery(string commandstring, params object[] parameters) => inner.NonQuery(commandstring, parameters);
    public int NonQuery(string commandstring, IEnumerable<object> parameters) => inner.NonQuery(commandstring, parameters);
    public int NonQuery(Transaction transaction, string commandstring, params object[] parameters) => inner.NonQuery(transaction, commandstring, parameters);
    public int NonQuery(Transaction transaction, string commandstring, IEnumerable<object> parameters) => inner.NonQuery(transaction, commandstring, parameters);
    public DataTable Query(string query, params object[] parameters) => inner.Query(query, parameters);
    public DataTable Query(string query, IEnumerable<object> parameters) => inner.Query(query, parameters);
    public DataTable Query(Transaction transaction, string query, params object[] parameters) => inner.Query(transaction, query, parameters);
    public DataTable Query(Transaction transaction, string query, IEnumerable<object> parameters) => inner.Query(transaction, query, parameters);
    public object Scalar(string query, params object[] parameters) => inner.Scalar(query, parameters);
    public object Scalar(string query, IEnumerable<object> parameters) => inner.Scalar(query, parameters);
    public object Scalar(Transaction transaction, string query, params object[] parameters) => inner.Scalar(transaction, query, parameters);
    public object Scalar(Transaction transaction, string query, IEnumerable<object> parameters) => inner.Scalar(transaction, query, parameters);
    public IEnumerable<object> Set(string query, params object[] parameters) => inner.Set(query, parameters);
    public IEnumerable<object> Set(string query, IEnumerable<object> parameters) => inner.Set(query, parameters);
    public IEnumerable<object> Set(Transaction transaction, string query, params object[] parameters) => inner.Set(transaction, query, parameters);
    public IEnumerable<object> Set(Transaction transaction, string query, IEnumerable<object> parameters) => inner.Set(transaction, query, parameters);
    public Task<int> NonQueryAsync(string commandstring, params object[] parameters) => inner.NonQueryAsync(commandstring, parameters);
    public Task<int> NonQueryAsync(string commandstring, IEnumerable<object> parameters) => inner.NonQueryAsync(commandstring, parameters);
    public Task<int> NonQueryAsync(Transaction transaction, string commandstring, params object[] parameters) => inner.NonQueryAsync(transaction, commandstring, parameters);
    public Task<int> NonQueryAsync(Transaction transaction, string commandstring, IEnumerable<object> parameters) => inner.NonQueryAsync(transaction, commandstring, parameters);
    public Task<int> NonQueryAsync(Transaction transaction, string commandstring, IEnumerable<object> parameters, CancellationToken cancellationToken) => inner.NonQueryAsync(transaction, commandstring, parameters, cancellationToken);
    public Task<DataTable> QueryAsync(string query, params object[] parameters) => inner.QueryAsync(query, parameters);
    public Task<DataTable> QueryAsync(string query, IEnumerable<object> parameters) => inner.QueryAsync(query, parameters);
    public Task<DataTable> QueryAsync(Transaction transaction, string query, params object[] parameters) => inner.QueryAsync(transaction, query, parameters);
    public Task<DataTable> QueryAsync(Transaction transaction, string query, IEnumerable<object> parameters) => inner.QueryAsync(transaction, query, parameters);
    public Task<DataTable> QueryAsync(Transaction transaction, string query, IEnumerable<object> parameters, CancellationToken cancellationToken) => inner.QueryAsync(transaction, query, parameters, cancellationToken);
    public Task<object> ScalarAsync(string query, params object[] parameters) => inner.ScalarAsync(query, parameters);
    public Task<object> ScalarAsync(string query, IEnumerable<object> parameters) => inner.ScalarAsync(query, parameters);
    public Task<object> ScalarAsync(Transaction transaction, string query, params object[] parameters) => inner.ScalarAsync(transaction, query, parameters);
    public Task<object> ScalarAsync(Transaction transaction, string query, IEnumerable<object> parameters) => inner.ScalarAsync(transaction, query, parameters);
    public Task<object> ScalarAsync(Transaction transaction, string query, IEnumerable<object> parameters, CancellationToken cancellationToken) => inner.ScalarAsync(transaction, query, parameters, cancellationToken);
    public IAsyncEnumerable<object> SetAsync(string query, params object[] parameters) => inner.SetAsync(query, parameters);
    public IAsyncEnumerable<object> SetAsync(string query, IEnumerable<object> parameters) => inner.SetAsync(query, parameters);
    public IAsyncEnumerable<object> SetAsync(Transaction transaction, string query, params object[] parameters) => inner.SetAsync(transaction, query, parameters);
    public IAsyncEnumerable<object> SetAsync(Transaction transaction, string query, IEnumerable<object> parameters) => inner.SetAsync(transaction, query, parameters);
    public IAsyncEnumerable<object> SetAsync(Transaction transaction, string query, IEnumerable<object> parameters, CancellationToken cancellationToken) => inner.SetAsync(transaction, query, parameters, cancellationToken);
    public int NonQueryPrepared(string commandstring, params object[] parameters) => inner.NonQueryPrepared(commandstring, parameters);
    public int NonQueryPrepared(string commandstring, IEnumerable<object> parameters) => inner.NonQueryPrepared(commandstring, parameters);
    public int NonQueryPrepared(Transaction transaction, string commandstring, params object[] parameters) => inner.NonQueryPrepared(transaction, commandstring, parameters);
    public int NonQueryPrepared(Transaction transaction, string commandstring, IEnumerable<object> parameters) => inner.NonQueryPrepared(transaction, commandstring, parameters);
    public DataTable QueryPrepared(string query, params object[] parameters) => inner.QueryPrepared(query, parameters);
    public DataTable QueryPrepared(string query, IEnumerable<object> parameters) => inner.QueryPrepared(query, parameters);
    public DataTable QueryPrepared(Transaction transaction, string query, params object[] parameters) => inner.QueryPrepared(transaction, query, parameters);
    public DataTable QueryPrepared(Transaction transaction, string query, IEnumerable<object> parameters) => inner.QueryPrepared(transaction, query, parameters);
    public object ScalarPrepared(string query, params object[] parameters) => inner.ScalarPrepared(query, parameters);
    public object ScalarPrepared(string query, IEnumerable<object> parameters) => inner.ScalarPrepared(query, parameters);
    public object ScalarPrepared(Transaction transaction, string query, params object[] parameters) => inner.ScalarPrepared(transaction, query, parameters);
    public object ScalarPrepared(Transaction transaction, string query, IEnumerable<object> parameters) => inner.ScalarPrepared(transaction, query, parameters);
    public IEnumerable<object> SetPrepared(string query, params object[] parameters) => inner.SetPrepared(query, parameters);
    public IEnumerable<object> SetPrepared(string query, IEnumerable<object> parameters) => inner.SetPrepared(query, parameters);
    public IEnumerable<object> SetPrepared(Transaction transaction, string query, params object[] parameters) => inner.SetPrepared(transaction, query, parameters);
    public IEnumerable<object> SetPrepared(Transaction transaction, string query, IEnumerable<object> parameters) => inner.SetPrepared(transaction, query, parameters);
    public Task<int> NonQueryPreparedAsync(string commandstring, params object[] parameters) => inner.NonQueryPreparedAsync(commandstring, parameters);
    public Task<int> NonQueryPreparedAsync(string commandstring, IEnumerable<object> parameters) => inner.NonQueryPreparedAsync(commandstring, parameters);
    public Task<int> NonQueryPreparedAsync(Transaction transaction, string commandstring, params object[] parameters) => inner.NonQueryPreparedAsync(transaction, commandstring, parameters);
    public Task<int> NonQueryPreparedAsync(Transaction transaction, string commandstring, IEnumerable<object> parameters) => inner.NonQueryPreparedAsync(transaction, commandstring, parameters);
    public Task<int> NonQueryPreparedAsync(Transaction transaction, string commandstring, IEnumerable<object> parameters, CancellationToken cancellationToken) => inner.NonQueryPreparedAsync(transaction, commandstring, parameters, cancellationToken);
    public Task<DataTable> QueryPreparedAsync(string query, params object[] parameters) => inner.QueryPreparedAsync(query, parameters);
    public Task<DataTable> QueryPreparedAsync(string query, IEnumerable<object> parameters) => inner.QueryPreparedAsync(query, parameters);
    public Task<DataTable> QueryPreparedAsync(Transaction transaction, string query, params object[] parameters) => inner.QueryPreparedAsync(transaction, query, parameters);
    public Task<DataTable> QueryPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters) => inner.QueryPreparedAsync(transaction, query, parameters);
    public Task<DataTable> QueryPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters, CancellationToken cancellationToken) => inner.QueryPreparedAsync(transaction, query, parameters, cancellationToken);
    public Task<object> ScalarPreparedAsync(string query, params object[] parameters) => inner.ScalarPreparedAsync(query, parameters);
    public Task<object> ScalarPreparedAsync(string query, IEnumerable<object> parameters) => inner.ScalarPreparedAsync(query, parameters);
    public Task<object> ScalarPreparedAsync(Transaction transaction, string query, params object[] parameters) => inner.ScalarPreparedAsync(transaction, query, parameters);
    public Task<object> ScalarPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters) => inner.ScalarPreparedAsync(transaction, query, parameters);
    public Task<object> ScalarPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters, CancellationToken cancellationToken) => inner.ScalarPreparedAsync(transaction, query, parameters, cancellationToken);
    public IAsyncEnumerable<object> SetPreparedAsync(string query, params object[] parameters) => inner.SetPreparedAsync(query, parameters);
    public IAsyncEnumerable<object> SetPreparedAsync(string query, IEnumerable<object> parameters) => inner.SetPreparedAsync(query, parameters);
    public IAsyncEnumerable<object> SetPreparedAsync(Transaction transaction, string query, params object[] parameters) => inner.SetPreparedAsync(transaction, query, parameters);
    public IAsyncEnumerable<object> SetPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters) => inner.SetPreparedAsync(transaction, query, parameters);
    public IAsyncEnumerable<object> SetPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters, CancellationToken cancellationToken) => inner.SetPreparedAsync(transaction, query, parameters, cancellationToken);
    public Reader Reader(Transaction transaction, string command, IEnumerable<object> parameters) => inner.Reader(transaction, command, parameters);
    public Reader Reader(Transaction transaction, string command, params object[] parameters) => inner.Reader(transaction, command, parameters);
    public Reader Reader(string command, IEnumerable<object> parameters) => inner.Reader(command, parameters);
    public Reader Reader(string command, params object[] parameters) => inner.Reader(command, parameters);
    public Task<Reader> ReaderAsync(Transaction transaction, string command, IEnumerable<object> parameters) => inner.ReaderAsync(transaction, command, parameters);
    public Task<Reader> ReaderAsync(Transaction transaction, string command, params object[] parameters) => inner.ReaderAsync(transaction, command, parameters);
    public Task<Reader> ReaderAsync(string command, IEnumerable<object> parameters) => inner.ReaderAsync(command, parameters);
    public Task<Reader> ReaderAsync(string command, params object[] parameters) => inner.ReaderAsync(command, parameters);
    public Reader ReaderPrepared(Transaction transaction, string command, IEnumerable<object> parameters) => inner.ReaderPrepared(transaction, command, parameters);
    public Reader ReaderPrepared(Transaction transaction, string command, params object[] parameters) => inner.ReaderPrepared(transaction, command, parameters);
    public Reader ReaderPrepared(string command, IEnumerable<object> parameters) => inner.ReaderPrepared(command, parameters);
    public Reader ReaderPrepared(string command, params object[] parameters) => inner.ReaderPrepared(command, parameters);
    public Task<Reader> ReaderPreparedAsync(Transaction transaction, string command, IEnumerable<object> parameters) => inner.ReaderPreparedAsync(transaction, command, parameters);
    public Task<Reader> ReaderPreparedAsync(Transaction transaction, string command, params object[] parameters) => inner.ReaderPreparedAsync(transaction, command, parameters);
    public Task<Reader> ReaderPreparedAsync(string command, IEnumerable<object> parameters) => inner.ReaderPreparedAsync(command, parameters);
    public Task<Reader> ReaderPreparedAsync(string command, params object[] parameters) => inner.ReaderPreparedAsync(command, parameters);
    public Transaction Transaction() => inner.Transaction();
}

[TestFixture, Parallelizable]
public class ExecutePagedAsyncSingleStatementTests {

    static (IEntityManager em, CountingDBClient counter) CreateEntityManagerWithCounter() {
        IDBClient realClient = TestData.CreateDatabaseAccess();
        CountingDBClient counter = new(realClient);
        IEntityManager em = new EntityManager(counter);
        return (em, counter);
    }

    static async Task InsertRows(IEntityManager em, int count) {
        for (int i = 0; i < count; i++)
            await em.Insert<ValueModel>().Columns(v => v.Integer).ExecuteAsync(i);
    }

    [Test, Parallelizable]
    public async Task ExecutePagedAsync_WithRows_ExactlyOneReaderCall() {
        (IEntityManager em, CountingDBClient counter) = CreateEntityManagerWithCounter();
        em.UpdateSchema<ValueModel>();
        await InsertRows(em, 10);

        PagedResult<ValueModel> result = await em.Load<ValueModel>().ExecutePagedAsync(5, 0);

        // Drain items
        await foreach (ValueModel _ in result.Items) { }

        Assert.AreEqual(1, counter.ReaderCallCount,
            "ExecutePagedAsync must issue exactly one reader call regardless of page size");
    }

    [Test, Parallelizable]
    public async Task ExecutePagedAsync_ZeroRows_ExactlyOneReaderCall() {
        (IEntityManager em, CountingDBClient counter) = CreateEntityManagerWithCounter();
        em.UpdateSchema<ValueModel>();

        // No inserts — empty table
        PagedResult<ValueModel> result = await em.Load<ValueModel>().ExecutePagedAsync(10, 0);

        await foreach (ValueModel _ in result.Items) { }

        Assert.AreEqual(1, counter.ReaderCallCount,
            "ExecutePagedAsync must issue exactly one reader call even for an empty result set");
    }

    [Test, Parallelizable]
    public async Task ExecutePagedAsync_WithPredicate_ExactlyOneReaderCall() {
        (IEntityManager em, CountingDBClient counter) = CreateEntityManagerWithCounter();
        em.UpdateSchema<ValueModel>();
        await InsertRows(em, 20);

        PagedResult<ValueModel> result = await em.Load<ValueModel>()
            .Where(v => v.Integer < 5)
            .ExecutePagedAsync(10, 0);

        await foreach (ValueModel _ in result.Items) { }

        Assert.AreEqual(1, counter.ReaderCallCount,
            "ExecutePagedAsync with a WHERE clause must still issue exactly one reader call");
    }

    [Test, Parallelizable]
    public async Task ExecutePagedAsync_DoesNotIssueReaderCallForCountQuery() {
        // Verify that Total is resolved from the single reader call, not a separate count query
        (IEntityManager em, CountingDBClient counter) = CreateEntityManagerWithCounter();
        em.UpdateSchema<ValueModel>();
        await InsertRows(em, 7);

        PagedResult<ValueModel> result = await em.Load<ValueModel>().ExecutePagedAsync(5, 0);

        long total = await result.Total;
        await foreach (ValueModel _ in result.Items) { }

        Assert.AreEqual(7L, total, "Total must reflect full table count");
        Assert.AreEqual(1, counter.ReaderCallCount,
            "Total resolution must not trigger a second reader call");
    }

    [Test, Parallelizable]
    public async Task ExecutePagedAsync_MultipleCallsOnSameOperation_EachCallIsOneStatement() {
        (IEntityManager em, CountingDBClient counter) = CreateEntityManagerWithCounter();
        em.UpdateSchema<ValueModel>();
        await InsertRows(em, 5);

        // First call
        PagedResult<ValueModel> result1 = await em.Load<ValueModel>().ExecutePagedAsync(5, 0);
        await foreach (ValueModel _ in result1.Items) { }

        int afterFirst = counter.ReaderCallCount;
        Assert.AreEqual(1, afterFirst, "First ExecutePagedAsync call: exactly one reader");

        // Second independent call via a fresh fluent chain
        PagedResult<ValueModel> result2 = await em.Load<ValueModel>().ExecutePagedAsync(5, 0);
        await foreach (ValueModel _ in result2.Items) { }

        Assert.AreEqual(2, counter.ReaderCallCount, "Two ExecutePagedAsync calls: exactly two readers total");
    }
}
