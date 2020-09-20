using System.Linq;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Tests.Data;
using NightlyCode.Database.Tests.Models;
using NUnit.Framework;

namespace NightlyCode.Database.Tests {

    [TestFixture, Parallelizable]
    public class InsertDataOperationTests {

        [Test, Parallelizable]
        public void InsertValues() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.UpdateSchema<ValueModel>();

            long result = entitymanager.InsertData("valuemodel")
                .Columns("integer", "single", "double", "string")
                .Values(1, 3.0f, 3.0, "7")
                .Execute();

            Assert.AreEqual(1, result);

            ValueModel entity = entitymanager.Load<ValueModel>().ExecuteEntities<ValueModel>().FirstOrDefault();
            Assert.NotNull(entity);
            Assert.AreEqual(1, entity.Integer);
            Assert.AreEqual(3.0f, entity.Single);
            Assert.AreEqual(3.0, entity.Double);
            Assert.AreEqual("7", entity.String);
        }

        [Test, Parallelizable]
        public void InsertUsingPreparedOperation() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.UpdateSchema<ValueModel>();

            PreparedOperation operation = entitymanager.InsertData("valuemodel")
                .Columns("integer", "single", "double", "string")
                .Prepare();

            long result = operation.Execute(1, 3.0f, 3.0, "7");

            Assert.AreEqual(1, result);

            ValueModel entity = entitymanager.Load<ValueModel>().ExecuteEntities<ValueModel>().FirstOrDefault();
            Assert.NotNull(entity);
            Assert.AreEqual(1, entity.Integer);
            Assert.AreEqual(3.0f, entity.Single);
            Assert.AreEqual(3.0, entity.Double);
            Assert.AreEqual("7", entity.String);
        }

        [Test, Parallelizable]
        public void InsertUsingPreparedOperationAndStringValues() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.UpdateSchema<ValueModel>();

            PreparedOperation operation = entitymanager.InsertData("valuemodel")
                .Columns("integer", "single", "double", "string")
                .Prepare();

            long result = operation.Execute("1", "3", "3", "7");

            Assert.AreEqual(1, result);

            ValueModel entity = entitymanager.Load<ValueModel>().ExecuteEntities<ValueModel>().FirstOrDefault();
            Assert.NotNull(entity);
            Assert.AreEqual(1, entity.Integer);
            Assert.AreEqual(3.0f, entity.Single);
            Assert.AreEqual(3.0, entity.Double);
            Assert.AreEqual("7", entity.String);
        }
    }
}