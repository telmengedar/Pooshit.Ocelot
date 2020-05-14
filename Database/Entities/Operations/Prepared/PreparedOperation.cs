using System.Linq;
using System.Threading.Tasks;
using NightlyCode.Database.Clients;

namespace NightlyCode.Database.Entities.Operations.Prepared {

    /// <summary>
    /// a prepared db operation
    /// </summary>
    public class PreparedOperation {

        /// <summary>
        /// creates a new prepared operation
        /// </summary>
        /// <param name="dbclient">access to database</param>
        /// <param name="commandText">sql query text</param>
        /// <param name="constantparameters">parameters for query</param>
        public PreparedOperation(IDBClient dbclient, string commandText, object[] constantparameters) {
            DBClient = dbclient;
            CommandText = commandText;
            ConstantParameters = constantparameters;
        }

        /// <summary>
        /// access to database
        /// </summary>
        /// <remarks>
        /// this usually is used to execute the operation
        /// </remarks>
        protected IDBClient DBClient { get; }

        /// <summary>
        /// text to execute
        /// </summary>
        public string CommandText { get; }

        /// <summary>
        /// parameters for command
        /// </summary>
        public object[] ConstantParameters { get; }

        /// <summary>
        /// executes the operation using custom parameters
        /// </summary>
        /// <param name="parameters">parameters for operation</param>
        /// <returns>number of affected rows if applicable</returns>
        public virtual long Execute(params object[] parameters) {
            return Execute(null, parameters);
        }

        /// <summary>
        /// executes the operation using custom parameters
        /// </summary>
        /// <param name="transaction">transaction used to execute operation</param>
        /// <param name="parameters">parameters for operation</param>
        /// <returns>number of affected rows if applicable</returns>
        public virtual long Execute(Transaction transaction, params object[] parameters) {
            return DBClient.NonQuery(transaction, CommandText, ConstantParameters.Concat(parameters));
        }

        /// <summary>
        /// executes the operation using custom parameters
        /// </summary>
        /// <param name="parameters">parameters for operation</param>
        /// <returns>number of affected rows if applicable</returns>
        public virtual Task<long> ExecuteAsync(params object[] parameters) {
            return ExecuteAsync(null, parameters);
        }

        /// <summary>
        /// executes the operation using custom parameters
        /// </summary>
        /// <param name="transaction">transaction used to execute operation</param>
        /// <param name="parameters">parameters for operation</param>
        /// <returns>number of affected rows if applicable</returns>
        public virtual async Task<long> ExecuteAsync(Transaction transaction, params object[] parameters) {
            return await DBClient.NonQueryAsync(transaction, CommandText, ConstantParameters.Concat(parameters));
        }

        /// <inheritdoc/>
        public override string ToString() {
            return CommandText;
        }
    }
}