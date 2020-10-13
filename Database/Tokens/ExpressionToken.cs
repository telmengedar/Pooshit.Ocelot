using System;
using System.Linq.Expressions;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Expressions;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Info;

namespace NightlyCode.Database.Tokens {
    
    /// <summary>
    /// expression translated to sql
    /// </summary>
    public class ExpressionToken : SqlToken {
        readonly Expression expression;

        internal ExpressionToken(Expression expression) {
            this.expression = expression;
        }

        /// <inheritdoc />
        public override void ToSql(IDBInfo dbinfo, IOperationPreparator preparator, Func<Type, EntityDescriptor> models, string tablealias) {
            CriteriaVisitor.GetCriteriaText(expression, models, dbinfo, preparator, tablealias);
        }
    }
}