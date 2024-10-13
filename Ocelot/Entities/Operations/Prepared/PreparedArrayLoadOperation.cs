using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Extern;
using DataTable = Pooshit.Ocelot.Clients.Tables.DataTable;

namespace Pooshit.Ocelot.Entities.Operations.Prepared {

    /// <summary>
    /// <see cref="PreparedArrayLoadOperation"/> containing array parameters
    /// </summary>
    /// <summary>
    /// the db prepare parameter is not supported for this operation, as the operation changes with every parameter array length
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
            : base(dbclient, modelcache, commandtext, parameters, false) {
            ConstantArrayParameters = arrayparameters;
        }

        /// <summary>
        /// array parameters for command
        /// </summary>
        public Array[] ConstantArrayParameters { get; }

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
        public override async Task<IEnumerable<TScalar>> ExecuteSetAsync<TScalar>(Transaction transaction, params object[] parameters)
        {
            PreparedOperationData operation = PreparedOperationData.Create(DBClient,
                CommandText,
                ConstantParameters,
                ConstantArrayParameters,
                parameters.Where(p => !(p is Array)).ToArray(),
                parameters.OfType<Array>().ToArray());
            return (await DBClient.SetAsync(transaction, operation.Command, operation.Parameters)).Select(v => Converter.Convert<TScalar>(v, true)).ToArray();
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
            : base(dbclient, modelcache, commandtext, parameters, false) {
            ConstantArrayParameters = arrayparameters;
        }

        /// <summary>
        /// array parameters for command
        /// </summary>
        public Array[] ConstantArrayParameters { get; }

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
        public override async Task<IEnumerable<TScalar>> ExecuteSetAsync<TScalar>(Transaction transaction, params object[] parameters) {
            PreparedOperationData operation = PreparedOperationData.Create(DBClient,
                CommandText,
                ConstantParameters,
                ConstantArrayParameters,
                parameters.Where(p => !(p is Array)).ToArray(),
                parameters.OfType<Array>().ToArray());
            return (await DBClient.SetAsync(transaction, operation.Command, operation.Parameters)).Select(v => Converter.Convert<TScalar>(v, true)).ToArray();
        }

        /// <inheritdoc />
        public override Reader ExecuteReader(Transaction transaction, params object[] parameters) {
            PreparedOperationData operation = PreparedOperationData.Create(DBClient,
                CommandText,
                ConstantParameters,
                ConstantArrayParameters,
                parameters.Where(p => !(p is Array)).ToArray(),
                parameters.OfType<Array>().ToArray());
            return DBClient.Reader(transaction, operation.Command, operation.Parameters);
        }

        /// <inheritdoc />
        public override async Task<Reader> ExecuteReaderAsync(Transaction transaction, params object[] parameters) {
            PreparedOperationData operation = PreparedOperationData.Create(DBClient,
                CommandText,
                ConstantParameters,
                ConstantArrayParameters,
                parameters.Where(p => !(p is Array)).ToArray(),
                parameters.OfType<Array>().ToArray());
            return await DBClient.ReaderAsync(transaction, operation.Command, operation.Parameters);
        }
    }
}