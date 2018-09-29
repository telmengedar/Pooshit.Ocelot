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
        /// <param name="columns">columns which have to have a combined unique value</param>
        public UniqueDescriptor(IEnumerable<string> columns)
        {
            this.columns = columns.ToArray();
        }

        /// <summary>
        /// columns linked to the unique specifier
        /// </summary>
        public IEnumerable<string> Columns => columns;
    }
}
