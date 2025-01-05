namespace Pooshit.Ocelot.Fields;

/// <summary>
/// aggregate with <see cref="IDBField"/> arguments
/// </summary>
public class Aggregate : DBField {

    /// <summary>
    /// creates a new <see cref="Aggregate"/>
    /// </summary>
    /// <param name="method">aggregate method</param>
    /// <param name="arguments">arguments for method</param>
    internal Aggregate(string method, params IDBField[] arguments) {
        Method = method;
        Arguments = arguments;
    }

    /// <summary>
    /// method name
    /// </summary>
    public string Method { get; }

    /// <summary>
    /// content of the function
    /// </summary>
    public IDBField[] Arguments { get; }
}