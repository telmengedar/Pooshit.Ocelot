using Pooshit.Ocelot.Entities.Attributes;

namespace Pooshit.Ocelot.Tests.Models {
    public class AutoIncrementEntity {

        [PrimaryKey, AutoIncrement]
        public long ID { get; set; }

        public string Bla { get; set; }
    }
}