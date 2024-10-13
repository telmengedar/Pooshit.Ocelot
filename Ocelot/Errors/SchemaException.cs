using System;

namespace Pooshit.Ocelot.Errors {
    
    /// <summary>
    /// error when processing a database schema
    /// </summary>
    public class SchemaException : Exception {
        
        /// <inheritdoc />
        public SchemaException(string message) : base(message) {
        }

        /// <inheritdoc />
        public SchemaException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}