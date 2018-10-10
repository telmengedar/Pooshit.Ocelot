﻿using System;
using System.Reflection;

namespace NightlyCode.DB.Entities.Attributes {

    /// <summary>
    /// specifies auto increment for a column
    /// </summary>
    public class AutoIncrementAttribute : Attribute {

        /// <summary>
        /// determines whether a property is flagged as auto increment
        /// </summary>
        /// <param name="property">property to be analyzed</param>
        /// <returns>true if property is flagged auto incrementing, false otherwise</returns>
        public static bool IsAutoIncrement(PropertyInfo property) {
            return IsDefined(property, typeof(AutoIncrementAttribute));
        }
    }
}
