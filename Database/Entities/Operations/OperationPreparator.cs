using System.Collections.Generic;
using System.Text;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Info;

namespace NightlyCode.Database.Entities.Operations {

    /// <summary>
    /// preparator for operations
    /// </summary>
    public class OperationPreparator {
        readonly IDBInfo dbinfo;

        /// <summary>
        /// creates a new <see cref="OperationPreparator"/>
        /// </summary>
        /// <param name="dbinfo">db specific information</param>
        public OperationPreparator(IDBInfo dbinfo) {
            this.dbinfo = dbinfo;
            CommandBuilder = new StringBuilder();
            Parameters = new List<object>();
        }

        /// <summary>
        /// appends a parameter to the command
        /// </summary>
        /// <param name="value">value of parameter (optional)</param>
        public void AppendParameter(object value=null) {
            Parameters.Add(value);
            AppendParameterIndex(Parameters.Count);
        }

        /// <summary>
        /// appends a reference to a parameter index to the command
        /// </summary>
        /// <param name="index">index of parameter to reference</param>
        public void AppendParameterIndex(int index) {
            CommandBuilder.Append($"{dbinfo.Parameter}{index}");
        }
        /// <summary>
        /// command text
        /// </summary>
        public StringBuilder CommandBuilder { get; }

        /// <summary>
        /// parameters
        /// </summary>
        public List<object> Parameters { get; }

        /// <summary>
        /// create prepared operation
        /// </summary>
        /// <param name="dbclient">client used to execute operation</param>
        /// <returns>operation which can get executed</returns>
        public PreparedOperation GetOperation(IDBClient dbclient) {
            return new PreparedOperation(dbclient, CommandBuilder.ToString(), Parameters.ToArray());
        }
    }
}