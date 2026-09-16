using Pooshit.Ocelot.Entities.Attributes;

namespace Pooshit.Ocelot.Tests.Security.Models;

public class InjectedDefaultEntity {
    [DefaultValue("a'b")]
    public string Name { get; set; }
}
