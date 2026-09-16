using Pooshit.Ocelot.Entities.Attributes;

namespace Pooshit.Ocelot.Tests.Security.Models;

[Table("activeData")]
public class MixedCaseTableEntity {
    public int Id { get; set; }
}
