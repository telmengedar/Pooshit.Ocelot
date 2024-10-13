using System.Linq;
using NightlyCode.Database.Tests.Data;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Clients.Tables;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Tokens;
using Pooshit.Ocelot.Tokens.Operations;

namespace NightlyCode.Database.Tests {

    [TestFixture, Parallelizable]
    public class LoadDataOperationTests {

        [Test, Parallelizable]
        public void LoadWithCriterias() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new(dbclient);

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
                .Where( new OperationToken(DB.Column("last"), Operand.Equal, DB.Constant("sonne")))
                .Execute();

            Assert.AreEqual(4, data.Rows.Length);
            Assert.That(data.Rows.All(r => r["last"]?.ToString() == "sonne"));
            Assert.That(new[] {"neubert", "newbert", "nawbert", "herbert"}.SequenceEqual(data.Rows.Select(r => r["first"])));
        }
        
        [Test, Parallelizable]
        public void LoadOffsetAndLimit() {
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
                .Offset(3)
                .Limit(2)
                .Execute();

            Assert.AreEqual(2, data.Rows.Length);
            Assert.That(new[] {"lisa", "herbert"}.SequenceEqual(data.Rows.Select(r => r["first"])));
        }

        [Test, Parallelizable]
        public void LoadWithoutExplicitColumns() {
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
                .Offset(3)
                .Limit(2)
                .Execute();

            Assert.AreEqual(2, data.Rows.Length);
            Assert.That(new[] {"lisa", "herbert"}.SequenceEqual(data.Rows.Select(r => r["first"]))); 
        }
    }
}