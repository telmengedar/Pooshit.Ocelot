using System;
using System.Collections.Generic;
using System.Linq;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Fields;
using NightlyCode.Database.Tests.Data;
using NightlyCode.Database.Tests.Models;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace NightlyCode.Database.Tests.Entities {

    [TestFixture, Parallelizable]
    public class EntityManagerTest {
        
        IEnumerable<TestEntityWithoutAnySpecifications> TestEntities {
            get {
                yield return new TestEntityWithoutAnySpecifications {
                    Column1 = "Bla",
                    IntegerValue = 1,
                    Something = 7.0,
                    BooleanValue=true
                };
                yield return new TestEntityWithoutAnySpecifications {
                    Column1 = "Bla1",
                    IntegerValue = 2,
                    Something = 7.0,
                    BooleanValue=true
                };
                yield return new TestEntityWithoutAnySpecifications {
                    Column1 = "Bla2",
                    IntegerValue = 3,
                    Something = 8.0,
                    BooleanValue = false
                };
            }
        }

        [Test, Parallelizable]
        public void TestEntityCreation() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();
            Assert.That(dbclient.DBInfo.CheckIfTableExists(dbclient, "testentitywithoutanyspecifications"));
        }

        [Test, Parallelizable]
        public void TestEntityDrop() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();
            entitymanager.Drop<TestEntityWithoutAnySpecifications>();
            Assert.That(!dbclient.DBInfo.CheckIfTableExists(dbclient, "testentitywithoutanyspecifications"));
        }

        [Test, Parallelizable]
        public void CreateView() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestView>();
            Assert.That(dbclient.DBInfo.CheckIfTableExists(dbclient, "testview"));
        }

        [Test, Parallelizable]
        public void DropView() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestView>();
            entitymanager.Drop<TestView>();
            Assert.That(!dbclient.DBInfo.CheckIfTableExists(dbclient, "testview"));
        }

        [Test, Parallelizable]
        public void InsertStatement() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();

            PreparedOperation operation = entitymanager.Insert<TestEntityWithoutAnySpecifications>()
                .Columns(t => t.Column1, t => t.BooleanValue, t => t.IntegerValue, t => t.Something)
                .Prepare();

            foreach (TestEntityWithoutAnySpecifications entity in TestEntities)
                operation.Execute(entity.Column1, entity.BooleanValue, entity.IntegerValue, entity.Something);

            Assert.AreEqual(TestEntities.Count(), entitymanager.Load<TestEntityWithoutAnySpecifications>(m => DBFunction.Count()).ExecuteScalar<int>());
        }

        [Test, Parallelizable]
        public void TestContains() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(TestEntities);

            int[] array = {2, 3};
            TestEntityWithoutAnySpecifications[] result = entitymanager.Load<TestEntityWithoutAnySpecifications>()
                .Where(e => array.Contains(e.IntegerValue))
                .ExecuteEntities<TestEntityWithoutAnySpecifications>().ToArray();
            Assert.AreEqual(2, result.Length);
        }

        [Test, Parallelizable]
        public void DoesNotContain() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(TestEntities);

            int[] array = { 2, 3 };
            TestEntityWithoutAnySpecifications[] result = entitymanager.Load<TestEntityWithoutAnySpecifications>()
                .Where(e => !array.Contains(e.IntegerValue))
                .ExecuteEntities<TestEntityWithoutAnySpecifications>().ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(1, result[0].IntegerValue);
        }

        [Test, Parallelizable]
        public void TestInsertWithReturn() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();

            TestEntityWithoutAnySpecifications[] testentities = TestEntities.ToArray();

            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(testentities);
            foreach(TestEntityWithoutAnySpecifications entity in testentities)
                Assert.AreNotEqual(0, entity.ThePrimaryKey);

            foreach(TestEntityWithoutAnySpecifications loaded in entitymanager.Load<TestEntityWithoutAnySpecifications>().ExecuteEntities<TestEntityWithoutAnySpecifications>()) {
                TestEntityWithoutAnySpecifications original = testentities.FirstOrDefault(t => t.ThePrimaryKey == loaded.ThePrimaryKey);
                Assert.NotNull(original);

                Assert.AreEqual(original.BooleanValue, loaded.BooleanValue);
                Assert.AreEqual(original.Column1, loaded.Column1);
                Assert.AreEqual(original.IntegerValue, loaded.IntegerValue);
                Assert.AreEqual(original.Something, loaded.Something);
            }
        }

        [Test, Parallelizable]
        public void TestUpdate() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();
            TestEntityWithoutAnySpecifications[] entities = TestEntities.ToArray();
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(entities);
            entities[1].Column1 = "Changed";
            entitymanager.UpdateEntities<TestEntityWithoutAnySpecifications>().Execute(entities);
            TestEntityWithoutAnySpecifications[] loaded = entitymanager.Load<TestEntityWithoutAnySpecifications>().ExecuteEntities<TestEntityWithoutAnySpecifications>().ToArray();
            Assert.AreEqual(entities.Length, loaded.Length);
            for(int i=0;i<entities.Length;++i)
                Assert.AreEqual(entities[i], loaded[i]);
        }
        
        [Test, Parallelizable]
        public void TestSave() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();
            List<TestEntityWithoutAnySpecifications> entities = new List<TestEntityWithoutAnySpecifications>(TestEntities);
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(entities);
            entities[1].Column1 = "Changed";
            entities.Add(new TestEntityWithoutAnySpecifications("added1", 1, 4.3));
            entitymanager.Save<TestEntityWithoutAnySpecifications>(entities);
            TestEntityWithoutAnySpecifications[] loaded = entitymanager.Load<TestEntityWithoutAnySpecifications>().ExecuteEntities<TestEntityWithoutAnySpecifications>().ToArray();
            Assert.AreEqual(entities.Count, loaded.Length);
            for(int i = 0; i < entities.Count; ++i)
                Assert.AreEqual(entities[i], loaded[i]);            
        }

        [Test, Parallelizable]
        public void InsertMultipleTimesFailsUniqueCheck() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(TestEntities);
            Assert.Throws(new AnyException(), () => entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(TestEntities));
        }

        [Test, Parallelizable]
        public void LoadAllEntities() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();

            TestEntityWithoutAnySpecifications[] source = TestEntities.ToArray();
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(source);
            TestEntityWithoutAnySpecifications[] loaded = entitymanager.Load<TestEntityWithoutAnySpecifications>().ExecuteEntities<TestEntityWithoutAnySpecifications>().ToArray();

            Assert.AreEqual(source.Length, loaded.Length, "Sourcelength does not match loaded length");
            EntityDescriptor descriptor = entitymanager.Model<TestEntityWithoutAnySpecifications>().Descriptor;
            foreach(EntityColumnDescriptor column in descriptor.Columns) {
                if(column.AutoIncrement)
                    continue;

                for(int i = 0; i < loaded.Length; ++i)
                    Assert.AreEqual(column.GetValue(source[i]), column.GetValue(loaded[i]), column.Name + " does not match at index " + i);

            }
        }

        [Test, Parallelizable]
        public void LoadEntitiesWithCriterias() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();

            TestEntityWithoutAnySpecifications[] source = TestEntities.ToArray();
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(source);
            TestEntityWithoutAnySpecifications[] loaded = entitymanager.Load<TestEntityWithoutAnySpecifications>()
                .Where(t => t.Something < 8.0).ExecuteEntities<TestEntityWithoutAnySpecifications>().ToArray();
            foreach(TestEntityWithoutAnySpecifications entity in loaded)
                Assert.Less(entity.Something, 8.0, "entity does not match criteria");
        }

        [Test, Parallelizable]
        public void OrderBy() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();

            TestEntityWithoutAnySpecifications[] source = TestEntities.ToArray();
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(source);
            entitymanager.Load<TestEntityWithoutAnySpecifications>().OrderBy(new OrderByCriteria(DBFunction.Random)).Execute();
        }

        [Test, Parallelizable]
        public void Limit() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();

            TestEntityWithoutAnySpecifications[] source = TestEntities.ToArray();
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(source);
            TestEntityWithoutAnySpecifications[] loaded = entitymanager.Load<TestEntityWithoutAnySpecifications>().Limit(1).ExecuteEntities<TestEntityWithoutAnySpecifications>().ToArray();
            Assert.AreEqual(1, loaded.Length, "only 1 entity should be loaded");
        }

        [Test, Parallelizable]
        public void Offset()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();

            TestEntityWithoutAnySpecifications[] source = TestEntities.ToArray();
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(source);
            TestEntityWithoutAnySpecifications[] loaded = entitymanager.Load<TestEntityWithoutAnySpecifications>().Offset(1).ExecuteEntities<TestEntityWithoutAnySpecifications>().ToArray();
            Assert.AreEqual(source.Length - 1, loaded.Length, "only 1 entity should be loaded");
        }

        [Test, Parallelizable]
        public void LimitAndOffset()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();

            TestEntityWithoutAnySpecifications[] source = TestEntities.ToArray();
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(source);
            TestEntityWithoutAnySpecifications[] loaded = entitymanager.Load<TestEntityWithoutAnySpecifications>().Limit(1).Offset(1).ExecuteEntities<TestEntityWithoutAnySpecifications>().ToArray();
            Assert.AreEqual(1, loaded.Length, "only 1 entity should be loaded");
        }

        [Test, Parallelizable]
        public void ComplexWhere() {
            int boo = 3;
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();

            TestEntityWithoutAnySpecifications[] source = TestEntities.ToArray();
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(source);
            TestEntityWithoutAnySpecifications[] loaded = entitymanager.Load<TestEntityWithoutAnySpecifications>()
                .Where(t => t.Something < 8.0 && t.IntegerValue == boo).ExecuteEntities<TestEntityWithoutAnySpecifications>().ToArray();
        }

        /*[Test]
        public void PreparedWithParameters() {
            IDBClient dbclient = CreateDBClient();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();

            DBParameter<int> parameter = new DBParameter<int>(1);
            TestEntityWithoutAnySpecifications[] source = TestEntities.ToArray();
            entitymanager.Insert(source);
            PreparedLoadEntitiesOperation<TestEntityWithoutAnySpecifications> operation = entitymanager.Load<TestEntityWithoutAnySpecifications>().Where(t => t.Something < 8.0 && t.IntegerValue == parameter.Value).Prepare();
            operation.Execute(3);
        }*/

        [Test, Parallelizable]
        public void UnaryOperations() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();

            TestEntityWithoutAnySpecifications[] source = TestEntities.ToArray();
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(source);
            entitymanager.Update<TestEntityWithoutAnySpecifications>().Set(e => e.BooleanValue == !e.BooleanValue).Execute();

            TestEntityWithoutAnySpecifications[] loaded = entitymanager.Load<TestEntityWithoutAnySpecifications>().ExecuteEntities<TestEntityWithoutAnySpecifications>().ToArray();
            Assert.AreEqual(source.Length, loaded.Length);

            for(int i = 0; i < source.Length;++i) {
                Assert.AreNotEqual(source[i].BooleanValue, loaded[i].BooleanValue, "Operation did not do anything");
            }
        }

        [Test, Parallelizable]
        public void BitwiseOperator() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();

            TestEntityWithoutAnySpecifications[] source = TestEntities.ToArray();
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(source);
            TestEntityWithoutAnySpecifications[] loaded = entitymanager.Load<TestEntityWithoutAnySpecifications>()
                .Where(t => (t.IntegerValue & 1) > 0 && t.Something < 8.0).ExecuteEntities<TestEntityWithoutAnySpecifications>().ToArray();
            foreach(TestEntityWithoutAnySpecifications entity in loaded) {
                Assert.That((entity.IntegerValue & 1) > 0, "Integer not matching bitwise operation");
                Assert.That(entity.Something < 8.0, "something not less than 8.0");
            }
        }

        [Test, Parallelizable]
        public void Aggregates() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();

            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(TestEntities);
            PreparedLoadOperation preparedstatement = entitymanager.Load<TestEntityWithoutAnySpecifications>()
                .GroupBy(EntityField.Create<TestEntityWithoutAnySpecifications>(e => e.Column1))
                .Having(t => DBFunction.Sum(t.IntegerValue) < 30)
                .Prepare();
            Console.WriteLine(preparedstatement.ToString());
            preparedstatement.Execute();
        }

        [Test, Parallelizable]
        public void Join() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();
            entitymanager.Create<OtherTestEntity>();

            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(TestEntities);
            PreparedLoadOperation preparedstatement = entitymanager.Load<TestEntityWithoutAnySpecifications>()
                .Join<OtherTestEntity>((s1, s2) => s1.Column1 == s2.SomeColumn)
                .Prepare();
            Console.WriteLine(preparedstatement.ToString());
            preparedstatement.Execute();            
        }

        [Test, Parallelizable]
        public void DBFieldComparision() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();
            entitymanager.Update<TestEntityWithoutAnySpecifications>().Set(t => t.Column1 == "bla").Where(t => DBFunction.RowID.Int32 == 7).Execute();
        }

        [Test, Parallelizable]
        public void LikeOperator() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();
            entitymanager.Update<TestEntityWithoutAnySpecifications>().Set(t => t.Column1 == "123").Where(t => t.Column1.Like("%something%")).Execute();
        }

        [Test, Parallelizable]
        public void Replace() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();
            entitymanager.Update<TestEntityWithoutAnySpecifications>().Set(t => t.Column1 == "123").Where(t => DBOperators.Replace(t.Column1, "1", "2") == "123").Execute();
        }

        [Test, Parallelizable]
        public void ToLower() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();
            entitymanager.Load<TestEntityWithoutAnySpecifications>().Where(t => t.Column1.ToLower() == "123".ToLower()).Execute();
        }

        [Test, Parallelizable]
        public void ToUpper() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();
            entitymanager.Load<TestEntityWithoutAnySpecifications>().Where(t => t.Column1.ToLower() == "123".ToUpper()).Execute();
        }

        [Test, Parallelizable]
        public void UseTransaction() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            IEntityManager entitymanager = new EntityManager(dbclient);
            using Transaction transaction = entitymanager.Transaction();
            transaction.Rollback();
        }
    }

    public class AnyException : IResolveConstraint {
        public IConstraint Resolve() {
            return new AnyExceptionConstraint();
        }
    }

    public class AnyExceptionConstraint : Constraint {
        public override ConstraintResult ApplyTo<TActual>(TActual actual)
        {
            return new ConstraintResult(this, actual, actual is Exception);
        }
    }
}