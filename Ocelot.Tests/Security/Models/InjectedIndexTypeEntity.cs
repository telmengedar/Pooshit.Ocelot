using Pooshit.Ocelot.Entities.Attributes;

namespace Pooshit.Ocelot.Tests.Security.Models;

public class InjectedIndexTypeEntity {
    [Index("idx", "btree; DROP TABLE x; --")]
    public int Id { get; set; }
}
