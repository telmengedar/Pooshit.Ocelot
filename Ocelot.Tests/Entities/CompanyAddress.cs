using Pooshit.Ocelot.Entities.Attributes;

namespace Pooshit.Ocelot.Tests.Entities {

    /// <summary>
    /// an address
    /// </summary>
    public class CompanyAddress {

        /// <summary>
        /// id of address
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public ulong ID { get; set; }

        /// <summary>
        /// id of company this address is linked to
        /// </summary>
        [Index("company")]
        public ulong CompanyID { get; set; }

        [Unique("data")]
        public string Country { get; set; }

        [Unique("data")]
        public string State { get; set; }

        [Unique("data")]
        public string City { get; set; }

        [Unique("data")]
        public string PostalCode { get; set; }

        /// <summary>
        /// line information (street)
        /// </summary>
        [Unique("data")]
        public string Line { get; set; }

    }
}