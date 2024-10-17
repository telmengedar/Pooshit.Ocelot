using System;
using System.Linq;
using NUnit.Framework;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Tests.Models;

namespace Pooshit.Ocelot.Tests.Entities
{

    [TestFixture]
    public class EntityDescriptorTest
    {

        [Test]
        public void TestCreationWithoutNameSpecifications() {
            EntityDescriptor descriptor = new EntityDescriptorCache().Get<TestEntityWithoutAnySpecifications>();
            Assert.AreEqual(nameof(TestEntityWithoutAnySpecifications).ToLower(), descriptor.TableName);
            Assert.AreEqual(5, descriptor.Columns.Count());
            Assert.NotNull(descriptor.PrimaryKeyColumn);
            Assert.AreEqual("theprimarykey", descriptor.PrimaryKeyColumn.Name);
            Assert.That(descriptor.GetColumn("column1").IsUnique);
            Assert.AreEqual(1, descriptor.Indices.Count(), "Entity must have exactly 1 index specification");
        }

        [Test]
        public void NullableProperties() {
            EntityDescriptor descriptor = new EntityDescriptorCache().Get<CampaignItemClick>();
            EntityColumnDescriptor column=descriptor.GetColumn("latitude");
            Assert.AreEqual(nameof(Double).ToLower(), column.Type);
        }
    }
}