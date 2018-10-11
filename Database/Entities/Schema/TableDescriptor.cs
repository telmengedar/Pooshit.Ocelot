using NightlyCode.Database.Entities.Descriptors;

namespace NightlyCode.Database.Entities.Schema {

    /// <summary>
    /// descriptor for a database table
    /// </summary>
    public class TableDescriptor : SchemaDescriptor {

        /// <summary>
        /// creates a new <see cref="TableDescriptor"/>
        /// </summary>
        /// <param name="name">name of table</param>
        public TableDescriptor(string name) {
            Name = name;
        }

        /// <summary>
        /// columns in table
        /// </summary>
        public SchemaColumnDescriptor[] Columns { get; set; }

        /// <summary>
        /// indices in table
        /// </summary>
        public IndexDescriptor[] Indices { get; set; }

        /// <summary>
        /// unique declarations in table
        /// </summary>
        public UniqueDescriptor[] Uniques { get; set; }

        /// <summary>
        /// type of schema
        /// </summary>
        public override SchemaType Type => SchemaType.Table;
    }
}