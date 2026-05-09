using System;
using System.Threading;
using System.Threading.Tasks;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Extern;

namespace Pooshit.Ocelot.Clients;

/// <summary>
/// internal <see cref="Reader"/> subclass that wraps an existing reader and intercepts the first
/// <c>ReadAsync</c> (and <c>Read</c>, defensively) to extract the windowed-aggregate column
/// ordinal and resolve the <see cref="WindowReader{TWindow}.WindowValue"/> task.
/// After the first read, all subsequent reads pass through unchanged to the inner reader.
/// </summary>
internal class WindowedReader<TWindow> : Reader {
    readonly TaskCompletionSource<TWindow> windowTcs;
    readonly string alias;
    readonly WindowReader<TWindow> carrier;
    bool firstReadDone;

    /// <summary>
    /// creates a new <see cref="WindowedReader{TWindow}"/>
    /// </summary>
    /// <param name="inner">the underlying reader that was opened by the database client</param>
    /// <param name="windowTcs">completion source to resolve with the windowed value after first read</param>
    /// <param name="alias">column alias used to locate the windowed aggregate ordinal on first read</param>
    /// <param name="carrier">the carrier whose <c>WindowOrdinal</c> is written back after first read</param>
    internal WindowedReader(
        Reader inner,
        TaskCompletionSource<TWindow> windowTcs,
        string alias,
        WindowReader<TWindow> carrier)
        : base(inner) {
        this.windowTcs = windowTcs;
        this.alias = alias;
        this.carrier = carrier;
    }

    /// <inheritdoc />
    public override Task<bool> ReadAsync() => ReadAsync(CancellationToken.None);

    /// <inheritdoc />
    public override Task<bool> ReadAsync(CancellationToken cancellationToken) {
        if (firstReadDone)
            return base.ReadAsync(cancellationToken);
        return InterceptFirstReadAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override bool Read() {
        if (firstReadDone)
            return base.Read();
        return InterceptFirstReadSync();
    }

    async Task<bool> InterceptFirstReadAsync(CancellationToken cancellationToken) {
        firstReadDone = true;
        bool hasRow;
        try {
            hasRow = await base.ReadAsync(cancellationToken);
        }
        catch (OperationCanceledException ex) {
            windowTcs.TrySetException(ex);
            throw;
        }
        catch (Exception ex) {
            windowTcs.TrySetException(ex);
            throw;
        }

        ResolveWindow(hasRow);
        return hasRow;
    }

    bool InterceptFirstReadSync() {
        firstReadDone = true;
        bool hasRow;
        try {
            hasRow = base.Read();
        }
        catch (Exception ex) {
            windowTcs.TrySetException(ex);
            throw;
        }

        ResolveWindow(hasRow);
        return hasRow;
    }

    void ResolveWindow(bool hasRow) {
        if (hasRow) {
            int ordinal = GetOrdinal(alias);
            carrier.WindowOrdinal = ordinal;
            object raw = GetValue(ordinal);
            TWindow value = Converter.Convert<TWindow>(raw, true);
            windowTcs.TrySetResult(value);
        }
        else {
            carrier.WindowOrdinal = -1;
            windowTcs.TrySetResult(default);
        }
    }
}
