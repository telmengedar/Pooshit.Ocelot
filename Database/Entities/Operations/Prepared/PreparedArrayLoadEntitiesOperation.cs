using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;

namespace NightlyCode.Database.Entities.Operations.Prepared {

    /// <summary>
    /// a <see cref="PreparedLoadEntitiesOperation{T}"/> using array parameters
    /// </summary>
    /// <typeparam name="T">type of entities to load</typeparam>
    class PreparedArrayLoadEntitiesOperation<T> : PreparedLoadEntitiesOperation<T> {

        /// <summary>
        /// creates a new <see cref="PreparedArrayLoadEntitiesOperation{T}"/>
        /// </summary>
        /// <param name="dbclient">access to database used for execution of operation</param>
        /// <param name="descriptor">descriptor used to create entities</param>
        /// <param name="commandtext">command text to execute</param>
        /// <param name="parameters">initial parameters of operation</param>
        /// <param name="arrayparameters">array parameters of operation</param>
        public PreparedArrayLoadEntitiesOperation(IDBClient dbclient, EntityDescriptor descriptor, string commandtext, object[] parameters, Array[] arrayparameters)
            : base(dbclient, descriptor, commandtext, parameters) {
            ConstantArrayParameters = arrayparameters;
        }

        /// <summary>
        /// array parameters for command
        /// </summary>
        public Array[] ConstantArrayParameters { get; }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <returns>entities created from result set</returns>
        public override IEnumerable<T> Execute(params object[] parameters) {
            return Execute(null, parameters);
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <param name="transaction">transaction to use for execution</param>
        /// <param name="parameters">parameters to use for execution</param>
        /// <returns>entities created from result set</returns>
        public override IEnumerable<T> Execute(Transaction transaction, params object[] parameters) {
            PreparedOperationData operation = PreparedOperationData.Create(DBClient,
                CommandText,
                ConstantParameters,
                ConstantArrayParameters,
                parameters.Where(p => !(p is Array)).ToArray(),
                parameters.OfType<Array>().ToArray());
            Clients.Tables.DataTable data = DBClient.Query(transaction, operation.Command, operation.Parameters);
            return CreateObjects(data);
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <returns>entities created from result set</returns>
        public override Task<IEnumerable<T>> ExecuteAsync(params object[] parameters)
        {
            return ExecuteAsync(null, parameters);
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <param name="transaction">transaction to use for execution</param>
        /// <param name="parameters">parameters to use for execution</param>
        /// <returns>entities created from result set</returns>
        public override async Task<IEnumerable<T>> ExecuteAsync(Transaction transaction, params object[] parameters)
        {
            PreparedOperationData operation = PreparedOperationData.Create(DBClient,
                CommandText,
                ConstantParameters,
                ConstantArrayParameters,
                parameters.Where(p => !(p is Array)).ToArray(),
                parameters.OfType<Array>().ToArray());
            Clients.Tables.DataTable data = await DBClient.QueryAsync(transaction, operation.Command, operation.Parameters);
            return CreateObjects(data);
        }

    }
}