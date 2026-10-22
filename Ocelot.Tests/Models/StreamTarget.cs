namespace Pooshit.Ocelot.Tests.Models;

/// <summary>
/// Target row for binary copy tests.
/// </summary>
public class StreamTarget {
    /// <summary>
    /// Row identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Binary content.
    /// </summary>
    public byte[] Content { get; set; }
}
