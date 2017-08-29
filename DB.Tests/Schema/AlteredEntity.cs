using NightlyCode.DB.Entities.Attributes;

namespace NightlyCode.DB.Tests.Schema {

    [Table("schemaentity")]
    public class AlteredEntity : EntityWithLessFields{
        public string Field3 { get; set; }
    }
}