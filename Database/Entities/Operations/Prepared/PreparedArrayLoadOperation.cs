﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Clients.Tables;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Extern;

namespace NightlyCode.Database.Entities.Operations.Prepared {

    /// <summary>
    /// <see cref="PreparedArrayLoadOperation"/> containing array parameters
    /// </summary>
    class PreparedArrayLoadOperation : PreparedLoadOperation {

        /// <summary>
        /// creates a new <see cref="PreparedArrayLoadOperation"/>
        /// </summary>
        /// <param name="dbclient">database access</param>
        /// <param name="modelcache">access to entity model information</param>
        /// <param name="commandtext">command text</param>
        /// <param name="parameters">parameters for operation</param>
        /// <param name="arrayparameters">array parameters for operation</param>
        public PreparedArrayLoadOperation(IDBClient dbclient, Func<Type, EntityDescriptor> modelcache, string commandtext, object[] parameters, Array[] arrayparameters)
            : base(dbclient, modelcache, commandtext, parameters) {
            ConstantArrayParameters = arrayparameters;
        }

        /// <summary>
        /// array parameters for command
        /// </summary>
        public Array[] ConstantArrayParameters { get; }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>data table containing results</returns>
        public override DataTable Execute(params object[] parameters) {
            return Execute(null, parameters);
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <param name="transaction">transaction used to execute operation</param>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>data table containing results</returns>
        public override DataTable Execute(Transaction transaction, params object[] parameters) {
            PreparedOperationData operation = PreparedOperationData.Create(DBClient,
                CommandText,
                ConstantParameters,
                ConstantArrayParameters,
                parameters.Where(p => !(p is Array)).ToArray(),
                parameters.OfType<Array>().ToArray());
            return DBClient.Query(transaction, operation.Command, operation.Parameters);
        }

        /// <summary>
        /// executes the statement returning a scalar
        /// </summary>
        /// <returns>first value of the result set or default of TScalar</returns>
        public override TScalar ExecuteScalar<TScalar>(params object[] parameters) {
            return ExecuteScalar<TScalar>(null, parameters);
        }

        /// <summary>
        /// executes the statement returning a scalar
        /// </summary>
        /// <returns>first value of the result set or default of TScalar</returns>
        public override TScalar ExecuteScalar<TScalar>(Transaction transaction, params object[] parameters) {
            PreparedOperationData operation = PreparedOperationData.Create(DBClient,
                CommandText,
                ConstantParameters,
                ConstantArrayParameters,
                parameters.Where(p => !(p is Array)).ToArray(),
                parameters.OfType<Array>().ToArray());
            return Converter.Convert<TScalar>(DBClient.Scalar(transaction, operation.Command, operation.Parameters), true);
        }

        /// <summary>
        /// executes the statement returning a set of scalars
        /// </summary>
        /// <typeparam name="TScalar">type of scalar to return</typeparam>
        /// <returns>values of first column of result set converted to TScalar</returns>
        public override IEnumerable<TScalar> ExecuteSet<TScalar>(params object[] parameters) {
            return ExecuteSet<TScalar>(null, parameters);
        }

        /// <summary>
        /// executes the statement returning a set of scalars
        /// </summary>
        /// <typeparam name="TScalar">type of scalar to return</typeparam>
        /// <returns>values of first column of result set converted to TScalar</returns>
        public override IEnumerable<TScalar> ExecuteSet<TScalar>(Transaction transaction, params object[] parameters) {
            PreparedOperationData operation = PreparedOperationData.Create(DBClient,
                CommandText,
                ConstantParameters,
                ConstantArrayParameters,
                parameters.Where(p => !(p is Array)).ToArray(),
                parameters.OfType<Array>().ToArray());
            foreach (object value in DBClient.Set(transaction, operation.Command, operation.Parameters))
                yield return Converter.Convert<TScalar>(value, true);
        }

        /// <summary>
        /// executes a query and stores the result in a custom result type
        /// </summary>
        /// <typeparam name="TType">type of result</typeparam>
        /// <param name="assignments">action used to assign values</param>
        /// <param name="parameters">custom parameters for query execution</param>
        /// <returns>enumeration of result types</returns>
        public override IEnumerable<TType> ExecuteType<TType>(Func<DataRow, TType> assignments, params object[] parameters) {
            return ExecuteType(null, assignments, parameters);
        }

        /// <summary>
        /// executes a query and stores the result in a custom result type
        /// </summary>
        /// <typeparam name="TType">type of result</typeparam>
        /// <param name="assignments">action used to assign values</param>
        /// <param name="transaction">transaction to use for execution</param>
        /// <param name="parameters">custom parameters for query execution</param>
        /// <returns>enumeration of result types</returns>
        public override IEnumerable<TType> ExecuteType<TType>(Transaction transaction, Func<DataRow, TType> assignments, params object[] parameters) {
            DataTable table = Execute(transaction, parameters);
            return table.Rows.Select(assignments);
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>data table containing results</returns>
        public override Task<DataTable> ExecuteAsync(params object[] parameters)
        {
            return ExecuteAsync(null, parameters);
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <param name="transaction">transaction used to execute operation</param>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>data table containing results</returns>
        public override Task<DataTable> ExecuteAsync(Transaction transaction, params object[] parameters)
        {
            PreparedOperationData operation = PreparedOperationData.Create(DBClient,
                CommandText,
                ConstantParameters,
                ConstantArrayParameters,
                parameters.Where(p => !(p is Array)).ToArray(),
                parameters.OfType<Array>().ToArray());
            return DBClient.QueryAsync(transaction, operation.Command, operation.Parameters);
        }

        /// <summary>
        /// executes the statement returning a scalar
        /// </summary>
        /// <returns>first value of the result set or default of TScalar</returns>
        public override Task<TScalar> ExecuteScalarAsync<TScalar>(params object[] parameters)
        {
            return ExecuteScalarAsync<TScalar>(null, parameters);
        }

        /// <summary>
        /// executes the statement returning a scalar
        /// </summary>
        /// <returns>first value of the result set or default of TScalar</returns>
        public override async Task<TScalar> ExecuteScalarAsync<TScalar>(Transaction transaction, params object[] parameters)
        {
            PreparedOperationData operation = PreparedOperationData.Create(DBClient,
                CommandText,
                ConstantParameters,
                ConstantArrayParameters,
                parameters.Where(p => !(p is Array)).ToArray(),
                parameters.OfType<Array>().ToArray());
            return Converter.Convert<TScalar>(await DBClient.ScalarAsync(transaction, operation.Command, operation.Parameters), true);
        }

        /// <summary>
        /// executes the statement returning a set of scalars
        /// </summary>
        /// <typeparam name="TScalar">type of scalar to return</typeparam>
        /// <returns>values of first column of result set converted to TScalar</returns>
        public override Task<TScalar[]> ExecuteSetAsync<TScalar>(params object[] parameters)
        {
            return ExecuteSetAsync<TScalar>(null, parameters);
        }

        /// <summary>
        /// executes the statement returning a set of scalars
        /// </summary>
        /// <typeparam name="TScalar">type of scalar to return</typeparam>
        /// <returns>values of first column of result set converted to TScalar</returns>
        public override async Task<TScalar[]> ExecuteSetAsync<TScalar>(Transaction transaction, params object[] parameters)
        {
            PreparedOperationData operation = PreparedOperationData.Create(DBClient,
                CommandText,
                ConstantParameters,
                ConstantArrayParameters,
                parameters.Where(p => !(p is Array)).ToArray(),
                parameters.OfType<Array>().ToArray());
            return (await DBClient.SetAsync(transaction, operation.Command, operation.Parameters)).Select(v => Converter.Convert<TScalar>(v, true)).ToArray();
        }

        /// <summary>
        /// executes a query and stores the result in a custom result type
        /// </summary>
        /// <typeparam name="TType">type of result</typeparam>
        /// <param name="assignments">action used to assign values</param>
        /// <param name="parameters">custom parameters for query execution</param>
        /// <returns>enumeration of result types</returns>
        public override Task<TType[]> ExecuteTypeAsync<TType>(Func<DataRow, TType> assignments, params object[] parameters)
        {
            return ExecuteTypeAsync(null, assignments, parameters);
        }

        /// <summary>
        /// executes a query and stores the result in a custom result type
        /// </summary>
        /// <typeparam name="TType">type of result</typeparam>
        /// <param name="assignments">action used to assign values</param>
        /// <param name="transaction">transaction to use for execution</param>
        /// <param name="parameters">custom parameters for query execution</param>
        /// <returns>enumeration of result types</returns>
        public override async Task<TType[]> ExecuteTypeAsync<TType>(Transaction transaction, Func<DataRow, TType> assignments, params object[] parameters)
        {
            DataTable table = await ExecuteAsync(transaction, parameters);
            return table.Rows.Select(assignments).ToArray();
        }
    }

    class PreparedArrayLoadOperation<T> : PreparedLoadOperation<T> {

        /// <summary>
        /// creates a new <see cref="PreparedArrayLoadOperation"/>
        /// </summary>
        /// <param name="dbclient">database access</param>
        /// <param name="modelcache">access to entity model information</param>
        /// <param name="commandtext">command text</param>
        /// <param name="parameters">parameters for operation</param>
        /// <param name="arrayparameters">array parameters for operation</param>
        public PreparedArrayLoadOperation(IDBClient dbclient, Func<Type, EntityDescriptor> modelcache, string commandtext, object[] parameters, Array[] arrayparameters)
            : base(dbclient, modelcache, commandtext, parameters) {
            ConstantArrayParameters = arrayparameters;
        }

        /// <summary>
        /// array parameters for command
        /// </summary>
        public Array[] ConstantArrayParameters { get; }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>data table containing results</returns>
        public override DataTable Execute(params object[] parameters) {
            return Execute(null, parameters);
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <param name="transaction">transaction used to execute operation</param>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>data table containing results</returns>
        public override DataTable Execute(Transaction transaction, params object[] parameters) {
            PreparedOperationData operation = PreparedOperationData.Create(DBClient,
                CommandText,
                ConstantParameters,
                ConstantArrayParameters,
                parameters.Where(p => !(p is Array)).ToArray(),
                parameters.OfType<Array>().ToArray());
            return DBClient.Query(transaction, operation.Command, operation.Parameters);
        }

        /// <summary>
        /// executes the statement returning a scalar
        /// </summary>
        /// <returns>first value of the result set or default of TScalar</returns>
        public override TScalar ExecuteScalar<TScalar>(params object[] parameters) {
            return ExecuteScalar<TScalar>(null, parameters);
        }

        /// <summary>
        /// executes the statement returning a scalar
        /// </summary>
        /// <returns>first value of the result set or default of TScalar</returns>
        public override TScalar ExecuteScalar<TScalar>(Transaction transaction, params object[] parameters) {
            PreparedOperationData operation = PreparedOperationData.Create(DBClient,
                CommandText,
                ConstantParameters,
                ConstantArrayParameters,
                parameters.Where(p => !(p is Array)).ToArray(),
                parameters.OfType<Array>().ToArray());
            return Converter.Convert<TScalar>(DBClient.Scalar(transaction, operation.Command, operation.Parameters), true);
        }

        /// <summary>
        /// executes the statement returning a set of scalars
        /// </summary>
        /// <typeparam name="TScalar">type of scalar to return</typeparam>
        /// <returns>values of first column of result set converted to TScalar</returns>
        public override IEnumerable<TScalar> ExecuteSet<TScalar>(params object[] parameters) {
            return ExecuteSet<TScalar>(null, parameters);
        }

        /// <summary>
        /// executes the statement returning a set of scalars
        /// </summary>
        /// <typeparam name="TScalar">type of scalar to return</typeparam>
        /// <returns>values of first column of result set converted to TScalar</returns>
        public override IEnumerable<TScalar> ExecuteSet<TScalar>(Transaction transaction, params object[] parameters) {
            PreparedOperationData operation = PreparedOperationData.Create(DBClient,
                CommandText,
                ConstantParameters,
                ConstantArrayParameters,
                parameters.Where(p => !(p is Array)).ToArray(),
                parameters.OfType<Array>().ToArray());
            foreach(object value in DBClient.Set(transaction, operation.Command, operation.Parameters))
                yield return Converter.Convert<TScalar>(value, true);
        }

        /// <summary>
        /// executes a query and stores the result in a custom result type
        /// </summary>
        /// <typeparam name="TType">type of result</typeparam>
        /// <param name="assignments">action used to assign values</param>
        /// <param name="parameters">custom parameters for query execution</param>
        /// <returns>enumeration of result types</returns>
        public override IEnumerable<TType> ExecuteType<TType>(Func<DataRow, TType> assignments, params object[] parameters) {
            return ExecuteType(null, assignments, parameters);
        }

        /// <summary>
        /// executes a query and stores the result in a custom result type
        /// </summary>
        /// <typeparam name="TType">type of result</typeparam>
        /// <param name="assignments">action used to assign values</param>
        /// <param name="transaction">transaction to use for execution</param>
        /// <param name="parameters">custom parameters for query execution</param>
        /// <returns>enumeration of result types</returns>
        public override IEnumerable<TType> ExecuteType<TType>(Transaction transaction, Func<DataRow, TType> assignments, params object[] parameters) {
            DataTable table = Execute(transaction, parameters);
            return table.Rows.Select(assignments);
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>data table containing results</returns>
        public override Task<DataTable> ExecuteAsync(params object[] parameters) {
            return ExecuteAsync(null, parameters);
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <param name="transaction">transaction used to execute operation</param>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>data table containing results</returns>
        public override Task<DataTable> ExecuteAsync(Transaction transaction, params object[] parameters) {
            PreparedOperationData operation = PreparedOperationData.Create(DBClient,
                CommandText,
                ConstantParameters,
                ConstantArrayParameters,
                parameters.Where(p => !(p is Array)).ToArray(),
                parameters.OfType<Array>().ToArray());
            return DBClient.QueryAsync(transaction, operation.Command, operation.Parameters);
        }

        /// <summary>
        /// executes the statement returning a scalar
        /// </summary>
        /// <returns>first value of the result set or default of TScalar</returns>
        public override Task<TScalar> ExecuteScalarAsync<TScalar>(params object[] parameters) {
            return ExecuteScalarAsync<TScalar>(null, parameters);
        }

        /// <summary>
        /// executes the statement returning a scalar
        /// </summary>
        /// <returns>first value of the result set or default of TScalar</returns>
        public override async Task<TScalar> ExecuteScalarAsync<TScalar>(Transaction transaction, params object[] parameters) {
            PreparedOperationData operation = PreparedOperationData.Create(DBClient,
                CommandText,
                ConstantParameters,
                ConstantArrayParameters,
                parameters.Where(p => !(p is Array)).ToArray(),
                parameters.OfType<Array>().ToArray());
            return Converter.Convert<TScalar>(await DBClient.ScalarAsync(transaction, operation.Command, operation.Parameters), true);
        }

        /// <summary>
        /// executes the statement returning a set of scalars
        /// </summary>
        /// <typeparam name="TScalar">type of scalar to return</typeparam>
        /// <returns>values of first column of result set converted to TScalar</returns>
        public override Task<TScalar[]> ExecuteSetAsync<TScalar>(params object[] parameters) {
            return ExecuteSetAsync<TScalar>(null, parameters);
        }

        /// <summary>
        /// executes the statement returning a set of scalars
        /// </summary>
        /// <typeparam name="TScalar">type of scalar to return</typeparam>
        /// <returns>values of first column of result set converted to TScalar</returns>
        public override async Task<TScalar[]> ExecuteSetAsync<TScalar>(Transaction transaction, params object[] parameters) {
            PreparedOperationData operation = PreparedOperationData.Create(DBClient,
                CommandText,
                ConstantParameters,
                ConstantArrayParameters,
                parameters.Where(p => !(p is Array)).ToArray(),
                parameters.OfType<Array>().ToArray());
            return (await DBClient.SetAsync(transaction, operation.Command, operation.Parameters)).Select(v => Converter.Convert<TScalar>(v, true)).ToArray();
        }

        /// <summary>
        /// executes a query and stores the result in a custom result type
        /// </summary>
        /// <typeparam name="TType">type of result</typeparam>
        /// <param name="assignments">action used to assign values</param>
        /// <param name="parameters">custom parameters for query execution</param>
        /// <returns>enumeration of result types</returns>
        public override Task<TType[]> ExecuteTypeAsync<TType>(Func<DataRow, TType> assignments, params object[] parameters) {
            return ExecuteTypeAsync(null, assignments, parameters);
        }

        /// <summary>
        /// executes a query and stores the result in a custom result type
        /// </summary>
        /// <typeparam name="TType">type of result</typeparam>
        /// <param name="assignments">action used to assign values</param>
        /// <param name="transaction">transaction to use for execution</param>
        /// <param name="parameters">custom parameters for query execution</param>
        /// <returns>enumeration of result types</returns>
        public override async Task<TType[]> ExecuteTypeAsync<TType>(Transaction transaction, Func<DataRow, TType> assignments, params object[] parameters) {
            DataTable table = await ExecuteAsync(transaction, parameters);
            return table.Rows.Select(assignments).ToArray();
        }

        /// <summary>
        /// executes the operation and creates entities from the result
        /// </summary>
        /// <typeparam name="T">type of entities to create</typeparam>
        /// <param name="transaction">transaction to use</param>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>created entities</returns>
        public override IEnumerable<T> ExecuteEntities(Transaction transaction, params object[] parameters) {
            DataTable table = Execute(transaction, parameters);
            return CreateObjects<T>(table);
        }

        /// <summary>
        /// executes the operation and creates entities from the result
        /// </summary>
        /// <typeparam name="T">type of entities to create</typeparam>
        /// <param name="transaction">transaction to use</param>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>created entities</returns>
        public override T ExecuteEntity(Transaction transaction, params object[] parameters) {
            DataTable table = Execute(transaction, parameters);
            if(table.Rows.Length == 0)
                return default;

            return ToObject<T>(table.Rows.First(), table.Columns, EntityDescriptor.Create(typeof(T)));
        }

        /// <summary>
        /// executes the operation and creates entities from the result
        /// </summary>
        /// <typeparam name="T">type of entities to create</typeparam>
        /// <param name="transaction">transaction to use</param>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>created entities</returns>
        public override async Task<T> ExecuteEntityAsync(Transaction transaction, params object[] parameters) {
            DataTable table = await ExecuteAsync(transaction, parameters);
            if(table.Rows.Length == 0)
                return default;

            return ToObject<T>(table.Rows.First(), table.Columns, EntityDescriptor.Create(typeof(T)));
        }

        /// <summary>
        /// executes the operation and creates entities from the result
        /// </summary>
        /// <typeparam name="T">type of entities to create</typeparam>
        /// <param name="transaction">transaction to use</param>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>created entities</returns>
        public override async Task<T[]> ExecuteEntitiesAsync(Transaction transaction, params object[] parameters) {
            DataTable table = await ExecuteAsync(transaction, parameters);
            return await CreateObjectsAsync<T>(table);
        }

        /// <summary>
        /// executes the operation and creates entities from the result
        /// </summary>
        /// <typeparam name="T">type of entities to create</typeparam>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>created entities</returns>
        public override Task<T[]> ExecuteEntitiesAsync(params object[] parameters) {
            return ExecuteEntitiesAsync<T>(null, parameters);
        }

        /// <summary>
        /// executes the operation and creates entities from the result
        /// </summary>
        /// <typeparam name="T">type of entities to create</typeparam>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>created entities</returns>
        public override IEnumerable<T> ExecuteEntities(params object[] parameters) {
            return ExecuteEntities<T>(null, parameters);
        }

        /// <summary>
        /// executes the operation and creates entities from the result
        /// </summary>
        /// <typeparam name="T">type of entities to create</typeparam>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>created entities</returns>
        public override Task<T> ExecuteEntityAsync(params object[] parameters) {
            return ExecuteEntityAsync<T>(null, parameters);
        }

        /// <summary>
        /// executes the operation and creates entities from the result
        /// </summary>
        /// <typeparam name="T">type of entities to create</typeparam>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>created entities</returns>
        public override T ExecuteEntity(params object[] parameters) {
            return ExecuteEntity<T>(null, parameters);
        }

    }
}