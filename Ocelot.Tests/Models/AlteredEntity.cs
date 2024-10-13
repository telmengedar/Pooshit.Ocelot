using Pooshit.Ocelot.Entities.Attributes;

namespace NightlyCode.Database.Tests.Models {

    [Table("schemaentity")]
    public class AlteredEntity : EntityWithLessFields{
        public string Field3 { get; set; }
    }
}