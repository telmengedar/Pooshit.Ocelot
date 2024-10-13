namespace Pooshit.Ocelot.Entities.Operations.Tables {
    
    /// <summary>
    /// field to be loaded
    /// </summary>
    public class DataField {
        
        /// <summary>
        /// creates a new <see cref="DataField"/>
        /// </summary>
        /// <param name="name">name of column or field</param>
        /// <param name="isColumn">determines whether name is masked as column</param>
        public DataField(string name, bool isColumn=false) {
            Name = name;
            IsColumn = isColumn;
        }

        /// <summary>
        /// name of column or field
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// determines whether name is masked as column
        /// </summary>
        public bool IsColumn { get; set; }
    }
}