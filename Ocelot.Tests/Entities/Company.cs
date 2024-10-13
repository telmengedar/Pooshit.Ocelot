using Pooshit.Ocelot.Entities.Attributes;

namespace NightlyCode.Database.Tests.Entities {

    /// <summary>
    /// company information
    /// </summary>
    public class Company : CompanyData {

        /// <summary>
        /// id of company
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public ulong ID { get; set; }
    }
}