using NightlyCode.Database.Entities.Attributes;

namespace NightlyCode.Database.Tests.Models {
    public class AutoIncrementEntity {

        [PrimaryKey, AutoIncrement]
        public long ID { get; set; }

        public string Bla { get; set; }
    }
}