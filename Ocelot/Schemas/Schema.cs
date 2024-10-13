namespace Pooshit.Ocelot.Schemas {
    
    /// <summary>
    /// schema of table or view in database
    /// </summary>
    public class Schema {
        
        /// <summary>
        /// name of table
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// type of schema
        /// </summary>
        public SchemaType Type { get; set; }
    }
}