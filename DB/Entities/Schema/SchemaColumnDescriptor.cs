using NightlyCode.DB.Entities.Descriptors;

namespace NightlyCode.DB.Entities.Schema {
    /// <summary>
    /// descriptor for a column of a <see cref="TableDescriptor"/>
    /// </summary>
    public class SchemaColumnDescriptor : ColumnDescriptor {

        /// <summary>
        /// creates a new <see cref="SchemaColumnDescriptor"/>
        /// </summary>
        protected SchemaColumnDescriptor() {}

        /// <summary>
        /// creates a new <see cref="SchemaColumnDescriptor"/>
        /// </summary>
        /// <param name="name">name of column</param>
        public SchemaColumnDescriptor(string name)
            : base(name) {}

        /// <summary>
        /// type of column
        /// </summary>
        public string Type { get; set; }
    }
}