using System;
using System.Reflection;

namespace NightlyCode.DB.Entities.Attributes {

    /// <summary>
    /// specifies properties to ignore for mapping
    /// </summary>
    public class IgnoreAttribute : Attribute {

        /// <summary>
        /// determines whether to ignore a property
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static bool HasIgnore(PropertyInfo property) {
            return IsDefined(property, typeof(IgnoreAttribute));
        }         
    }
}