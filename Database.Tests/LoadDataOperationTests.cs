using System.Linq;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Clients.Tables;
using NightlyCode.Database.Entities;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Tests.Data;
using NUnit.Framework;

namespace NightlyCode.Database.Tests {

    [TestFixture, Parallelizable]
    public class LoadDataOperationTests {

        [Test, Parallelizable]
        public void LoadWithCriterias() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.CreateTable("testtable")
                .Column("first")
                .Column("last")
                .Execute();

            Assert.IsTrue(entitymanager.Exists("testtable"));

            PreparedOperation insertoperation=entitymanager.InsertData("testtable")
                .Columns("first", "last")
                .Prepare();

            insertoperation.Execute("neubert", "sonne");
            insertoperation.Execute("newbert", "sonne");
            insertoperation.Execute("nawbert", "sonne");
            insertoperation.Execute("lisa", "bensch");
            insertoperation.Execute("herbert", "sonne");
            insertoperation.Execute("thomas", "bensch");

            DataTable data = entitymanager.LoadData("testtable")
                .Columns("first", "last")
                .Where("last", "=", "sonne")
                .Execute();

            Assert.AreEqual(4, data.Rows.Length);
            Assert.That(data.Rows.All(r => r["last"]?.ToString() == "sonne"));
            Assert.That(new[] {"neubert", "newbert", "nawbert", "herbert"}.SequenceEqual(data.Rows.Select(r => r["first"])));
        }
    }
}