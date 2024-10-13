using System;
using System.Linq.Expressions;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Expressions;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Tokens {
    
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