using System;
using NightlyCode.Database.Entities.Attributes;

namespace NightlyCode.Database.Tests.Models
{

    /// <summary>
    /// active object in symphony
    /// </summary>
    [Table("activeData")]
    public class ActiveData
    {

        /// <summary>
        /// id of object
        /// </summary>
        [PrimaryKey]
        public long ID { get; set; }

        /// <summary>
        /// object revision
        /// </summary>
        public int Revision { get; set; }

        /// <summary>
        /// name of object
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// object class
        /// </summary>
        public string Class { get; set; }

        /// <summary>
        /// usually indicates validity start of object
        /// </summary>
        public DateTime From { get; set; }

        /// <summary>
        /// usually indicates validity end of object
        /// </summary>
        public DateTime To { get; set; }

        /// <summary>
        /// custom amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// time when object was changed last
        /// </summary>
        [Column("lastWrite")]
        public DateTime LastWrite { get; set; }

        /// <summary>
        /// realm object belongs to
        /// </summary>
        [Column("ownerRealm")]
        public int OwnerRealm { get; set; }

        /// <summary>
        /// id of owner
        /// </summary>
        [Column("ownerId")]
        public int OwnerID { get; set; }

        /// <summary>
        /// object data
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// determines whether object history data is archived
        /// </summary>
        [Column("revisionSaveing")]
        public bool RevisionSaving { get; set; }
    }
}