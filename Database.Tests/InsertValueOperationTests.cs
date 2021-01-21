using System;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities;
using NightlyCode.Database.Entities.Operations;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Tests.Data;
using NightlyCode.Database.Tests.Models;
using NightlyCode.Database.Tokens;
using NUnit.Framework;

namespace NightlyCode.Database.Tests {

    [TestFixture, Parallelizable]
    public class InsertValueOperationTests {

        [Test, Parallelizable]
        public void TestReturnID() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.UpdateSchema<AutoIncrementEntity>();

            PreparedOperation insertop = entitymanager.Insert<AutoIncrementEntity>().Columns(c => c.Bla).ReturnID().Prepare();

            long id = insertop.Execute("blubb");
            Assert.AreEqual(1, id);
            id = insertop.Execute("blobb");
            Assert.AreEqual(2, id);
        }

        [Test, Parallelizable]
        public void InsertNullableDateTime() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.UpdateSchema<ValueModel>();

            PreparedOperation insertop = entitymanager.Insert<ValueModel>().Columns(c => c.NDatetime).Prepare();

            long id = insertop.Execute(DateTime.UtcNow);
            Assert.AreEqual(1, id);
        }

        [Test, Parallelizable]
        public void InsertParametersWithoutPrepare() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.UpdateSchema<ValueModel>();

            InsertValuesOperation<ValueModel> insertop = entitymanager.Insert<ValueModel>().Columns(c => c.String);

            long id = insertop.Execute("lala");
            Assert.AreEqual(1, id);
        }

        [Test, Parallelizable]
        public void InsertBulk() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.UpdateSchema<ValueModel>();

            PreparedBulkInsertOperation insertop = entitymanager.Insert<ValueModel>()
                .Columns(c => c.String, c => c.Integer, c => c.Single)
                .PrepareBulk();

            insertop.Execute(new object[]{new object[] {"Rolf", 32, 1.0f}, new object[] {"Ulf", 11, 0.5f}, new object[] {"Lutz", 25, 0.8f}});

            Assert.AreEqual(3, entitymanager.Load<ValueModel>(DB.Count(DB.All)).ExecuteScalar<int>());
        }
    }
}