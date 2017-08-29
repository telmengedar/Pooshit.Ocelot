using System;

namespace NightlyCode.DB.Entities.Attributes {

    /// <summary>
    /// specifies an index name a column is part of
    /// </summary>
    public class IndexAttribute : Attribute {
        readonly string name;

        /// <summary>
        /// creates a new <see cref="IndexAttribute"/>
        /// </summary>
        /// <param name="name"></param>
        public IndexAttribute(string name) {
            this.name = name;
        }

        /// <summary>
        /// name of the index
        /// </summary>
        public string Name => name;
    }
}
