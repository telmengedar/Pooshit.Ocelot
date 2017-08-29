using System;
using System.Linq.Expressions;
using NightlyCode.DB.Entities.Descriptors;
using NightlyCode.DB.Info;

namespace NightlyCode.DB.Entities.Operations {

    /// <summary>
    /// field describing a property of an entity
    /// </summary>
    public class EntityField : DBField {

        /// <summary>
        /// ctor
        /// </summary>
        internal EntityField(Expression fieldexpression) {
            FieldExpression = fieldexpression;
        }

        /// <summary>
        /// expression describing the field
        /// </summary>
        public Expression FieldExpression { get; private set; }

        public override void PrepareCommand(OperationPreparator preparator, IDBInfo dbinfo, Func<Type, EntityDescriptor> descriptorgetter) {
            CriteriaVisitor.GetCriteriaText(FieldExpression, descriptorgetter, dbinfo, preparator);
        }

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