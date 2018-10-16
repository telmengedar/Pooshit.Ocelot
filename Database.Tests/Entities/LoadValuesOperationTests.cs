using System;
using System.Linq;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Clients.Tables;
using NightlyCode.Database.Entities;
using NightlyCode.Database.Tests.Models;
using NUnit.Framework;

namespace NightlyCode.Database.Tests.Entities {

    [TestFixture]
    public class LoadValuesOperationTests {

        [Test]
        public void LoadSet() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<ValueModel>();

            for(int i = 0; i < 5; ++i)
                entitymanager.Insert<ValueModel>().Columns(m => m.Integer, m=>m.Single, m=>m.Double).Values(i,0.0f,0.0).Execute();

            int[] set=entitymanager.Load<ValueModel>(v => v.Integer).ExecuteSet<int>().ToArray();
            Assert.AreEqual(5, set.Length);
            for(int i = 0; i < 5; ++i)
                Assert.AreEqual(i, set[i]);
        }

        [Test]
        public void LoadSetTransaction()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<ValueModel>();

            int[] set;
            using (Transaction transaction = entitymanager.Transaction()) {
                for(int i = 0; i < 5; ++i)
                    entitymanager.Insert<ValueModel>().Columns(m => m.Integer, m => m.Single, m => m.Double).Values(i, 0.0f,0.0).Execute(transaction);

                set = entitymanager.Load<ValueModel>(v => v.Integer).ExecuteSet<int>(transaction).ToArray();
                transaction.Commit();
            }

            Assert.AreEqual(5, set.Length);
            for (int i = 0; i < 5; ++i)
                Assert.AreEqual(i, set[i]);
        }

        [Test]
        public void LoadScalarTransaction()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<ValueModel>();

            using (Transaction transaction = entitymanager.Transaction())
            {
                entitymanager.Insert<ValueModel>().Columns(m => m.Integer, m => m.Single, m => m.Double).Values(3, 0.0f,0.0).Execute(transaction);

                Assert.AreEqual(3, entitymanager.Load<ValueModel>(v => v.Integer).ExecuteScalar<int>(transaction));
                transaction.Commit();
            }
        }

        [Test]
        public void LoadSetQueryTransaction()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<ValueModel>();

            DataTable set;
            using (Transaction transaction = entitymanager.Transaction())
            {
                for (int i = 0; i < 5; ++i)
                    entitymanager.Insert<ValueModel>().Columns(m => m.Integer, m => m.Single, m => m.Double).Values(i,0.0f,0.0).Execute(transaction);

                set = entitymanager.Load<ValueModel>(v => v.Integer, v=>v.String).Execute(transaction);
                transaction.Commit();
            }

            Assert.AreEqual(5, set.Rows.Length);
            for(int i = 0; i < 5; ++i) {
                Assert.AreEqual(i, set.Rows[i]["integer"]);
                Assert.AreEqual(DBNull.Value, set.Rows[i]["string"]);
            }
        }
    }
}