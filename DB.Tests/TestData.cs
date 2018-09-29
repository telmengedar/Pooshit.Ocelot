using Microsoft.Data.Sqlite;
using NightlyCode.DB.Clients;
using NightlyCode.DB.Info;

namespace NightlyCode.DB.Tests {
    public static class TestData {

        public static IDBClient CreateDatabaseAccess() {
            return new DBClient(new SqliteConnection("Data Source=:memory:"), new SQLiteInfo());
        }
    }
}