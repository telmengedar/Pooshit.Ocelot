using Pooshit.Ocelot.Entities.Attributes;

namespace NightlyCode.Database.Tests.Entities {
    public class UnionA {

        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }
        public string Name { get; set; }

        public decimal Number { get; set; }
    }
}