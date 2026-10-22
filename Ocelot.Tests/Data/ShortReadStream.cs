using System;
using System.IO;

namespace Pooshit.Ocelot.Tests.Data;

/// <summary>
/// Nonseekable source returning at most two bytes per read.
/// </summary>
public sealed class ShortReadStream : Stream {
    readonly MemoryStream source;

    /// <summary>
    /// Creates a nonseekable short-reading source.
    /// </summary>
    /// <param name="bytes">Source bytes.</param>
    public ShortReadStream(byte[] bytes) => source = new MemoryStream(bytes);

    /// <summary>
    /// Number of read calls.
    /// </summary>
    public int Reads { get; private set; }

    /// <inheritdoc />
    public override bool CanRead => source.CanRead;
    /// <inheritdoc />
    public override bool CanSeek => false;
    /// <inheritdoc />
    public override bool CanWrite => false;
    /// <inheritdoc />
    public override long Length => throw new NotSupportedException();
    /// <inheritdoc />
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count) {
        Reads++;
        return source.Read(buffer, offset, Math.Min(count, 2));
    }
    /// <inheritdoc />
    public override void Flush() => throw new NotSupportedException();
    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    /// <inheritdoc />
    public override void SetLength(long value) => throw new NotSupportedException();
    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    /// <inheritdoc />
    protected override void Dispose(bool disposing) {
        if (disposing)
            source.Dispose();
        base.Dispose(disposing);
    }
}
