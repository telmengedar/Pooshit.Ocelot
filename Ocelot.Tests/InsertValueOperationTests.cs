using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Extensions;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Models;
using Pooshit.Ocelot.Tokens;

namespace Pooshit.Ocelot.Tests;

[TestFixture, Parallelizable]
public class InsertValueOperationTests {

    [Test, Parallelizable]
    public void TestReturnID() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new(dbclient);
        entitymanager.UpdateSchema<AutoIncrementEntity>();

        PreparedOperation insertop = entitymanager.Insert<AutoIncrementEntity>().Columns(c => c.Bla).ReturnID().Prepare();

        long id = insertop.Execute("blubb");
        Assert.AreEqual(1, id);
        id = insertop.Execute("blobb");
        Assert.AreEqual(2, id);
    }

    [Test, Parallelizable]
    public void InsertNullableDateTime() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new EntityManager(dbclient);
        entitymanager.UpdateSchema<ValueModel>();

        PreparedOperation insertop = entitymanager.Insert<ValueModel>().Columns(c => c.NDatetime).Prepare();

        long id = insertop.Execute(DateTime.UtcNow);
        Assert.AreEqual(1, id);
    }

    [Test, Parallelizable]
    public void InsertParametersWithoutPrepare() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new(dbclient);
        entitymanager.UpdateSchema<ValueModel>();

        InsertValuesOperation<ValueModel> insertop = entitymanager.Insert<ValueModel>().Columns(c => c.String);

        long id = insertop.Execute("lala");
        Assert.AreEqual(1, id);
    }

    [Test, Parallelizable]
    public void InsertBulk() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new EntityManager(dbclient);
        entitymanager.UpdateSchema<ValueModel>();

        PreparedBulkInsertOperation insertop = entitymanager.Insert<ValueModel>()
                                                            .Columns(c => c.String, c => c.Integer, c => c.Single)
                                                            .PrepareBulk();

        insertop.Execute(new object[]{new object[] {"Rolf", 32, 1.0f}, new object[] {"Ulf", 11, 0.5f}, new object[] {"Lutz", 25, 0.8f}});

        Assert.AreEqual(3, entitymanager.Load<ValueModel>(DB.Count(DB.All)).ExecuteScalar<int>());
    }
        
    [Test, Parallelizable]
    public void InsertNullableDBNull() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new(dbclient);
        entitymanager.UpdateSchema<ValueModel>();

        InsertValuesOperation<ValueModel> insertop = entitymanager.Insert<ValueModel>().Columns(c => c.NDatetime);

        long id = insertop.Execute(DBNull.Value);
        Assert.AreEqual(1, id);
    }

    [Test, Parallelizable]
    public async Task InsertUsingSelect() {
        IDBClient dbClient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new(dbClient);

        entitymanager.UpdateSchema<ValueModel>();
        entitymanager.UpdateSchema<ValueModel2>();
            
        InsertValuesOperation<ValueModel> insertop = entitymanager.Insert<ValueModel>().Columns(c => c.Integer);
        for (int i = 0; i < 10; ++i)
            await insertop.ExecuteAsync(i);

        await entitymanager.Insert<ValueModel2>().Columns(v=>v.Integer).Select(entitymanager.Load<ValueModel>(v=>v.Integer)).ExecuteAsync();

        int[] values = (await entitymanager.Load<ValueModel2>(v => v.Integer).ExecuteSetAsync<int>().ToArray()).OrderBy(v => v).ToArray();
        Assert.AreEqual(10, values.Length);
        for (int i = 0; i < 10; ++i)
            Assert.AreEqual(i, values[i]);
    }

    [Test, Parallelizable]
    public async Task InsertUsingSelectWithoutColumns() {
        IDBClient dbClient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new(dbClient);

        entitymanager.UpdateSchema<ValueModel>();
        entitymanager.UpdateSchema<ValueModel2>();
            
        InsertValuesOperation<ValueModel> insertop = entitymanager.Insert<ValueModel>().Columns(c => c.Integer);
        for (int i = 0; i < 10; ++i)
            await insertop.ExecuteAsync(i);

        await entitymanager.Insert<ValueModel2>().Select(entitymanager.Load<ValueModel>()).ExecuteAsync();

        int[] values = (await entitymanager.Load<ValueModel2>(v => v.Integer).ExecuteSetAsync<int>().ToArray()).OrderBy(v => v).ToArray();
        Assert.AreEqual(10, values.Length);
        for (int i = 0; i < 10; ++i)
            Assert.AreEqual(i, values[i]);
    }

}