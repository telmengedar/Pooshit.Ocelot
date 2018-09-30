using NightlyCode.DB.Clients;
using NightlyCode.DB.Entities;
using NightlyCode.DB.Entities.Operations;
using NightlyCode.DB.Tests.Models;
using NUnit.Framework;

namespace NightlyCode.DB.Tests.Entities {

    [TestFixture]
    public class AggregateTests {

        [Test]
        public void TestSumFields() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities(new ValueModel {Integer = 5}, 
                new ValueModel(), 
                new ValueModel {Integer = 11}, 
                new ValueModel {Integer = 3}, 
                new ValueModel {Integer = 7});

            int result = entitymanager.Load<ValueModel>(DBFunction.Sum(EntityField.Create<ValueModel>(m => m.Integer))).ExecuteScalar<int>();
            Assert.AreEqual(26, result);
        }

        [Test]
        public void TestSumExpressions()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities(new ValueModel { Integer = 5 },
                new ValueModel(),
                new ValueModel { Integer = 11 },
                new ValueModel { Integer = 3 },
                new ValueModel { Integer = 7 });

            int result = entitymanager.Load<ValueModel>(DBFunction.Sum<ValueModel>(m => m.Integer)).ExecuteScalar<int>();
            Assert.AreEqual(26, result);
        }

        [Test]
        public void TestTotalFields()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities(new ValueModel { Double = 5 },
                new ValueModel(),
                new ValueModel { Double = 11.3 },
                new ValueModel { Double = 3 },
                new ValueModel { Double = 7 });

            double result = entitymanager.Load<ValueModel>(DBFunction.Total(EntityField.Create<ValueModel>(m => m.Double))).ExecuteScalar<double>();
            Assert.AreEqual(26.3, result);
        }

        [Test]
        public void TestTotalExpressions()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities(new ValueModel { Double = 5 },
                new ValueModel(),
                new ValueModel { Double = 11.3 },
                new ValueModel { Double = 3 },
                new ValueModel { Double = 7 });

            double result = entitymanager.Load<ValueModel>(DBFunction.Total<ValueModel>(m => m.Double)).ExecuteScalar<double>();
            Assert.AreEqual(26.3, result);
        }

        [Test]
        public void TestAverageFields()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities(new ValueModel { Integer = 5 },
                new ValueModel(),
                new ValueModel { Integer = 11 },
                new ValueModel { Integer = 3 },
                new ValueModel { Integer = 7 });

            double result = entitymanager.Load<ValueModel>(DBFunction.Average(EntityField.Create<ValueModel>(m => m.Integer))).ExecuteScalar<double>();
            Assert.AreEqual(5.2, result);
        }

        [Test]
        public void TestAverageExpressions()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities(new ValueModel { Integer = 5 },
                new ValueModel(),
                new ValueModel { Integer = 11 },
                new ValueModel { Integer = 3 },
                new ValueModel { Integer = 7 });

            double result = entitymanager.Load<ValueModel>(DBFunction.Average<ValueModel>(m => m.Integer)).ExecuteScalar<double>();
            Assert.AreEqual(5.2, result);
        }

        [Test]
        public void TestMinFields()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities(new ValueModel { Integer = 5 },
                new ValueModel(),
                new ValueModel { Integer = 11 },
                new ValueModel { Integer = 3 },
                new ValueModel { Integer = 7 });

            int result = entitymanager.Load<ValueModel>(DBFunction.Min(EntityField.Create<ValueModel>(m => m.Integer))).ExecuteScalar<int>();
            Assert.AreEqual(0, result);
        }

        [Test]
        public void TestMinExpressions()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities(new ValueModel { Integer = 5 },
                new ValueModel(),
                new ValueModel { Integer = 11 },
                new ValueModel { Integer = 3 },
                new ValueModel { Integer = 7 });

            int result = entitymanager.Load<ValueModel>(DBFunction.Min<ValueModel>(m => m.Integer)).ExecuteScalar<int>();
            Assert.AreEqual(0, result);
        }

        [Test]
        public void TestMaxFields()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities(new ValueModel { Integer = 5 },
                new ValueModel(),
                new ValueModel { Integer = 11 },
                new ValueModel { Integer = 3 },
                new ValueModel { Integer = 7 });

            int result = entitymanager.Load<ValueModel>(DBFunction.Max(EntityField.Create<ValueModel>(m => m.Integer))).ExecuteScalar<int>();
            Assert.AreEqual(11, result);
        }

        [Test]
        public void TestMaxExpressions()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities(new ValueModel { Integer = 5 },
                new ValueModel(),
                new ValueModel { Integer = 11 },
                new ValueModel { Integer = 3 },
                new ValueModel { Integer = 7 });

            int result = entitymanager.Load<ValueModel>(DBFunction.Max<ValueModel>(m => m.Integer)).ExecuteScalar<int>();
            Assert.AreEqual(11, result);
        }

    }
}