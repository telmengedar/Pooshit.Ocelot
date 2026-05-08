using System;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Tokens;
using Pooshit.Ocelot.Tokens.Partitions;

namespace Pooshit.Ocelot.Tests.Tokens;

[TestFixture, Parallelizable]
public class WindowedAggregateTests {

    IDBClient CreateClient() {
        DBInfo dbInfo = new SQLiteInfo();
        Mock<IDBClient> dbclient = new();
        dbclient.SetupGet(c => c.DBInfo).Returns(dbInfo);
        return dbclient.Object;
    }

    string GetSql(Action<OperationPreparator> build) {
        IDBClient client = CreateClient();
        OperationPreparator preparator = new();
        build(preparator);
        PreparedOperation operation = preparator.GetOperation(client, false);
        return operation.CommandText;
    }

    [Test, Parallelizable]
    public void CountOverEmpty_EmitsCountStarOverParens() {
        WindowedAggregate token = new(DB.Count(DB.All));
        string sql = GetSql(p => token.ToSql(new SQLiteInfo(), p, EntityDescriptor.Create, null));
        Assert.AreEqual("COUNT(*) OVER()", sql.Trim());
    }

    [Test, Parallelizable]
    public void CountOverWithAlias_EmitsAlias() {
        WindowedAggregate token = new(DB.Count(DB.All), alias: "__total");
        string sql = GetSql(p => token.ToSql(new SQLiteInfo(), p, EntityDescriptor.Create, null));
        StringAssert.Contains("COUNT(*) OVER()", sql);
        StringAssert.Contains("AS", sql);
        StringAssert.Contains("__total", sql);
    }

    [Test, Parallelizable]
    public void DbCountOver_ProducesEquivalentToManualConstruction() {
        WindowedAggregate fromFactory = DB.CountOver();
        WindowedAggregate manual = new(DB.Count(DB.All));

        string factorySql = GetSql(p => fromFactory.ToSql(new SQLiteInfo(), p, EntityDescriptor.Create, null));
        string manualSql = GetSql(p => manual.ToSql(new SQLiteInfo(), p, EntityDescriptor.Create, null));

        Assert.AreEqual(manualSql.Trim(), factorySql.Trim());
    }

    [Test, Parallelizable]
    public void DbCountOverWithAlias_EmitsAlias() {
        WindowedAggregate token = DB.CountOver(alias: "__total");
        string sql = GetSql(p => token.ToSql(new SQLiteInfo(), p, EntityDescriptor.Create, null));
        StringAssert.Contains("COUNT(*) OVER()", sql);
        StringAssert.Contains("__total", sql);
    }

    [Test, Parallelizable]
    public void RowNumberOver_StillEmitsRowNumber() {
        // Regression guard: RowNumberOver must remain unchanged
        RowNumberOver token = new(new OrderByCriteria(DB.Column("id")));
        string sql = GetSql(p => token.ToSql(new SQLiteInfo(), p, EntityDescriptor.Create, null));
        StringAssert.Contains("ROW_NUMBER()", sql);
        StringAssert.Contains("OVER(", sql);
        StringAssert.Contains("ORDER BY", sql);
    }

    [Test, Parallelizable]
    public void CountOverWithPartitionBy_EmitsPartitionByClause() {
        WindowedAggregate token = DB.CountOver(partitionBy: DB.Column("category"));
        string sql = GetSql(p => token.ToSql(new SQLiteInfo(), p, EntityDescriptor.Create, null));
        StringAssert.Contains("PARTITION BY", sql);
        StringAssert.Contains("category", sql);
    }

    [Test, Parallelizable]
    public void SumOver_EmitsSumFunction() {
        WindowedAggregate token = DB.SumOver(DB.Column("value"));
        string sql = GetSql(p => token.ToSql(new SQLiteInfo(), p, EntityDescriptor.Create, null));
        StringAssert.Contains("SUM(", sql);
        StringAssert.Contains("OVER(", sql);
    }

    [Test, Parallelizable]
    public void AvgOver_EmitsAvgFunction() {
        WindowedAggregate token = DB.AvgOver(DB.Column("score"));
        string sql = GetSql(p => token.ToSql(new SQLiteInfo(), p, EntityDescriptor.Create, null));
        StringAssert.Contains("AVG(", sql);
    }

    [Test, Parallelizable]
    public void MinOver_EmitsMinFunction() {
        WindowedAggregate token = DB.MinOver(DB.Column("price"));
        string sql = GetSql(p => token.ToSql(new SQLiteInfo(), p, EntityDescriptor.Create, null));
        StringAssert.Contains("MIN(", sql);
    }

    [Test, Parallelizable]
    public void MaxOver_EmitsMaxFunction() {
        WindowedAggregate token = DB.MaxOver(DB.Column("price"));
        string sql = GetSql(p => token.ToSql(new SQLiteInfo(), p, EntityDescriptor.Create, null));
        StringAssert.Contains("MAX(", sql);
    }

    [Test, Parallelizable]
    public void SumOverWithPartitionByAndOrderBy_EmitsFullWindowClause() {
        WindowedAggregate token = DB.SumOver(
            DB.Column("amount"),
            partitionBy: DB.Column("category"),
            orderBy: new OrderByCriteria(DB.Column("created"), ascending: false));
        string sql = GetSql(p => token.ToSql(new SQLiteInfo(), p, EntityDescriptor.Create, null));
        StringAssert.Contains("SUM(", sql);
        StringAssert.Contains("PARTITION BY", sql);
        StringAssert.Contains("ORDER BY", sql);
        StringAssert.Contains("DESC", sql);
    }
}
