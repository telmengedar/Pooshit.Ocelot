using System;

namespace NightlyCode.Database.Dto {

    /// <summary>
    /// type to use in an entity load union statement
    /// </summary>
    public class TypeUnion {

        /// <summary>
        /// type to use for union statement
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// alias to use for union statement
        /// </summary>
        public string Alias { get; set; }
    }
}