using Pooshit.Ocelot.Entities.Attributes;

namespace Pooshit.Ocelot.Tests.Security.Models;

public class RenderEntity {
    [PrimaryKey]
    public long Id { get; set; }

    public int Integer { get; set; }
}
