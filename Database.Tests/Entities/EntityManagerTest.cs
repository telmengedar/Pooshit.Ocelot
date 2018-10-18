using System;
using System.Collections.Generic;
using System.Linq;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations;
using NightlyCode.Database.Entities.Operations.Fields;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Tests.Data;
using NightlyCode.Database.Tests.Models;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace NightlyCode.Database.Tests.Entities {

    [TestFixture]
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

        [Test]
        public void TestEntityCreation() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();
            Assert.That(dbclient.DBInfo.CheckIfTableExists(dbclient, "testentitywithoutanyspecifications"));
        }

        [Test]
        public void InsertEntities() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(TestEntities.ToArray());
        }

        [Test]
        public void InsertStatement() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();

            PreparedOperation operation = entitymanager.Insert<TestEntityWithoutAnySpecifications>()
                .Columns(t => t.Column1, t => t.BooleanValue, t => t.IntegerValue, t => t.Something)
                .Prepare();

            foreach (TestEntityWithoutAnySpecifications entity in TestEntities)
                operation.Execute(entity.Column1, entity.BooleanValue, entity.IntegerValue, entity.Something);

            Assert.AreEqual(TestEntities.Count(), entitymanager.Load<TestEntityWithoutAnySpecifications>(DBFunction.Count).ExecuteScalar<int>());
        }

        [Test]
        public void TestContains() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(TestEntities);

            int[] array = {2, 3};
            TestEntityWithoutAnySpecifications[] result = entitymanager.LoadEntities<TestEntityWithoutAnySpecifications>().Where(e => array.Contains(e.IntegerValue)).Execute().ToArray();
            Assert.AreEqual(2, result.Length);
        }

        [Test]
        public void TestInsertWithReturn() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();

            TestEntityWithoutAnySpecifications[] testentities = TestEntities.ToArray();

            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(testentities);
            foreach(TestEntityWithoutAnySpecifications entity in testentities)
                Assert.AreNotEqual(0, entity.ThePrimaryKey);

            foreach(TestEntityWithoutAnySpecifications loaded in entitymanager.LoadEntities<TestEntityWithoutAnySpecifications>().Execute()) {
                TestEntityWithoutAnySpecifications original = testentities.FirstOrDefault(t => t.ThePrimaryKey == loaded.ThePrimaryKey);
                Assert.NotNull(original);

                Assert.AreEqual(original.BooleanValue, loaded.BooleanValue);
                Assert.AreEqual(original.Column1, loaded.Column1);
                Assert.AreEqual(original.IntegerValue, loaded.IntegerValue);
                Assert.AreEqual(original.Something, loaded.Something);
            }
        }

        [Test]
        public void TestUpdate() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();
            TestEntityWithoutAnySpecifications[] entities = TestEntities.ToArray();
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(entities);
            entities[1].Column1 = "Changed";
            entitymanager.UpdateEntities<TestEntityWithoutAnySpecifications>().Execute(entities);
            TestEntityWithoutAnySpecifications[] loaded = entitymanager.LoadEntities<TestEntityWithoutAnySpecifications>().Execute().ToArray();
            Assert.AreEqual(entities.Length, loaded.Length);
            for(int i=0;i<entities.Length;++i)
                Assert.AreEqual(entities[i], loaded[i]);
        }
        
        [Test]
        public void TestSave() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();
            List<TestEntityWithoutAnySpecifications> entities = new List<TestEntityWithoutAnySpecifications>(TestEntities);
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(entities);
            entities[1].Column1 = "Changed";
            entities.Add(new TestEntityWithoutAnySpecifications("added1", 1, 4.3));
            entitymanager.Save<TestEntityWithoutAnySpecifications>(entities);
            TestEntityWithoutAnySpecifications[] loaded = entitymanager.LoadEntities<TestEntityWithoutAnySpecifications>().Execute().ToArray();
            Assert.AreEqual(entities.Count, loaded.Length);
            for(int i = 0; i < entities.Count; ++i)
                Assert.AreEqual(entities[i], loaded[i]);            
        }

        [Test]
        public void InsertMultipleTimesFailsUniqueCheck() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(TestEntities);
            Assert.Throws(new AnyException(), () => entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(TestEntities));
        }

        [Test]
        public void LoadAllEntities() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();

            TestEntityWithoutAnySpecifications[] source = TestEntities.ToArray();
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(source);
            TestEntityWithoutAnySpecifications[] loaded = entitymanager.LoadEntities<TestEntityWithoutAnySpecifications>().Execute().ToArray();

            Assert.AreEqual(source.Length, loaded.Length, "Sourcelength does not match loaded length");
            EntityDescriptor descriptor = entitymanager.Model<TestEntityWithoutAnySpecifications>().Descriptor;
            foreach(EntityColumnDescriptor column in descriptor.Columns) {
                if(column.AutoIncrement)
                    continue;

                for(int i = 0; i < loaded.Length; ++i)
                    Assert.AreEqual(column.GetValue(source[i]), column.GetValue(loaded[i]), column.Name + " does not match at index " + i);

            }
        }

        [Test]
        public void LoadEntitiesWithCriterias() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();

            TestEntityWithoutAnySpecifications[] source = TestEntities.ToArray();
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(source);
            TestEntityWithoutAnySpecifications[] loaded = entitymanager.LoadEntities<TestEntityWithoutAnySpecifications>().Where(t => t.Something < 8.0).Execute().ToArray();
            foreach(TestEntityWithoutAnySpecifications entity in loaded)
                Assert.Less(entity.Something, 8.0, "entity does not match criteria");
        }

        [Test]
        public void OrderBy() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();

            TestEntityWithoutAnySpecifications[] source = TestEntities.ToArray();
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(source);
            entitymanager.LoadEntities<TestEntityWithoutAnySpecifications>().OrderBy(new OrderByCriteria(DBFunction.Random)).Execute();
        }

        [Test]
        public void Limit() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();

            TestEntityWithoutAnySpecifications[] source = TestEntities.ToArray();
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(source);
            TestEntityWithoutAnySpecifications[] loaded = entitymanager.LoadEntities<TestEntityWithoutAnySpecifications>().Limit(1).Execute().ToArray();
            Assert.AreEqual(1, loaded.Length, "only 1 entity should be loaded");
        }

        [Test]
        public void Offset()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();

            TestEntityWithoutAnySpecifications[] source = TestEntities.ToArray();
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(source);
            TestEntityWithoutAnySpecifications[] loaded = entitymanager.LoadEntities<TestEntityWithoutAnySpecifications>().Offset(1).Execute().ToArray();
            Assert.AreEqual(source.Length - 1, loaded.Length, "only 1 entity should be loaded");
        }

        [Test]
        public void LimitAndOffset()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();

            TestEntityWithoutAnySpecifications[] source = TestEntities.ToArray();
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(source);
            TestEntityWithoutAnySpecifications[] loaded = entitymanager.LoadEntities<TestEntityWithoutAnySpecifications>().Limit(1).Offset(1).Execute().ToArray();
            Assert.AreEqual(1, loaded.Length, "only 1 entity should be loaded");
        }

        [Test]
        public void ComplexWhere() {
            int boo = 3;
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();

            TestEntityWithoutAnySpecifications[] source = TestEntities.ToArray();
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(source);
            TestEntityWithoutAnySpecifications[] loaded = entitymanager.LoadEntities<TestEntityWithoutAnySpecifications>().Where(t => t.Something < 8.0 && t.IntegerValue == boo).Execute().ToArray();
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

        [Test]
        public void UnaryOperations() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();

            TestEntityWithoutAnySpecifications[] source = TestEntities.ToArray();
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(source);
            entitymanager.Update<TestEntityWithoutAnySpecifications>().Set(e => e.BooleanValue == !e.BooleanValue).Execute();

            TestEntityWithoutAnySpecifications[] loaded = entitymanager.LoadEntities<TestEntityWithoutAnySpecifications>().Execute().ToArray();
            Assert.AreEqual(source.Length, loaded.Length);

            for(int i = 0; i < source.Length;++i) {
                Assert.AreNotEqual(source[i].BooleanValue, loaded[i].BooleanValue, "Operation did not do anything");
            }
        }

        [Test]
        public void BitwiseOperator() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();

            TestEntityWithoutAnySpecifications[] source = TestEntities.ToArray();
            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(source);
            TestEntityWithoutAnySpecifications[] loaded = entitymanager.LoadEntities<TestEntityWithoutAnySpecifications>().Where(t => (t.IntegerValue & 1) > 0 && t.Something < 8.0).Execute().ToArray();
            foreach(TestEntityWithoutAnySpecifications entity in loaded) {
                Assert.That((entity.IntegerValue & 1) > 0, "Integer not matching bitwise operation");
                Assert.That(entity.Something < 8.0, "something not less than 8.0");
            }
        }

        [Test]
        public void Aggregates() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();

            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(TestEntities);
            PreparedLoadEntitiesOperation<TestEntityWithoutAnySpecifications> preparedstatement = entitymanager.LoadEntities<TestEntityWithoutAnySpecifications>()
                                                                                                            .GroupBy(EntityField.Create<TestEntityWithoutAnySpecifications>(e => e.Column1))
                                                                                                            .Having(t => DBFunction.Sum(EntityField.Create<TestEntityWithoutAnySpecifications>(e=>e.IntegerValue)) < Constant.Create(30))
                                                                                                            .Prepare();
            Console.WriteLine(preparedstatement.ToString());
            preparedstatement.Execute();
        }

        [Test]
        public void Join() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();
            entitymanager.Create<OtherTestEntity>();

            entitymanager.InsertEntities<TestEntityWithoutAnySpecifications>().Execute(TestEntities);
            PreparedLoadEntitiesOperation<TestEntityWithoutAnySpecifications> preparedstatement = entitymanager.LoadEntities<TestEntityWithoutAnySpecifications>().Join<OtherTestEntity>((s1, s2) => s1.Column1== s2.SomeColumn).Prepare();
            Console.WriteLine(preparedstatement.ToString());
            preparedstatement.Execute();            
        }

        [Test]
        public void DBFieldComparision() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();
            entitymanager.Update<TestEntityWithoutAnySpecifications>().Set(t => t.Column1 == "bla").Where(t => DBFunction.RowID.Int32 == 7).Execute();
        }

        [Test]
        public void LikeOperator() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();
            entitymanager.Update<TestEntityWithoutAnySpecifications>().Set(t => t.Column1 == "123").Where(t => t.Column1.Like("%something%")).Execute();
        }

        [Test]
        public void Replace() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();
            entitymanager.Update<TestEntityWithoutAnySpecifications>().Set(t => t.Column1 == "123").Where(t => DBOperators.Replace(t.Column1, "1", "2") == "123").Execute();
        }

        [Test]
        public void ToLower() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();
            entitymanager.LoadEntities<TestEntityWithoutAnySpecifications>().Where(t => t.Column1.ToLower() == "123".ToLower()).Execute();
        }

        [Test]
        public void ToUpper() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();
            entitymanager.LoadEntities<TestEntityWithoutAnySpecifications>().Where(t => t.Column1.ToLower() == "123".ToUpper()).Execute();
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