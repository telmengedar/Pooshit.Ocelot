using Pooshit.Ocelot.Entities.Attributes;

namespace Pooshit.Ocelot.Tests.Models {

    [Table("schemaentity")]
    public class AlteredEntity : EntityWithLessFields{
        public string Field3 { get; set; }
    }
}