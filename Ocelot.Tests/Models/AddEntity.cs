using Pooshit.Ocelot.Entities.Attributes;

namespace Pooshit.Ocelot.Tests.Models {

    [Table("schemaentity")]
    public class AddEntity : OriginalEntity {

        [Index("field4")]
        public int Field4 { get; set; }
    }
}