namespace Pooshit.Ocelot.Models;

/// <summary>
/// options for paged operations
/// </summary>
public class PageOptions {
    
    /// <summary>
    /// offset to use
    /// </summary>
    public int Offset { get; set; }

    /// <summary>
    /// number of items to display
    /// </summary>
    public int Items { get; set; }
}