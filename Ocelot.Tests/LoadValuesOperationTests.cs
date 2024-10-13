using System;
using System.Linq;
using System.Threading.Tasks;
using NightlyCode.Database.Tests.Data;
using NightlyCode.Database.Tests.Entities;
using NightlyCode.Database.Tests.Models;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Fields;

namespace NightlyCode.Database.Tests {

    [TestFixture, Parallelizable]
    public class LoadValuesOperationTests {

        [Test, Parallelizable]
        public void LoadWithArrayParameter() {
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

            
            int[] result = entitymanager.Load<ValueModel>(m => m.Integer, m => m.String)
                .Where(v => v.Integer.In(DBParameter<int[]>.Value))
                .ExecuteSet<int>(new[] { 5, 75, 124 })
                .ToArray();
            Assert.True(result.SequenceEqual(new[] { 5, 75, 124 }));
        }

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

            Tuple<int, string>[] result = entitymanager.Load<ValueModel>(m => m.Integer, m => m.String).ExecuteTypes(row => new Tuple<int, string>((int)(long)row[0], null)).ToArray();
            Assert.True(result.Select(r => r.Item1).SequenceEqual(new[] { 1, 2, 5, 3, 75, 234, 124 }));
        }

        [Test, Parallelizable]
        public async Task ExecuteTypeAsync() {
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
                    .ExecuteTypesAsync(row => new Tuple<int, string>((int)(long)row[0], null)))
                    .ToArray();
            Assert.True(result.Select(r => r.Item1).SequenceEqual(new[] { 1, 2, 5, 3, 75, 234, 124 }));
        }

        [Test, Parallelizable]
        public void ExecuteTypeTransaction() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            using Transaction transaction = entitymanager.Transaction();
            entitymanager.InsertEntities<ValueModel>().Execute(transaction,
                new ValueModel(1),
                new ValueModel(2),
                new ValueModel(5),
                new ValueModel(3),
                new ValueModel(75),
                new ValueModel(234),
                new ValueModel(124)
            );

            Tuple<int, string>[] result = entitymanager.Load<ValueModel>(m => m.Integer, m => m.String).ExecuteTypes(row => new Tuple<int, string>((int)(long)row[0], null), transaction).ToArray();
            Assert.True(result.Select(r => r.Item1).SequenceEqual(new[] { 1, 2, 5, 3, 75, 234, 124 }));
        }

        [Test, Parallelizable]
        public async Task ExecuteTypeTransactionAsync() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            using Transaction transaction = entitymanager.Transaction();
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
                    .ExecuteTypesAsync(row => new Tuple<int, string>((int)(long)row[0], null), transaction))
                .ToArray();
            Assert.True(result.Select(r => r.Item1).SequenceEqual(new[] { 1, 2, 5, 3, 75, 234, 124 }));
        }

        [Test, Parallelizable]
        public void ExecuteTypeWithParameters() {
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

            PreparedLoadOperation loadvaluesoperation = entitymanager.Load<ValueModel>(m => m.Integer, m => m.String).Where(v => v.Integer > DBParameter.Int32).Prepare();
            Tuple<int, string>[] result = loadvaluesoperation.ExecuteTypes(row => new Tuple<int, string>((int)(long)row[0], null), 50).ToArray();
            Assert.True(result.Select(r => r.Item1).SequenceEqual(new[] { 75, 234, 124 }));
        }

        [Test, Parallelizable]
        public async Task ExecuteTypeWithParametersAsync() {
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

            PreparedLoadOperation loadvaluesoperation = entitymanager.Load<ValueModel>(m => m.Integer, m => m.String).Where(v => v.Integer > DBParameter.Int32).Prepare();
            Tuple<int, string>[] result = (await loadvaluesoperation.ExecuteTypesAsync(row => new Tuple<int, string>((int)(long)row[0], null), 50)).ToArray();
            Assert.True(result.Select(r => r.Item1).SequenceEqual(new[] { 75, 234, 124 }));
        }

        [Test, Parallelizable]
        public void ExecuteTypeWithParametersTransaction() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            using Transaction transaction = entitymanager.Transaction();
            entitymanager.InsertEntities<ValueModel>().Execute(transaction,
                new ValueModel(1),
                new ValueModel(2),
                new ValueModel(5),
                new ValueModel(3),
                new ValueModel(75),
                new ValueModel(234),
                new ValueModel(124)
            );

            PreparedLoadOperation loadvaluesoperation = entitymanager.Load<ValueModel>(m => m.Integer, m => m.String).Where(v => v.Integer > DBParameter.Int32).Prepare();
            Tuple<int, string>[] result = loadvaluesoperation.ExecuteTypes(transaction, row => new Tuple<int, string>((int)(long)row[0], null), 50).ToArray();
            Assert.True(result.Select(r => r.Item1).SequenceEqual(new[] { 75, 234, 124 }));
        }

        [Test, Parallelizable]
        public async Task ExecuteTypeWithParametersTransactionAsync() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            using Transaction transaction = entitymanager.Transaction();
            entitymanager.InsertEntities<ValueModel>().Execute(transaction,
                new ValueModel(1),
                new ValueModel(2),
                new ValueModel(5),
                new ValueModel(3),
                new ValueModel(75),
                new ValueModel(234),
                new ValueModel(124)
            );

            PreparedLoadOperation loadvaluesoperation = entitymanager.Load<ValueModel>(m => m.Integer, m => m.String).Where(v => v.Integer > DBParameter.Int32).Prepare();
            Tuple<int, string>[] result = (await loadvaluesoperation.ExecuteTypesAsync(transaction, row => new Tuple<int, string>((int)(long)row[0], null), 50)).ToArray();
            Assert.True(result.Select(r => r.Item1).SequenceEqual(new[] { 75, 234, 124 }));
        }

        [Test, Parallelizable]
        public void ExecuteScalar() {
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

            int result = entitymanager.Load<ValueModel>(m => m.Integer).ExecuteScalar<int>();
            Assert.AreEqual(1, result);
        }

        [Test, Parallelizable]
        public void Distinct() {
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
            Assert.That(result.SequenceEqual(new[] { 1, 2, 5, 9 }));
        }

        [Test, Parallelizable]
        public async Task ExecuteScalarAsync() {
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
        public void ExecuteScalarTransaction() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            using Transaction transaction = entitymanager.Transaction();
            entitymanager.InsertEntities<ValueModel>().Execute(transaction,
                new ValueModel(1),
                new ValueModel(2),
                new ValueModel(5),
                new ValueModel(3),
                new ValueModel(75),
                new ValueModel(234),
                new ValueModel(124)
            );

            int result = entitymanager.Load<ValueModel>(m => m.Integer).ExecuteScalar<int>(transaction);
            Assert.AreEqual(1, result);
        }

        [Test, Parallelizable]
        public async Task ExecuteScalarTransactionAsync() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            using Transaction transaction = entitymanager.Transaction();
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

        [Test, Parallelizable]
        public void ExecuteScalarWithParameters() {
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

            PreparedLoadOperation loadvaluesoperation = entitymanager.Load<ValueModel>(m => m.Integer, m => m.String).Where(v => v.Integer > DBParameter.Int32).Prepare();
            int result = loadvaluesoperation.ExecuteScalar<int>(50);
            Assert.AreEqual(75, result);
        }

        [Test, Parallelizable]
        public async Task ExecuteScalarWithParametersAsync() {
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

            PreparedLoadOperation loadvaluesoperation = entitymanager.Load<ValueModel>(m => m.Integer, m => m.String).Where(v => v.Integer > DBParameter.Int32).Prepare();
            int result = await loadvaluesoperation.ExecuteScalarAsync<int>(50);
            Assert.AreEqual(75, result);
        }

        [Test, Parallelizable]
        public void ExecuteScalarWithParametersTransaction() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            using Transaction transaction = entitymanager.Transaction();
            entitymanager.InsertEntities<ValueModel>().Execute(transaction,
                new ValueModel(1),
                new ValueModel(2),
                new ValueModel(5),
                new ValueModel(3),
                new ValueModel(75),
                new ValueModel(234),
                new ValueModel(124)
            );

            PreparedLoadOperation loadvaluesoperation = entitymanager.Load<ValueModel>(m => m.Integer, m => m.String).Where(v => v.Integer > DBParameter.Int32).Prepare();
            int result = loadvaluesoperation.ExecuteScalar<int>(transaction, 50);
            Assert.AreEqual(75, result);
        }

        [Test, Parallelizable]
        public async Task ExecuteScalarWithParametersTransactionAsync() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            using Transaction transaction = entitymanager.Transaction();
            entitymanager.InsertEntities<ValueModel>().Execute(transaction,
                new ValueModel(1),
                new ValueModel(2),
                new ValueModel(5),
                new ValueModel(3),
                new ValueModel(75),
                new ValueModel(234),
                new ValueModel(124)
            );

            PreparedLoadOperation loadvaluesoperation = entitymanager.Load<ValueModel>(m => m.Integer, m => m.String).Where(v => v.Integer > DBParameter.Int32).Prepare();
            int result = await loadvaluesoperation.ExecuteScalarAsync<int>(transaction, 50);
            Assert.AreEqual(75, result);
        }

        [Test, Parallelizable]
        public void ExecuteSet() {
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
        public async Task ExecuteSetAsync() {
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
        public void ExecuteSetTransaction() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            using Transaction transaction = entitymanager.Transaction();
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

        [Test, Parallelizable]
        public void ExecuteSetTransactionAsync() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            using Transaction transaction = entitymanager.Transaction();
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

        [Test, Parallelizable]
        public void ExecuteSetWithParameters() {
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

            PreparedLoadOperation loadvaluesoperation = entitymanager.Load<ValueModel>(m => m.Integer, m => m.String).Where(v => v.Integer > DBParameter.Int32).Prepare();
            int[] result = loadvaluesoperation.ExecuteSet<int>(50).ToArray();
            Assert.True(result.SequenceEqual(new[] { 75, 234, 124 }));
        }

        [Test, Parallelizable]
        public void ExecuteSetWithParametersTransaction() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            using Transaction transaction = entitymanager.Transaction();
            entitymanager.InsertEntities<ValueModel>().Execute(transaction,
                new ValueModel(1),
                new ValueModel(2),
                new ValueModel(5),
                new ValueModel(3),
                new ValueModel(75),
                new ValueModel(234),
                new ValueModel(124)
            );

            PreparedLoadOperation loadvaluesoperation = entitymanager.Load<ValueModel>(m => m.Integer, m => m.String).Where(v => v.Integer > DBParameter.Int32).Prepare();
            int[] result = loadvaluesoperation.ExecuteSet<int>(transaction, 50).ToArray();
            Assert.True(result.SequenceEqual(new[] { 75, 234, 124 }));
        }

        [Test, Parallelizable]
        public void ExecuteJoin() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<Option>();
            entitymanager.UpdateSchema<ConstructOption>();

            Guid guid = new Guid();
            entitymanager.Load<Option>()
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

            PreparedLoadOperation operation = entitymanager.Load<Option>()
                .Where(o => o.Name == "Test")
                .Join<ConstructOption>((o, co) => co.OptionId == o.Id)
                .Prepare();

            Assert.That(operation.ToString().Contains("WHERE"));
        }

        [Test, Parallelizable]
        public void LeftJoinWithAlias() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<Option>();
            entitymanager.UpdateSchema<ConstructOption>();

            PreparedLoadOperation operation = entitymanager.Load<Option>(o => o.Id)
                .Where(o => o.Name == "Test")
                .LeftJoin<ConstructOption>((o, co) => co.OptionId == o.Id, "test")
                .Prepare();

            Assert.AreEqual("SELECT t.[id] FROM option AS t LEFT JOIN constructoption AS test ON test.[optionid] = t.[id] WHERE t.[name] = @1", operation.CommandText);
        }

        [Test, Parallelizable]
        public void WhereAfterJoinUsesAlias() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<Option>();
            entitymanager.UpdateSchema<ConstructOption>();

            PreparedLoadOperation operation = entitymanager.Load<Option>(o => o.Id)
                .LeftJoin<ConstructOption>((o, co) => co.OptionId == o.Id, "test")
                .Where((o, co) => co.OptionId == Guid.Empty)
                .Prepare();

            Assert.AreEqual("SELECT t.[id] FROM option AS t LEFT JOIN constructoption AS test ON test.[optionid] = t.[id] WHERE test.[optionid] = @1", operation.CommandText);
        }

        [Test, Parallelizable]
        public void SpecifyAlias() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            PreparedLoadOperation operation = entitymanager.Load<Option>(o => o.Id)
                .Alias("o666")
                .Where((o) => o.Id == Guid.Empty)
                .Prepare();

            Assert.AreEqual("SELECT o666.[id] FROM option AS o666 WHERE o666.[id] = @1", operation.CommandText);
        }

        [Test, Parallelizable]
        public void Union() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.UpdateSchema<UnionA>();
            entitymanager.UpdateSchema<UnionB>();

            entitymanager.Insert<UnionA>().Columns(u => u.Name, u => u.Number).Execute("Heinz", 7);
            entitymanager.Insert<UnionB>().Columns(u => u.Name, u => u.NumberOfDoom).Execute("Hina", 12);

            UnionA[] result = entitymanager.Load<UnionA>(u => u.Id, u => u.Name, u => u.Number)
                .Union(entitymanager.Load<UnionB>(u => u.CrazyId, u => u.Name, u => u.NumberOfDoom)).ExecuteTypes(
                r => new UnionA {
                    Id = r.GetValue<long>(0),
                    Name = r.GetValue<string>(1),
                    Number = r.GetValue<decimal>(2)
                }
            ).ToArray();

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(1, result[0].Id);
            Assert.AreEqual("Heinz", result[0].Name);
            Assert.AreEqual(7, result[0].Number);
            Assert.AreEqual(1, result[1].Id);
            Assert.AreEqual("Hina", result[1].Name);
            Assert.AreEqual(12, result[1].Number);
        }
    }
}