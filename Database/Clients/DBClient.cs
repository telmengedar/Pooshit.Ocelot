using System;
using System.Collections.Generic;
using System.Data;
using Database.Info;

namespace Database.Clients
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

        /// <summary>
        /// prepares a command for execution
        /// </summary>
        /// <param name="commandtext">command text containing sql to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>command which can get executed</returns>
        IDbCommand PrepareCommand(string commandtext, params object[] parameters)
        {
            return PrepareCommand(commandtext, null, parameters);
        }

        IDbCommand PrepareCommand(string commandtext, Transaction transaction, params object[] parameters) {
            IDbCommand command = connection.CreateCommand();
            command.CommandText = commandtext;
            command.CommandTimeout = 0;

            if (transaction != null)
                command.Transaction = transaction.DbTransaction;

            foreach(object o in parameters) {
                IDbDataParameter parameter = command.CreateParameter();
                parameter.ParameterName = dbinfo.Parameter + (command.Parameters.Count + 1);
                parameter.Value = o ?? DBNull.Value;
                command.Parameters.Add(parameter);
            }

            if(connection.State != ConnectionState.Open) {
                connection.Open();
            }

            return command;
        }

        public Transaction BeginTransaction() {
            lock (connectionlock)
                return new Transaction(this, connection.BeginTransaction());
        }

        Tables.DataTable CreateTable(IDataReader reader) {

            return Tables.DataTable.FromReader(reader);
            /*Tables.DataTable table = new Tables.DataTable();
            table.Load(reader);
            return table;*/
        }

        public Tables.DataTable Query(string query, params object[] parameters) {
            lock(connectionlock) {
                using(IDbCommand command = PrepareCommand(query, parameters)) {
                    return CreateTable(command.ExecuteReader());
                }
            }
        }

        public int NonQuery(string commandstring, params object[] parameters) {
            lock(connectionlock) {
                using(IDbCommand command = PrepareCommand(commandstring, parameters)) {
                    return command.ExecuteNonQuery();
                }
            }
        }

        public object Scalar(string query, params object[] parameters) {
            lock(connectionlock) {
                using(IDbCommand command = PrepareCommand(query, parameters)) {
                    return command.ExecuteScalar();
                }
            }
        }

        public IEnumerable<object> Set(string query, params object[] parameters) {
            lock(connectionlock) {
                using(IDbCommand command = PrepareCommand(query, parameters)) {
                    using(IDataReader reader = command.ExecuteReader()) {
                        while(reader.Read())
                            yield return reader.GetValue(0);
                        reader.Close();
                    }
                }
            }
        }

        public int NonQuery(Transaction transaction, string commandstring, params object[] parameters)
        {
            using (IDbCommand command = PrepareCommand(commandstring, parameters))
            {
                command.Transaction = transaction.DbTransaction;
                return command.ExecuteNonQuery();
            }
        }

        public Tables.DataTable Query(Transaction transaction, string query, params object[] parameters)
        {
            using (IDbCommand command = PrepareCommand(query, parameters))
            {
                command.Transaction = transaction.DbTransaction;
                return CreateTable(command.ExecuteReader());
            }
        }

        public object Scalar(Transaction transaction, string query, params object[] parameters)
        {
            using (IDbCommand command = PrepareCommand(query, parameters))
            {
                command.Transaction = transaction.DbTransaction;
                return command.ExecuteScalar();
            }
        }

        public IEnumerable<object> Set(Transaction transaction, string query, params object[] parameters)
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
