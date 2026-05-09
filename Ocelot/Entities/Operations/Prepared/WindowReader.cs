using System.Threading.Tasks;
using Pooshit.Ocelot.Clients;

namespace Pooshit.Ocelot.Entities.Operations.Prepared;

/// <summary>
/// carrier returned by <see cref="PreparedLoadOperation{T}.ExecuteWindowedReaderAsync{TWindow}(Pooshit.Ocelot.Tokens.Partitions.WindowedAggregate,System.Threading.CancellationToken)"/>
/// exposing the open reader positioned at the start of the result set (before any read), the
/// column ordinal of the injected windowed aggregate, and a task that resolves to the aggregate
/// value once the consumer has read at least the first row.
/// </summary>
/// <remarks>
/// The caller owns the reader and is responsible for disposing it.
/// <see cref="WindowValue"/> resolves to <c>default(<typeparamref name="TWindow"/>)</c> for a
/// zero-row result set and is resolved by the <see cref="WindowedReader{TWindow}"/> proxy
/// intercepting the first <c>ReadAsync</c> call on <see cref="Reader"/>.
/// </remarks>
/// <typeparam name="TWindow">type of the windowed aggregate value</typeparam>
public class WindowReader<TWindow> {
    /// <summary>
    /// creates a new <see cref="WindowReader{TWindow}"/>
    /// </summary>
    /// <param name="windowValue">task that resolves when the consumer reads the first row</param>
    internal WindowReader(Task<TWindow> windowValue) {
        WindowValue = windowValue;
        WindowOrdinal = -1;
    }

    /// <summary>
    /// the open data reader, positioned at the start of the result set (no read has been performed).
    /// Iterate with <c>await reader.ReadAsync()</c>. The caller owns disposal.
    /// </summary>
    public Reader Reader { get; internal set; }

    /// <summary>
    /// resolves to the windowed-aggregate value once the consumer has read at least one row via
    /// <see cref="Reader"/>. Resolves to <c>default(<typeparamref name="TWindow"/>)</c> for a
    /// zero-row result.
    /// </summary>
    public Task<TWindow> WindowValue { get; }

    /// <summary>
    /// the column ordinal of the windowed aggregate within the result set. <c>-1</c> until the
    /// consumer has performed the first <c>ReadAsync</c>. Read this after the first
    /// <c>ReadAsync</c> returns to know which ordinal to skip during entity materialization.
    /// </summary>
    public int WindowOrdinal { get; internal set; }
}
