namespace Pooshit.Ocelot.Schemas {
    
    /// <summary>
    /// schema for a table
    /// </summary>
    public class TableSchema : Schema {
        
        /// <summary>
        /// columns in table
        /// </summary>
        public ColumnDescriptor[] Columns { get; set; }

        /// <summary>
        /// unique column specifications
        /// </summary>
        public UniqueDescriptor[] Unique { get; set; }

        /// <summary>
        /// index columns
        /// </summary>
        public IndexDescriptor[] Index { get; set; }
    }
}