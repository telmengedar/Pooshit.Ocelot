using System.Collections.Generic;
using System.Linq;

namespace Database.Entities.Descriptors {

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

        /// <inheritdocs/>
        protected bool Equals(IndexDescriptor other)
        {
            return Name.Equals(other.Name) && columns.SequenceEqual(other.Columns);
        }

        /// <inheritdocs/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((IndexDescriptor)obj);
        }

        /// <inheritdocs/>
        public override int GetHashCode()
        {
            int hashcode = Name.GetHashCode();

            if (columns == null)
                return hashcode;

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
            if (string.IsNullOrEmpty(Name))
                return $"{Name} {string.Join(",", columns)}";
            return string.Join(",", columns);
        }

    }
}
