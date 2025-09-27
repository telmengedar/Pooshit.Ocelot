using System;
using System.Reflection;

namespace Pooshit.Ocelot.Entities.Attributes;

/// <summary>
/// size of an array
/// </summary>
public class SizeAttribute : Attribute {
	
	/// <summary>
	/// creates a new <see cref="SizeAttribute"/>
	/// </summary>
	/// <param name="length"></param>
	public SizeAttribute(int length) => Length = length;
	
	/// <summary>
	/// length of array
	/// </summary>
	public int Length { get; set; }

	/// <summary>
	/// get length of a property
	/// </summary>
	/// <param name="property">property to read</param>
	/// <returns>length of array</returns>
	public static int GetLength(PropertyInfo property) {
		return(GetCustomAttribute(property, typeof(SizeAttribute)) as SizeAttribute)?.Length ?? 0;
	}
}