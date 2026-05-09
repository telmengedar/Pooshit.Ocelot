using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pooshit.Ocelot.Entities.Operations.Prepared;

/// <summary>
/// result of a windowed load operation, containing both the streamed items and a windowed aggregate value
/// resolved from the first row without a second SQL round trip
/// </summary>
/// <remarks>
/// Obtain instances via <see cref="PreparedLoadOperation{T}.ExecuteWindowedAsync{TWindow}(Pooshit.Ocelot.Tokens.Partitions.WindowedAggregate,System.Threading.CancellationToken)"/>
/// or <see cref="PreparedLoadOperation{T}.ExecutePagedAsync(int,int,System.Threading.CancellationToken)"/>.
/// On SQLite (single-connection), all items are buffered in memory before this result is observable.
/// On multi-connection databases (Postgres, MySQL, MSSQL) items are streamed live.
/// The reserved alias <c>__window</c> is used for the windowed aggregate column when
/// <c>WindowedAggregate.Alias</c> is not specified. Avoid entity property names that collide.
/// </remarks>
/// <typeparam name="TItem">entity type of items</typeparam>
/// <typeparam name="TWindow">type of the windowed aggregate value</typeparam>
public class WindowResult<TItem, TWindow> {
    /// <summary>
    /// creates a new <see cref="WindowResult{TItem,TWindow}"/>
    /// </summary>
    /// <param name="items">streamed items</param>
    /// <param name="windowValue">task that resolves to the windowed aggregate value</param>
    internal WindowResult(IAsyncEnumerable<TItem> items, Task<TWindow> windowValue) {
        Items = items;
        WindowValue = windowValue;
    }

    /// <summary>
    /// the streamed items; iterate with <c>await foreach</c>
    /// </summary>
    /// <remarks>
    /// On SQLite (single-connection), all items are buffered in memory during
    /// <see cref="PreparedLoadOperation{T}.ExecuteWindowedAsync{TWindow}(Pooshit.Ocelot.Tokens.Partitions.WindowedAggregate,System.Threading.CancellationToken)"/>
    /// before this property is accessible. On multi-connection databases (Postgres, MySQL, MSSQL) items are streamed live.
    /// </remarks>
    public IAsyncEnumerable<TItem> Items { get; }

    /// <summary>
    /// the windowed aggregate value recovered from the first row's reserved alias column
    /// </summary>
    /// <remarks>
    /// Resolved from the windowed aggregate column injected into the projection — no second SQL round trip.
    /// For a zero-row result set, resolves to <c>default(<typeparamref name="TWindow"/>)</c>.
    /// If the stream is canceled or the connection drops before the first row is read,
    /// this task faults with the same exception as <see cref="Items"/>.
    /// The alias <c>__window</c> is reserved by <see cref="PreparedLoadOperation{T}.ExecuteWindowedAsync{TWindow}(Pooshit.Ocelot.Tokens.Partitions.WindowedAggregate,System.Threading.CancellationToken)"/>
    /// when no explicit alias is supplied; entity properties with that name will be overwritten with the aggregate value.
    /// </remarks>
    public Task<TWindow> WindowValue { get; }
}
