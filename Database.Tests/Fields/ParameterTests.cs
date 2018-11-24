﻿using System.Linq;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities;
using NightlyCode.Database.Entities.Operations;
using NightlyCode.Database.Entities.Operations.Fields;
using NightlyCode.Database.Entities.Operations.Prepared;
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

        [Test]
        public void ParameterArrayContains() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.UpdateSchema<ValueModel>();
            entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(0, 0.0f, 0.0).Execute();
            entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(1, 0.0f, 0.0).Execute();
            entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(2, 0.0f, 0.0).Execute();
            entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(3, 0.0f, 0.0).Execute();
            ValueModel[] result = entitymanager.LoadEntities<ValueModel>().Where(v => DBParameter<int[]>.Value.Contains(v.Integer)).Prepare().Execute(new[] {1, 2}).ToArray();
            Assert.AreEqual(2, result.Length);
            for (int i = 0; i < 2; ++i)
                Assert.AreEqual(i + 1, result[i].Integer);
        }

        [Test]
        public void CustomParameterIndexValue() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.UpdateSchema<ValueModel>();
            PreparedLoadEntitiesOperation<ValueModel> operation = entitymanager.LoadEntities<ValueModel>().Where(v => v.Integer == DBParameter<int>.Index(1).Data).Prepare();
        }

        [Test]
        public void ParameterArrayWithOtherParametersContains()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.UpdateSchema<ValueModel>();
            entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(0, 0.0f, 1.0).Execute();
            entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(1, 0.0f, 0.0).Execute();
            entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(2, 0.0f, 1.0).Execute();
            entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(3, 0.0f, 0.0).Execute();
            ValueModel[] result = entitymanager.LoadEntities<ValueModel>().Where(v => DBParameter<int[]>.Value.Contains(v.Integer) && v.Double==DBParameter.Double).Prepare().Execute(new[] { 1, 2 }, 1.0).ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(2, result[0].Integer);
            Assert.AreEqual(1.0, result[0].Double);
        }
    }
}