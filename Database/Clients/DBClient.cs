using System;
using System.Collections.Generic;
using System.Data;
using NightlyCode.Database.Info;

namespace NightlyCode.Database.Clients
{

    /// <summary>
    /// client to execute database commands
    /// </summary>
    public class DBClient : IDBClient {
        readonly IDbConnection connection;
        readonly IDBInfo dbinfo;
        readonly object connectionlock = new object();

        /// <summary>
        /// creates a new <see cref="DBClient"/>
        /// </summary>
        /// <param name="connection">connection to database</param>
        /// <param name="dbinfo">information for database statements</param>
        public DBClient(IDbConnection connection, IDBInfo dbinfo)
        {
            this.connection = connection;
            this.dbinfo = dbinfo;
        }

        /// <summary>
        /// database information
        /// </summary>
        public IDBInfo DBInfo => dbinfo;

        IDbCommand PrepareCommand(string commandtext, IEnumerable<object> parameters)
        {
            IDbCommand command = connection.CreateCommand();
            command.CommandText = commandtext;
            command.CommandTimeout = 0;

            foreach (object value in parameters)
            {
                IDbDataParameter parameter = command.CreateParameter();
                parameter.ParameterName = dbinfo.Parameter + (command.Parameters.Count + 1);
                parameter.Value = value ?? DBNull.Value;
                command.Parameters.Add(parameter);
            }

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            return command;
        }

        /// <summary>
        /// begins a transaction
        /// </summary>
        /// <returns>Transaction object to use</returns>
        public Transaction BeginTransaction() {
            lock (connectionlock)
                return new Transaction(connection.BeginTransaction());
        }

        Tables.DataTable CreateTable(IDataReader reader) {

            return Tables.DataTable.FromReader(reader);
        }

        /// <summary>
        /// executes a query
        /// </summary>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>table containing result data</returns>
        public Tables.DataTable Query(string query, params object[] parameters) {
            return Query(query, (IEnumerable<object>)parameters);
        }

        /// <summary>
        /// executes a query
        /// </summary>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>table containing result data</returns>
        public Tables.DataTable Query(string query, IEnumerable<object> parameters)
        {
            lock (connectionlock)
            {
                using (IDbCommand command = PrepareCommand(query, parameters))
                {
                    return CreateTable(command.ExecuteReader());
                }
            }
        }

        /// <summary>
        /// executes a non query
        /// </summary>
        /// <param name="commandstring">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        public int NonQuery(string commandstring, params object[] parameters) {
            return NonQuery(commandstring, (IEnumerable<object>)parameters);
        }

        /// <summary>
        /// executes a non query
        /// </summary>
        /// <param name="commandstring">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        public int NonQuery(string commandstring, IEnumerable<object> parameters)
        {
            lock (connectionlock)
            {
                using (IDbCommand command = PrepareCommand(commandstring, parameters))
                {
                    return command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// executes a query returning a scalar
        /// </summary>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting scalar</returns>
        public object Scalar(string query, params object[] parameters) {
            return Scalar(query, (IEnumerable<object>)parameters);
        }

        /// <summary>
        /// executes a query returning a scalar
        /// </summary>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting scalar</returns>
        public object Scalar(string query, IEnumerable<object> parameters)
        {
            lock (connectionlock)
            {
                using (IDbCommand command = PrepareCommand(query, parameters))
                {
                    return command.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// executes a query returning a set of values
        /// </summary>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting set of values</returns>
        public IEnumerable<object> Set(string query, params object[] parameters) {
            return Set(query, (IEnumerable<object>)parameters);
        }

        /// <summary>
        /// executes a query returning a set of values
        /// </summary>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting set of values</returns>
        public IEnumerable<object> Set(string query, IEnumerable<object> parameters)
        {
            lock (connectionlock)
            {
                using (IDbCommand command = PrepareCommand(query, parameters))
                {
                    using (IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            yield return reader.GetValue(0);
                        reader.Close();
                    }
                }
            }
        }

        /// <summary>
        /// executes a non query
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="commandstring">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        public int NonQuery(Transaction transaction, string commandstring, params object[] parameters) {
            return NonQuery(transaction, commandstring, (IEnumerable<object>)parameters);
        }

        /// <summary>
        /// executes a non query
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="commandstring">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        public int NonQuery(Transaction transaction, string commandstring, IEnumerable<object> parameters)
        {
            using (IDbCommand command = PrepareCommand(commandstring, parameters))
            {
                command.Transaction = transaction.DbTransaction;
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// executes a query
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>table containing result data</returns>
        public Tables.DataTable Query(Transaction transaction, string query, params object[] parameters) {
            return Query(transaction, query, (IEnumerable<object>)parameters);
        }

        /// <summary>
        /// executes a query
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>table containing result data</returns>
        public Tables.DataTable Query(Transaction transaction, string query, IEnumerable<object> parameters)
        {
            using (IDbCommand command = PrepareCommand(query, parameters))
            {
                command.Transaction = transaction.DbTransaction;
                return CreateTable(command.ExecuteReader());
            }
        }

        /// <summary>
        /// executes a query returning a scalar
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting scalar</returns>
        public object Scalar(Transaction transaction, string query, params object[] parameters) {
            return Scalar(transaction, query, (IEnumerable<object>)parameters);
        }

        /// <summary>
        /// executes a query returning a scalar
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting scalar</returns>
        public object Scalar(Transaction transaction, string query, IEnumerable<object> parameters)
        {
            using (IDbCommand command = PrepareCommand(query, parameters))
            {
                command.Transaction = transaction.DbTransaction;
                return command.ExecuteScalar();
            }
        }

        /// <summary>
        /// executes a query returning a set of values
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting set of values</returns>
        public IEnumerable<object> Set(Transaction transaction, string query, params object[] parameters) {
            return Set(transaction, query, (IEnumerable<object>)parameters);
        }

        /// <summary>
        /// executes a query returning a set of values
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting set of values</returns>
        public IEnumerable<object> Set(Transaction transaction, string query, IEnumerable<object> parameters)
        {
            using (IDbCommand command = PrepareCommand(query, parameters))
            {
                command.Transaction = transaction.DbTransaction;
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        yield return reader.GetValue(0);
                    reader.Close();
                }
            }
        }
    }
}
