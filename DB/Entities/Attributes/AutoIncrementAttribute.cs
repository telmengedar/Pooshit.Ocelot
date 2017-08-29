using System;
using System.Reflection;

namespace NightlyCode.DB.Entities.Attributes {

    /// <summary>
    /// specifies auto increment for a column
    /// </summary>
    public class AutoIncrementAttribute : Attribute {

        public static bool IsAutoIncrement(PropertyInfo property) {
            return IsDefined(property, typeof(AutoIncrementAttribute));
        }
    }
}
