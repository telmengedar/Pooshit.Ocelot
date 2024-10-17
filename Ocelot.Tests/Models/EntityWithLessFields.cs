using Pooshit.Ocelot.Entities.Attributes;

namespace Pooshit.Ocelot.Tests.Models {

    [Table("schemaentity")]
    public class EntityWithLessFields {
        public int Field1 { get; set; }
        public string Field2 { get; set; }

    }
}