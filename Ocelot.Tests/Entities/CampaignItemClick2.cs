using System;
using Pooshit.Ocelot.Entities.Attributes;

namespace NightlyCode.Database.Tests.Entities {
    
    /// <summary>
    /// click on campaign item
    /// </summary>
    [Table("campaignitemclick")]
    public class CampaignItemClick2 {

        /// <summary>
        /// id of click
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }
        
        /// <summary>
        /// time when session was registered
        /// </summary>
        [Index("time")]
        public DateTime Time { get; set; }

        /// <summary>
        /// id of campaign item target
        /// </summary>
        [Index("item")]
        public long TargetId { get; set; }

        /// <summary>
        /// id of job
        /// </summary>
        [Index("job")]
        public long JobId { get; set; }

        /// <summary>
        /// id of campaign
        /// </summary>
        [Index("campaign")]
        public long CampaignId { get; set; }

        /// <summary>
        /// id of broker
        /// </summary>
        [Index("broker")]
        public long BrokerId { get; set; }

        /// <summary>
        /// cpc value
        /// </summary>
        public decimal Cpc { get; set; }
        
        /// <summary>
        /// determines whether the session was hit without paying cpc value
        /// </summary>
        public bool IsOrganic { get; set; }

        /// <summary>
        /// type of click
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// flags for event
        /// </summary>
        [DefaultValue(0)]
        public int Flags { get; set; }
        
        /// <summary>
        /// value for grouped clicks
        /// </summary>
        /// <remarks>
        /// used for indeed clicks since clicks are only reported accumulated over a day
        /// </remarks>
        [DefaultValue(1)] 
        public int Value { get; set; }

        /// <summary>
        /// costs of entry
        /// </summary>
        /// <remarks>
        /// should be the same as cpc for most click types, only organic clicks and grouped clicks (indeed) differ
        /// </remarks>
        [DefaultValue(0)]
        public decimal Costs { get; set; }
        
        /// <summary>
        /// fingerprint of client
        /// </summary>
        [Index("fingerprint")]
        public string FingerPrint { get; set; }

        /// <summary>
        /// click id of broker
        /// </summary>
        /// <remarks>
        /// when click type is not <see cref="ClickType.Visit"/> this is the
        /// id of the original visit
        /// </remarks>
        public string BrokerClickId { get; set; }

        /// <summary>
        /// country of origin
        /// </summary>
        public string Country { get; set; }
        
        /// <inheritdoc />
        public override string ToString() {
            return $"{Time:yyyy-MM-dd hh:mm} - {Type} - {Flags}";
        }
    }
}