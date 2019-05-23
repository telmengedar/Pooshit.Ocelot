using System;
using System.Linq;
using System.Threading.Tasks;
using NightlyCode.Database.Clients;

namespace NightlyCode.Database.Entities.Operations.Prepared {

    /// <summary>
    /// <see cref="PreparedOperation"/> containing array parameters
    /// </summary>
    class PreparedArrayOperation : PreparedOperation {

        /// <summary>
        /// creates a new <see cref="PreparedArrayOperation"/>
        /// </summary>
        /// <param name="dbclient">access to database</param>
        /// <param name="commandText">sql query text</param>
        /// <param name="parameters">parameters for query</param>
        /// <param name="arrayparameters">array parameters for query</param>
        public PreparedArrayOperation(IDBClient dbclient, string commandText, object[] parameters, Array[] arrayparameters)
            : base(dbclient, commandText, parameters) {
            ConstantArrayParameters = arrayparameters;
        }

        /// <summary>
        /// array parameters for command
        /// </summary>
        public Array[] ConstantArrayParameters { get; }

        /// <summary>
        /// executes the operation using custom parameters
        /// </summary>
        /// <param name="parameters">parameters for operation</param>
        /// <returns>number of affected rows if applicable</returns>
        public override int Execute(params object[] parameters) {
            return Execute(null, parameters);
        }

        /// <summary>
        /// executes the operation using custom parameters
        /// </summary>
        /// <param name="transaction">transaction used to execute operation</param>
        /// <param name="parameters">parameters for operation</param>
        /// <returns>number of affected rows if applicable</returns>
        public override int Execute(Transaction transaction, params object[] parameters) {
            PreparedOperationData operation = PreparedOperationData.Create(DBClient,
                CommandText,
                ConstantParameters,
                ConstantArrayParameters,
                parameters.Where(p => !(p is Array)).ToArray(),
                parameters.OfType<Array>().ToArray());
            return DBClient.NonQuery(transaction, operation.Command, operation.Parameters);
        }

        /// <summary>
        /// executes the operation using custom parameters
        /// </summary>
        /// <param name="parameters">parameters for operation</param>
        /// <returns>number of affected rows if applicable</returns>
        public override Task<int> ExecuteAsync(params object[] parameters)
        {
            return ExecuteAsync(null, parameters);
        }

        /// <summary>
        /// executes the operation using custom parameters
        /// </summary>
        /// <param name="transaction">transaction used to execute operation</param>
        /// <param name="parameters">parameters for operation</param>
        /// <returns>number of affected rows if applicable</returns>
        public override Task<int> ExecuteAsync(Transaction transaction, params object[] parameters)
        {
            PreparedOperationData operation = PreparedOperationData.Create(DBClient,
                CommandText,
                ConstantParameters,
                ConstantArrayParameters,
                parameters.Where(p => !(p is Array)).ToArray(),
                parameters.OfType<Array>().ToArray());
            return DBClient.NonQueryAsync(transaction, operation.Command, operation.Parameters);
        }

    }
}