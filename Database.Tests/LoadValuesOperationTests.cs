using System;
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
    public class LoadValuesOperationTests {

        [Test]
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

            Tuple<int, string>[] result = entitymanager.Load<ValueModel>(m => m.Integer, m => m.String).ExecuteType<Tuple<int, string>>(row => new Tuple<int, string>((int)(long)row[0], null)).ToArray();
        }

        [Test]
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

                Tuple<int, string>[] result = entitymanager.Load<ValueModel>(m => m.Integer, m => m.String).ExecuteType<Tuple<int, string>>(transaction, row => new Tuple<int, string>((int)(long)row[0], null)).ToArray();
            }
        }

        [Test]
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
            Tuple<int, string>[] result = loadvaluesoperation.ExecuteType<Tuple<int, string>>(row => new Tuple<int, string>((int)(long)row[0], null), 50).ToArray();
        }

        [Test]
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
                Tuple<int, string>[] result = loadvaluesoperation.ExecuteType<Tuple<int, string>>(transaction, row => new Tuple<int, string>((int)(long)row[0], null), 50).ToArray();
            }
        }

        [Test]
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

            entitymanager.Load<ValueModel>(m => m.Integer).ExecuteScalar<int>();
        }

        [Test]
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

                entitymanager.Load<ValueModel>(m => m.Integer).ExecuteScalar<int>(transaction);
            }
        }

        [Test]
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
            loadvaluesoperation.ExecuteScalar<int>(50);
        }

        [Test]
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
                loadvaluesoperation.ExecuteScalar<int>(transaction, 50);
            }
        }

        [Test]
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

            foreach(int value in entitymanager.Load<ValueModel>(m => m.Integer).ExecuteSet<int>());
        }

        [Test]
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

                foreach(int value in entitymanager.Load<ValueModel>(m => m.Integer).ExecuteSet<int>(transaction));
            }
        }

        [Test]
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
            foreach (int value in loadvaluesoperation.ExecuteSet<int>(50)) ;
        }

        [Test]
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
                foreach(int value in loadvaluesoperation.ExecuteSet<int>(transaction, 50));
            }
        }

    }
}