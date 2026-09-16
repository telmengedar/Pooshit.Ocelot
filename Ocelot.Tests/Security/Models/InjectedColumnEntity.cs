using Pooshit.Ocelot.Entities.Attributes;

namespace Pooshit.Ocelot.Tests.Security.Models;

public class InjectedColumnEntity {
    [Column("victim; DROP TABLE x; --")]
    public int Id { get; set; }
}
