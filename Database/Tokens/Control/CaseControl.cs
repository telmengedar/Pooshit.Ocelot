using System;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Info;

namespace NightlyCode.Database.Tokens.Control {
    
    /// <summary>
    /// token used to generate a case statement
    /// </summary>
    public class CaseControl : SqlToken {
        readonly When[] cases;
        readonly ISqlToken elsetoken;

        internal CaseControl(When[] cases, ISqlToken elsetoken) {
            this.cases = cases;
            this.elsetoken = elsetoken;
        }

        /// <inheritdoc />
        public override void ToSql(IDBInfo dbinfo, IOperationPreparator preparator, Func<Type, EntityDescriptor> models, string tablealias) {
            preparator.AppendText("CASE");
            foreach (When when in cases)
                when.ToSql(dbinfo, preparator, models, tablealias);

            if (elsetoken != null) {
                preparator.AppendText("ELSE");
                elsetoken.ToSql(dbinfo, preparator, models, tablealias);
            }

            preparator.AppendText("END");
        }
    }
}