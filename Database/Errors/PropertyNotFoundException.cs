using System;

namespace NightlyCode.Database.Errors {

    /// <summary>
    /// thrown when a requested property was not found
    /// </summary>
    public class PropertyNotFoundException : Exception {

        /// <summary>
        /// creates a new <see cref="PropertyNotFoundException"/>
        /// </summary>
        /// <param name="propertyname">name of requested property</param>
        /// <param name="message">error message</param>
        /// <param name="innerException">exception which lead to this exception</param>
        public PropertyNotFoundException(string propertyname, string message = null, Exception innerException = null)
            : base(message ?? $"Property {propertyname} was not found", innerException) {
            PropertyName = propertyname;
        }

        /// <summary>
        /// name of requested property
        /// </summary>
        public string PropertyName { get; }
    }
}