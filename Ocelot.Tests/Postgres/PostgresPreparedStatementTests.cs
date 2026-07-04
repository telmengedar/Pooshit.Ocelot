using System;
using System.Threading.Tasks;
using Npgsql;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Tests.Models;

namespace Pooshit.Ocelot.Tests.Postgres;

/// <summary>
/// Regression tests for server-side prepared-statement correctness (DiVoid #3270, #3266).
/// Gated on the POSTGRES_CONNECTION env var — Assert.Inconclusive when unset.
///
/// Uses a private <see cref="PostgreInfoPrepared"/> subclass that always returns
/// <c>PreparationSupported = true</c> so these tests exercise the prepared-statement
/// code paths regardless of the production flag value during the pre-fix repro phase.
///
/// All tests are intentionally NON-parallel within this fixture: they share the
/// <c>pstmt_test</c> table and DROP/CREATE it in <see cref="SetUp"/>. Each test
/// asserts that the prepared-statement code path produces correct results —
/// failures here are the load-bearing regression proof for DiVoid #3266.
/// </summary>
[TestFixture, Parallelizable]
public class PostgresPreparedStatementTests {

    /// <summary>
    /// PostgreInfo subclass that hard-enables the prepared-statement code paths.
    /// Without this, <c>PreparationSupported = false</c> silently routes every prepared
    /// call to the unprepared path, masking all bugs during the pre-fix repro.
    /// </summary>
    class PostgreInfoPrepared : PostgreInfo {
        public override bool PreparationSupported => true;
    }

    IDBClient client;
    EntityManager em;

    [SetUp]
    public void SetUp() {
        string connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");
        if (string.IsNullOrEmpty(connectionString))
            Assert.Inconclusive("POSTGRES_CONNECTION not set — skipping live-Postgres tests");

        client = ClientFactory.Create(() => new NpgsqlConnection(connectionString), new PostgreInfoPrepared(), true);
        em = new EntityManager(client);

        // Drop + recreate to get a clean table — avoids CreateOrUpdateSchema which
        // triggers an unrelated PgColumn.IsIdentity bool-conversion failure.
        client.NonQuery("DROP TABLE IF EXISTS pstmt_test CASCADE");
        em.Create<PreparedTestEntity>();
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Surface 1 repro (DiVoid #3266): null parameter through the prepared code path.
    // Pre-fix: Npgsql's manual Prepare() freezes OIDs; a null value has no inferable
    //          type → "could not determine data type of parameter $N".
    // Post-fix: no manual Prepare(); auto-prepare uses OID 0; null persists correctly.
    // ──────────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task PreparedInsert_NullLabelParameter_PersistsCorrectly() {
        PreparedOperation insert = em.Insert<PreparedTestEntity>()
                                     .Columns(e => e.Id, e => e.Label)
                                     .Prepare();

        // Passing null for the nullable Label column — this is the Surface-1 trigger
        await insert.ExecuteAsync(42, null);

        PreparedTestEntity loaded = await em.Load<PreparedTestEntity>()
                                            .Where(e => e.Id == 42)
                                            .ExecuteEntityAsync();

        Assert.IsNotNull(loaded, "Row must be persisted through the prepared path");
        Assert.AreEqual(42, loaded.Id);
        Assert.IsNull(loaded.Label, "Null label must round-trip through the prepared path");
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Surface 4 repro (DiVoid #3266, task #3233): NonQueryPreparedAsync early-dispose.
    // Pre-fix: method is not async; PreparedCommand disposed before ExecuteNonQueryAsync
    //          completes → use-after-dispose race causing data loss or an exception.
    // Post-fix: method is async/await; disposal happens after the operation completes;
    //           every row is persisted.
    // ──────────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task PreparedAsyncInsert_RepeatedExecutions_AllRowsPersist() {
        PreparedOperation insert = em.Insert<PreparedTestEntity>()
                                     .Columns(e => e.Id)
                                     .Prepare();

        const int rowCount = 20;
        for (int i = 0; i < rowCount; i++)
            await insert.ExecuteAsync(i);

        // Count rows via a direct scalar query — avoids the DB.Count() column-selector API
        long stored = (long)(await client.ScalarAsync(null, "SELECT COUNT(*) FROM pstmt_test"));
        Assert.AreEqual(rowCount, stored,
            "All rows must be present — early-dispose race would cause fewer rows or exceptions");
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Prepared load reuse: same PreparedLoadOperation executed with different parameter
    // values must return correct results (validates typed-param OID stability).
    // ──────────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task PreparedLoad_DifferentParameterValues_ReturnsCorrectRows() {
        PreparedOperation insert = em.Insert<PreparedTestEntity>()
                                     .Columns(e => e.Id)
                                     .Prepare();
        await insert.ExecuteAsync(100);
        await insert.ExecuteAsync(200);

        PreparedLoadOperation<PreparedTestEntity> load = em.Load<PreparedTestEntity>()
                                                           .Where(e => e.Id == DBParameter<int>.Value)
                                                           .Prepare();

        PreparedTestEntity result100 = await load.ExecuteEntityAsync(100);
        PreparedTestEntity result200 = await load.ExecuteEntityAsync(200);

        Assert.IsNotNull(result100, "Row 100 must be found");
        Assert.IsNotNull(result200, "Row 200 must be found");
        Assert.AreEqual(100, result100.Id);
        Assert.AreEqual(200, result200.Id);
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Mixed-type parameters: int then double — validates typed-parameter round-trip.
    // ──────────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task PreparedInsert_MixedTypes_AllValuesRoundTrip() {
        PreparedOperation insert = em.Insert<PreparedTestEntity>()
                                     .Columns(e => e.Id, e => e.Value)
                                     .Prepare();

        await insert.ExecuteAsync(7, 3.14);

        PreparedTestEntity loaded = await em.Load<PreparedTestEntity>()
                                            .Where(e => e.Id == 7)
                                            .ExecuteEntityAsync();

        Assert.IsNotNull(loaded);
        Assert.AreEqual(7, loaded.Id);
        Assert.AreEqual(3.14, loaded.Value, 0.001);
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Prepared update (async non-query): validates Surface-4 fix for UPDATE statements.
    // ──────────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task PreparedUpdate_AsyncWrite_PersistsCorrectly() {
        PreparedOperation insert = em.Insert<PreparedTestEntity>()
                                     .Columns(e => e.Id, e => e.Label)
                                     .Prepare();
        await insert.ExecuteAsync(1, "original");

        PreparedOperation update = em.Update<PreparedTestEntity>()
                                     .Set(e => e.Label == DBParameter<string>.Value)
                                     .Where(e => e.Id == 1)
                                     .Prepare();

        await update.ExecuteAsync("updated");

        PreparedTestEntity loaded = await em.Load<PreparedTestEntity>()
                                            .Where(e => e.Id == 1)
                                            .ExecuteEntityAsync();

        Assert.IsNotNull(loaded);
        Assert.AreEqual("updated", loaded.Label);
    }
}
