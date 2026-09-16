using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Entities.Operations.Tables {

    /// <summary>
    /// field to be loaded
    /// </summary>
    public class DataField {

        DataField(string name, bool isColumn) {
            Name = name;
            IsColumn = isColumn;
        }

        /// <summary>
        /// creates a new <see cref="DataField"/> referencing a column
        /// </summary>
        /// <param name="name">name of the column</param>
        public DataField(string name)
            : this(IdentifierGuard.Simple(name, "column", "use DataField.Raw(...) to emit a sql expression verbatim"), true) {
        }

        /// <summary>
        /// creates a <see cref="DataField"/> emitting <paramref name="sql"/> verbatim
        /// </summary>
        /// <param name="sql">sql expression to emit as-is; must not carry caller-supplied input</param>
        /// <returns>raw data field</returns>
        public static DataField Raw(string sql) {
            return new(sql, false);
        }

        /// <summary>
        /// name of column or raw sql expression
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// determines whether name is masked as column
        /// </summary>
        public bool IsColumn { get; }
    }
}