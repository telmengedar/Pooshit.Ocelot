using System;
using Pooshit.Ocelot.Entities.Attributes;

namespace Pooshit.Ocelot.Tests.Entities
{

    /// <summary>
    /// workflow definition
    /// </summary>
    public class Workflow
    {

        /// <summary>
        /// id of workflow
        /// </summary>
        [PrimaryKey]
        public long ID { get; set; }

        /// <summary>
        /// workflow revision
        /// </summary>
        public int Rev { get; set; }

        /// <summary>
        /// name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// class
        /// </summary>
        public string Class { get; set; }

        /// <summary>
        /// serialized workflow data
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// owner workflow belongs to
        /// </summary>
        public int Owner { get; set; }

        /// <summary>
        /// realm workflow belongs to
        /// </summary>
        [Column("ownerRealm")]
        public int OwnerRealm { get; set; }

        /// <summary>
        /// workflow priority
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// whether workflow saves revisions
        /// </summary>
        [Column("revisionSaving")]
        public bool RevisionSaving { get; set; }

        /// <summary>
        /// time when workflow was changed
        /// </summary>
        [Column("lastWrite")]
        public DateTime LastWrite { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{ID}: {Class}\\{Name}";
        }
    }
}