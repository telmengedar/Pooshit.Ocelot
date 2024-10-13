using NightlyCode.Database.Tests.Data;
using NightlyCode.Database.Tests.Models;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Tokens;
using Pooshit.Ocelot.Tokens.Operations;

namespace NightlyCode.Database.Tests;

[TestFixture, Parallelizable]
public class UpdateDataOperationTests {

    [Test, Parallelizable]
    public void UpdateWithoutParameters() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new(dbclient);
        entitymanager.UpdateSchema<ValueModel>();

        entitymanager.Insert<ValueModel>()
                     .Columns(v => v.String, v => v.Integer, v => v.Single, v => v.Double)
                     .Values("hallo", 7, 1.0f, 3.0)
                     .Execute();


        entitymanager.UpdateData("valuemodel")
                     .Set("string", "single", "double")
                     .Where(new OperationToken(DB.Column("integer"), Operand.Equal, DB.Constant(7)))
                     .Execute("hello", 5.0f, 10.0);

        ValueModel result = entitymanager.Load<ValueModel>().ExecuteEntity<ValueModel>();

        Assert.NotNull(result);
        Assert.AreEqual("hello", result.String);
        Assert.AreEqual(5.0f, result.Single);
        Assert.AreEqual(10.0, result.Double);
    }
}