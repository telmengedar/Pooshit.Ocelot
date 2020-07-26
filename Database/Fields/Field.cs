using System;
using NightlyCode.Database.Fields.Sql;

namespace NightlyCode.Database.Fields {

    /// <summary>
    /// helper class used to create expression fields
    /// </summary>
    public static class Field {

        /// <summary>
        /// all fields
        /// </summary>
        const string All = "*";
        
        /// <summary>
        /// creates a new <see cref="PropName"/>
        /// </summary>
        /// <param name="entitytype">type of entity of which a property is referenced</param>
        /// <param name="property">name of referenced property</param>
        /// <param name="ignorecase">ignores casing when looking up property</param>
        /// <returns>field to use in an expression</returns>
        public static PropName Property(Type entitytype, string property, bool ignorecase = false) {
            return new PropName(entitytype, property, ignorecase);
        }

        /// <summary>
        /// creates a new <see cref="PropName"/>
        /// </summary>
        /// <param name="property">name of referenced property</param>
        /// <param name="ignorecase">ignores casing when looking up property</param>
        /// <returns>field to use in an expression</returns>
        public static PropName<T> Property<T>(string property, bool ignorecase = false) {
            return new PropName<T>(property, ignorecase);
        }
    }
}