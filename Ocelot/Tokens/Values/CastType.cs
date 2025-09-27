
namespace Pooshit.Ocelot.Tokens.Values;

/// <summary>
/// target type of cast
/// </summary>
public enum CastType {
        
    /// <summary>
    /// date
    /// </summary>
    Date,
        
    /// <summary>
    /// datetime
    /// </summary>
    DateTime,
        
    /// <summary>
    /// extracts year from a datetime or timestamp
    /// </summary>
    Year,

    /// <summary>
    /// extracts month from a datetime or timestamp
    /// </summary>
    Month,
        
    /// <summary>
    /// extracts day of month from a datetime or timestamp
    /// </summary>
    DayOfMonth,
        
    /// <summary>
    /// extracts hour from a datetime or timestamp
    /// </summary>
    Hour,
        
    /// <summary>
    /// extracts minute from a datetime or timestamp
    /// </summary>
    Minute,
        
    /// <summary>
    /// extracts second from a datetime or timestamp
    /// </summary>
    Second,
        
    /// <summary>
    /// extracts day of year from a datetime or timestamp
    /// </summary>
    DayOfYear,
        
    /// <summary>
    /// extracts day of week from a datetime or timestamp
    /// </summary>
    DayOfWeek,

    /// <summary>
    /// extracts week of year from a datetime or timestamp
    /// </summary>
    WeekOfYear,
    
    /// <summary>
    /// integer
    /// </summary>
    Integer,
        
    /// <summary>
    /// floating point
    /// </summary>
    Float,
        
    /// <summary>
    /// text
    /// </summary>
    Text,
    
    /// <summary>
    /// extracts number of ticks from an interval
    /// </summary>
    Ticks,
    
    /// <summary>
    /// vector
    /// </summary>
    Vector
}