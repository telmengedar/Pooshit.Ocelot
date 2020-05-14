using System.Linq;
using Moq;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Schema;
using NightlyCode.Database.Info;
using NightlyCode.Database.Tests.Data;
using NightlyCode.Database.Tests.Entities;
using NightlyCode.Database.Tests.Models;
using NUnit.Framework;

namespace NightlyCode.Database.Tests {

    [TestFixture, Parallelizable]
    public class SchemaUpdateTests {
        readonly EntityDescriptorCache modelcache = new EntityDescriptorCache();
        SchemaCreator creator;
        SchemaUpdater updater;

        [SetUp]
        public void Setup() {
            creator = new SchemaCreator(modelcache);
            updater = new SchemaUpdater(modelcache);
        }

        [Test, Parallelizable]
        public void AddColumns() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            creator.Create(typeof(OriginalEntity), dbclient);
            updater.Update<AddEntity>(dbclient);

            TableDescriptor descriptor = dbclient.DBInfo.GetSchema(dbclient, modelcache.Get<AddEntity>().TableName) as TableDescriptor;
            Assert.NotNull(descriptor);
            Assert.That(descriptor.Columns.Any(c => c.Name == "field1"));
            Assert.That(descriptor.Columns.Any(c => c.Name == "field2"));
            Assert.That(descriptor.Columns.Any(c => c.Name == "field3"));
            Assert.That(descriptor.Columns.Any(c => c.Name == "field4"));
            Assert.That(descriptor.Indices.Any(i => i.Name == "field3" && i.Columns.Any(c => c == "field3")));
            Assert.That(descriptor.Indices.Any(i => i.Name == "field4" && i.Columns.Any(c => c == "field4")));
        }

        [Test, Parallelizable]
        public void RemoveColumns() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            creator.Create(typeof(OriginalEntity), dbclient);
            updater.Update<EntityWithLessFields>(dbclient);

            TableDescriptor descriptor = dbclient.DBInfo.GetSchema(dbclient, modelcache.Get<EntityWithLessFields>().TableName) as TableDescriptor;
            Assert.NotNull(descriptor);
            Assert.That(descriptor.Columns.Any(c => c.Name == "field1"));
            Assert.That(descriptor.Columns.Any(c => c.Name == "field2"));
            Assert.That(descriptor.Columns.All(c => c.Name != "field3"));
            Assert.That(descriptor.Columns.All(c => c.Name != "field4"));

            Assert.That(!descriptor.Indices.Any(i => i.Name == "field3" && i.Columns.Any(c => c == "field3")));
            Assert.That(!descriptor.Indices.Any(i => i.Name == "field4" && i.Columns.Any(c => c == "field4")));
        }

        [Test, Parallelizable]
        public void AlterColumns() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            creator.Create(typeof(OriginalEntity), dbclient);
            updater.Update<AlteredEntity>(dbclient);

            TableDescriptor descriptor = dbclient.DBInfo.GetSchema(dbclient, modelcache.Get<AlteredEntity>().TableName) as TableDescriptor;
            Assert.NotNull(descriptor);
            Assert.That(descriptor.Columns.Any(c => c.Name == "field1"));
            Assert.That(descriptor.Columns.Any(c => c.Name == "field2"));
            Assert.That(descriptor.Columns.Any(c => c.Name == "field3" && c.Type == "TEXT"));
            Assert.That(!descriptor.Indices.Any(i => i.Name == "field3" && i.Columns.Any(c => c == "field3")));
            Assert.That(!descriptor.Indices.Any(i => i.Name == "field4" && i.Columns.Any(c => c == "field4")));
        }

        [Test, Parallelizable]
        public void AlterColumnsInFilledTable() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();
            for(int i = 0; i < 50; ++i) {
                entitymanager.Insert<ValueModel>().Columns(c => c.Integer, c => c.Single, c => c.Double, c => c.String)
                    .Values(i, (float)i, (double)i, i.ToString()).Execute();
            }

            entitymanager.Model<ValueModel>().Column(c => c.Integer, "Gangolf");
            entitymanager.UpdateSchema<ValueModel>();

            ValueModel[] data = entitymanager.LoadEntities<ValueModel>().Execute().ToArray();
            Assert.AreEqual(50, data.Length);
            foreach(ValueModel value in data)
                Assert.AreEqual(0, value.Integer);
        }

        [Test, Parallelizable]
        public void ChangeUniqueSpecifiers() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.Model<ValueModel>().Unique(m => m.String, m => m.Double).Unique(m => m.Double, m => m.Integer).Unique(m => m.Integer, m => m.Single);
            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.Model<ValueModel>().DropUnique(m => m.Double, m => m.Integer).Unique(m => m.Single, m => m.String);
            entitymanager.UpdateSchema<ValueModel>();

            TableDescriptor descriptor = dbclient.DBInfo.GetSchema(dbclient, modelcache.Get<ValueModel>().TableName) as TableDescriptor;
            Assert.NotNull(descriptor);
            Assert.AreEqual(3, descriptor.Uniques.Length);
            Assert.That(descriptor.Uniques.Any(u => u.Columns.SequenceEqual(new[] { "string", "double" })));
            Assert.That(descriptor.Uniques.Any(u => u.Columns.SequenceEqual(new[] { "integer", "single" })));
            Assert.That(descriptor.Uniques.Any(u => u.Columns.SequenceEqual(new[] { "single", "string" })));
            Assert.That(descriptor.Uniques.All(u => !u.Columns.SequenceEqual(new[] { "double", "integer" })));
        }

        [Test, Parallelizable]
        public void AddMissingComplexUniqueConstraint() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.Model<ValueModel>().Unique(m => m.Single, m => m.String);
            entitymanager.UpdateSchema<ValueModel>();

            TableDescriptor descriptor = dbclient.DBInfo.GetSchema(dbclient, modelcache.Get<ValueModel>().TableName) as TableDescriptor;
            Assert.NotNull(descriptor);
            Assert.AreEqual(1, descriptor.Uniques.Length);
            Assert.That(descriptor.Uniques.Any(u => u.Columns.SequenceEqual(new[] { "single", "string" })));
        }

        [Test, Parallelizable]
        public void AddGuidColumn() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Model<GuidEntity>().DropColumn(e => e.Guid);
            entitymanager.UpdateSchema<GuidEntity>();

            for(int i = 0; i < 5; ++i)
                entitymanager.Insert<GuidEntity>().Columns(e => e.SomeValue).Values(i).Execute();

            entitymanager.Model<GuidEntity>().AddColumn(e => e.Guid);
            entitymanager.UpdateSchema<GuidEntity>();

            Assert.DoesNotThrow(() => entitymanager.LoadEntities<GuidEntity>().Execute().ToArray());
        }

        [Test, Parallelizable]
        public void DontCreateViewTwice() {
            Mock<IDBInfo> dbinfomock = new Mock<IDBInfo>();
            dbinfomock.Setup(i => i.DropView(It.IsAny<IDBClient>(), It.IsAny<ViewDescriptor>())).Callback(() => { Assert.Fail("View should not get dropped"); });
            dbinfomock.Setup(i => i.GetSchema(It.IsAny<IDBClient>(), It.IsAny<string>())).Returns(new ViewDescriptor("testview") {
                SQL = "SELECT 2 AS first,\n\t3+4 AS second,\n\t'test' AS third"
            });

            Mock<IDBClient> clientmock = new Mock<IDBClient>();
            clientmock.SetupGet(c => c.DBInfo).Returns(dbinfomock.Object);

            EntityDescriptorCache cache = new EntityDescriptorCache();
            SchemaUpdater updater = new SchemaUpdater(cache);

            updater.Update<TestView>(clientmock.Object);
        }
    }

}