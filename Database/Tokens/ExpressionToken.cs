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
        readonly bool useBraces;

        internal ExpressionToken(Expression expression, bool useBraces=false) {
            this.expression = expression;
            this.useBraces = useBraces;
        }

        /// <inheritdoc />
        public override void ToSql(IDBInfo dbinfo, IOperationPreparator preparator, Func<Type, EntityDescriptor> models, string tablealias) {
            if (useBraces)
                preparator.AppendText("(");
            CriteriaVisitor.GetCriteriaText(expression, models, dbinfo, preparator, tablealias);
            if (useBraces)
                preparator.AppendText(")");
        }
    }
}