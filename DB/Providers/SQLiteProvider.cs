using System.Data;
using NightlyCode.DB.Info;
#if UNITY
using Mono.Data.Sqlite;
#else
using System.Data.SQLite;
#endif

namespace NightlyCode.DB.Providers {

    /// <summary>
    /// provider to sqlite database
    /// </summary>
    public class SQLiteProvider : IDBProvider {

        readonly SQLiteInfo info = new SQLiteInfo();

        public IDbConnection CreateConnection(string connectionstring) {
#if UNITY
            return new SqliteConnection(connectionstring);
#else
            return new SQLiteConnection(connectionstring);
#endif
        }

        public IDBInfo DatabaseInfo => info;
    }
}