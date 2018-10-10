using System;
using System.Reflection;

namespace Database.Entities.Attributes {

    /// <summary>
    /// specifies that the column is the primary key
    /// </summary>
    public class PrimaryKeyAttribute : Attribute {

        /// <summary>
        /// get the primary key attribute for the property
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static PrimaryKeyAttribute Get(PropertyInfo property) {
            return (PrimaryKeyAttribute)GetCustomAttribute(property, typeof(PrimaryKeyAttribute));
        }

        /// <summary>
        /// determines whether the specified property is a primary key
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static bool IsPrimaryKey(PropertyInfo property) {
            return IsDefined(property, typeof(PrimaryKeyAttribute));
        }
    }
}
