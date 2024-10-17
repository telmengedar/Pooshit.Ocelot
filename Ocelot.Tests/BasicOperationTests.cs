using System.Threading.Tasks;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Statements;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Entities;
using Pooshit.Ocelot.Tokens;

namespace Pooshit.Ocelot.Tests {
    
    [TestFixture, Parallelizable]
    public class BasicOperationTests {

        [Test, Parallelizable]
        public async Task TruncateTable() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            
            entitymanager.UpdateSchema<Company>();

            await entitymanager.Insert<Company>()
                .Columns(v => v.Name, v => v.Url)
                .Values("hallo", "www.w.w")
                .ExecuteAsync();

            await entitymanager.Truncate<Company>(new TruncateOptions {
                ResetIdentity = true
            });
            
            Assert.AreEqual(0, await entitymanager.Load<Company>(DB.Count()).ExecuteScalarAsync<long>());
            long id=await entitymanager.Insert<Company>()
                .Columns(v => v.Name, v => v.Url)
                .Values("hallo", "www.w.w")
                .ReturnID()
                .ExecuteAsync();
            Assert.AreEqual(1, id);
        }
    }
}