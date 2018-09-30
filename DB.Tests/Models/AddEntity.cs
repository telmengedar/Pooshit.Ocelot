using NightlyCode.DB.Entities.Attributes;

namespace NightlyCode.DB.Tests.Models {

    [Table("schemaentity")]
    public class AddEntity : OriginalEntity {

        [Index("field4")]
        public int Field4 { get; set; }
    }
}