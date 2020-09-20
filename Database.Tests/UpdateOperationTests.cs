using System.Threading.Tasks;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Fields;
using NightlyCode.Database.Tests.Data;
using NightlyCode.Database.Tests.Models;
using NUnit.Framework;

namespace NightlyCode.Database.Tests {

    [TestFixture, Parallelizable]
    public class UpdateOperationTests {

        [Test, Parallelizable]
        [Description("Updates an entity with an operation containing a constant expression ensuring that the constant parameter is not overridden")]
        public async Task UpdateWithConstantExpression() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();
            await entitymanager.Insert<ValueModel>().Columns(v => v.String, v => v.Integer, v => v.Double, v => v.Single).Values("lala", 5, 2.0, 3.0f).ExecuteAsync();
            PreparedOperation operation=entitymanager.Update<ValueModel>().Set(v => v.Integer == v.Integer + 1, v=>v.String==DBParameter.String).Prepare();
            await operation.ExecuteAsync("Bolle");
            ValueModel value = await entitymanager.Load<ValueModel>().ExecuteEntityAsync<ValueModel>();
            Assert.NotNull(value);
            Assert.AreEqual("Bolle", value.String);
            Assert.AreEqual(6, value.Integer);
            Assert.AreEqual(2.0, value.Double);
            Assert.AreEqual(3.0f, value.Single);
        }

        [Test, Parallelizable]
        [Description("Updates an entity with an operation containing a constant expression ensuring that the constant parameter is not overridden")]
        public async Task UpdateWithConstantExpressionTransaction()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();
            PreparedOperation operation = entitymanager.Update<ValueModel>().Set(v => v.Integer == v.Integer + 1, v => v.String == DBParameter.String).Prepare();
            using (Transaction transaction = entitymanager.Transaction()) {
                await entitymanager.Insert<ValueModel>()
                    .Columns(v => v.String, v => v.Integer, v => v.Double, v => v.Single)
                    .Values("lala", 5, 2.0, 3.0f).ExecuteAsync(transaction);
                await operation.ExecuteAsync(transaction, "Bolle");
                transaction.Commit();
            }
            ValueModel value = await entitymanager.Load<ValueModel>().ExecuteEntityAsync<ValueModel>();
            Assert.NotNull(value);
            Assert.AreEqual("Bolle", value.String);
            Assert.AreEqual(6, value.Integer);
            Assert.AreEqual(2.0, value.Double);
            Assert.AreEqual(3.0f, value.Single);
        }
    }
}