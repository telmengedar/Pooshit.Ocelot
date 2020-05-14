﻿using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Tests.Data;
using NightlyCode.Database.Tests.Models;
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
    }
}