using System.Collections.Generic;
using System.Text;
using NightlyCode.DB.Info;

namespace NightlyCode.DB.Entities.Operations {

    /// <summary>
    /// preparator for operations
    /// </summary>
    public class OperationPreparator {
        readonly IDBInfo dbinfo;
        int indexer = 1;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="dbinfo"></param>
        public OperationPreparator(IDBInfo dbinfo) {
            this.dbinfo = dbinfo;
            CommandBuilder = new StringBuilder();
            Parameters = new List<DBParameter>();
        }

        /// <summary>
        /// appends a parameter without value to the command (specified later)
        /// </summary>
        public void AppendParameter() {
            CommandBuilder.Append(dbinfo.Parameter + indexer++);
        }

        /// <summary>
        /// appends a parameter to the command
        /// </summary>
        /// <param name="value"></param>
        public void AppendParameter(object value) {
            DBParameter parameter = new DBParameter(indexer++, value);
            CommandBuilder.Append(dbinfo.Parameter + parameter.Index);
            Parameters.Add(parameter);
        }

        /// <summary>
        /// command text
        /// </summary>
        public StringBuilder CommandBuilder { get; private set; }

        /// <summary>
        /// parameters
        /// </summary>
        public List<DBParameter> Parameters { get; private set; }

        public PreparedOperation GetOperation() {
            return new PreparedOperation(CommandBuilder.ToString(), Parameters.ToArray());
        }
    }
}