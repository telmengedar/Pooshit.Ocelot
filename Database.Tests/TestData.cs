using Database.Clients;
using Database.Info;
using Microsoft.Data.Sqlite;

namespace NightlyCode.Database.Tests {
    public static class TestData {

        public static IDBClient CreateDatabaseAccess() {
            return new DBClient(new SqliteConnection("Data Source=:memory:"), new SQLiteInfo());
        }
    }
}