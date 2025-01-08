using System.Linq;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Models;
using Pooshit.Ocelot.Tokens;

namespace Pooshit.Ocelot.Tests.Fields;

[TestFixture, Parallelizable]
public class FieldTests {

    [Test, Parallelizable]
    public void LoadAllFields()
    {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new(dbclient);
        entitymanager.UpdateSchema<ValueModel>();
        entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(0, 0.0f, 1.0).Execute();
        entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(1, 0.0f, 0.0).Execute();
        entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(2, 1.0f, 1.0).Execute();
        entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(3, 1.0f, 0.0).Execute();
        ValueModel[] result = entitymanager.Load<ValueModel>(m => DBFunction.All)
                                           .ExecuteTypes(r => new ValueModel((int) (long) r["integer"]))
                                           .ToArray();
        Assert.IsTrue(new[] {0, 1, 2, 3}.SequenceEqual(result.Select(r => r.Integer)));
    }

    [Test, Parallelizable]
    public void ReferenceFields()
    {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new(dbclient);
        entitymanager.UpdateSchema<ValueModel>();
        entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(4, 2.0f, 1.0).Execute();
        entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(21, 7.0f, 0.0).Execute();
        entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(40, 10.0f, 1.0).Execute();
        entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Values(60, 12.0f, 0.0).Execute();
        ValueModel[] result = entitymanager.Load<ValueModel>(v=>DB.As(v.Integer, "field1"),
                                                             v=>DB.As(v.Single, "field2"),
                                                             v=>DB.Field("field1")/DB.Field("field2"))
                                           .ExecuteTypes(r => new ValueModel((int) (double) r[2]))
                                           .ToArray();
        Assert.IsTrue(new[] {2, 3, 4, 5}.SequenceEqual(result.Select(r => r.Integer)));
    }

    [Test, Parallelizable]
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