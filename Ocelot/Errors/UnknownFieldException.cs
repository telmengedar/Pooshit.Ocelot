using System.Collections.Generic;
using System.Linq;

namespace Pooshit.Ocelot.Errors {

    /// <summary>
    /// thrown when a requested field name is not registered in the mapper
    /// </summary>
    public class UnknownFieldException : KeyNotFoundException {

        /// <summary>
        /// creates a new <see cref="UnknownFieldException"/>
        /// </summary>
        /// <param name="fieldName">the offending field name as supplied by the caller</param>
        /// <param name="availableNames">the set of field names registered in the mapper at the time of the throw</param>
        public UnknownFieldException(string fieldName, IEnumerable<string> availableNames)
            : base($"Unknown field '{fieldName}'. Available: {string.Join(", ", availableNames)}") {
            FieldName = fieldName;
            AvailableNames = availableNames.ToArray();
        }

        /// <summary>
        /// the first offending field name as supplied by the caller (preserving case)
        /// </summary>
        public string FieldName { get; }

        /// <summary>
        /// a snapshot of the mapper's registered field names at the moment of the throw
        /// </summary>
        public IReadOnlyList<string> AvailableNames { get; }
    }
}
