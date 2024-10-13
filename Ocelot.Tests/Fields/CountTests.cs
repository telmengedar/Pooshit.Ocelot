using NightlyCode.Database.Tests.Data;
using NightlyCode.Database.Tests.Models;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Fields;

namespace NightlyCode.Database.Tests.Fields {
    
    [TestFixture, Parallelizable]
    public class CountTests {

        [Test, Parallelizable]
        public void CountEntityField()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel { String = "" },
                new ValueModel(),
                new ValueModel { String = "a" },
                new ValueModel { String = "b" },
                new ValueModel { String = "c" });

            long result = entitymanager.Load<ValueModel>(v => DBFunction.Count(EntityField.Create<ValueModel>(m => m.String))).ExecuteScalar<long>();
            Assert.AreEqual(4, result);
        }
        
        [Test, Parallelizable]
        public void CountDirectField()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel { String = "" },
                new ValueModel(),
                new ValueModel { String = "a" },
                new ValueModel { String = "b" },
                new ValueModel { String = "c" });

            long result = entitymanager.Load<ValueModel>(v => DBFunction.Count(v.String)).ExecuteScalar<long>();
            Assert.AreEqual(4, result);
        }
    }
}