using System;
using System.Reflection;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Entities.Attributes;

/// <summary>
/// declares a <c>float[]</c> property as a native vector column of a fixed number of dimensions, honoured only by dialects with a native vector type
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class VectorAttribute : Attribute {

	/// <summary>
	/// highest dimension count pgvector supports for the <c>vector</c> type
	/// </summary>
	public const int MaxDimensions = 16000;

	/// <summary>
	/// creates a new <see cref="VectorAttribute"/>
	/// </summary>
	/// <param name="dimensions">number of dimensions of the vector</param>
	public VectorAttribute(int dimensions) {
		if (dimensions < 1 || dimensions > MaxDimensions)
			throw new ArgumentOutOfRangeException(nameof(dimensions), dimensions, $"Vector dimensions must be between 1 and {MaxDimensions}");
		Dimensions = dimensions;
	}

	/// <summary>
	/// number of dimensions of the vector
	/// </summary>
	public int Dimensions { get; }

	/// <summary>
	/// get the native column type of a vector property for a dialect
	/// </summary>
	/// <param name="property">property to check</param>
	/// <param name="dbinfo">dialect for which to get the type</param>
	/// <returns>native vector type, or null when the property is not declared as vector or the dialect has no native vector type</returns>
	public static string GetColumnType(PropertyInfo property, IDBInfo dbinfo) {
		if (GetCustomAttribute(property, typeof(VectorAttribute)) is not VectorAttribute vector)
			return null;

		if (property.PropertyType != typeof(float[]))
			throw new InvalidOperationException($"Property '{property.Name}' is declared as vector but is of type '{property.PropertyType.Name}', only float[] is supported");

		return dbinfo.GetVectorType(vector.Dimensions);
	}
}
