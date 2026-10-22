namespace Pooshit.Ocelot.Tests.Models;

/// <summary>
/// Source row for binary parameter tests.
/// </summary>
public class StreamSource {
    /// <summary>
    /// Row identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Binary content.
    /// </summary>
    public byte[] Content { get; set; }
}
