using System.Net;
using System.Threading.Tasks;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities;
using NightlyCode.Database.Fields;
using NightlyCode.Database.Info;
using NightlyCode.Database.Tests.Entities;
using Npgsql;
using NUnit.Framework;

namespace NightlyCode.Database.Tests.Postgres {
    
    /// <summary>
    /// only used for local tests to test against a real postgres
    /// deactivated in repository
    /// </summary>
    [TestFixture, Parallelizable]
    public class PostgresLocalTests {

        [Test, Parallelizable]
        public async Task LoadArrayPrepared() {
            if (Dns.GetHostName() != "Gangolf")
                Assert.Inconclusive("Test only active on local dev machine");

            IDBClient dbclient = ClientFactory.Create(() => new NpgsqlConnection("Server=elviag;Port=5432;User Id=gangolf;Password=gangolf"), new PostgreInfo(), true);
            EntityManager entitymanager = new EntityManager(dbclient);

            string[] terms = { "motherfucker", "retard", "asshole", "bitch", "fuck" };
            Word[] result = await entitymanager.Load<Word>()
                .Where(w => w.Text.In(DBParameter<string[]>.Value))
                .ExecuteEntitiesAsync(new object[]{terms});
            Assert.AreEqual(5, result.Length);
        }
        
        [Test, Parallelizable]
        public async Task LoadNotInArrayPrepared() {
            if (Dns.GetHostName() != "Gangolf")
                Assert.Inconclusive("Test only active on local dev machine");

            IDBClient dbclient = ClientFactory.Create(() => new NpgsqlConnection("Server=elviag;Port=5432;User Id=gangolf;Password=gangolf"), new PostgreInfo(), true);
            EntityManager entitymanager = new EntityManager(dbclient);

            string[] terms = { "motherfucker", "retard", "asshole", "bitch", "fuck" };
            Word[] result = await entitymanager.Load<Word>()
                .Where(w => !w.Text.In(DBParameter<string[]>.Value))
                .Limit(10)
                .ExecuteEntitiesAsync(new object[]{terms});
            Assert.AreEqual(10, result.Length);
        }
    }
}