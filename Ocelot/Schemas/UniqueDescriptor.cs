using System.Collections.Generic;
using System.Linq;

namespace Pooshit.Ocelot.Schemas {

    /// <summary>
    /// descriptor for an index
    /// </summary>
    public class UniqueDescriptor {

        /// <summary>
        /// creates a new <see cref="UniqueDescriptor"/>
        /// </summary>
        public UniqueDescriptor(){}
        
        /// <summary>
        /// creates a new <see cref="UniqueDescriptor"/>
        /// </summary>
        /// <param name="columns">columns which have to have a combined unique value</param>
        public UniqueDescriptor(string name, IEnumerable<string> columns) {
            Name = name;
            Columns = columns.ToArray();
        }

        /// <summary>
        /// name of unique index
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// columns linked to the unique specifier
        /// </summary>
        public string[] Columns { get; set; }

        /// <inheritdocs/>
        bool Equals(UniqueDescriptor other) {
            return Columns.OrderBy(c => c).SequenceEqual(other.Columns.OrderBy(c => c));
        }

        /// <inheritdocs/>
        public override bool Equals(object obj) {
            if(ReferenceEquals(null, obj))
                return false;
            if(ReferenceEquals(this, obj))
                return true;
            if(obj.GetType() != GetType())
                return false;
            return Equals((UniqueDescriptor)obj);
        }

        /// <inheritdocs/>
        public override int GetHashCode() {
            if(Columns == null)
                return 0;

            int hashcode = 0;
            foreach(string column in Columns.OrderBy(c => c)) {
                hashcode *= 397;
                hashcode ^= column.GetHashCode();
            }

            return hashcode;
        }

        /// <inheritdocs/>
        public override string ToString() {
            return string.Join(",", Columns);
        }
    }
}
