using NUnit.Framework;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Models;
using Pooshit.Ocelot.Tokens;
using Pooshit.Ocelot.Tokens.Expressions;

namespace Pooshit.Ocelot.Tests;

[TestFixture, Parallelizable]
public class TokenTests {

    [Test, Parallelizable]
    public void CountSpecificValues() {
        IEntityManager database = TestData.CreateEntityManager();
        database.UpdateSchema<ValueModel>();

        PreparedOperation insert = database.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Prepare();

        for (int i = 0; i < 16; ++i)
            insert.Execute(i, 0.0f, 0.0);

        long count = database.Load<ValueModel>(DB.Count(DB.If(DB.Predicate<ValueModel>(v => v.Integer > 3 && v.Integer < 8), DB.Constant(1)))).ExecuteScalar<long>();
        Assert.AreEqual(4, count);
    }

    [Test, Parallelizable]
    public void CountSpecificValuesUsingExpressions() {
        IEntityManager database = TestData.CreateEntityManager();
        database.UpdateSchema<ValueModel>();

        PreparedOperation insert = database.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Prepare();

        for (int i = 0; i < 16; ++i)
            insert.Execute(i, 0.0f, 0.0);

        long count = database.Load<ValueModel>(v=>DB.Count(DB.If(v.Integer > 3 && v.Integer < 8, 1, null))).ExecuteScalar<long>();
        Assert.AreEqual(4, count);
    }

    [Test, Parallelizable]
    public void SumSpecificValuesUsingExpressionsCase() {
        IEntityManager database = TestData.CreateEntityManager();
        database.UpdateSchema<ValueModel>();

        PreparedOperation insert = database.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Prepare();

        for (int i = 0; i < 16; ++i)
            insert.Execute(i, 0.0f, 0.0);

        long count = database.Load<ValueModel>(v=>DB.Sum(
                                                           DB.Case(new[]{
                                                                            DB.When(v.Integer < 3, 1),
                                                                            DB.When(v.Integer > 10, 2)
                                                                        }, 5))).ExecuteScalar<long>();
        Assert.AreEqual(53, count);
    }

    [Test, Parallelizable]
    public void SumSpecificValuesUsingExpressionsCaseNoDefault() {
        IEntityManager database = TestData.CreateEntityManager();
        database.UpdateSchema<ValueModel>();

        PreparedOperation insert = database.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Prepare();

        for (int i = 0; i < 16; ++i)
            insert.Execute(i, 0.0f, 0.0);

        long count = database.Load<ValueModel>(v=>DB.Sum(
                                                         DB.Case(new[]{
                                                                          DB.When(v.Integer < 3, 1),
                                                                          DB.When(v.Integer > 10, 2)
                                                                      }, null))).ExecuteScalar<long>();
        Assert.AreEqual(13, count);
    }

    [Test, Parallelizable]
    public void CountSpecificValuesInExpression() {
        IEntityManager database = TestData.CreateEntityManager();
        database.UpdateSchema<ValueModel>();

        PreparedOperation insert = database.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Prepare();

        for (int i = 0; i < 16; ++i)
            insert.Execute(i, 0.0f, 0.0);

        long count = database.Load<ValueModel>(v=>Xpr.Count(Xpr.If(Xpr.Predicate(v.Integer > 3 && v.Integer < 8), Xpr.Constant(1)))).ExecuteScalar<long>();
        Assert.AreEqual(4, count);
    }
        
    [Test, Parallelizable]
    public void CountSpecificValuesInSimplifiedExpression() {
        IEntityManager database = TestData.CreateEntityManager();
        database.UpdateSchema<ValueModel>();

        PreparedOperation insert = database.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Prepare();

        for (int i = 0; i < 16; ++i)
            insert.Execute(i, 0.0f, 0.0);

        long count = database.Load<ValueModel>(v=>Xpr.Count(Xpr.If(v.Integer > 3 && v.Integer < 8, 1))).ExecuteScalar<long>();
        Assert.AreEqual(4, count);
    }

}