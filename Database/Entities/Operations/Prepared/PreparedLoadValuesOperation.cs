using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Clients.Tables;
using Converter = NightlyCode.Database.Extern.Converter;

namespace NightlyCode.Database.Entities.Operations.Prepared {

    /// <summary>
    /// a prepared load values operation
    /// </summary>
    public class PreparedLoadValuesOperation : PreparedOperation {

        /// <summary>
        /// creates a new <see cref="PreparedLoadValuesOperation"/>
        /// </summary>
        /// <param name="dbclient">database access</param>
        /// <param name="commandtext">command text</param>
        /// <param name="parameters">parameters for operation</param>
        public PreparedLoadValuesOperation(IDBClient dbclient, string commandtext, params object[] parameters)
            : base(dbclient, commandtext, parameters) { }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>data table containing results</returns>
        public new virtual DataTable Execute(params object[] parameters) {
            return Execute(null, parameters);
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <param name="transaction">transaction used to execute operation</param>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>data table containing results</returns>
        public new virtual DataTable Execute(Transaction transaction, params object[] parameters) {
            return DBClient.Query(transaction, CommandText, ConstantParameters.Concat(parameters));
        }

        /// <summary>
        /// executes the statement returning a scalar
        /// </summary>
        /// <returns>first value of the result set or default of TScalar</returns>
        public virtual TScalar ExecuteScalar<TScalar>(params object[] parameters) {
            return ExecuteScalar<TScalar>(null, parameters);
        }

        /// <summary>
        /// executes the statement returning a scalar
        /// </summary>
        /// <returns>first value of the result set or default of TScalar</returns>
        public virtual TScalar ExecuteScalar<TScalar>(Transaction transaction, params object[] parameters) {
            return Converter.Convert<TScalar>(DBClient.Scalar(transaction, CommandText, ConstantParameters.Concat(parameters)), true);
        }

        /// <summary>
        /// executes the statement returning a set of scalars
        /// </summary>
        /// <typeparam name="TScalar">type of scalar to return</typeparam>
        /// <returns>values of first column of result set converted to TScalar</returns>
        public virtual IEnumerable<TScalar> ExecuteSet<TScalar>(params object[] parameters) {
            return ExecuteSet<TScalar>(null, parameters);
        }

        /// <summary>
        /// executes the statement returning a set of scalars
        /// </summary>
        /// <typeparam name="TScalar">type of scalar to return</typeparam>
        /// <returns>values of first column of result set converted to TScalar</returns>
        public virtual IEnumerable<TScalar> ExecuteSet<TScalar>(Transaction transaction, params object[] parameters) {
            foreach(object value in DBClient.Set(transaction, CommandText, ConstantParameters.Concat(parameters)))
                yield return Converter.Convert<TScalar>(value, true);
        }

        /// <summary>
        /// executes a query and stores the result in a custom result type
        /// </summary>
        /// <typeparam name="TType">type of result</typeparam>
        /// <param name="assignments">action used to assign values</param>
        /// <param name="parameters">custom parameters for query execution</param>
        /// <returns>enumeration of result types</returns>
        public virtual IEnumerable<TType> ExecuteType<TType>(Func<DataRow, TType> assignments, params object[] parameters) {
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
        public virtual IEnumerable<TType> ExecuteType<TType>(Transaction transaction, Func<DataRow, TType> assignments, params object[] parameters) {
            DataTable table = Execute(transaction, parameters);
            return table.Rows.Select(assignments);
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>data table containing results</returns>
        public new virtual Task<DataTable> ExecuteAsync(params object[] parameters)
        {
            return ExecuteAsync(null, parameters);
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <param name="transaction">transaction used to execute operation</param>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>data table containing results</returns>
        public new virtual Task<DataTable> ExecuteAsync(Transaction transaction, params object[] parameters)
        {
            return DBClient.QueryAsync(transaction, CommandText, ConstantParameters.Concat(parameters));
        }

        /// <summary>
        /// executes the statement returning a scalar
        /// </summary>
        /// <returns>first value of the result set or default of TScalar</returns>
        public virtual Task<TScalar> ExecuteScalarAsync<TScalar>(params object[] parameters)
        {
            return ExecuteScalarAsync<TScalar>(null, parameters);
        }

        /// <summary>
        /// executes the statement returning a scalar
        /// </summary>
        /// <returns>first value of the result set or default of TScalar</returns>
        public virtual async Task<TScalar> ExecuteScalarAsync<TScalar>(Transaction transaction, params object[] parameters)
        {
            return Converter.Convert<TScalar>(await DBClient.ScalarAsync(transaction, CommandText, ConstantParameters.Concat(parameters)), true);
        }

        /// <summary>
        /// executes the statement returning a set of scalars
        /// </summary>
        /// <typeparam name="TScalar">type of scalar to return</typeparam>
        /// <returns>values of first column of result set converted to TScalar</returns>
        public virtual Task<TScalar[]> ExecuteSetAsync<TScalar>(params object[] parameters)
        {
            return ExecuteSetAsync<TScalar>(null, parameters);
        }

        /// <summary>
        /// executes the statement returning a set of scalars
        /// </summary>
        /// <typeparam name="TScalar">type of scalar to return</typeparam>
        /// <returns>values of first column of result set converted to TScalar</returns>
        public virtual async Task<TScalar[]> ExecuteSetAsync<TScalar>(Transaction transaction, params object[] parameters) {
            return (await DBClient.SetAsync(transaction, CommandText, ConstantParameters.Concat(parameters))).Select(v => Converter.Convert<TScalar>(v, true)).ToArray();
        }

        /// <summary>
        /// executes a query and stores the result in a custom result type
        /// </summary>
        /// <typeparam name="TType">type of result</typeparam>
        /// <param name="assignments">action used to assign values</param>
        /// <param name="parameters">custom parameters for query execution</param>
        /// <returns>enumeration of result types</returns>
        public virtual Task<TType[]> ExecuteTypeAsync<TType>(Func<DataRow, TType> assignments, params object[] parameters)
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
        public virtual async Task<TType[]> ExecuteTypeAsync<TType>(Transaction transaction, Func<DataRow, TType> assignments, params object[] parameters)
        {
            DataTable table = await ExecuteAsync(transaction, parameters);
            return table.Rows.Select(assignments).ToArray();
        }

    }
}