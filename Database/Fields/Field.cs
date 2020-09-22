using System;
using System.Linq.Expressions;
using NightlyCode.Database.Entities.Operations;
using NightlyCode.Database.Fields.Sql;

namespace NightlyCode.Database.Fields {

    /// <summary>
    /// helper class used to create expression fields
    /// </summary>
    public static class Field {

        /// <summary>
        /// all fields
        /// </summary>
        public static readonly IDBField All = new AllColumnsField();
        
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

        /// <summary>
        /// creates a new <see cref="Column"/>
        /// </summary>
        /// <param name="table">name of table/view/alias of which to reference column</param>
        /// <param name="column">name of column to reference</param>
        /// <returns>field to be used in expressions</returns>
        public static ColumnField Column(string table, string column) {
            return new ColumnField(table, column);
        }

        /// <summary>
        /// references a property of an entity using an expression
        /// </summary>
        /// <typeparam name="T">type of entity to reference</typeparam>
        /// <param name="expression">expression pointing to property</param>
        /// <returns>field to be used in statements</returns>
        public static EntityField Property<T>(Expression<Func<T, object>> expression) {
            return new EntityField(expression);
        }
    }
}