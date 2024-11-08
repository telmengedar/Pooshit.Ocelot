using Pooshit.Ocelot.Entities.Attributes;

namespace Pooshit.Ocelot.Tests.Entities;

[View("Pooshit.Ocelot.Tests.Entities.testview.sql")]
public class TestView {
    public int First { get; set; }
    public int Second { get; set; }

    public string Third { get; set; }
}