using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities;
using NightlyCode.Database.Entities.Operations;
using NightlyCode.Database.Entities.Operations.Fields;
using NightlyCode.Database.Tests.Models;
using NUnit.Framework;

namespace NightlyCode.Database.Tests.Fields {

    [TestFixture]
    public class ParameterTests {

        [Test]
        public void PrepareCustomParameter() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            PreparedOperation operation = entitymanager.Update<EnumEntity>().Set(e => e.Enum == DBParameter<TestEnum>.Value).Prepare();
        }
    }
}