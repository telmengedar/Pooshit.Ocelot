using System;
using System.IO;

namespace Pooshit.Ocelot.CustomTypes;

/// <summary>
/// A caller-owned binary stream with an exact remaining byte count.
/// </summary>
public sealed class StreamParameter {
    /// <summary>
    /// Creates a binary stream parameter.
    /// </summary>
    /// <param name="stream">Readable stream kept open by the caller until execution completes.</param>
    /// <param name="length">Exact number of remaining bytes from the position at binding.</param>
    public StreamParameter(Stream stream, int length) {
        Stream = stream ?? throw new ArgumentNullException(nameof(stream));
        if (!stream.CanRead)
            throw new ArgumentException("The stream must be readable.", nameof(stream));
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));
        Length = length;
    }

    /// <summary>
    /// Caller-owned readable source stream.
    /// </summary>
    public Stream Stream { get; }

    /// <summary>
    /// Exact remaining byte count at binding.
    /// </summary>
    public int Length { get; }

    internal void Validate(bool requireSeekable) {
        if (!Stream.CanRead)
            throw new ArgumentException("The stream must be readable.", nameof(Stream));
        if (!Stream.CanSeek) {
            if (requireSeekable)
                throw new NotSupportedException("This provider requires a seekable stream parameter.");
            return;
        }
        if (Stream.Length - Stream.Position != Length)
            throw new ArgumentException("The declared length must equal the stream's remaining byte count.", nameof(Length));
    }
}
