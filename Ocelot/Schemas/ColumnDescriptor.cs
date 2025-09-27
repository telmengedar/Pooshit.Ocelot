namespace Pooshit.Ocelot.Schemas {
    
    /// <summary>
    /// descriptor for a column of a database entity
    /// </summary>
    public class ColumnDescriptor {

        /// <summary>
        /// creates a new <see cref="ColumnDescriptor"/>
        /// </summary>
        public ColumnDescriptor() {}

        /// <summary>
        /// creates a new <see cref="ColumnDescriptor"/>
        /// </summary>
        /// <param name="name">name of the column</param>
        public ColumnDescriptor(string name) {
            Name = name;
        }

        /// <summary>
        /// creates a new <see cref="ColumnDescriptor"/>
        /// </summary>
        /// <param name="name">name of the column</param>
        /// <param name="type">type of column</param>
        public ColumnDescriptor(string name, string type) 
        : this(name) {
            Type = type;
        }

        /// <summary>
        /// name of the column
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// type of column
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// size of array types
        /// </summary>
        public int Length { get; set; }
        
        /// <summary>
        /// determines whether the column is primary key
        /// </summary>
        public bool PrimaryKey { get; set; }

        /// <summary>
        /// determines whether value in column has to be unique
        /// </summary>
        public bool IsUnique { get; set; }

        /// <summary>
        /// determines whether the value of the column is set by auto increment
        /// </summary>
        public bool AutoIncrement { get; set; }

        /// <summary>
        /// determines whether the column is allowed to contain null values
        /// </summary>
        public bool NotNull { get; set; }

        /// <summary>
        /// default value of the column
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString() {
            return $"{Name} (PK: {PrimaryKey}, UQ: {IsUnique}, AI: {AutoIncrement}, NN: {NotNull}, Default: {DefaultValue}";
        }
    }
}