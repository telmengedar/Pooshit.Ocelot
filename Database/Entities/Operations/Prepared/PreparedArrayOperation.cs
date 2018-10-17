using System.Linq;
using NightlyCode.Database.Clients;

namespace NightlyCode.Database.Entities.Operations.Prepared {

    /// <summary>
    /// <see cref="PreparedOperation"/> containing array parameters
    /// </summary>
    public class PreparedArrayOperation : PreparedOperation {

        /// <summary>
        /// creates a new <see cref="PreparedArrayOperation"/>
        /// </summary>
        /// <param name="dbclient">access to database</param>
        /// <param name="commandText">sql query text</param>
        /// <param name="parameters">parameters for query</param>
        /// <param name="arrayparameters">array parameters for query</param>
        public PreparedArrayOperation(IDBClient dbclient, string commandText, object[] parameters, object[] arrayparameters)
            : base(dbclient, commandText, parameters) {
            ArrayParameters = arrayparameters;
        }

        /// <summary>
        /// array parameters for command
        /// </summary>
        public object[] ArrayParameters { get; }

        /// <summary>
        /// executes the operation
        /// </summary>
        /// <returns>number of affected rows if applicable</returns>
        public override int Execute() {
            return Execute(Parameters.Concat(ArrayParameters).ToArray());
        }

        /// <summary>
        /// executes the operation using custom parameters
        /// </summary>
        /// <param name="parameters">parameters for operation</param>
        /// <returns>number of affected rows if applicable</returns>
        public override int Execute(params object[] parameters) {
            PreparedOperationData operation = PreparedOperationData.Create(DBClient, CommandText, parameters);
            return DBClient.NonQuery(operation.Command, operation.Parameters);
        }

        /// <summary>
        /// executes the operation using custom parameters
        /// </summary>
        /// <param name="transaction">transaction used to execute operation</param>
        /// <returns>number of affected rows if applicable</returns>
        public override int Execute(Transaction transaction) {
            return Execute(transaction, Parameters.Concat(ArrayParameters).ToArray());
        }

        /// <summary>
        /// executes the operation using custom parameters
        /// </summary>
        /// <param name="transaction">transaction used to execute operation</param>
        /// <param name="parameters">parameters for operation</param>
        /// <returns>number of affected rows if applicable</returns>
        public override int Execute(Transaction transaction, params object[] parameters) {
            PreparedOperationData operation = PreparedOperationData.Create(DBClient, CommandText, parameters);
            return DBClient.NonQuery(transaction, operation.Command, operation.Parameters);
        }
    }
}