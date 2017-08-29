using System.Data;
using MySql.Data.MySqlClient;
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
    }
}