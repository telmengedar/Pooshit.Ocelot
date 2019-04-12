using NightlyCode.Database.Entities;
using NightlyCode.Database.Entities.Operations;
using NightlyCode.Database.Tests.Data;
using NightlyCode.Database.Tests.Models;
using NUnit.Framework;

namespace NightlyCode.Database.Tests.Entities {

    [TestFixture, Parallelizable]
    public class LoadTests {

        [Test, Parallelizable]
        public void MultiJoinWithAlias() {
            IEntityManager entitymanager = new EntityManager(TestData.CreateDatabaseAccess());
            entitymanager.UpdateSchema<ValueModel>();
            entitymanager.UpdateSchema<EntityWithLessFields>();

            entitymanager.LoadEntities<ValueModel>()
                .Join<EntityWithLessFields>((v, e) => v.Integer == e.Field1 && v.String == e.Field2)
                .Join<EntityWithLessFields>((v, e) => v.Integer == e.Field1 && v.String == e.Field2)
                .Execute();
        }

        [Test, Parallelizable]
        public void MultiJoinNonFluent()
        {
            IEntityManager entitymanager = new EntityManager(TestData.CreateDatabaseAccess());
            entitymanager.UpdateSchema<ValueModel>();
            entitymanager.UpdateSchema<EntityWithLessFields>();

            LoadEntitiesOperation<ValueModel> query=entitymanager.LoadEntities<ValueModel>();
            query.Join<EntityWithLessFields>((v, e) => v.Integer == e.Field1 && v.String == e.Field2);
            query.Join<EntityWithLessFields>((v, e) => v.Integer == e.Field1 && v.String == e.Field2);
            query.Execute();
        }

        [Test, Parallelizable]
        public void MultiJoinWithDerivedClasses()
        {
            IEntityManager entitymanager = new EntityManager(TestData.CreateDatabaseAccess());
            entitymanager.UpdateSchema<ActiveData>();
            entitymanager.UpdateSchema<ObjectIndexDecimal>();
            entitymanager.UpdateSchema<ObjectIndexString>();

            LoadEntitiesOperation<ActiveData> query = entitymanager.LoadEntities<ActiveData>();
            query.Join<ObjectIndexDecimal>((v, e) => v.ID == e.Object && e.Value == 0.0m);
            query.Join<ObjectIndexString>((v, e) => v.ID == e.Object && e.Value == "");
            query.Execute();
        }

        [Test, Parallelizable]
        public void LoadFromJoin()
        {
            IEntityManager entitymanager = new EntityManager(TestData.CreateDatabaseAccess());
            entitymanager.UpdateSchema<ActiveData>();
            entitymanager.UpdateSchema<ObjectIndexDecimal>();
            entitymanager.UpdateSchema<ObjectIndexString>();

            LoadEntitiesOperation<ObjectIndexString> load = entitymanager.LoadEntities<ObjectIndexString>();
            load.Where(i => i.Key == "OrderId" && i.Value == "d1316bc9-0381-471d-908f-4459eca24b0c");
            load.Join<ObjectIndexDecimal>((i1, i2) => i1.Object == i2.Object && i2.Key == "Deleted" && i2.Value == 0);
            load.Join<ActiveData>((i, d) => i.Object == d.ID && d.Class == "Position");
            load.Execute<ActiveData>();
        }

        [Test, Parallelizable]
        public void JoinSameTable() {
            IEntityManager entitymanager = new EntityManager(TestData.CreateDatabaseAccess());
            entitymanager.UpdateSchema<ActiveData>();
            entitymanager.UpdateSchema<ObjectIndexDecimal>();
            entitymanager.UpdateSchema<ObjectIndexString>();

            LoadEntitiesOperation<ObjectIndexDecimal> load = entitymanager.LoadEntities<ObjectIndexDecimal>();
            load.Where(i => i.Key == "Key" && i.Value == 42);
            load.Join<ObjectIndexDecimal>((i1, i2) => i1.Object == i2.Object && i2.Key == "Kee" && i2.Value == 70);
            Assert.That(load.Prepare().CommandText.Contains("ON t.\"object\" = j0.\"object\" AND j0.\"key\" = @1 AND j0.\"value\" = @2"));
        }
    }
}