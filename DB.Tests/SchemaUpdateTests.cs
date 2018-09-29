using System.Linq;
using NightlyCode.DB.Clients;
using NightlyCode.DB.Entities.Descriptors;
using NightlyCode.DB.Entities.Schema;
using NightlyCode.DB.Tests.Schema;
using NUnit.Framework;

namespace NightlyCode.DB.Tests {

    [TestFixture]
    public class SchemaUpdateTests {
        readonly EntityDescriptorCache modelcache = new EntityDescriptorCache();
        SchemaCreator creator;
        SchemaUpdater updater;

        [SetUp]
        public void Setup() {
            creator = new SchemaCreator(modelcache);
            updater = new SchemaUpdater(modelcache);
        }

        [Test]
        public void AddColumns() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            creator.Create(typeof(OriginalEntity), dbclient);
            updater.Update<AddEntity>(dbclient);

            TableDescriptor descriptor = dbclient.DBInfo.GetSchema(dbclient, modelcache.Get<AddEntity>().TableName) as TableDescriptor;
            Assert.That(descriptor.Columns.Any(c => c.Name == "field1"));
            Assert.That(descriptor.Columns.Any(c => c.Name == "field2"));
            Assert.That(descriptor.Columns.Any(c => c.Name == "field3"));
            Assert.That(descriptor.Columns.Any(c => c.Name == "field4"));
            Assert.That(descriptor.Indices.Any(i => i.Name == "field3" && i.Columns.Any(c => c == "field3")));
            Assert.That(descriptor.Indices.Any(i => i.Name == "field4" && i.Columns.Any(c => c == "field4")));
        }

        [Test]
        public void RemoveColumns() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            creator.Create(typeof(OriginalEntity), dbclient);
            updater.Update<EntityWithLessFields>(dbclient);

            TableDescriptor descriptor = dbclient.DBInfo.GetSchema(dbclient, modelcache.Get<EntityWithLessFields>().TableName) as TableDescriptor;
            Assert.That(descriptor.Columns.Any(c => c.Name == "field1"));
            Assert.That(descriptor.Columns.Any(c => c.Name == "field2"));
            Assert.That(descriptor.Columns.All(c => c.Name != "field3"));
            Assert.That(descriptor.Columns.All(c => c.Name != "field4"));

            Assert.That(!descriptor.Indices.Any(i => i.Name == "field3" && i.Columns.Any(c => c == "field3")));
            Assert.That(!descriptor.Indices.Any(i => i.Name == "field4" && i.Columns.Any(c => c == "field4")));
        }

        [Test]
        public void AlterColumns() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            creator.Create(typeof(OriginalEntity), dbclient);
            updater.Update<AlteredEntity>(dbclient);

            TableDescriptor descriptor = dbclient.DBInfo.GetSchema(dbclient, modelcache.Get<AlteredEntity>().TableName) as TableDescriptor;
            Assert.That(descriptor.Columns.Any(c => c.Name == "field1"));
            Assert.That(descriptor.Columns.Any(c => c.Name == "field2"));
            Assert.That(descriptor.Columns.Any(c => c.Name == "field3" && c.Type == "TEXT"));
            Assert.That(!descriptor.Indices.Any(i => i.Name == "field3" && i.Columns.Any(c => c == "field3")));
            Assert.That(!descriptor.Indices.Any(i => i.Name == "field4" && i.Columns.Any(c => c == "field4")));
        }
    }

}