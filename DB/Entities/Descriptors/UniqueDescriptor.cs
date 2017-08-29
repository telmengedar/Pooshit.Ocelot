using System.Collections.Generic;
using System.Linq;

namespace NightlyCode.DB.Entities.Descriptors
{

    /// <summary>
    /// descriptor for an index
    /// </summary>
    public class UniqueDescriptor
    {
        readonly string[] columns;

        /// <summary>
        /// creates a new <see cref="UniqueDescriptor"/>
        /// </summary>
        /// <param name="name">name of the descriptor</param>
        /// <param name="columns">columns which have to have a combined unique value</param>
        public UniqueDescriptor(string name, IEnumerable<string> columns)
        {
            Name = name;
            this.columns = columns.ToArray();
        }

        /// <summary>
        /// name of the index
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// columns linked to the index
        /// </summary>
        public IEnumerable<string> Columns => columns;
    }
}
