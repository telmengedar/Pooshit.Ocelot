using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Extensions;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Models;
using Pooshit.Ocelot.Tokens;
using Pooshit.Ocelot.Tokens.Values;

namespace Pooshit.Ocelot.Tests.Fields;

[TestFixture]
public class AggregateTests {

    [Test]
    public void TestSumFields() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new(dbclient);

        entitymanager.UpdateSchema<ValueModel>();

        entitymanager.InsertEntities<ValueModel>().Execute(
                                                           new ValueModel {Integer = 5},
                                                           new ValueModel(),
                                                           new ValueModel {Integer = 11},
                                                           new ValueModel {Integer = 3},
                                                           new ValueModel {Integer = 7});

        int result = entitymanager.Load<ValueModel>(m=>DBFunction.Sum(m.Integer)).ExecuteScalar<int>();
        Assert.AreEqual(26, result);
    }

    [Test]
    public void TestSumFieldsByDB() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new(dbclient);

        entitymanager.UpdateSchema<ValueModel>();

        entitymanager.InsertEntities<ValueModel>().Execute(
                                                           new ValueModel {Integer = 5},
                                                           new ValueModel(),
                                                           new ValueModel {Integer = 11},
                                                           new ValueModel {Integer = 3},
                                                           new ValueModel {Integer = 7});

        int result = entitymanager.Load<ValueModel>(m=>DB.Sum(m.Integer)).ExecuteScalar<int>();
        Assert.AreEqual(26, result);
    }

    [Test]
    public void LoadAnyFromGroup() {
	    IDBClient dbclient = TestData.CreateDatabaseAccess();
	    EntityManager entitymanager = new(dbclient);

	    entitymanager.UpdateSchema<ValueModel>();

	    entitymanager.InsertEntities<ValueModel>()
	                 .Execute(
	                          new ValueModel { Integer = 5, String = "haha" },
	                          new ValueModel { String = "haha" },
	                          new ValueModel { Integer = 11, String = "haha" },
	                          new ValueModel { Integer = 3, String = "haha" },
	                          new ValueModel { Integer = 7, String = "haha" });

	    string[] result = entitymanager.Load<ValueModel>(DB.Any(DB.Property<ValueModel>(v => v.String)))
	                                   .GroupBy(v => v.Integer)
	                                   .ExecuteSet<string>()
	                                   .ToArray();
    }

    [Test]
    public void TestSumExpressions()
    {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new(dbclient);

        entitymanager.UpdateSchema<ValueModel>();

        entitymanager.InsertEntities<ValueModel>().Execute(
                                                           new ValueModel { Integer = 5 },
                                                           new ValueModel(),
                                                           new ValueModel { Integer = 11 },
                                                           new ValueModel { Integer = 3 },
                                                           new ValueModel { Integer = 7 });

        int result = entitymanager.Load<ValueModel>(m=>DBFunction.Sum(m.Integer)).ExecuteScalar<int>();
        Assert.AreEqual(26, result);
    }

    [Test, Parallelizable]
    public async Task TestAbs()
    {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new(dbclient);

        entitymanager.UpdateSchema<ValueModel>();

        entitymanager.InsertEntities<ValueModel>().Execute(
                                                           new ValueModel { Integer = 5 },
                                                           new ValueModel(),
                                                           new ValueModel { Integer = 11 },
                                                           new ValueModel { Integer = 3 },
                                                           new ValueModel { Integer = 7 });

        int[] result = await entitymanager.Load<ValueModel>(m=>m.Integer).OrderBy(m=>DB.Abs(m.Integer-6)).ExecuteSetAsync<int>().ToArray();
        CollectionAssert.AreEqual(new[]{5,7,3,11,0}, result);
    }

    [Test, Parallelizable]
    public async Task TestCountDistinct()
    {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entityManager = new(dbclient);

        entityManager.UpdateSchema<ValueModel>();

        entityManager.InsertEntities<ValueModel>().Execute(
                                                           new ValueModel { String = "A", Integer = 5 },
                                                           new ValueModel { String = "A" },
                                                           new ValueModel { String = "B", Integer = 11 },
                                                           new ValueModel { String = "B", Integer = 11 },
                                                           new ValueModel { String = "B", Integer = 7 });

        decimal value = await entityManager.Load<ValueModel>(x => DB.Cast(DB.Count(DB.Distinct(x.Integer)), CastType.Float).Decimal / DB.Count().Decimal)
                                     .ExecuteScalarAsync<decimal>();
        Assert.AreEqual(0.8m, value);
    }

    [Test, Parallelizable]
    public async Task TestCountDistinctSubSelect()
    {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entityManager = new(dbclient);

        entityManager.UpdateSchema<ValueModel>();

        entityManager.InsertEntities<ValueModel>().Execute(
                                                           new ValueModel { String = "A", Integer = 5 },
                                                           new ValueModel { String = "A" },
                                                           new ValueModel { String = "B", Integer = 11 },
                                                           new ValueModel { String = "B", Integer = 11 },
                                                           new ValueModel { String = "B", Integer = 7 });

        ISqlToken stringField = DB.Property<ValueModel>(m => m.String);
        IDatabaseOperation subSelect = entityManager.Load<ValueModel>(x=> DB.As(stringField, "string"), x => DB.As(DB.Cast(DB.Count(DB.Distinct(x.Integer)), CastType.Float).Decimal / DB.Count().Decimal, "ratio"))
                                                    .GroupBy(v => v.String);

        Tuple<string, float>[] result = await entityManager.Load(() => DB.Column("ss", "string"), () => DB.Column("ss", "ratio"))
                                                           .From(subSelect)
                                                           .Alias("ss")
                                                           .ExecuteTypesAsync(r => new Tuple<string, float>(r.GetValue<string>(0), r.GetValue<float>(1)))
                                                           .ToArray();

        Assert.AreEqual(2, result.Length);
        Tuple<string, float> a = result.FirstOrDefault(r=>r.Item1=="A");
        Tuple<string, float> b = result.FirstOrDefault(r=>r.Item1=="B");

        Assert.NotNull(a);
        Assert.NotNull(b);
        
        Assert.AreEqual(1.0f, a.Item2);
        Assert.AreEqual(2.0f/3.0f, b.Item2);
    }

    [Test]
    public void TestTotalFields()
    {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new(dbclient);

        entitymanager.UpdateSchema<ValueModel>();

        entitymanager.InsertEntities<ValueModel>().Execute(
                                                           new ValueModel { Double = 5 },
                                                           new ValueModel(),
                                                           new ValueModel { Double = 11.3 },
                                                           new ValueModel { Double = 3 },
                                                           new ValueModel { Double = 7 });

        double result = entitymanager.Load<ValueModel>(v => DBFunction.Total(EntityField.Create<ValueModel>(m => m.Double))).ExecuteScalar<double>();
        Assert.AreEqual(26.3, result);
    }

    [Test]
    public void TestTotalExpressions()
    {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new(dbclient);

        entitymanager.UpdateSchema<ValueModel>();

        entitymanager.InsertEntities<ValueModel>().Execute(
                                                           new ValueModel { Double = 5 },
                                                           new ValueModel(),
                                                           new ValueModel { Double = 11.3 },
                                                           new ValueModel { Double = 3 },
                                                           new ValueModel { Double = 7 });

        double result = entitymanager.Load<ValueModel>(v => DBFunction.Total(v.Double)).ExecuteScalar<double>();
        Assert.AreEqual(26.3, result);
    }

    [Test]
    public void TestAverageFields()
    {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new(dbclient);

        entitymanager.UpdateSchema<ValueModel>();

        entitymanager.InsertEntities<ValueModel>().Execute(
                                                           new ValueModel { Integer = 5 },
                                                           new ValueModel(),
                                                           new ValueModel { Integer = 11 },
                                                           new ValueModel { Integer = 3 },
                                                           new ValueModel { Integer = 7 });

        double result = entitymanager.Load<ValueModel>(m=>DBFunction.Average(m.Integer)).ExecuteScalar<double>();
        Assert.AreEqual(5.2, result);
    }

    [Test]
    public void TestAverageExpressions()
    {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new EntityManager(dbclient);

        entitymanager.UpdateSchema<ValueModel>();

        entitymanager.InsertEntities<ValueModel>().Execute(
                                                           new ValueModel { Integer = 5 },
                                                           new ValueModel(),
                                                           new ValueModel { Integer = 11 },
                                                           new ValueModel { Integer = 3 },
                                                           new ValueModel { Integer = 7 });

        double result = entitymanager.Load<ValueModel>(m=>DBFunction.Average(m.Integer)).ExecuteScalar<double>();
        Assert.AreEqual(5.2, result);
    }

    [Test]
    public void TestMinFields()
    {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new(dbclient);

        entitymanager.UpdateSchema<ValueModel>();

        entitymanager.InsertEntities<ValueModel>().Execute(
                                                           new ValueModel { Integer = 5 },
                                                           new ValueModel(),
                                                           new ValueModel { Integer = 11 },
                                                           new ValueModel { Integer = 3 },
                                                           new ValueModel { Integer = 7 });

        int result = entitymanager.Load<ValueModel>(m=>DB.Min(m.Integer)).ExecuteScalar<int>();
        Assert.AreEqual(0, result);
    }

    [Test]
    public void TestMinExpressions()
    {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new EntityManager(dbclient);

        entitymanager.UpdateSchema<ValueModel>();

        entitymanager.InsertEntities<ValueModel>().Execute(
                                                           new ValueModel { Integer = 5 },
                                                           new ValueModel(),
                                                           new ValueModel { Integer = 11 },
                                                           new ValueModel { Integer = 3 },
                                                           new ValueModel { Integer = 7 });

        int result = entitymanager.Load<ValueModel>(m=>DBFunction.Min(m.Integer)).ExecuteScalar<int>();
        Assert.AreEqual(0, result);
    }

    [Test]
    public void TestMaxFields()
    {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new EntityManager(dbclient);

        entitymanager.UpdateSchema<ValueModel>();

        entitymanager.InsertEntities<ValueModel>().Execute(
                                                           new ValueModel { Integer = 5 },
                                                           new ValueModel(),
                                                           new ValueModel { Integer = 11 },
                                                           new ValueModel { Integer = 3 },
                                                           new ValueModel { Integer = 7 });

        int result = entitymanager.Load<ValueModel>(m=>DBFunction.Max(m.Integer)).ExecuteScalar<int>();
        Assert.AreEqual(11, result);
    }

    [Test]
    public void TestMaxExpressions()
    {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new EntityManager(dbclient);

        entitymanager.UpdateSchema<ValueModel>();

        entitymanager.InsertEntities<ValueModel>().Execute(
                                                           new ValueModel { Integer = 5 },
                                                           new ValueModel(),
                                                           new ValueModel { Integer = 11 },
                                                           new ValueModel { Integer = 3 },
                                                           new ValueModel { Integer = 7 });

        int result = entitymanager.Load<ValueModel>(m=>DBFunction.Max(m.Integer)).ExecuteScalar<int>();
        Assert.AreEqual(11, result);
    }

    [Test]
    public void TestMaxUpdate() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new EntityManager(dbclient);

        entitymanager.UpdateSchema<ValueModel>();

        entitymanager.InsertEntities<ValueModel>().Execute(new ValueModel {Integer = 0});

        entitymanager.Update<ValueModel>().Set(v => v.Integer == DBFunction.Max(v.Integer - 2, -1)).Execute();

        Assert.AreEqual(-1, entitymanager.Load<ValueModel>(i => i.Integer).ExecuteScalar<int>());
    }

    [Test]
    public void TestMinUpdate()
    {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new EntityManager(dbclient);

        entitymanager.UpdateSchema<ValueModel>();

        entitymanager.InsertEntities<ValueModel>().Execute(new ValueModel { Integer = 4 });

        entitymanager.Update<ValueModel>().Set(v => v.Integer == DBFunction.Min(v.Integer * v.Integer, 12)).Execute();

        Assert.AreEqual(12, entitymanager.Load<ValueModel>(i => i.Integer).ExecuteScalar<int>());
    }
}