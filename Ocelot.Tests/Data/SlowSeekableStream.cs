using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Pooshit.Ocelot.Tests.Data;

/// <summary>
/// Seekable zero-byte source with delayed asynchronous reads.
/// </summary>
public sealed class SlowSeekableStream : Stream {
    readonly long length;
    readonly TaskCompletionSource<bool> firstRead = new(TaskCreationOptions.RunContinuationsAsynchronously);
    long position;
    bool disposed;

    /// <summary>
    /// Creates a delayed source.
    /// </summary>
    /// <param name="length">Total source length.</param>
    public SlowSeekableStream(long length) => this.length = length;

    /// <summary>
    /// Completion of the first asynchronous read.
    /// </summary>
    public Task FirstRead => firstRead.Task;

    /// <inheritdoc />
    public override bool CanRead => !disposed;
    /// <inheritdoc />
    public override bool CanSeek => !disposed;
    /// <inheritdoc />
    public override bool CanWrite => false;
    /// <inheritdoc />
    public override long Length => length;
    /// <inheritdoc />
    public override long Position {
        get => position;
        set {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            position = value;
        }
    }
    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count) {
        if (disposed)
            throw new ObjectDisposedException(nameof(SlowSeekableStream));
        int read = (int)Math.Min(Math.Min(count, 8192), Math.Max(0, length - position));
        Array.Clear(buffer, offset, read);
        position += read;
        return read;
    }
    /// <inheritdoc />
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
        await Task.Delay(40, cancellationToken);
        int read = Read(buffer, offset, count);
        firstRead.TrySetResult(true);
        return read;
    }
    /// <inheritdoc />
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) {
        await Task.Delay(40, cancellationToken);
        if (disposed)
            throw new ObjectDisposedException(nameof(SlowSeekableStream));
        int read = (int)Math.Min(Math.Min(buffer.Length, 8192), Math.Max(0, length - position));
        buffer.Span.Slice(0, read).Clear();
        position += read;
        firstRead.TrySetResult(true);
        return read;
    }
    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) {
        Position = origin switch {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => position + offset,
            SeekOrigin.End => length + offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin))
        };
        return position;
    }
    /// <inheritdoc />
    public override void Flush() => throw new NotSupportedException();
    /// <inheritdoc />
    public override void SetLength(long value) => throw new NotSupportedException();
    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    /// <inheritdoc />
    protected override void Dispose(bool disposing) {
        disposed = true;
        base.Dispose(disposing);
    }
}
