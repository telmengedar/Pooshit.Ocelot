using NightlyCode.Database.Entities.Attributes;

namespace NightlyCode.Database.Tests.Entities {

    [View("NightlyCode.Database.Tests.Entities.testview.sql")]
    public class TestView {
        public int First { get; set; }
        public int Second { get; set; }

        public string Third { get; set; }
    }
}