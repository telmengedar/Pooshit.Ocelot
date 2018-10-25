using System.Linq;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities;
using NightlyCode.Database.Entities.Operations.Fields;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Tests.Data;
using NightlyCode.Database.Tests.Models;
using NUnit.Framework;

namespace NightlyCode.Database.Tests {

    [TestFixture]
    public class ValueMethodTests {

        [Test]
        public void LoadSet() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            PreparedOperation operation = entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v=>v.Single, v=>v.Double).Prepare();
            for (int i = 0; i < 5; ++i)
                operation.Execute(i, 0.0f, 0.0);

            int[] result = entitymanager.Load<ValueModel>(v => v.Integer).ExecuteSet<int>().ToArray();
            Assert.True(new[] {0, 1, 2, 3, 4}.SequenceEqual(result));
        }

        [Test]
        public void LoadScalar()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(4,0.0f,0.0).Execute();

            Assert.AreEqual(4, entitymanager.Load<ValueModel>(v => v.Integer).ExecuteScalar<int>());
        }

        [Test]
        public void DeleteAll() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            PreparedOperation operation = entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Prepare();
            for (int i = 0; i < 5; ++i)
                operation.Execute(i, 0.0f, 0.0);

            entitymanager.Delete<ValueModel>().Execute();
            Assert.AreEqual(0, entitymanager.Load<ValueModel>(m => DBFunction.Count).ExecuteScalar<int>());
        }

        [Test]
        public void DeleteSingle()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            PreparedOperation operation = entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Prepare();
            for (int i = 0; i < 5; ++i)
                operation.Execute(i, 0.0f, 0.0);

            entitymanager.Delete<ValueModel>().Where(v => v.Integer == 3).Execute();

            int[] values = entitymanager.Load<ValueModel>(v => v.Integer).ExecuteSet<int>().ToArray();
            Assert.AreEqual(4, values.Length);
            Assert.False(values.Any(v => v == 3));
        }

        [Test]
        public void DeleteTransactionRollback() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            PreparedOperation operation = entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Prepare();
            for (int i = 0; i < 5; ++i)
                operation.Execute(i, 0.0f, 0.0);

            using (Transaction transaction = entitymanager.Transaction()) {
                entitymanager.Delete<ValueModel>().Execute(transaction);
                transaction.Rollback();
            }

            Assert.AreEqual(5, entitymanager.Load<ValueModel>(m => DBFunction.Count).ExecuteScalar<int>());
        }
    }
}