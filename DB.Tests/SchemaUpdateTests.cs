using System.Linq;
using NightlyCode.DB.Clients;
using NightlyCode.DB.Entities.Schema;
using NightlyCode.DB.Providers;
using NightlyCode.DB.Tests.Schema;
using NUnit.Framework;

namespace NightlyCode.DB.Tests {

    [TestFixture]
    public class SchemaUpdateTests {
        readonly SchemaCreator creator = new SchemaCreator();
        readonly SchemaUpdater updater = new SchemaUpdater();

        [Test]
        public void AddColumns() {
            IDBClient dbclient = Providers.SQLiteProvider.CreateSQLite(null, false);
            creator.Create(typeof(OriginalEntity), dbclient);
            updater.Update<AddEntity>(dbclient);

            TableDescriptor descriptor = dbclient.DBInfo.GetSchema<AddEntity>(dbclient) as TableDescriptor;
            Assert.That(descriptor.Columns.Any(c => c.Name == "field1"));
            Assert.That(descriptor.Columns.Any(c => c.Name == "field2"));
            Assert.That(descriptor.Columns.Any(c => c.Name == "field3"));
            Assert.That(descriptor.Columns.Any(c => c.Name == "field4"));
            Assert.That(descriptor.Indices.Any(i => i.Name == "field3" && i.Columns.Any(c => c == "field3")));
            Assert.That(descriptor.Indices.Any(i => i.Name == "field4" && i.Columns.Any(c => c == "field4")));
        }

        [Test]
        public void RemoveColumns() {
            IDBClient dbclient = SQLiteProvider.CreateSQLite(null, false);
            creator.Create(typeof(OriginalEntity), dbclient);
            updater.Update<EntityWithLessFields>(dbclient);

            TableDescriptor descriptor = dbclient.DBInfo.GetSchema<EntityWithLessFields>(dbclient) as TableDescriptor;
            Assert.That(descriptor.Columns.Any(c => c.Name == "field1"));
            Assert.That(descriptor.Columns.Any(c => c.Name == "field2"));
            Assert.That(descriptor.Columns.All(c => c.Name != "field3"));
            Assert.That(descriptor.Columns.All(c => c.Name != "field4"));

            Assert.That(!descriptor.Indices.Any(i => i.Name == "field3" && i.Columns.Any(c => c == "field3")));
            Assert.That(!descriptor.Indices.Any(i => i.Name == "field4" && i.Columns.Any(c => c == "field4")));
        }

        [Test]
        public void AlterColumns() {
            IDBClient dbclient = SQLiteProvider.CreateSQLite(null, false);
            creator.Create(typeof(OriginalEntity), dbclient);
            updater.Update<AlteredEntity>(dbclient);

            TableDescriptor descriptor = dbclient.DBInfo.GetSchema<AlteredEntity>(dbclient) as TableDescriptor;
            Assert.That(descriptor.Columns.Any(c => c.Name == "field1"));
            Assert.That(descriptor.Columns.Any(c => c.Name == "field2"));
            Assert.That(descriptor.Columns.Any(c => c.Name == "field3" && c.Type == "TEXT"));
            Assert.That(!descriptor.Indices.Any(i => i.Name == "field3" && i.Columns.Any(c => c == "field3")));
            Assert.That(!descriptor.Indices.Any(i => i.Name == "field4" && i.Columns.Any(c => c == "field4")));
        }
    }

}