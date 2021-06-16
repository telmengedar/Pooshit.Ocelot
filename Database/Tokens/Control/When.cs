using System;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Info;

namespace NightlyCode.Database.Tokens.Control {
    
    /// <summary>
    /// a when token in a case statement
    /// </summary>
    public class When : SqlToken {
        readonly ISqlToken condition;
        readonly ISqlToken value;

        internal When(ISqlToken condition, ISqlToken value) {
            this.condition = condition;
            this.value = value;
        }
        
        /// <inheritdoc />
        public override void ToSql(IDBInfo dbinfo, IOperationPreparator preparator, Func<Type, EntityDescriptor> models, string tablealias) {
            preparator.AppendText("WHEN");
            condition.ToSql(dbinfo, preparator, models, tablealias);
            preparator.AppendText("THEN");
            value.ToSql(dbinfo, preparator, models, tablealias);
        }
    }
}