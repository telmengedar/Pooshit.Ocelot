using System.Linq;
using NightlyCode.DB.Entities.Descriptors;
using NUnit.Framework;

namespace NightlyCode.DB.Tests.Entities
{

    [TestFixture]
    public class EntityDescriptorTest
    {

        [Test]
        public void TestCreationWithoutNameSpecifications() {
            EntityDescriptor descriptor = new EntityDescriptorCache().Get<TestEntityWithoutAnySpecifications>();
            Assert.AreEqual(typeof(TestEntityWithoutAnySpecifications).Name.ToLower(), descriptor.TableName);
            Assert.AreEqual(5, descriptor.Columns.Count());
            Assert.NotNull(descriptor.PrimaryKeyColumn);
            Assert.AreEqual("theprimarykey", descriptor.PrimaryKeyColumn.Name);
            Assert.That(descriptor.GetColumn("column1").IsUnique);
            Assert.AreEqual(1, descriptor.Indices.Count(), "Entity must have exactly 1 index specification");
        }

    }
}