using NightlyCode.DB.Entities.Attributes;

namespace NightlyCode.DB.Tests.Schema {

    [Table("schemaentity")]
    public class OriginalEntity : EntityWithLessFields {
        [Index("field3")]
        public bool Field3 { get; set; } 
    }
}