using Pooshit.Ocelot.Entities.Attributes;

namespace Pooshit.Ocelot.Tests.Security.Models;

public class ComposedIndexEntity {
    [PrimaryKey, AutoIncrement]
    public long Id { get; set; }

    [Index("time")]
    public long Timestamp { get; set; }
}
