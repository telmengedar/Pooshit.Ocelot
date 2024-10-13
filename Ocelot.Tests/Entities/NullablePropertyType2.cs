using System.ComponentModel.DataAnnotations.Schema;

namespace NightlyCode.Database.Tests.Entities {
    
    [Table("nullablepropertytype")]
    public class NullablePropertyType2 {
        public string Existing { get; set; }
    }
}