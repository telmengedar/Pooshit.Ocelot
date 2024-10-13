using Pooshit.Ocelot.Entities.Attributes;

namespace Pooshit.Ocelot.Info.Postgre {

    /// <summary>
    /// view definition in postgresql
    /// </summary>
    [Table("pg_views")]
    public class PgView {

        /// <summary>
        /// schema in which view is defined
        /// </summary>
        [Column("schemaname")]
        public string Schema { get; set; }

        /// <summary>
        /// name of view
        /// </summary>
        [Column("viewname")]
        public string Name { get; set; }

        /// <summary>
        /// view owner
        /// </summary>
        [Column("viewowner")]
        public string Owner { get; set; }

        /// <summary>
        /// view definition SQL
        /// </summary>
        public string Definition { get; set; }
    }
}