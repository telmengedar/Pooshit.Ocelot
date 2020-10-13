using NightlyCode.Database.Entities.Attributes;

namespace NightlyCode.Database.Info.Postgre {

    /// <summary>
    /// column information in postgres database
    /// </summary>
    [Table("information_schema.columns")]
    public class PgColumn {

        /// <summary>
        /// catalog (database) table is registered in
        /// </summary>
        [Column("table_catalog")]
        public string Catalog { get; set; }

        /// <summary>
        /// schema of table
        /// </summary>
        [Column("table_schema")]
        public string Schema { get; set; }

        /// <summary>
        /// table of column
        /// </summary>
        [Column("table_name")]
        public string Table { get; set; }

        /// <summary>
        /// column name
        /// </summary>
        [Column("column_name")]
        public string Column { get; set; }

        /// <summary>
        /// type of column data
        /// </summary>
        [Column("data_type")]
        public string DataType { get; set; }

        /// <summary>
        /// determines whether column is nullable
        /// </summary>
        [Column("is_nullable")]
        public string IsNullable { get; set; }

        /// <summary>
        /// default value of column
        /// </summary>
        [Column("column_default")]
        public string Default { get; set; }
    }
}