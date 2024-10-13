namespace Pooshit.Ocelot.Clients.Tables; 

/// <summary>
/// row wrapping one row of a data reader
/// </summary>
public class Row {
    readonly Reader reader;

    /// <summary>
    /// creates a new <see cref="Row" />
    /// </summary>
    /// <param name="reader">reader containing current row</param>
    public Row(Reader reader) => this.reader = reader;

    /// <summary>
    /// indexer for row data
    /// </summary>
    /// <param name="index">index at which to get value</param>
    /// <returns>value at the specified index</returns>
    public object this[int index] => reader[index];

    /// <summary>
    /// indexer for row data
    /// </summary>
    /// <param name="name">name of column to retrieve</param>
    /// <returns>value at the specified index</returns>
    public object this[string name] => reader[name];

    /// <summary>
    /// get typed value from row
    /// </summary>
    /// <typeparam name="T">type of value to get</typeparam>
    /// <param name="index">index of column of which to read value</param>
    /// <returns>converted value</returns>
    public T GetValue<T>(int index) => reader.GetValue<T>(index);
    //return Converter.Convert<T>(this[index], true);
}