using System;
using System.Linq.Expressions;

namespace Database.Entities.Operations {

    /// <summary>
    /// field describing a property of an entity
    /// </summary>
    public class EntityField : DBField {

        /// <summary>
        /// creates a new <see cref="EntityField"/>
        /// </summary>
        internal EntityField(Expression fieldexpression) {
            FieldExpression = fieldexpression;
        }

        /// <summary>
        /// expression describing the field
        /// </summary>
        public Expression FieldExpression { get; }

        /// <summary>
        /// creates an entity field
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldexpression"></param>
        /// <returns></returns>
        public static EntityField Create<T>(Expression<Func<T, object>> fieldexpression) {
            return new EntityField(fieldexpression);
        }
    }
}