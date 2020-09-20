using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities;
using NightlyCode.Database.Tests.Data;
using NightlyCode.Database.Tests.Models;
using NUnit.Framework;

namespace NightlyCode.Database.Tests {

    [TestFixture, Parallelizable]
    public class UpdateDataOperationTests {

        [Test, Parallelizable]
        public void UpdateWithoutParameters() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.Insert<ValueModel>()
                .Columns(v => v.String, v => v.Integer, v => v.Single, v => v.Double)
                .Values("hallo", 7, 1.0f, 3.0)
                .Execute();


            entitymanager.UpdateData("valuemodel")
                .Set("string", "single", "double").Where("integer", "=", "7")
                .Execute("hello", 5.0f, 10.0);

            ValueModel result = entitymanager.Load<ValueModel>().ExecuteEntity<ValueModel>();

            Assert.NotNull(result);
            Assert.AreEqual("hello", result.String);
            Assert.AreEqual(5.0f, result.Single);
            Assert.AreEqual(10.0, result.Double);
        }
    }
}