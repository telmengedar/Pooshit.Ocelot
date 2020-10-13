using Microsoft.Data.Sqlite;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities;
using NightlyCode.Database.Fields;
using NightlyCode.Database.Info;

namespace NightlyCode.Database.Tests.Data {
    public static class TestData {

        public static IDBClient CreateDatabaseAccess() {
            return ClientFactory.Create(new SqliteConnection("Data Source=:memory:"), new SQLiteInfo());
        }

        public static IEntityManager CreateEntityManager() {
            return new EntityManager(CreateDatabaseAccess());
        }
    }
}