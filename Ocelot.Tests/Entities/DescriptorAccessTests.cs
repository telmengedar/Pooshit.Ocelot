using System.Linq;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Schema;
using Pooshit.Ocelot.Schemas;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Models;

namespace Pooshit.Ocelot.Tests.Entities {

    [TestFixture, Parallelizable]
    public class DescriptorAccessTests {


        [Test, Parallelizable]
        public void SetAutoIncrementingPrimaryKey() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new(dbclient);

            entitymanager.Model<ValueModel>().PrimaryKey(m => m.Integer).AutoIncrement(m => m.Integer);

            EntityDescriptor descriptor = entitymanager.Model<ValueModel>().Descriptor;
            Assert.That(descriptor.GetColumnByProperty(nameof(ValueModel.Integer)).PrimaryKey);
            Assert.That(descriptor.GetColumnByProperty(nameof(ValueModel.Integer)).AutoIncrement);

            entitymanager.UpdateSchema<ValueModel>();

            TableDescriptor schema = entitymanager.DBClient.DBInfo.GetSchema(entitymanager.DBClient, descriptor.TableName) as TableDescriptor;
            Assert.NotNull(schema);

            Assert.That(schema.Columns.First(c => c.Name == nameof(ValueModel.Integer).ToLower()).PrimaryKey);
            Assert.That(schema.Columns.First(c => c.Name == nameof(ValueModel.Integer).ToLower()).AutoIncrement);
        }

        [Test, Parallelizable]
        public void SetColumnNullable()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.Model<ValueModel>().Nullable(m => m.Integer);

            EntityDescriptor descriptor = entitymanager.Model<ValueModel>().Descriptor;
            Assert.AreEqual(false, descriptor.GetColumnByProperty(nameof(ValueModel.Integer)).NotNull);

            entitymanager.UpdateSchema<ValueModel>();

            TableDescriptor schema = entitymanager.DBClient.DBInfo.GetSchema(entitymanager.DBClient, descriptor.TableName) as TableDescriptor;
            Assert.NotNull(schema);

            Assert.AreEqual(false, schema.Columns.First(c => c.Name == nameof(ValueModel.Integer).ToLower()).NotNull);
        }

        [Test, Parallelizable]
        public void SetColumnNotNullable()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.Model<ValueModel>().NotNull(m => m.String);

            EntityDescriptor descriptor = entitymanager.Model<ValueModel>().Descriptor;
            Assert.That(descriptor.GetColumnByProperty(nameof(ValueModel.String)).NotNull);

            entitymanager.UpdateSchema<ValueModel>();

            TableDescriptor schema = entitymanager.DBClient.DBInfo.GetSchema(entitymanager.DBClient, descriptor.TableName) as TableDescriptor;
            Assert.NotNull(schema);

            Assert.That(schema.Columns.First(c => c.Name == nameof(ValueModel.String).ToLower()).NotNull);
        }

        [Test, Parallelizable]
        public void SetColumnUnique()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.Model<ValueModel>().Unique(m => m.String);

            EntityDescriptor descriptor = entitymanager.Model<ValueModel>().Descriptor;
            Assert.That(descriptor.GetColumnByProperty(nameof(ValueModel.String)).IsUnique);

            entitymanager.UpdateSchema<ValueModel>();

            TableDescriptor schema = entitymanager.DBClient.DBInfo.GetSchema(entitymanager.DBClient, descriptor.TableName) as TableDescriptor;
            Assert.NotNull(schema);

            Assert.That(schema.Columns.First(c => c.Name == nameof(ValueModel.String).ToLower()).IsUnique);
        }

        [Test, Parallelizable]
        public void SetMultiColumnUnique()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.Model<ValueModel>().Unique(m => m.String, m=>m.Single);

            EntityDescriptor descriptor = entitymanager.Model<ValueModel>().Descriptor;

            entitymanager.UpdateSchema<ValueModel>();

            TableDescriptor schema = entitymanager.DBClient.DBInfo.GetSchema(entitymanager.DBClient, descriptor.TableName) as TableDescriptor;
            Assert.NotNull(schema);

            Assert.AreEqual(1, schema.Uniques.Length);
            Assert.AreEqual(2, schema.Uniques[0].Columns.Count());
            Assert.That(schema.Uniques[0].Columns.Any(c=>c == nameof(ValueModel.String).ToLower()));
            Assert.That(schema.Uniques[0].Columns.Any(c=>c == nameof(ValueModel.Single).ToLower()));
        }

        [Test, Parallelizable]
        public void SetMultipleMultiColumnUnique()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.Model<ValueModel>().Unique(m => m.String, m => m.Single);
            entitymanager.Model<ValueModel>().Unique(m => m.Double, m => m.Integer);

            EntityDescriptor descriptor = entitymanager.Model<ValueModel>().Descriptor;
            Assert.That(!descriptor.GetColumnByProperty(nameof(ValueModel.String)).IsUnique);
            Assert.That(!descriptor.GetColumnByProperty(nameof(ValueModel.Single)).IsUnique);
            Assert.That(!descriptor.GetColumnByProperty(nameof(ValueModel.Double)).IsUnique);
            Assert.That(!descriptor.GetColumnByProperty(nameof(ValueModel.Integer)).IsUnique);

            entitymanager.UpdateSchema<ValueModel>();

            TableDescriptor schema = entitymanager.DBClient.DBInfo.GetSchema(entitymanager.DBClient, descriptor.TableName) as TableDescriptor;
            Assert.NotNull(schema);

            Assert.AreEqual(2, schema.Uniques.Length);
            Assert.AreEqual(2, schema.Uniques[0].Columns.Count());
            Assert.AreEqual(2, schema.Uniques[1].Columns.Count());
            Assert.That(schema.Uniques[0].Columns.Any(c => c == nameof(ValueModel.String).ToLower()));
            Assert.That(schema.Uniques[0].Columns.Any(c => c == nameof(ValueModel.Single).ToLower()));
            Assert.That(schema.Uniques[1].Columns.Any(c => c == nameof(ValueModel.Double).ToLower()));
            Assert.That(schema.Uniques[1].Columns.Any(c => c == nameof(ValueModel.Integer).ToLower()));
        }

        [Test, Parallelizable]
        public void SetColumnIndex()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.Model<ValueModel>().Index("wat", m => m.String, m => m.Integer);

            EntityDescriptor descriptor = entitymanager.Model<ValueModel>().Descriptor;
            Assert.That(descriptor.Indices.Count()==1);
            IndexDescriptor index = descriptor.Indices.First();
            Assert.AreEqual("wat", index.Name);
            Assert.That(index.Columns.Any(c => c == nameof(ValueModel.String).ToLower()));
            Assert.That(index.Columns.Any(c => c == nameof(ValueModel.Integer).ToLower()));

            entitymanager.UpdateSchema<ValueModel>();

            TableDescriptor schema = entitymanager.DBClient.DBInfo.GetSchema(entitymanager.DBClient, descriptor.TableName) as TableDescriptor;
            Assert.NotNull(schema);

            Assert.That(schema.Indices.Length == 1);
            index = schema.Indices.First();
            Assert.AreEqual("wat", index.Name);
            Assert.That(index.Columns.Any(c => c == nameof(ValueModel.String).ToLower()));
            Assert.That(index.Columns.Any(c => c == nameof(ValueModel.Integer).ToLower()));
        }

        [Test, Parallelizable]
        public void SetColumnDefault() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.Model<ValueModel>().Default(m => m.Integer, 7);

            EntityDescriptor descriptor = entitymanager.Model<ValueModel>().Descriptor;
            Assert.AreEqual(7, descriptor.GetColumn(nameof(ValueModel.Integer).ToLower()).DefaultValue);

            entitymanager.UpdateSchema<ValueModel>();

            TableDescriptor schema = entitymanager.DBClient.DBInfo.GetSchema(entitymanager.DBClient, descriptor.TableName) as TableDescriptor;
            Assert.NotNull(schema);
            Assert.AreEqual("7", schema.Columns.Single(c => c.Name == nameof(ValueModel.Integer).ToLower()).DefaultValue);
        }

        [Test, Parallelizable]
        public void SetColumnName() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.Model<ValueModel>().Column(m => m.Integer, "winteger_green");

            EntityDescriptor descriptor = entitymanager.Model<ValueModel>().Descriptor;
            Assert.AreEqual("winteger_green", descriptor.Columns.Single(c => c.Property.Name == nameof(ValueModel.Integer)).Name);

            entitymanager.UpdateSchema<ValueModel>();
            TableDescriptor schema = entitymanager.DBClient.DBInfo.GetSchema(entitymanager.DBClient, descriptor.TableName) as TableDescriptor;
            Assert.NotNull(schema);
            Assert.That(schema.Columns.Any(c => c.Name == "winteger_green"));
        }

        [Test, Parallelizable]
        public void SetTableName()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.Model<ValueModel>().Table("value");

            EntityDescriptor descriptor = entitymanager.Model<ValueModel>().Descriptor;
            Assert.AreEqual("value", descriptor.TableName);

            entitymanager.UpdateSchema<ValueModel>();

            TableDescriptor schema = entitymanager.DBClient.DBInfo.GetSchema(entitymanager.DBClient, "value") as TableDescriptor;
            Assert.NotNull(schema);

            Assert.AreEqual("value", schema.Name);
        }
    }
}