using Pooshit.Ocelot.Entities.Attributes;

namespace Pooshit.Ocelot.Tests.Security.Models;

[Table("victim; DROP TABLE x; --")]
public class InjectedTableEntity {
    public int Id { get; set; }
}
