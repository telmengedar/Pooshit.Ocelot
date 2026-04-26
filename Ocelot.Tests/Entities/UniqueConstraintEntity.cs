using Pooshit.Ocelot.Entities.Attributes;

namespace Pooshit.Ocelot.Tests.Entities {

    /// <summary>
    /// entity used to test that named single-column UNIQUE constraints are not duplicated on UpdateSchema
    /// </summary>
    [Table("uniqueconstraintentity")]
    public class UniqueConstraintEntity {

        /// <summary>
        /// primary key
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }

        /// <summary>
        /// column with a named unique constraint — exercises [Unique("name")] path
        /// </summary>
        [Unique("uq_uniqueconstraintentity_url")]
        public string Url { get; set; }

        /// <summary>
        /// column with an unnamed unique constraint — exercises [Unique] path
        /// </summary>
        [Unique]
        public string Slug { get; set; }
    }
}
