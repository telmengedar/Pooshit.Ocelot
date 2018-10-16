using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities;
using NightlyCode.Database.Entities.Operations;
using NightlyCode.Database.Entities.Operations.Fields;
using NightlyCode.Database.Tests.Data;
using NightlyCode.Database.Tests.Models;
using NUnit.Framework;

namespace NightlyCode.Database.Tests.Fields {

    [TestFixture]
    public class ParameterTests {

        [Test]
        public void PrepareCustomParameter() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Update<EnumEntity>().Set(e => e.Enum == DBParameter<TestEnum>.Value).Prepare();
        }

        [Test]
        public void IndexedParameters() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.UpdateSchema<SquareValue>();
            entitymanager.Insert<SquareValue>().Columns(v => v.Value, v => v.Square).Values(0, 0).Execute();
            entitymanager.Update<SquareValue>().Set(v => v.Value == DBParameter.Index(1), v => v.Square == DBParameter.Index(1) * DBParameter.Index(1))
                .Prepare().Execute(4);
            Assert.AreEqual(16, entitymanager.Load<SquareValue>(v => v.Square).ExecuteScalar<int>());
        }
    }
}