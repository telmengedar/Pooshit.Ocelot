﻿using NightlyCode.Database.Tests.Data;
using NightlyCode.Database.Tests.Models;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Errors;
using Pooshit.Ocelot.Fields;

namespace NightlyCode.Database.Tests.Entities
{

    [TestFixture]
    public class ConstraintTests
    {

        [Test]
        public void MultiUniqueDoesntAffectSingleColumns()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.Model<ValueModel>().Unique(u => u.Integer, u => u.String);
            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.Insert<ValueModel>()
                .Columns(u => u.Integer, u => u.String, u => u.Single, u => u.Double)
                .Values(0, "String1", 0.0f, 0.0).Execute();
            entitymanager.Insert<ValueModel>()
                .Columns(u => u.Integer, u => u.String, u => u.Single, u => u.Double)
                .Values(0, "String2", 0.0f, 0.0).Execute();
            entitymanager.Insert<ValueModel>()
                .Columns(u => u.Integer, u => u.String, u => u.Single, u => u.Double)
                .Values(0, "String3", 0.0f, 0.0).Execute();

            Assert.AreEqual(3, entitymanager.Load<ValueModel>(m=>DBFunction.Count()).ExecuteScalar<int>());
        }

        [Test]
        public void MultiUniqueFailsOnDoubleInsert()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.Model<ValueModel>().Unique(u => u.Integer, u => u.String);
            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.Insert<ValueModel>()
                .Columns(u => u.Integer, u => u.String, u => u.Single, u => u.Double)
                .Values(0, "String1", 0.0f, 0.0).Execute();
            Assert.Throws<StatementException>(() =>
            {
                entitymanager.Insert<ValueModel>()
                    .Columns(u => u.Integer, u => u.String, u => u.Single, u => u.Double)
                    .Values(0, "String1", 0.0f, 0.0).Execute();
            });
        }
    }
}