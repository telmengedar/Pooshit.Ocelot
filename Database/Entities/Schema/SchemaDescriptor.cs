
namespace Database.Entities.Schema {

    /// <summary>
    /// descriptor for a schema
    /// </summary>
    public abstract class SchemaDescriptor {

        /// <summary>
        /// name of schema
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// type of schema
        /// </summary>
        public abstract SchemaType Type { get; } 
    }
}