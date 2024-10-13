namespace Pooshit.Ocelot.CustomTypes; 

/// <summary>
/// range type for databases supporting it (pg)
/// </summary>
/// <typeparam name="T">type of data</typeparam>
public class Range<T> {
    
    /// <summary>
    /// creates a new <see cref="Range{T}"/>
    /// </summary>
    /// <param name="lower">lower bound of range</param>
    /// <param name="upper">upper bound of range</param>
    public Range(T lower, T upper) {
        Lower = lower;
        Upper = upper;
    }

    /// <summary>
    /// lower bound of range
    /// </summary>
    public T Lower { get; }
    
    /// <summary>
    /// upper bound of range
    /// </summary>
    public T Upper { get; }
    
    /// <summary>
    /// determines whether lower bound is inclusive
    /// </summary>
    public bool LowerInclusive { get; set; } = true;
    
    /// <summary>
    /// determines whether upper bound is inclusive
    /// </summary>
    public bool UpperInclusive { get; set; } = true;
}