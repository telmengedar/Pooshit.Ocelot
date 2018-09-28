using System.Data;
using NightlyCode.DB.Info;
using NightlyCode.DB.Clients;
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

        /// <summary>
        /// creates an sqlite client
        /// </summary>
        /// <param name="filename">filename of the database or null for an in memory db</param>
        /// <param name="synchroneous">whether to activate synchroneous write mode</param>
        /// <returns></returns>
        public static IDBClient CreateSQLite(string filename, bool synchroneous = true)
        {
            Clients.DBClient dbclient = new Clients.DBClient(new SQLiteProvider(), "Data Source=" + (filename ?? ":memory:"));
            dbclient.NonQuery("PRAGMA temp_store=2");
            dbclient.NonQuery("PRAGMA journal_mode=TRUNCATE");
            if (!synchroneous)
                dbclient.NonQuery("PRAGMA synchronous=OFF");
            return dbclient;

        }

    }
}