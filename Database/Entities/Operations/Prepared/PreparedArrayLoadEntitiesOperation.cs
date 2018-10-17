using System.Collections.Generic;
using System.Linq;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;

namespace NightlyCode.Database.Entities.Operations.Prepared {

    /// <summary>
    /// a <see cref="PreparedLoadEntitiesOperation{T}"/> using array parameters
    /// </summary>
    /// <typeparam name="T">type of entities to load</typeparam>
    public class PreparedArrayLoadEntitiesOperation<T> : PreparedLoadEntitiesOperation<T> {

        /// <summary>
        /// creates a new <see cref="PreparedArrayLoadEntitiesOperation{T}"/>
        /// </summary>
        /// <param name="dbclient">access to database used for execution of operation</param>
        /// <param name="descriptor">descriptor used to create entities</param>
        /// <param name="commandtext">command text to execute</param>
        /// <param name="parameters">initial parameters of operation</param>
        /// <param name="arrayparameters">array parameters of operation</param>
        public PreparedArrayLoadEntitiesOperation(IDBClient dbclient, EntityDescriptor descriptor, string commandtext, object[] parameters, object[] arrayparameters)
            : base(dbclient, descriptor, commandtext, parameters) {
            ArrayParameters = arrayparameters;
        }

        /// <summary>
        /// array parameters for command
        /// </summary>
        public object[] ArrayParameters { get; }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <returns>entities created from result set</returns>
        public override IEnumerable<T> Execute() {
            return Execute(Parameters.Concat(ArrayParameters).ToArray());
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <returns>entities created from result set</returns>
        public override IEnumerable<T> Execute(params object[] parameters) {
            PreparedOperationData operation = PreparedOperationData.Create(DBClient, CommandText, parameters);
            Clients.Tables.DataTable data = DBClient.Query(operation.Command, operation.Parameters);
            return CreateObjects(data);
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <param name="transaction">transaction to use for execution</param>
        /// <returns>entities created from result set</returns>
        public override IEnumerable<T> Execute(Transaction transaction) {
            return Execute(transaction, Parameters.Concat(ArrayParameters).ToArray());
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <param name="transaction">transaction to use for execution</param>
        /// <param name="parameters">parameters to use for execution</param>
        /// <returns>entities created from result set</returns>
        public override IEnumerable<T> Execute(Transaction transaction, params object[] parameters) {
            PreparedOperationData operation = PreparedOperationData.Create(DBClient, CommandText, parameters);
            Clients.Tables.DataTable data = DBClient.Query(transaction, operation.Command, operation.Parameters);
            return CreateObjects(data);
        }
    }
}