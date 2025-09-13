using System;
using System.Linq;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Models;

namespace Pooshit.Ocelot.Tests.Fields;

[TestFixture]
public class ParameterTests {

    [Test]
    public void PrepareCustomParameter() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new EntityManager(dbclient);
        entitymanager.Update<EnumEntity>().Set(e => e.Enum == DBParameter<TestEnum>.Value).Prepare();
    }

    [Test]
    public void PrepareConstantEnumParameter() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new EntityManager(dbclient);
        entitymanager.Update<EnumEntity>().Set(e => e.Enum == TestEnum.Insane).Prepare();
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

    [Test, Parallelizable]
    public void IndexedParametersMixedWithNull() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new(dbclient);
        entitymanager.UpdateSchema<ValueModel>();
        entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.NDatetime, v=>v.Single).Values(0, null, 0.5f).Execute();
        entitymanager.Update<ValueModel>()
                     .Set(v => v.Integer == DBParameter.Index(1), 
                          v => v.Single == DBParameter.Index(2) * DBParameter.Index(2),
                          v=>v.NDatetime==null)
                     .Where(v=>v.Integer==DBParameter.Index(1))
                     .Prepare().Execute(0,2);
        Assert.AreEqual(4.0f, entitymanager.Load<ValueModel>(v => v.Single).ExecuteScalar<int>());
    }

    [Test, Parallelizable]
    public void AddToTimespan() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new(dbclient);
        entitymanager.UpdateSchema<ValueModel>();
        DateTime now = new(2025, 01, 01);
        DateTime later = new(2025, 01, 02);
        entitymanager.Insert<ValueModel>()
                     .Columns(v => v.Timespan, v=>v.NDatetime)
                     .Values(TimeSpan.FromHours(0), now)
                     .Execute();
        entitymanager.Update<ValueModel>()
                     .Set(v => v.Timespan == v.Timespan + (later - v.NDatetime))
                     .Prepare()
                     .Execute();
        Assert.AreEqual(TimeSpan.FromDays(1), entitymanager.Load<ValueModel>(v => v.Timespan).ExecuteScalar<TimeSpan>());
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
        ValueModel[] result = entitymanager.Load<ValueModel>().Where(v => DBParameter<int[]>.Value.Contains(v.Integer))
                                           .Prepare().ExecuteEntities<ValueModel>(new[] { 1, 2 }).ToArray();
        Assert.AreEqual(2, result.Length);
        for(int i = 0; i < 2; ++i)
            Assert.AreEqual(i + 1, result[i].Integer);
    }

    [Test]
    public void CustomParameterIndexValue() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new EntityManager(dbclient);
        entitymanager.UpdateSchema<ValueModel>();
        PreparedLoadOperation operation = entitymanager.Load<ValueModel>()
                                                       .Where(v => v.Integer == DBParameter<int>.Index(1).Data)
                                                       .Prepare();
    }

    [Test]
    public void ParameterArrayWithOtherParametersContains() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new EntityManager(dbclient);
        entitymanager.UpdateSchema<ValueModel>();
        entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(0, 0.0f, 1.0).Execute();
        entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(1, 0.0f, 0.0).Execute();
        entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(2, 0.0f, 1.0).Execute();
        entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(3, 0.0f, 0.0).Execute();
        ValueModel[] result = entitymanager.Load<ValueModel>().Where(v => DBParameter<int[]>.Value.Contains(v.Integer) && v.Double == DBParameter.Double)
                                           .Prepare().ExecuteEntities<ValueModel>(new[] { 1, 2 }, 1.0).ToArray();
        Assert.AreEqual(1, result.Length);
        Assert.AreEqual(2, result[0].Integer);
        Assert.AreEqual(1.0, result[0].Double);
    }
}