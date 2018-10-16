using NightlyCode.Database.Clients;

namespace NightlyCode.Database.Entities.Operations {

    /// <summary>
    /// a prepared db operation
    /// </summary>
    public class PreparedOperation {
        readonly IDBClient dbclient;

        /// <summary>
        /// creates a new prepared operation
        /// </summary>
        /// <param name="dbclient">access to database</param>
        /// <param name="commandText">sql query text</param>
        /// <param name="parameters">parameters for query</param>
        public PreparedOperation(IDBClient dbclient, string commandText, params object[] parameters) {
            this.dbclient = dbclient;
            CommandText = commandText;
            Parameters = parameters;
        }

        /// <summary>
        /// access to database
        /// </summary>
        /// <remarks>
        /// this usually is used to execute the operation
        /// </remarks>
        protected IDBClient DBClient => dbclient;

        /// <summary>
        /// text to execute
        /// </summary>
        public string CommandText { get; }

        /// <summary>
        /// parameters for command
        /// </summary>
        public object[] Parameters { get; }

        /// <summary>
        /// executes the operation
        /// </summary>
        /// <returns>number of affected rows if applicable</returns>
        public int Execute() {
            return DBClient.NonQuery(CommandText, Parameters);
        }

        /// <summary>
        /// executes the operation using custom parameters
        /// </summary>
        /// <param name="parameters">parameters for operation</param>
        /// <returns>number of affected rows if applicable</returns>
        public int Execute(params object[] parameters) {
            return DBClient.NonQuery(CommandText, parameters);
        }

        /// <summary>
        /// executes the operation using custom parameters
        /// </summary>
        /// <param name="transaction">transaction used to execute operation</param>
        /// <returns>number of affected rows if applicable</returns>
        public int Execute(Transaction transaction) {
            return DBClient.NonQuery(transaction, CommandText, Parameters);
        }

        /// <summary>
        /// executes the operation using custom parameters
        /// </summary>
        /// <param name="transaction">transaction used to execute operation</param>
        /// <param name="parameters">parameters for operation</param>
        /// <returns>number of affected rows if applicable</returns>
        public int Execute(Transaction transaction, params object[] parameters) {
            return DBClient.NonQuery(transaction, CommandText, parameters);
        }

        /// <inheritdoc/>
        public override string ToString() {
            return CommandText;
        }
    }
}