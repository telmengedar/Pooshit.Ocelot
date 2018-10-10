using System;
using System.Reflection;

namespace Database.Entities.Attributes {

    /// <summary>
    /// specifies that a column must not be null
    /// </summary>
    public class NotNullAttribute : Attribute {

        /// <summary>
        /// determines whether the property has a not null specification
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static bool HasNotNull(PropertyInfo property) {
            return IsDefined(property, typeof(NotNullAttribute));
        }
    }
}
