using Microsoft.Data.Sqlite;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Info;

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