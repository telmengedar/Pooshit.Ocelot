using Pooshit.Ocelot.Entities.Attributes;

namespace Pooshit.Ocelot.Tests.Security.Models;

public class InjectedIndexNameEntity {
    [Index("victim; DROP TABLE x; --")]
    public int Id { get; set; }
}
