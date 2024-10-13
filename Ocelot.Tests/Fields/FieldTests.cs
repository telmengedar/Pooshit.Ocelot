﻿using System.Linq;
using NightlyCode.Database.Tests.Data;
using NightlyCode.Database.Tests.Models;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Fields;

namespace NightlyCode.Database.Tests.Fields {

    [TestFixture]
    public class FieldTests {

        [Test]
        public void LoadAllFields()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.UpdateSchema<ValueModel>();
            entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(0, 0.0f, 1.0).Execute();
            entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(1, 0.0f, 0.0).Execute();
            entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(2, 1.0f, 1.0).Execute();
            entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(3, 1.0f, 0.0).Execute();
            ValueModel[] result = entitymanager.Load<ValueModel>(m => DBFunction.All).Prepare().ExecuteTypes(r => new ValueModel((int) (long) r["integer"])).ToArray();
            Assert.IsTrue(new[] {0, 1, 2, 3}.SequenceEqual(result.Select(r => r.Integer)));
        }

        [Test]
        public void LoadAllFieldsImplicitely()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.UpdateSchema<ValueModel>();
            entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(0, 0.0f, 1.0).Execute();
            entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(1, 0.0f, 0.0).Execute();
            entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(2, 1.0f, 1.0).Execute();
            entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(3, 1.0f, 0.0).Execute();
            ValueModel[] result = entitymanager.Load<ValueModel>().Prepare().ExecuteTypes(r => new ValueModel((int)(long)r["integer"])).ToArray();
            Assert.IsTrue(new[] { 0, 1, 2, 3 }.SequenceEqual(result.Select(r => r.Integer)));
        }

    }
}