using System;
using System.Collections.Generic;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Operations;
using Converter = NightlyCode.Database.Extern.Converter;

namespace NightlyCode.Database.Entities {
    /// <summary>
    /// a prepared load values operation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PreparedLoadValuesOperation<T> : PreparedOperation {
        /// <summary>
        /// creates a new <see cref="PreparedLoadValuesOperation{T}"/>
        /// </summary>
        /// <param name="dbclient">database access</param>
        /// <param name="commandtext">command text</param>
        /// <param name="parameters">parameters for operation</param>
        public PreparedLoadValuesOperation(IDBClient dbclient, string commandtext, params object[] parameters)
            : base(dbclient, commandtext, parameters) { }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <returns>data table containing results</returns>
        public new Clients.Tables.DataTable Execute()
        {
            return DBClient.Query(CommandText, Parameters);
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>data table containing results</returns>
        public new Clients.Tables.DataTable Execute(params object[] parameters)
        {
            return DBClient.Query(CommandText, parameters);
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <param name="transaction">transaction used to execute operation</param>
        /// <returns>data table containing results</returns>
        public new Clients.Tables.DataTable Execute(Transaction transaction)
        {
            return DBClient.Query(transaction, CommandText, Parameters);
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <param name="transaction">transaction used to execute operation</param>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>data table containing results</returns>
        public new Clients.Tables.DataTable Execute(Transaction transaction, params object[] parameters)
        {
            return DBClient.Query(transaction, CommandText, parameters);
        }

        /// <summary>
        /// executes the statement returning a scalar
        /// </summary>
        /// <returns></returns>
        public TScalar ExecuteScalar<TScalar>(Transaction transaction)
        {
            return Converter.Convert<TScalar>(DBClient.Scalar(transaction, CommandText, Parameters), true);
        }

        /// <summary>
        /// executes the statement returning a scalar
        /// </summary>
        /// <returns></returns>
        public TScalar ExecuteScalar<TScalar>() {
            return Converter.Convert<TScalar>(DBClient.Scalar(CommandText, Parameters), true);
        }

        /// <summary>
        /// executes the statement returning a scalar
        /// </summary>
        /// <returns></returns>
        public TScalar ExecuteScalar<TScalar>(Transaction transaction, params object[] parameters) {
            return Converter.Convert<TScalar>(DBClient.Scalar(transaction, CommandText, parameters), true);
        }

        /// <summary>
        /// executes the statement returning a scalar
        /// </summary>
        /// <returns></returns>
        public TScalar ExecuteScalar<TScalar>(params object[] parameters)
        {
            return Converter.Convert<TScalar>(DBClient.Scalar(CommandText, parameters), true);
        }

        public IEnumerable<TScalar> ExecuteSet<TScalar>(Transaction transaction, params object[] parameters)
        {
            foreach (object value in DBClient.Set(transaction, CommandText, parameters))
                yield return Converter.Convert<TScalar>(value, true);
        }

        public IEnumerable<TScalar> ExecuteSet<TScalar>(params object[] parameters) {
            foreach (object value in DBClient.Set(CommandText, parameters))
                yield return Converter.Convert<TScalar>(value, true);
        }

        /// <summary>
        /// executes a query and stores the result in a custom result type
        /// </summary>
        /// <typeparam name="TType">type of result</typeparam>
        /// <param name="assignments">action used to assign values</param>
        /// <returns>enumeration of result types</returns>
        public IEnumerable<TType> ExecuteType<TType>(Action<Clients.Tables.DataRow, TType> assignments)
            where TType : new()
        {
            Clients.Tables.DataTable table = Execute();
            foreach(Clients.Tables.DataRow row in table.Rows) {
                TType type = new TType();
                assignments(row, type);
                yield return type;
            }
        }
    }
}