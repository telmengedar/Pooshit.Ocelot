using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pooshit.Ocelot.Entities.Operations.Prepared;

/// <summary>
/// result of a paged load operation, containing both the page items and the total row count
/// </summary>
/// <remarks>
/// Obtain instances via <see cref="PreparedLoadOperation{T}.ExecutePagedAsync(int,int,System.Threading.CancellationToken)"/>.
/// Do not iterate <see cref="Items"/> on SQLite without consuming it fully unless you no longer need the underlying
/// connection — the semaphore is held until the iterator is disposed.
/// </remarks>
/// <typeparam name="T">entity type of page items</typeparam>
public class PagedResult<T> {
    /// <summary>
    /// creates a new <see cref="PagedResult{T}"/>
    /// </summary>
    /// <param name="items">streamed page items</param>
    /// <param name="total">task that resolves to the total matching row count</param>
    internal PagedResult(IAsyncEnumerable<T> items, Task<long> total) {
        Items = items;
        Total = total;
    }

    /// <summary>
    /// the streamed page items; iterate with <c>await foreach</c>
    /// </summary>
    /// <remarks>
    /// On SQLite (single-connection), all items are buffered in memory during <see cref="PreparedLoadOperation{T}.ExecutePagedAsync"/>
    /// before this property is accessible. On multi-connection databases (Postgres, MySQL, MSSQL) items are streamed live.
    /// </remarks>
    public IAsyncEnumerable<T> Items { get; }

    /// <summary>
    /// the total number of rows matching the query WHERE clause (before LIMIT/OFFSET)
    /// </summary>
    /// <remarks>
    /// Resolved from the <c>COUNT(*) OVER ()</c> windowed aggregate — no second SQL round trip.
    /// For a zero-row result set, resolves to 0.
    /// If the stream is canceled or the connection drops before the first row is read,
    /// this task faults with the same exception as <see cref="Items"/>.
    /// The alias <c>__total</c> is reserved by this method; entity properties with that name
    /// will be overwritten with the windowed count value.
    /// </remarks>
    public Task<long> Total { get; }
}
