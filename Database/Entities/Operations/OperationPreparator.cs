using System.Collections.Generic;
using System.Linq;
using System.Text;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Operations.Prepared;
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
            ArrayParameters = new List<object>();
        }

        /// <summary>
        /// appends an array parameter to the command
        /// </summary>
        /// <param name="value"></param>
        public void AppendArrayParameter(object value = null) {
            CommandBuilder.Append($"[{ArrayParameters.Count}]");
            ArrayParameters.Add(value);
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
        /// parameters containing arrays
        /// </summary>
        public List<object> ArrayParameters { get; }

        /// <summary>
        /// create prepared operation
        /// </summary>
        /// <param name="dbclient">client used to execute operation</param>
        /// <returns>operation which can get executed</returns>
        public PreparedOperation GetOperation(IDBClient dbclient) {
            if (ArrayParameters.Any())
                return new PreparedArrayOperation(dbclient, CommandBuilder.ToString(), Parameters.ToArray(), ArrayParameters.ToArray());
            return new PreparedOperation(dbclient, CommandBuilder.ToString(), Parameters.ToArray());
        }
    }
}