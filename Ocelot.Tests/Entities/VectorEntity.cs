using Pooshit.Ocelot.Entities.Attributes;

namespace Pooshit.Ocelot.Tests.Entities;

/// <summary>
/// entity with a property declared as native vector column
/// </summary>
[Table("vectorentity")]
public class VectorEntity {

    /// <summary>
    /// primary key
    /// </summary>
    [PrimaryKey, AutoIncrement]
    public long Id { get; set; }

    /// <summary>
    /// vector with three dimensions
    /// </summary>
    [Vector(3)]
    public float[] Embedding { get; set; }
}
