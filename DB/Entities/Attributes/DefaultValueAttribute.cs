using System;
using System.Reflection;

namespace NightlyCode.DB.Entities.Attributes {

    /// <summary>
    /// specifies a default value for a column
    /// </summary>
    public class DefaultValueAttribute : Attribute{
        readonly object value;

        /// <summary>
        /// creates a new <see cref="DefaultValueAttribute"/>
        /// </summary>
        /// <param name="value"></param>
        public DefaultValueAttribute(object value) {
            this.value = value;
        }

        /// <summary>
        /// default value
        /// </summary>
        public object Value => value;

        /// <summary>
        /// get default value for a property
        /// </summary>
        /// <param name="property">property of which to get default value</param>
        /// <returns>default value of property to use for database columns</returns>
        public static object GetDefaultValue(PropertyInfo property) {
            DefaultValueAttribute attribute = (DefaultValueAttribute)GetCustomAttribute(property, typeof(DefaultValueAttribute));
            return attribute?.Value;
        }
    }
}
