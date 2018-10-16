using System;
using System.Collections.Generic;
using NightlyCode.Database.Clients;
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
        /// <returns>first value of the result set or default of TScalar</returns>
        public TScalar ExecuteScalar<TScalar>()
        {
            return Converter.Convert<TScalar>(DBClient.Scalar(CommandText, Parameters), true);
        }

        /// <summary>
        /// executes the statement returning a scalar
        /// </summary>
        /// <returns>first value of the result set or default of TScalar</returns>
        public TScalar ExecuteScalar<TScalar>(params object[] parameters)
        {
            return Converter.Convert<TScalar>(DBClient.Scalar(CommandText, parameters), true);
        }

        /// <summary>
        /// executes the statement returning a scalar
        /// </summary>
        /// <returns>first value of the result set or default of TScalar</returns>
        public TScalar ExecuteScalar<TScalar>(Transaction transaction)
        {
            return Converter.Convert<TScalar>(DBClient.Scalar(transaction, CommandText, Parameters), true);
        }

        /// <summary>
        /// executes the statement returning a scalar
        /// </summary>
        /// <returns>first value of the result set or default of TScalar</returns>
        public TScalar ExecuteScalar<TScalar>(Transaction transaction, params object[] parameters) {
            return Converter.Convert<TScalar>(DBClient.Scalar(transaction, CommandText, parameters), true);
        }

        /// <summary>
        /// executes the statement returning a set of scalars
        /// </summary>
        /// <typeparam name="TScalar">type of scalar to return</typeparam>
        /// <returns>values of first column of result set converted to TScalar</returns>
        public IEnumerable<TScalar> ExecuteSet<TScalar>()
        {
            foreach (object value in DBClient.Set(CommandText, Parameters))
                yield return Converter.Convert<TScalar>(value, true);
        }

        /// <summary>
        /// executes the statement returning a set of scalars
        /// </summary>
        /// <typeparam name="TScalar">type of scalar to return</typeparam>
        /// <returns>values of first column of result set converted to TScalar</returns>
        public IEnumerable<TScalar> ExecuteSet<TScalar>(params object[] parameters)
        {
            foreach (object value in DBClient.Set(CommandText, parameters))
                yield return Converter.Convert<TScalar>(value, true);
        }

        /// <summary>
        /// executes the statement returning a set of scalars
        /// </summary>
        /// <typeparam name="TScalar">type of scalar to return</typeparam>
        /// <returns>values of first column of result set converted to TScalar</returns>
        public IEnumerable<TScalar> ExecuteSet<TScalar>(Transaction transaction)
        {
            foreach (object value in DBClient.Set(transaction, CommandText, Parameters))
                yield return Converter.Convert<TScalar>(value, true);
        }

        /// <summary>
        /// executes the statement returning a set of scalars
        /// </summary>
        /// <typeparam name="TScalar">type of scalar to return</typeparam>
        /// <returns>values of first column of result set converted to TScalar</returns>
        public IEnumerable<TScalar> ExecuteSet<TScalar>(Transaction transaction, params object[] parameters)
        {
            foreach (object value in DBClient.Set(transaction, CommandText, parameters))
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

        /// <summary>
        /// executes a query and stores the result in a custom result type
        /// </summary>
        /// <typeparam name="TType">type of result</typeparam>
        /// <param name="assignments">action used to assign values</param>
        /// <param name="parameters">custom parameters for query execution</param>
        /// <returns>enumeration of result types</returns>
        public IEnumerable<TType> ExecuteType<TType>(Action<Clients.Tables.DataRow, TType> assignments, params object[] parameters)
            where TType : new()
        {
            Clients.Tables.DataTable table = Execute(parameters);
            foreach (Clients.Tables.DataRow row in table.Rows)
            {
                TType type = new TType();
                assignments(row, type);
                yield return type;
            }
        }

        /// <summary>
        /// executes a query and stores the result in a custom result type
        /// </summary>
        /// <typeparam name="TType">type of result</typeparam>
        /// <param name="assignments">action used to assign values</param>
        /// <param name="transaction">transaction to use for execution</param>
        /// <returns>enumeration of result types</returns>
        public IEnumerable<TType> ExecuteType<TType>(Action<Clients.Tables.DataRow, TType> assignments, Transaction transaction)
            where TType : new()
        {
            Clients.Tables.DataTable table = Execute(transaction);
            foreach (Clients.Tables.DataRow row in table.Rows)
            {
                TType type = new TType();
                assignments(row, type);
                yield return type;
            }
        }

        /// <summary>
        /// executes a query and stores the result in a custom result type
        /// </summary>
        /// <typeparam name="TType">type of result</typeparam>
        /// <param name="assignments">action used to assign values</param>
        /// <param name="transaction">transaction to use for execution</param>
        /// <param name="parameters">custom parameters for query execution</param>
        /// <returns>enumeration of result types</returns>
        public IEnumerable<TType> ExecuteType<TType>(Action<Clients.Tables.DataRow, TType> assignments, Transaction transaction, params object[] parameters)
            where TType : new()
        {
            Clients.Tables.DataTable table = Execute(parameters);
            foreach (Clients.Tables.DataRow row in table.Rows)
            {
                TType type = new TType();
                assignments(row, type);
                yield return type;
            }
        }

    }
}