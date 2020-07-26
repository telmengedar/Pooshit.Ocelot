using System.Linq;
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
    public class ValueMethodTests {

        [Test, Parallelizable]
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

        [Test, Parallelizable]
        public async Task LoadSetAsync()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            PreparedOperation operation = entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Prepare();
            for (int i = 0; i < 5; ++i)
                await operation.ExecuteAsync(i, 0.0f, 0.0);

            int[] result = (await entitymanager.Load<ValueModel>(v => v.Integer).ExecuteSetAsync<int>()).ToArray();
            Assert.True(new[] { 0, 1, 2, 3, 4 }.SequenceEqual(result));
        }

        [Test, Parallelizable]
        public void LoadScalar()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(4,0.0f,0.0).Execute();

            Assert.AreEqual(4, entitymanager.Load<ValueModel>(v => v.Integer).ExecuteScalar<int>());
        }

        [Test, Parallelizable]
        public async Task LoadScalarAsync()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            await entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(4, 0.0f, 0.0).ExecuteAsync();

            Assert.AreEqual(4, await entitymanager.Load<ValueModel>(v => v.Integer).ExecuteScalarAsync<int>());
        }

        [Test, Parallelizable]
        public void DeleteAll() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            PreparedOperation operation = entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Prepare();
            for (int i = 0; i < 5; ++i)
                operation.Execute(i, 0.0f, 0.0);

            entitymanager.Delete<ValueModel>().Execute();
            Assert.AreEqual(0, entitymanager.Load<ValueModel>(m => DBFunction.Count()).ExecuteScalar<int>());
        }

        [Test, Parallelizable]
        public async Task DeleteAllAsync()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            PreparedOperation operation = entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Prepare();
            for (int i = 0; i < 5; ++i)
                await operation.ExecuteAsync(i, 0.0f, 0.0);

            await entitymanager.Delete<ValueModel>().ExecuteAsync();
            Assert.AreEqual(0, await entitymanager.Load<ValueModel>(m => DBFunction.Count()).ExecuteScalarAsync<int>());
        }

        [Test, Parallelizable]
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

        [Test, Parallelizable]
        public async Task DeleteSingleAsync()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            PreparedOperation operation = entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Prepare();
            for (int i = 0; i < 5; ++i)
                await operation.ExecuteAsync(i, 0.0f, 0.0);

            await entitymanager.Delete<ValueModel>().Where(v => v.Integer == 3).ExecuteAsync();

            int[] values = (await entitymanager.Load<ValueModel>(v => v.Integer).ExecuteSetAsync<int>()).ToArray();
            Assert.AreEqual(4, values.Length);
            Assert.False(values.Any(v => v == 3));
        }

        [Test, Parallelizable]
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

            Assert.AreEqual(5, entitymanager.Load<ValueModel>(m => DBFunction.Count()).ExecuteScalar<int>());
        }

        [Test, Parallelizable]
        public async Task DeleteTransactionRollbackAsync()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            PreparedOperation operation = entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Prepare();
            for (int i = 0; i < 5; ++i)
                await operation.ExecuteAsync(i, 0.0f, 0.0);

            using (Transaction transaction = entitymanager.Transaction())
            {
                await entitymanager.Delete<ValueModel>().ExecuteAsync(transaction);
                transaction.Rollback();
            }

            Assert.AreEqual(5, await entitymanager.Load<ValueModel>(m => DBFunction.Count()).ExecuteScalarAsync<int>());
        }
    }
}