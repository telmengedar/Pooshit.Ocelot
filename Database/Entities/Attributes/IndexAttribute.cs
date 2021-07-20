using System;

namespace NightlyCode.Database.Entities.Attributes {

    /// <summary>
    /// specifies an index name a column is part of
    /// </summary>
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Field, AllowMultiple = true)]
    public class IndexAttribute : Attribute {
        
        /// <summary>
        /// creates a new <see cref="IndexAttribute"/>
        /// </summary>
        /// <param name="name"></param>
        public IndexAttribute(string name) {
            Name = name;
        }

        /// <summary>
        /// name of the index
        /// </summary>
        public string Name { get; }
    }
}
