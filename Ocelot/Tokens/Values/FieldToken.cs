using Pooshit.Ocelot.Fields;

namespace Pooshit.Ocelot.Tokens.Values;

/// <summary>
/// token used to reference a statement field
/// </summary>
public class FieldToken : DBField {
	
	/// <summary>
	/// creates a new <see cref="FieldToken"/>
	/// </summary>
	/// <param name="field">name of field</param>
	public FieldToken(string field) => Field = field;
	
	/// <summary>
	/// name of field
	/// </summary>
	public string Field { get; }
}