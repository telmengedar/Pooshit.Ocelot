using System.ComponentModel.DataAnnotations.Schema;

namespace Pooshit.Ocelot.Tests.Entities {
    
    [Table("nullablepropertytype")]
    public class NullablePropertyType2 {
        public string Existing { get; set; }
    }
}