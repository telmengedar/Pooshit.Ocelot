namespace NightlyCode.Database.Entities.Schema {

    /// <summary>
    /// descriptor for a database view
    /// </summary>
    public class ViewDescriptor : SchemaDescriptor {

        /// <summary>
        /// creates a new <see cref="ViewDescriptor"/>
        /// </summary>
        /// <param name="name"></param>
        public ViewDescriptor(string name) {
            Name = name;
        }

        /// <summary>
        /// sql used to create view
        /// </summary>
        public string SQL { get; set; }

        /// <summary>
        /// type of schema
        /// </summary>
        public override SchemaType Type => SchemaType.View;
    }
}