using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities;
using NightlyCode.Database.Entities.Operations;
using NightlyCode.Database.Entities.Operations.Fields;
using NightlyCode.Database.Tests.Data;
using NightlyCode.Database.Tests.Models;
using NUnit.Framework;

namespace NightlyCode.Database.Tests.Fields {

    [TestFixture]
    public class AggregateTests {

        [Test]
        public void TestSumFields() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel {Integer = 5},
                new ValueModel(),
                new ValueModel {Integer = 11},
                new ValueModel {Integer = 3},
                new ValueModel {Integer = 7});

            int result = entitymanager.Load<ValueModel>(m=>DBFunction.Sum(m.Integer)).ExecuteScalar<int>();
            Assert.AreEqual(26, result);
        }

        [Test]
        public void TestSumExpressions()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel { Integer = 5 },
                new ValueModel(),
                new ValueModel { Integer = 11 },
                new ValueModel { Integer = 3 },
                new ValueModel { Integer = 7 });

            int result = entitymanager.Load<ValueModel>(m=>DBFunction.Sum(m.Integer)).ExecuteScalar<int>();
            Assert.AreEqual(26, result);
        }

        [Test]
        public void TestTotalFields()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel { Double = 5 },
                new ValueModel(),
                new ValueModel { Double = 11.3 },
                new ValueModel { Double = 3 },
                new ValueModel { Double = 7 });

            double result = entitymanager.Load<ValueModel>(v => DBFunction.Total(EntityField.Create<ValueModel>(m => m.Double))).ExecuteScalar<double>();
            Assert.AreEqual(26.3, result);
        }

        [Test]
        public void TestTotalExpressions()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel { Double = 5 },
                new ValueModel(),
                new ValueModel { Double = 11.3 },
                new ValueModel { Double = 3 },
                new ValueModel { Double = 7 });

            double result = entitymanager.Load<ValueModel>(v => DBFunction.Total(v.Double)).ExecuteScalar<double>();
            Assert.AreEqual(26.3, result);
        }

        [Test]
        public void TestAverageFields()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel { Integer = 5 },
                new ValueModel(),
                new ValueModel { Integer = 11 },
                new ValueModel { Integer = 3 },
                new ValueModel { Integer = 7 });

            double result = entitymanager.Load<ValueModel>(m=>DBFunction.Average(m.Integer)).ExecuteScalar<double>();
            Assert.AreEqual(5.2, result);
        }

        [Test]
        public void TestAverageExpressions()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel { Integer = 5 },
                new ValueModel(),
                new ValueModel { Integer = 11 },
                new ValueModel { Integer = 3 },
                new ValueModel { Integer = 7 });

            double result = entitymanager.Load<ValueModel>(m=>DBFunction.Average(m.Integer)).ExecuteScalar<double>();
            Assert.AreEqual(5.2, result);
        }

        [Test]
        public void TestMinFields()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel { Integer = 5 },
                new ValueModel(),
                new ValueModel { Integer = 11 },
                new ValueModel { Integer = 3 },
                new ValueModel { Integer = 7 });

            int result = entitymanager.Load<ValueModel>(m=>DBFunction.Min(m.Integer)).ExecuteScalar<int>();
            Assert.AreEqual(0, result);
        }

        [Test]
        public void TestMinExpressions()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel { Integer = 5 },
                new ValueModel(),
                new ValueModel { Integer = 11 },
                new ValueModel { Integer = 3 },
                new ValueModel { Integer = 7 });

            int result = entitymanager.Load<ValueModel>(m=>DBFunction.Min(m.Integer)).ExecuteScalar<int>();
            Assert.AreEqual(0, result);
        }

        [Test]
        public void TestMaxFields()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel { Integer = 5 },
                new ValueModel(),
                new ValueModel { Integer = 11 },
                new ValueModel { Integer = 3 },
                new ValueModel { Integer = 7 });

            int result = entitymanager.Load<ValueModel>(m=>DBFunction.Max(m.Integer)).ExecuteScalar<int>();
            Assert.AreEqual(11, result);
        }

        [Test]
        public void TestMaxExpressions()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel { Integer = 5 },
                new ValueModel(),
                new ValueModel { Integer = 11 },
                new ValueModel { Integer = 3 },
                new ValueModel { Integer = 7 });

            int result = entitymanager.Load<ValueModel>(m=>DBFunction.Max(m.Integer)).ExecuteScalar<int>();
            Assert.AreEqual(11, result);
        }

        [Test]
        public void TestMaxUpdate() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(new ValueModel {Integer = 0});

            entitymanager.Update<ValueModel>().Set(v => v.Integer == DBFunction.Max(v.Integer - 2, -1)).Execute();

            Assert.AreEqual(-1, entitymanager.Load<ValueModel>(i => i.Integer).ExecuteScalar<int>());
        }

        [Test]
        public void TestMinUpdate()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(new ValueModel { Integer = 4 });

            entitymanager.Update<ValueModel>().Set(v => v.Integer == DBFunction.Min(v.Integer * v.Integer, 12)).Execute();

            Assert.AreEqual(12, entitymanager.Load<ValueModel>(i => i.Integer).ExecuteScalar<int>());
        }
    }
}