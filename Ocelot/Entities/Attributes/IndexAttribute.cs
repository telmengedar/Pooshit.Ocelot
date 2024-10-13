using System;

namespace Pooshit.Ocelot.Entities.Attributes {

    /// <summary>
    /// specifies an index name a column is part of
    /// </summary>
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Field, AllowMultiple = true)]
    public class IndexAttribute : Attribute {
        
        /// <summary>
        /// creates a new <see cref="IndexAttribute"/>
        /// </summary>
        /// <param name="name">name of index</param>
        /// <param name="type">type of index (optional)</param>
        public IndexAttribute(string name, string type=null) {
            Name = name;
            Type = type;
        }

        /// <summary>
        /// name of the index
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// type of index
        /// </summary>
        public string Type { get; set; }
    }
}
