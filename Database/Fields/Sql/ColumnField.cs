using System;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Fields.Sql;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Info;

namespace NightlyCode.Database.Fields.Sql {

    /// <summary>
    /// field which references a column directly
    /// </summary>
    public class ColumnField : SqlField {

        /// <summary>
        /// creates a new <see cref="ColumnField"/>
        /// </summary>
        /// <param name="table">table/view/alias of which to reference column</param>
        /// <param name="name">name of column</param>
        public ColumnField(string table, string name) {
            Table = table;
            Name = name;
        }

        /// <summary>
        /// name of table
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// name of column
        /// </summary>
        public string Name { get; set; }

        /// <inheritdoc />
        public override void ToSql(IDBInfo dbinfo, IOperationPreparator preparator, Func<Type, EntityDescriptor> models, string tablealias) {
            preparator.AppendText($"{Table}.{dbinfo.MaskColumn(Name)}");
        }
    }
}