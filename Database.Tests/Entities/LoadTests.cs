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

    }
}