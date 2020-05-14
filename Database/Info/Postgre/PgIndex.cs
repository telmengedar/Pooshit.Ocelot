using NightlyCode.Database.Entities.Attributes;

namespace NightlyCode.Database.Info.Postgre {

    /// <summary>
    /// index definition
    /// </summary>
    [Table("pg_indexes")]
    public class PgIndex {

        /// <summary>
        /// schema for which index is defined
        /// </summary>
        [Column("schemaname")]
        public string Schema { get; set; }

        /// <summary>
        /// table in which index is defined
        /// </summary>
        [Column("tablename")]
        public string Table { get; set; }

        /// <summary>
        /// name of index
        /// </summary>
        [Column("indexname")]
        public string Name { get; set; }

        /// <summary>
        /// index sql definition
        /// </summary>
        [Column("indexdef")]
        public string Definition { get; set; }
    }
}