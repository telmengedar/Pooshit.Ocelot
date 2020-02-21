using System;
using System.Linq;
using System.Threading.Tasks;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities;
using NightlyCode.Database.Entities.Operations.Fields;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Tests.Data;
using NightlyCode.Database.Tests.Models;
using NUnit.Framework;

namespace NightlyCode.Database.Tests {

    [TestFixture, Parallelizable]
    public class LoadValuesOperationTests {

        [Test, Parallelizable]
        public void ExecuteType() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel(1),
                new ValueModel(2),
                new ValueModel(5),
                new ValueModel(3),
                new ValueModel(75),
                new ValueModel(234),
                new ValueModel(124)
            );

            Tuple<int, string>[] result = entitymanager.Load<ValueModel>(m => m.Integer, m => m.String).ExecuteType(row => new Tuple<int, string>((int)(long)row[0], null)).ToArray();
            Assert.True(result.Select(r => r.Item1).SequenceEqual(new[] {1, 2, 5, 3, 75, 234, 124}));
        }

        [Test, Parallelizable]
        public async Task ExecuteTypeAsync()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel(1),
                new ValueModel(2),
                new ValueModel(5),
                new ValueModel(3),
                new ValueModel(75),
                new ValueModel(234),
                new ValueModel(124)
            );

            Tuple<int, string>[] result = (await entitymanager.Load<ValueModel>(m => m.Integer, m => m.String)
                    .ExecuteTypeAsync(row => new Tuple<int, string>((int) (long) row[0], null)))
                    .ToArray();
            Assert.True(result.Select(r => r.Item1).SequenceEqual(new[] { 1, 2, 5, 3, 75, 234, 124 }));
        }

        [Test, Parallelizable]
        public void ExecuteTypeTransaction()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            using (Transaction transaction=entitymanager.Transaction()) {
                entitymanager.InsertEntities<ValueModel>().Execute(transaction,
                    new ValueModel(1),
                    new ValueModel(2),
                    new ValueModel(5),
                    new ValueModel(3),
                    new ValueModel(75),
                    new ValueModel(234),
                    new ValueModel(124)
                );

                Tuple<int, string>[] result = entitymanager.Load<ValueModel>(m => m.Integer, m => m.String).ExecuteType(row => new Tuple<int, string>((int)(long)row[0], null), transaction).ToArray();
                Assert.True(result.Select(r => r.Item1).SequenceEqual(new[] { 1, 2, 5, 3, 75, 234, 124 }));
            }
        }

        [Test, Parallelizable]
        public async Task ExecuteTypeTransactionAsync()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            using (Transaction transaction = entitymanager.Transaction())
            {
                entitymanager.InsertEntities<ValueModel>().Execute(transaction,
                    new ValueModel(1),
                    new ValueModel(2),
                    new ValueModel(5),
                    new ValueModel(3),
                    new ValueModel(75),
                    new ValueModel(234),
                    new ValueModel(124)
                );

                Tuple<int, string>[] result = (await entitymanager.Load<ValueModel>(m => m.Integer, m => m.String)
                        .ExecuteTypeAsync(row => new Tuple<int, string>((int) (long) row[0], null), transaction))
                        .ToArray();
                Assert.True(result.Select(r => r.Item1).SequenceEqual(new[] { 1, 2, 5, 3, 75, 234, 124 }));
            }
        }

        [Test, Parallelizable]
        public void ExecuteTypeWithParameters()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel(1),
                new ValueModel(2),
                new ValueModel(5),
                new ValueModel(3),
                new ValueModel(75),
                new ValueModel(234),
                new ValueModel(124)
            );

            PreparedLoadValuesOperation loadvaluesoperation = entitymanager.Load<ValueModel>(m => m.Integer, m => m.String).Where(v => v.Integer > DBParameter.Int32).Prepare();
            Tuple<int, string>[] result = loadvaluesoperation.ExecuteType(row => new Tuple<int, string>((int)(long)row[0], null), 50).ToArray();
            Assert.True(result.Select(r => r.Item1).SequenceEqual(new[] { 75, 234, 124 }));
        }

        [Test, Parallelizable]
        public async Task ExecuteTypeWithParametersAsync()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel(1),
                new ValueModel(2),
                new ValueModel(5),
                new ValueModel(3),
                new ValueModel(75),
                new ValueModel(234),
                new ValueModel(124)
            );

            PreparedLoadValuesOperation loadvaluesoperation = entitymanager.Load<ValueModel>(m => m.Integer, m => m.String).Where(v => v.Integer > DBParameter.Int32).Prepare();
            Tuple<int, string>[] result = (await loadvaluesoperation.ExecuteTypeAsync(row => new Tuple<int, string>((int)(long)row[0], null), 50)).ToArray();
            Assert.True(result.Select(r => r.Item1).SequenceEqual(new[] { 75, 234, 124 }));
        }

        [Test, Parallelizable]
        public void ExecuteTypeWithParametersTransaction()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            using (Transaction transaction = entitymanager.Transaction())
            {
                entitymanager.InsertEntities<ValueModel>().Execute(transaction,
                    new ValueModel(1),
                    new ValueModel(2),
                    new ValueModel(5),
                    new ValueModel(3),
                    new ValueModel(75),
                    new ValueModel(234),
                    new ValueModel(124)
                );

                PreparedLoadValuesOperation loadvaluesoperation = entitymanager.Load<ValueModel>(m => m.Integer, m => m.String).Where(v => v.Integer > DBParameter.Int32).Prepare();
                Tuple<int, string>[] result = loadvaluesoperation.ExecuteType(transaction, row => new Tuple<int, string>((int)(long)row[0], null), 50).ToArray();
                Assert.True(result.Select(r => r.Item1).SequenceEqual(new[] { 75, 234, 124 }));
            }
        }

        [Test, Parallelizable]
        public async Task ExecuteTypeWithParametersTransactionAsync()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            using (Transaction transaction = entitymanager.Transaction())
            {
                entitymanager.InsertEntities<ValueModel>().Execute(transaction,
                    new ValueModel(1),
                    new ValueModel(2),
                    new ValueModel(5),
                    new ValueModel(3),
                    new ValueModel(75),
                    new ValueModel(234),
                    new ValueModel(124)
                );

                PreparedLoadValuesOperation loadvaluesoperation = entitymanager.Load<ValueModel>(m => m.Integer, m => m.String).Where(v => v.Integer > DBParameter.Int32).Prepare();
                Tuple<int, string>[] result = (await loadvaluesoperation.ExecuteTypeAsync(transaction, row => new Tuple<int, string>((int)(long)row[0], null), 50)).ToArray();
                Assert.True(result.Select(r => r.Item1).SequenceEqual(new[] { 75, 234, 124 }));
            }
        }

        [Test, Parallelizable]
        public void ExecuteScalar()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel(1),
                new ValueModel(2),
                new ValueModel(5),
                new ValueModel(3),
                new ValueModel(75),
                new ValueModel(234),
                new ValueModel(124)
            );

            int result=entitymanager.Load<ValueModel>(m => m.Integer).ExecuteScalar<int>();
            Assert.AreEqual(1, result);
        }

        [Test, Parallelizable]
        public void Distinct()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel(1),
                new ValueModel(2),
                new ValueModel(5),
                new ValueModel(5),
                new ValueModel(9),
                new ValueModel(9),
                new ValueModel(9)
            );

            int[] result = entitymanager.Load<ValueModel>(m => m.Integer).Distinct().ExecuteSet<int>().ToArray();
            Assert.That(result.SequenceEqual(new[] {1, 2, 5, 9}));
        }

        [Test, Parallelizable]
        public async Task ExecuteScalarAsync()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel(1),
                new ValueModel(2),
                new ValueModel(5),
                new ValueModel(3),
                new ValueModel(75),
                new ValueModel(234),
                new ValueModel(124)
            );

            int result = await entitymanager.Load<ValueModel>(m => m.Integer).ExecuteScalarAsync<int>();
            Assert.AreEqual(1, result);
        }

        [Test, Parallelizable]
        public void ExecuteScalarTransaction()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            using (Transaction transaction = entitymanager.Transaction())
            {
                entitymanager.InsertEntities<ValueModel>().Execute(transaction,
                    new ValueModel(1),
                    new ValueModel(2),
                    new ValueModel(5),
                    new ValueModel(3),
                    new ValueModel(75),
                    new ValueModel(234),
                    new ValueModel(124)
                );

                int result=entitymanager.Load<ValueModel>(m => m.Integer).ExecuteScalar<int>(transaction);
                Assert.AreEqual(1, result);
            }
        }

        [Test, Parallelizable]
        public async Task ExecuteScalarTransactionAsync()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            using (Transaction transaction = entitymanager.Transaction())
            {
                entitymanager.InsertEntities<ValueModel>().Execute(transaction,
                    new ValueModel(1),
                    new ValueModel(2),
                    new ValueModel(5),
                    new ValueModel(3),
                    new ValueModel(75),
                    new ValueModel(234),
                    new ValueModel(124)
                );

                int result = await entitymanager.Load<ValueModel>(m => m.Integer).ExecuteScalarAsync<int>(transaction);
                Assert.AreEqual(1, result);
            }
        }

        [Test, Parallelizable]
        public void ExecuteScalarWithParameters()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel(1),
                new ValueModel(2),
                new ValueModel(5),
                new ValueModel(3),
                new ValueModel(75),
                new ValueModel(234),
                new ValueModel(124)
            );

            PreparedLoadValuesOperation loadvaluesoperation = entitymanager.Load<ValueModel>(m => m.Integer, m => m.String).Where(v => v.Integer > DBParameter.Int32).Prepare();
            int result=loadvaluesoperation.ExecuteScalar<int>(50);
            Assert.AreEqual(75, result);
        }

        [Test, Parallelizable]
        public async Task ExecuteScalarWithParametersAsync()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel(1),
                new ValueModel(2),
                new ValueModel(5),
                new ValueModel(3),
                new ValueModel(75),
                new ValueModel(234),
                new ValueModel(124)
            );

            PreparedLoadValuesOperation loadvaluesoperation = entitymanager.Load<ValueModel>(m => m.Integer, m => m.String).Where(v => v.Integer > DBParameter.Int32).Prepare();
            int result = await loadvaluesoperation.ExecuteScalarAsync<int>(50);
            Assert.AreEqual(75, result);
        }

        [Test, Parallelizable]
        public void ExecuteScalarWithParametersTransaction()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            using (Transaction transaction = entitymanager.Transaction())
            {
                entitymanager.InsertEntities<ValueModel>().Execute(transaction,
                    new ValueModel(1),
                    new ValueModel(2),
                    new ValueModel(5),
                    new ValueModel(3),
                    new ValueModel(75),
                    new ValueModel(234),
                    new ValueModel(124)
                );

                PreparedLoadValuesOperation loadvaluesoperation = entitymanager.Load<ValueModel>(m => m.Integer, m => m.String).Where(v => v.Integer > DBParameter.Int32).Prepare();
                int result=loadvaluesoperation.ExecuteScalar<int>(transaction, 50);
                Assert.AreEqual(75, result);
            }
        }

        [Test, Parallelizable]
        public async Task ExecuteScalarWithParametersTransactionAsync()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            using (Transaction transaction = entitymanager.Transaction())
            {
                entitymanager.InsertEntities<ValueModel>().Execute(transaction,
                    new ValueModel(1),
                    new ValueModel(2),
                    new ValueModel(5),
                    new ValueModel(3),
                    new ValueModel(75),
                    new ValueModel(234),
                    new ValueModel(124)
                );

                PreparedLoadValuesOperation loadvaluesoperation = entitymanager.Load<ValueModel>(m => m.Integer, m => m.String).Where(v => v.Integer > DBParameter.Int32).Prepare();
                int result = await loadvaluesoperation.ExecuteScalarAsync<int>(transaction, 50);
                Assert.AreEqual(75, result);
            }
        }

        [Test, Parallelizable]
        public void ExecuteSet()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel(1),
                new ValueModel(2),
                new ValueModel(5),
                new ValueModel(3),
                new ValueModel(75),
                new ValueModel(234),
                new ValueModel(124)
            );

            int[] result = entitymanager.Load<ValueModel>(m => m.Integer).ExecuteSet<int>().ToArray();
            Assert.True(result.SequenceEqual(new[] { 1, 2, 5, 3, 75, 234, 124 }));
        }

        [Test, Parallelizable]
        public async Task ExecuteSetAsync()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel(1),
                new ValueModel(2),
                new ValueModel(5),
                new ValueModel(3),
                new ValueModel(75),
                new ValueModel(234),
                new ValueModel(124)
            );

            int[] result = (await entitymanager.Load<ValueModel>(m => m.Integer).ExecuteSetAsync<int>()).ToArray();
            Assert.True(result.SequenceEqual(new[] { 1, 2, 5, 3, 75, 234, 124 }));
        }

        [Test, Parallelizable]
        public void ExecuteSetTransaction()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            using (Transaction transaction = entitymanager.Transaction())
            {
                entitymanager.InsertEntities<ValueModel>().Execute(transaction,
                    new ValueModel(1),
                    new ValueModel(2),
                    new ValueModel(5),
                    new ValueModel(3),
                    new ValueModel(75),
                    new ValueModel(234),
                    new ValueModel(124)
                );

                int[] result = entitymanager.Load<ValueModel>(m => m.Integer).ExecuteSet<int>(transaction).ToArray();
                Assert.True(result.SequenceEqual(new[] { 1, 2, 5, 3, 75, 234, 124 }));
            }
        }

        [Test, Parallelizable]
        public void ExecuteSetTransactionAsync()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            using (Transaction transaction = entitymanager.Transaction())
            {
                entitymanager.InsertEntities<ValueModel>().Execute(transaction,
                    new ValueModel(1),
                    new ValueModel(2),
                    new ValueModel(5),
                    new ValueModel(3),
                    new ValueModel(75),
                    new ValueModel(234),
                    new ValueModel(124)
                );

                int[] result = entitymanager.Load<ValueModel>(m => m.Integer).ExecuteSet<int>(transaction).ToArray();
                Assert.True(result.SequenceEqual(new[] { 1, 2, 5, 3, 75, 234, 124 }));
            }
        }

        [Test, Parallelizable]
        public void ExecuteSetWithParameters()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel(1),
                new ValueModel(2),
                new ValueModel(5),
                new ValueModel(3),
                new ValueModel(75),
                new ValueModel(234),
                new ValueModel(124)
            );

            PreparedLoadValuesOperation loadvaluesoperation = entitymanager.Load<ValueModel>(m => m.Integer, m => m.String).Where(v => v.Integer > DBParameter.Int32).Prepare();
            int[] result = loadvaluesoperation.ExecuteSet<int>(50).ToArray();
            Assert.True(result.SequenceEqual(new[] { 75, 234, 124 }));
        }

        [Test, Parallelizable]
        public void ExecuteSetWithParametersTransaction()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            using (Transaction transaction = entitymanager.Transaction())
            {
                entitymanager.InsertEntities<ValueModel>().Execute(transaction,
                    new ValueModel(1),
                    new ValueModel(2),
                    new ValueModel(5),
                    new ValueModel(3),
                    new ValueModel(75),
                    new ValueModel(234),
                    new ValueModel(124)
                );

                PreparedLoadValuesOperation loadvaluesoperation = entitymanager.Load<ValueModel>(m => m.Integer, m => m.String).Where(v => v.Integer > DBParameter.Int32).Prepare();
                int[] result= loadvaluesoperation.ExecuteSet<int>(transaction, 50).ToArray();
                Assert.True(result.SequenceEqual(new[] { 75, 234, 124 }));
            }
        }

        [Test, Parallelizable]
        public void ExecuteJoin() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<Option>();
            entitymanager.UpdateSchema<ConstructOption>();

            Guid guid = new Guid();
            entitymanager.LoadEntities<Option>()
                .Join<ConstructOption>((o, co) => co.OptionId == o.Id)
                .Where((o, co) => co.ConstructId == guid)
                .Execute();
        }

        [Test, Parallelizable]
        public void JoinContainsWhereOfBase() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<Option>();
            entitymanager.UpdateSchema<ConstructOption>();

            PreparedLoadValuesOperation operation = entitymanager.Load<Option>()
                .Where(o => o.Name == "Test")
                .Join<ConstructOption>((o, co) => co.OptionId == o.Id)
                .Prepare();

            Assert.That(operation.ToString().Contains("WHERE"));
        }
    }
}