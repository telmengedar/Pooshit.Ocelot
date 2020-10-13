using System;
using System.Reflection;

namespace NightlyCode.Database.Entities.Attributes {

    /// <summary>
    /// specifies auto increment for a column
    /// </summary>
    public class AutoIncrementAttribute : Attribute {

        /// <summary>
        /// determines whether a property is marked as auto increment
        /// </summary>
        /// <param name="property">property to be analysed</param>
        /// <returns>true if property is marked as autoincrement, false otherwise</returns>
        public static bool IsAutoIncrement(PropertyInfo property) {
            return IsDefined(property, typeof(AutoIncrementAttribute));
        }
    }
}
