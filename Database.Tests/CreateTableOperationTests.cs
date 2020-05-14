using NightlyCode.Database.Clients;
using NightlyCode.Database.Clients.Tables;
using NightlyCode.Database.Entities;
using NightlyCode.Database.Tests.Data;
using NUnit.Framework;

namespace NightlyCode.Database.Tests {

    [TestFixture, Parallelizable]
    public class CreateTableOperationTests {

        [Test, Parallelizable]
        public void CreateTableWithoutTypeSpecification() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.CreateTable("testtable")
                .Column("bumm")
                .Column("bommel")
                .Execute();

            Assert.IsTrue(entitymanager.Exists("testtable"));

            long result = entitymanager.InsertData("testtable")
                .Columns("bumm", "bommel")
                .Values("neubert", "sonne")
                .Execute();

            Assert.AreEqual(1, result);

            DataTable data = entitymanager.LoadData("testtable")
                .Columns("bumm", "bommel")
                .Execute();

            Assert.AreEqual(1, data.Rows.Length);
            Assert.AreEqual("neubert", data.Rows[0]["bumm"]);
            Assert.AreEqual("sonne", data.Rows[0]["bommel"]);
        }
    }
}