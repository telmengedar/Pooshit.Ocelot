using System.Data;
using MySql.Data.MySqlClient;
using NightlyCode.DB.Clients;
using NightlyCode.DB.Info;

namespace NightlyCode.DB.Providers {

    /// <summary>
    /// provider for mysql database
    /// </summary>
    public class MySQLProvider : IDBProvider {
        readonly IDBInfo dbinfo = new MySQLInfo();

        /// <summary>
        /// creates a new connection to the database
        /// </summary>
        /// <returns></returns>
        public IDbConnection CreateConnection(string connectionstring) {
            return new MySqlConnection(connectionstring);
        }

        /// <summary>
        /// provider specific info
        /// </summary>
        public IDBInfo DatabaseInfo => dbinfo;

        /// <summary>
        /// creates a connection to a mysql server
        /// </summary>
        /// <param name="host">host where server is running</param>
        /// <param name="port">tcp port (default 3306)</param>
        /// <param name="database">database instance to connect to</param>
        /// <param name="user">user to connect</param>
        /// <param name="password">password to connect</param>
        /// <returns>client which is connected to the specified mysql database</returns>
        public static DBClient CreateMySQL(string host, int port, string database, string user, string password)
        {
            return new DBClient(new MySQLProvider(), $"Server={host};Port={port};Database={database};Uid={user}; Pwd={password}");
        }

    }
}