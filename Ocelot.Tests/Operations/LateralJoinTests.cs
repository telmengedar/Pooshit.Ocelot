using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Npgsql;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Models;

namespace Pooshit.Ocelot.Tests.Operations;

/// <summary>
/// Tests for LATERAL JOIN expression support (DiVoid #449).
/// SQLite-backed tests run unconditionally; Postgres round-trip tests are gated on
/// POSTGRES_CONNECTION and call Assert.Inconclusive when the variable is absent.
/// </summary>
[TestFixture, Parallelizable]
public class LateralJoinTests {

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    static IEntityManager CreateEntityManager() => TestData.CreateEntityManager();

    /// <summary>
    /// Creates a mock IDBClient backed by the specified IDBInfo.
    /// Suitable for Prepare()-only tests that capture CommandText without executing.
    /// </summary>
    static IDBClient CreateClientWithInfo(IDBInfo info) {
        Mock<IDBClient> mock = new();
        mock.SetupGet(c => c.DBInfo).Returns(info);
        return mock.Object;
    }

    static Func<Type, EntityDescriptor> CreateDescriptorGetter() {
        EntityDescriptorCache cache = new();
        return t => cache.Get(t);
    }

    // -------------------------------------------------------------------------
    // 1. Capability flag sanity
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public void SupportsLateralJoin_SQLite_ReturnsFalse() {
        Assert.IsFalse(new SQLiteInfo().SupportsLateralJoin,
            "SQLiteInfo must report SupportsLateralJoin = false");
    }

    [Test, Parallelizable]
    public void SupportsLateralJoin_Postgres_ReturnsTrue() {
        Assert.IsTrue(new PostgreInfo().SupportsLateralJoin,
            "PostgreInfo must report SupportsLateralJoin = true");
    }

    [Test, Parallelizable]
    public void SupportsLateralJoin_MySQL_ReturnsTrue() {
        Assert.IsTrue(new MySQLInfo().SupportsLateralJoin,
            "MySQLInfo must report SupportsLateralJoin = true");
    }

    [Test, Parallelizable]
    public void SupportsLateralJoin_MsSql_ReturnsTrue() {
        Assert.IsTrue(new MsSqlInfo().SupportsLateralJoin,
            "MsSqlInfo must report SupportsLateralJoin = true");
    }

    // -------------------------------------------------------------------------
    // 2. SQLite throws NotSupportedException at Prepare() time
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public void LateralJoin_SQLite_ThrowsNotSupportedAtPrepare() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();
        em.UpdateSchema<ValueModel2>();

        // Build phase must succeed — the throw happens at Prepare time
        LoadOperation<ValueModel> op = em.Load<ValueModel>()
            .LateralJoin(em.Load<ValueModel2>(), joinAlias: "lat0");

        NotSupportedException ex = Assert.Throws<NotSupportedException>(() => op.Prepare(false));
        StringAssert.Contains("LATERAL", ex.Message,
            "Exception message must name the unsupported feature");
    }

    [Test, Parallelizable]
    public void LeftLateralJoin_SQLite_ThrowsNotSupportedAtPrepare() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();
        em.UpdateSchema<ValueModel2>();

        LoadOperation<ValueModel> op = em.Load<ValueModel>()
            .LeftLateralJoin(em.Load<ValueModel2>(), joinAlias: "lat0");

        NotSupportedException ex = Assert.Throws<NotSupportedException>(() => op.Prepare(false));
        StringAssert.Contains("LATERAL", ex.Message);
    }

    [Test, Parallelizable]
    public void LateralJoin_NullInner_ThrowsArgumentNullException() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();

        Assert.Throws<ArgumentNullException>(() =>
            em.Load<ValueModel>().LateralJoin(null));
    }

    [Test, Parallelizable]
    public void LeftLateralJoin_NullInner_ThrowsArgumentNullException() {
        IEntityManager em = CreateEntityManager();
        em.UpdateSchema<ValueModel>();

        Assert.Throws<ArgumentNullException>(() =>
            em.Load<ValueModel>().LeftLateralJoin(null));
    }

    // -------------------------------------------------------------------------
    // 3. Generated SQL — Postgres dialect
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public void LateralJoin_Postgres_EmitsInnerJoinLateral_OnTrue() {
        IDBInfo info = new PostgreInfo();
        IDBClient client = CreateClientWithInfo(info);
        Func<Type, EntityDescriptor> getter = CreateDescriptorGetter();

        // Build inner and outer directly to control the dialect
        LoadOperation<ValueModel2> inner = new(client, getter,
            new IDBField[] { Field.Property<ValueModel2>(v => v.Integer) });
        LoadOperation<ValueModel> outer = new LoadOperation<ValueModel>(client, getter,
            Array.Empty<IDBField>())
            .LateralJoin(inner, joinAlias: "lat0");

        string sql = outer.Prepare(false).CommandText;

        StringAssert.Contains("INNER JOIN LATERAL", sql, "Postgres LateralJoin must emit INNER JOIN LATERAL");
        StringAssert.Contains("AS lat0", sql, "Postgres LateralJoin must use the supplied alias");
        StringAssert.Contains("ON TRUE", sql, "Postgres LateralJoin without criteria must emit ON TRUE");
    }

    [Test, Parallelizable]
    public void LeftLateralJoin_Postgres_EmitsLeftJoinLateral_OnTrue() {
        IDBInfo info = new PostgreInfo();
        IDBClient client = CreateClientWithInfo(info);
        Func<Type, EntityDescriptor> getter = CreateDescriptorGetter();

        LoadOperation<ValueModel2> inner = new(client, getter,
            new IDBField[] { Field.Property<ValueModel2>(v => v.Integer) });
        LoadOperation<ValueModel> outer = new LoadOperation<ValueModel>(client, getter,
            Array.Empty<IDBField>())
            .LeftLateralJoin(inner, joinAlias: "lat0");

        string sql = outer.Prepare(false).CommandText;

        StringAssert.Contains("LEFT JOIN LATERAL", sql, "Postgres LeftLateralJoin must emit LEFT JOIN LATERAL");
        StringAssert.Contains("AS lat0", sql, "Postgres LeftLateralJoin must use the supplied alias");
        StringAssert.Contains("ON TRUE", sql, "Postgres LeftLateralJoin without criteria must emit ON TRUE");
    }

    [Test, Parallelizable]
    public void AutoAlias_LateralJoin_GeneratesLat0() {
        IDBInfo info = new PostgreInfo();
        IDBClient client = CreateClientWithInfo(info);
        Func<Type, EntityDescriptor> getter = CreateDescriptorGetter();

        LoadOperation<ValueModel2> inner = new(client, getter,
            new IDBField[] { Field.Property<ValueModel2>(v => v.Integer) });
        LoadOperation<ValueModel> outer = new LoadOperation<ValueModel>(client, getter,
            Array.Empty<IDBField>())
            .LateralJoin(inner); // no explicit alias — should auto-generate lat0

        string sql = outer.Prepare(false).CommandText;

        StringAssert.Contains("AS lat0", sql, "Auto-generated alias for first lateral join must be lat0");
    }

    [Test, Parallelizable]
    public void AutoAlias_TwoLateralJoins_GeneratesLat0AndLat1() {
        IDBInfo info = new PostgreInfo();
        IDBClient client = CreateClientWithInfo(info);
        Func<Type, EntityDescriptor> getter = CreateDescriptorGetter();

        LoadOperation<ValueModel2> inner1 = new(client, getter,
            new IDBField[] { Field.Property<ValueModel2>(v => v.Integer) });
        LoadOperation<ValueModel2> inner2 = new(client, getter,
            new IDBField[] { Field.Property<ValueModel2>(v => v.Single) });

        // Chain two lateral joins — aliases must differ
        LoadOperation<ValueModel> outer = new LoadOperation<ValueModel>(client, getter,
            Array.Empty<IDBField>())
            .LateralJoin(inner1)
            .LateralJoin(inner2);

        string sql = outer.Prepare(false).CommandText;

        StringAssert.Contains("AS lat0", sql, "First auto-alias must be lat0");
        StringAssert.Contains("AS lat1", sql, "Second auto-alias must be lat1");
    }

    // -------------------------------------------------------------------------
    // 4. Generated SQL — MSSQL dialect (CROSS APPLY / OUTER APPLY)
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public void LateralJoin_MsSql_EmitsCrossApply() {
        IDBInfo info = new MsSqlInfo();
        IDBClient client = CreateClientWithInfo(info);
        Func<Type, EntityDescriptor> getter = CreateDescriptorGetter();

        LoadOperation<ValueModel2> inner = new(client, getter,
            new IDBField[] { Field.Property<ValueModel2>(v => v.Integer) });
        LoadOperation<ValueModel> outer = new LoadOperation<ValueModel>(client, getter,
            Array.Empty<IDBField>())
            .LateralJoin(inner, joinAlias: "lat0");

        string sql = outer.Prepare(false).CommandText;

        StringAssert.Contains("CROSS APPLY", sql, "MSSQL LateralJoin must emit CROSS APPLY");
        StringAssert.DoesNotContain("LATERAL", sql, "MSSQL LateralJoin must not emit LATERAL");
    }

    [Test, Parallelizable]
    public void LeftLateralJoin_MsSql_EmitsOuterApply() {
        IDBInfo info = new MsSqlInfo();
        IDBClient client = CreateClientWithInfo(info);
        Func<Type, EntityDescriptor> getter = CreateDescriptorGetter();

        LoadOperation<ValueModel2> inner = new(client, getter,
            new IDBField[] { Field.Property<ValueModel2>(v => v.Integer) });
        LoadOperation<ValueModel> outer = new LoadOperation<ValueModel>(client, getter,
            Array.Empty<IDBField>())
            .LeftLateralJoin(inner, joinAlias: "lat0");

        string sql = outer.Prepare(false).CommandText;

        StringAssert.Contains("OUTER APPLY", sql, "MSSQL LeftLateralJoin must emit OUTER APPLY");
        StringAssert.DoesNotContain("LATERAL", sql, "MSSQL LeftLateralJoin must not emit LATERAL");
    }

    // -------------------------------------------------------------------------
    // 5. Regression: regular Join / LeftJoin still produce correct SQL
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public void RegularInnerJoin_Postgres_StillEmitsInnerJoin() {
        IDBInfo info = new PostgreInfo();
        IDBClient client = CreateClientWithInfo(info);
        Func<Type, EntityDescriptor> getter = CreateDescriptorGetter();

        LoadOperation<ValueModel, ValueModel2> outer = new LoadOperation<ValueModel>(client, getter,
            Array.Empty<IDBField>())
            .Join<ValueModel2>((a, b) => a.Integer == b.Integer, "j0");

        string sql = outer.Prepare(false).CommandText;

        StringAssert.Contains("INNER JOIN", sql);
        StringAssert.DoesNotContain("LATERAL", sql);
        StringAssert.DoesNotContain("APPLY", sql);
    }

    [Test, Parallelizable]
    public void RegularLeftJoin_Postgres_StillEmitsLeftJoin() {
        IDBInfo info = new PostgreInfo();
        IDBClient client = CreateClientWithInfo(info);
        Func<Type, EntityDescriptor> getter = CreateDescriptorGetter();

        LoadOperation<ValueModel, ValueModel2> outer = new LoadOperation<ValueModel>(client, getter,
            Array.Empty<IDBField>())
            .LeftJoin<ValueModel2>((a, b) => a.Integer == b.Integer, "j0");

        string sql = outer.Prepare(false).CommandText;

        StringAssert.Contains("LEFT JOIN", sql);
        StringAssert.DoesNotContain("LATERAL", sql);
        StringAssert.DoesNotContain("APPLY", sql);
    }

    // -------------------------------------------------------------------------
    // 6. Composition: LateralJoin chains with Where, OrderBy, Limit
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public void LateralJoin_Postgres_ChainsWith_Where() {
        IDBInfo info = new PostgreInfo();
        IDBClient client = CreateClientWithInfo(info);
        Func<Type, EntityDescriptor> getter = CreateDescriptorGetter();

        LoadOperation<ValueModel2> inner = new(client, getter,
            new IDBField[] { Field.Property<ValueModel2>(v => v.Integer) });
        LoadOperation<ValueModel> outer = new LoadOperation<ValueModel>(client, getter,
            Array.Empty<IDBField>())
            .LateralJoin(inner, joinAlias: "lat0")
            .Where(v => v.Integer > 0);

        string sql = outer.Prepare(false).CommandText;

        StringAssert.Contains("INNER JOIN LATERAL", sql);
        StringAssert.Contains("WHERE", sql);
    }

    [Test, Parallelizable]
    public void LateralJoin_Postgres_ChainsWith_OrderBy_Limit() {
        IDBInfo info = new PostgreInfo();
        IDBClient client = CreateClientWithInfo(info);
        Func<Type, EntityDescriptor> getter = CreateDescriptorGetter();

        LoadOperation<ValueModel2> inner = new(client, getter,
            new IDBField[] { Field.Property<ValueModel2>(v => v.Integer) });
        LoadOperation<ValueModel> outer = new LoadOperation<ValueModel>(client, getter,
            Array.Empty<IDBField>())
            .LateralJoin(inner, joinAlias: "lat0")
            .OrderBy(new OrderByCriteria(Field.Property<ValueModel>(v => v.Integer), false))
            .Limit(10);

        string sql = outer.Prepare(false).CommandText;

        StringAssert.Contains("INNER JOIN LATERAL", sql);
        StringAssert.Contains("ORDER BY", sql);
        StringAssert.Contains("DESC", sql);
        StringAssert.Contains("LIMIT", sql);
    }

    // -------------------------------------------------------------------------
    // 7. Postgres round-trip — gated on POSTGRES_CONNECTION
    // -------------------------------------------------------------------------

    [Test]
    public async Task LateralJoin_Postgres_RoundTrip_EachOuterRowJoinsInner() {
        string connString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");
        if (string.IsNullOrEmpty(connString))
            Assert.Inconclusive("POSTGRES_CONNECTION not set — Postgres round-trip tests skipped");

        IDBClient dbClient = ClientFactory.Create(() => new NpgsqlConnection(connString), new PostgreInfo(), true);
        IEntityManager em = new EntityManager(dbClient);

        em.UpdateSchema<ValueModel>();
        em.UpdateSchema<ValueModel2>();
        em.Delete<ValueModel>().Execute();
        em.Delete<ValueModel2>().Execute();

        // Seed: three outer rows
        await em.Insert<ValueModel>().Columns(v => v.Integer, v => v.String).ExecuteAsync(1, "outer-1");
        await em.Insert<ValueModel>().Columns(v => v.Integer, v => v.String).ExecuteAsync(2, "outer-2");
        await em.Insert<ValueModel>().Columns(v => v.Integer, v => v.String).ExecuteAsync(3, "outer-3");

        // Seed: two inner rows
        await em.Insert<ValueModel2>().Columns(v => v.Integer).ExecuteAsync(10);
        await em.Insert<ValueModel2>().Columns(v => v.Integer).ExecuteAsync(20);

        // LATERAL query: each outer row joined with the first inner row (Limit 1)
        LoadOperation<ValueModel2> lateralInner = em.Load<ValueModel2>().Limit(1);

        List<ValueModel> results = [];
        await foreach (ValueModel row in em.Load<ValueModel>()
                           .LateralJoin(lateralInner, joinAlias: "lat0")
                           .ExecuteEntitiesAsync())
            results.Add(row);

        // Each outer row joins with exactly one inner row — 3 result rows
        Assert.AreEqual(3, results.Count,
            "Each outer row should join with exactly one inner row via LateralJoin");
    }

    [Test]
    public async Task LeftLateralJoin_Postgres_RoundTrip_PreservesOuterRowsWhenNoInner() {
        string connString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");
        if (string.IsNullOrEmpty(connString))
            Assert.Inconclusive("POSTGRES_CONNECTION not set — Postgres round-trip tests skipped");

        IDBClient dbClient = ClientFactory.Create(() => new NpgsqlConnection(connString), new PostgreInfo(), true);
        IEntityManager em = new EntityManager(dbClient);

        em.UpdateSchema<ValueModel>();
        em.UpdateSchema<ValueModel2>();
        em.Delete<ValueModel>().Execute();
        em.Delete<ValueModel2>().Execute();

        // Three outer rows but NO inner rows
        await em.Insert<ValueModel>().Columns(v => v.Integer).ExecuteAsync(1);
        await em.Insert<ValueModel>().Columns(v => v.Integer).ExecuteAsync(2);
        await em.Insert<ValueModel>().Columns(v => v.Integer).ExecuteAsync(3);

        LoadOperation<ValueModel2> lateralInner = em.Load<ValueModel2>().Limit(1);

        List<ValueModel> results = [];
        await foreach (ValueModel row in em.Load<ValueModel>()
                           .LeftLateralJoin(lateralInner, joinAlias: "lat0")
                           .ExecuteEntitiesAsync())
            results.Add(row);

        // All three outer rows preserved (LEFT JOIN LATERAL)
        Assert.AreEqual(3, results.Count,
            "LeftLateralJoin must preserve all outer rows when the lateral yields no rows");
    }
}
