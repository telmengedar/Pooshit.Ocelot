using Pooshit.Ocelot.Entities.Attributes;

namespace NightlyCode.Database.Tests.Models {

    [Table("schemaentity")]
    public class OriginalEntity : EntityWithLessFields {
        [Index("field3")]
        public bool Field3 { get; set; } 
    }
}