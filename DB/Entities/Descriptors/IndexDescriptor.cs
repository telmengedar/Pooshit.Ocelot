using System.Collections.Generic;
using System.Linq;

namespace NightlyCode.DB.Entities.Descriptors {

    /// <summary>
    /// descriptor for an index
    /// </summary>
    public class IndexDescriptor {
        readonly string[] columns;

        /// <summary>
        /// creates a new <see cref="IndexDescriptor"/>
        /// </summary>
        /// <param name="name">name of index</param>
        /// <param name="columns">name of columns referenced by the index</param>
        public IndexDescriptor(string name, IEnumerable<string> columns) {
            Name = name;
            this.columns = columns.ToArray();
        }

        /// <summary>
        /// name of the index
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// columns linked to the index
        /// </summary>
        public IEnumerable<string> Columns => columns;
    }
}
