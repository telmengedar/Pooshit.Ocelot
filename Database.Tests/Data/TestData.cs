using Microsoft.Data.Sqlite;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Info;

namespace NightlyCode.Database.Tests.Data {
    public static class TestData {

        public static IDBClient CreateDatabaseAccess() {
            return new DBClient(new SqliteConnection("Data Source=:memory:"), new SQLiteInfo());
        }
    }
}