using System;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Fields.Sql;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Info;

namespace NightlyCode.Database.Fields.Sql {

    /// <summary>
    /// field which is referencing all columns
    /// </summary>
    public class AllColumnsField : SqlField {

        /// <inheritdoc />
        public override void ToSql(IDBInfo dbinfo, IOperationPreparator preparator, Func<Type, EntityDescriptor> models, string tablealias) {
            preparator.AppendText("*");
        }
    }
}