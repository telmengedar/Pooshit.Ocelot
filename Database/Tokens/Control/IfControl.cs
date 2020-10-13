using System;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Info;

namespace NightlyCode.Database.Tokens.Control {
    
    /// <summary>
    /// control statement selecting one of multiple possible values
    /// </summary>
    public class IfControl : SqlToken {
        readonly ISqlToken condition;
        readonly ISqlToken value;
        readonly ISqlToken falsevalue;

        internal IfControl(ISqlToken condition, ISqlToken value, ISqlToken falsevalue=null) {
            this.condition = condition;
            this.value = value;
            this.falsevalue = falsevalue;
        }

        /// <inheritdoc />
        public override void ToSql(IDBInfo dbinfo, IOperationPreparator preparator, Func<Type, EntityDescriptor> models, string tablealias) {
            preparator.AppendText("CASE WHEN");
            condition.ToSql(dbinfo, preparator, models, tablealias);
            preparator.AppendText("THEN");
            value.ToSql(dbinfo, preparator, models, tablealias);
            if (falsevalue != null) {
                preparator.AppendText("ELSE");
                falsevalue.ToSql(dbinfo, preparator, models, tablealias);
            }

            preparator.AppendText("END");
        }
    }
}