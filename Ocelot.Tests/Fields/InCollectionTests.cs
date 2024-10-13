using System;
using System.Linq;
using NightlyCode.Database.Tests.Data;
using NightlyCode.Database.Tests.Models;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Fields;

namespace NightlyCode.Database.Tests.Fields {
    
    [TestFixture, Parallelizable]
    public class InCollectionTests {

        [Test, Parallelizable]
        public void ValueInArray() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel {Integer = 5},
                new ValueModel(),
                new ValueModel {Integer = 11},
                new ValueModel {Integer = 3},
                new ValueModel {Integer = 7});

            ValueModel[] values = entitymanager.Load<ValueModel>().Where(m => m.Integer.In(new long[] {0, 7, 11})).ExecuteEntities<ValueModel>().ToArray();
            Assert.AreEqual(3, values.Length);
        }

        [Test, Parallelizable]
        public void ValueNotInArray() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel {Integer = 5},
                new ValueModel(),
                new ValueModel {Integer = 11},
                new ValueModel {Integer = 3},
                new ValueModel {Integer = 7});

            ValueModel[] values = entitymanager.Load<ValueModel>().Where(m => !m.Integer.In(new long[] {2, 4, 11})).ExecuteEntities<ValueModel>().ToArray();
            Assert.AreEqual(4, values.Length);
        }
        
        [Test, Parallelizable]
        public void ValueInArrayVariable() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel {Integer = 5},
                new ValueModel(),
                new ValueModel {Integer = 11},
                new ValueModel {Integer = 3},
                new ValueModel {Integer = 7});

            Array array = new long[] {0, 7, 11};
            ValueModel[] values = entitymanager.Load<ValueModel>().Where(m => m.Integer.In(array)).ExecuteEntities<ValueModel>().ToArray();
            Assert.AreEqual(3, values.Length);
        }

        [Test, Parallelizable]
        public void ValueInStatement() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.InsertEntities<ValueModel>().Execute(
                new ValueModel {Integer = 5},
                new ValueModel(),
                new ValueModel {Integer = 11},
                new ValueModel {Integer = 3},
                new ValueModel {Integer = 7});

            LoadOperation<ValueModel> loadstatement=entitymanager.Load<ValueModel>(v => v.Integer).Where(v => v.Integer < 7);
            ValueModel[] values = entitymanager.Load<ValueModel>().Where(m => m.Integer.In(loadstatement)).ExecuteEntities<ValueModel>().ToArray();
            Assert.AreEqual(3, values.Length);
        }

    }
}