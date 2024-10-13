using System.Collections.Generic;
using System.Linq;

namespace Pooshit.Ocelot.Schemas {

    /// <summary>
    /// descriptor for an index
    /// </summary>
    public class IndexDescriptor {

        /// <summary>
        /// creates a new <see cref="IndexDescriptor"/>
        /// </summary>
        public IndexDescriptor(){}

        /// <summary>
        /// creates a new <see cref="IndexDescriptor"/>
        /// </summary>
        /// <param name="name">name of index</param>
        /// <param name="columns">name of columns referenced by the index</param>
        /// <param name="type">type of index</param>
        public IndexDescriptor(string name, IEnumerable<string> columns, string type) {
            Name = name;
            Columns = columns.ToArray();
            Type = type;
        }

        /// <summary>
        /// name of the index
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// type of index (optional)
        /// </summary>
        public string Type { get; set; }
        
        /// <summary>
        /// columns linked to the index
        /// </summary>
        public string[] Columns { get; set; }

        /// <inheritdocs/>
        bool Equals(IndexDescriptor other) {
            return Name.Equals(other.Name) && Columns.OrderBy(c => c).SequenceEqual(other.Columns.OrderBy(c => c));
        }

        /// <inheritdocs/>
        public override bool Equals(object obj) {
            if(ReferenceEquals(null, obj))
                return false;
            if(ReferenceEquals(this, obj))
                return true;
            if(obj.GetType() != GetType())
                return false;
            return Equals((IndexDescriptor)obj);
        }

        /// <inheritdocs/>
        public override int GetHashCode() {
            int hashcode = Name.GetHashCode();

            if(Columns == null)
                return hashcode;

            foreach(string column in Columns.OrderBy(c => c)) {
                hashcode *= 397;
                hashcode ^= column.GetHashCode();
            }

            return hashcode;
        }

        /// <inheritdocs/>
        public override string ToString() {
            if(string.IsNullOrEmpty(Name))
                return $"{Name} {string.Join(",", Columns)}";
            return string.Join(",", Columns);
        }

    }
}
