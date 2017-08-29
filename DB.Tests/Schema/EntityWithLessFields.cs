using NightlyCode.DB.Entities.Attributes;

namespace NightlyCode.DB.Tests.Schema {

    [Table("schemaentity")]
    public class EntityWithLessFields {
        public int Field1 { get; set; }
        public string Field2 { get; set; }

    }
}