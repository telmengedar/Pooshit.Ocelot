using System;
using System.Collections.Generic;
using System.Data;
using NightlyCode.DB.Info;
using NightlyCode.DB.Providers;

namespace NightlyCode.DB.Clients
{

    /// <summary>
    /// client to execute database commands
    /// </summary>
    public class DBClient : IDBClient {
        readonly IDbConnection connection;
        readonly IDBInfo dbinfo;
        readonly object connectionlock = new object();

        readonly object transactionlock = new object();

        /// <summary>
        /// creates a new database client
        /// </summary>
        /// <param name="provider">provider for database connection</param>
        /// <param name="connectionstring">connection string</param>
        DBClient(IDBProvider provider, string connectionstring) {
            connection = provider.CreateConnection(connectionstring);
            dbinfo = provider.DatabaseInfo;
        }

        public IDBInfo DBInfo => dbinfo;

        IDbCommand PrepareCommand(string commandtext, params object[] parameters) {
            IDbCommand command = connection.CreateCommand();
            command.CommandText = commandtext;
            command.CommandTimeout = 0;

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
            Transaction transaction = new Transaction(transactionlock);
            lock(connectionlock)
                transaction.DbTransaction = connection.BeginTransaction();
            return transaction;
        }

        DataTable CreateTable(IDataReader reader) {
            DataTable table = new DataTable();
            table.Load(reader);
            return table;
        }

        public DataTable Query(string query, params object[] parameters) {
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

        /// <summary>
        /// creates an sqlite client
        /// </summary>
        /// <param name="filename">filename of the database or null for an in memory db</param>
        /// <param name="synchroneous">whether to activate synchroneous write mode</param>
        /// <returns></returns>
        public static DBClient CreateSQLite(string filename, bool synchroneous=true) {
            DBClient dbclient = new DBClient(new SQLiteProvider(), "Data Source=" + (filename ?? ":memory:"));
            dbclient.NonQuery("PRAGMA temp_store=2");
            dbclient.NonQuery("PRAGMA journal_mode=TRUNCATE");
            if (!synchroneous)
                dbclient.NonQuery("PRAGMA synchronous=OFF");
            return dbclient;

        }

#if !UNITY
        public static DBClient CreatePostgre(string host, int port, string database, string user, string password) {
            return new DBClient(new PostgreProvider(), $"Server={host}; Port={port}; Database={database}; User Id={user}; Password={password}");
        }

        /// <summary>
        /// creates a connection to a mysql server
        /// </summary>
        /// <param name="host">host where server is running</param>
        /// <param name="port">tcp port (default 3306)</param>
        /// <param name="database">database instance to connect to</param>
        /// <param name="user">user to connect</param>
        /// <param name="password">password to connect</param>
        /// <returns>client which is connected to the specified mysql database</returns>
        public static DBClient CreateMySQL(string host, int port, string database, string user, string password) {
            return new DBClient(new MySQLProvider(), $"Server={host};Port={port};Database={database};Uid={user}; Pwd={password}");
        }
#endif
    }
}
