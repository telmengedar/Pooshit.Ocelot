using Pooshit.Ocelot.Fields;

namespace Pooshit.Ocelot.Tokens.Values;

/// <summary>
/// tuple of related values
/// </summary>
public class TupleToken : DBField {
	
	/// <summary>
	/// creates a new <see cref="TupleToken"/>
	/// </summary>
	/// <param name="values">values to be contained in tuple</param>
	public TupleToken(object[] values) => Values = values;

	/// <summary>
	/// values contained by tuple
	/// </summary>
	public object[] Values { get; }
}