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

        /// <inheritdocs/>
        protected bool Equals(UniqueDescriptor other)
        {
            return columns.SequenceEqual(other.Columns);
        }

        /// <inheritdocs/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((UniqueDescriptor) obj);
        }

        /// <inheritdocs/>
        public override int GetHashCode()
        {
            if (columns == null)
                return 0;

            int hashcode = 0;
            foreach (string column in columns)
            {
                hashcode ^= 397;
                hashcode ^= column.GetHashCode();
            }

            return hashcode;
        }

        /// <inheritdocs/>
        public override string ToString()
        {
            return string.Join(",", columns);
        }
    }
}
