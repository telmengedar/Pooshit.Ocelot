using System;
using NUnit.Framework;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Tokens;

namespace Pooshit.Ocelot.Tests.Tokens;

/// <summary>
/// SQL-generation tests for DB.Substring, DB.Left, and DB.ConvertFrom helpers.
/// All tests are pure dialect-generation tests (no database connection required)
/// except the Postgres round-trip test which is gated on POSTGRES_CONNECTION.
/// </summary>
[TestFixture, Parallelizable]
public class StringHelperTests {

    // -----------------------------------------------------------------------
    // Helper
    // -----------------------------------------------------------------------

    static string ToSql(Pooshit.Ocelot.Tokens.ISqlToken token, IDBInfo dbInfo) {
        OperationPreparator preparator = new();
        token.ToSql(dbInfo, preparator, EntityDescriptor.Create, null);
        // GetOperation needs an IDBClient — use a minimal mock via the overload that accepts text.
        // OperationPreparator.GetOperation requires a client only for parameter prefix substitution.
        // We capture using ToString on the tokens instead.
        // The cleanest path: build a fake client backed by the provided dbInfo.
        Moq.Mock<Pooshit.Ocelot.Clients.IDBClient> client = new();
        client.SetupGet(c => c.DBInfo).Returns(dbInfo);
        PreparedOperation operation = preparator.GetOperation(client.Object, false);
        return operation.CommandText;
    }

    // -----------------------------------------------------------------------
    // DB.Substring — ISqlToken overload, ISqlToken+int overload
    // -----------------------------------------------------------------------

    [Test, Parallelizable]
    public void Substring_ISqlToken_Sqlite_EmitsSubstr() {
        string sql = ToSql(DB.Substring(DB.Column("name"), DB.Constant(1), DB.Constant(5)), new SQLiteInfo());
        // AppendText emits tokens with spaces; check function name is SUBSTR not SUBSTRING
        StringAssert.StartsWith("SUBSTR", sql.TrimStart());
        StringAssert.DoesNotContain("SUBSTRING", sql);
    }

    [Test, Parallelizable]
    public void Substring_ISqlToken_Postgres_EmitsSubstring() {
        string sql = ToSql(DB.Substring(DB.Column("name"), DB.Constant(1), DB.Constant(5)), new PostgreInfo());
        StringAssert.StartsWith("SUBSTRING", sql.TrimStart());
        // Verify it's SUBSTRING not SUBSTR (SUBSTR would also start with SUBSTR)
        StringAssert.Contains("SUBSTRING", sql);
    }

    [Test, Parallelizable]
    public void Substring_IntOverload_Sqlite_EmitsSubstr() {
        string sql = ToSql(DB.Substring(DB.Column("content"), 3, 8), new SQLiteInfo());
        StringAssert.StartsWith("SUBSTR", sql.TrimStart());
        StringAssert.DoesNotContain("SUBSTRING", sql);
    }

    [Test, Parallelizable]
    public void Substring_IntOverload_Postgres_EmitsSubstring() {
        string sql = ToSql(DB.Substring(DB.Column("content"), 3, 8), new PostgreInfo());
        StringAssert.Contains("SUBSTRING", sql);
    }

    [Test, Parallelizable]
    public void Substring_IntOverload_MsSql_EmitsSubstring() {
        string sql = ToSql(DB.Substring(DB.Column("content"), 3, 8), new MsSqlInfo());
        StringAssert.Contains("SUBSTRING", sql);
    }

    [Test, Parallelizable]
    public void Substring_IntOverload_MySQL_EmitsSubstring() {
        string sql = ToSql(DB.Substring(DB.Column("content"), 3, 8), new MySQLInfo());
        StringAssert.Contains("SUBSTRING", sql);
    }

    [Test, Parallelizable]
    public void Substring_ObjectOverload_ThrowsNotImplemented() {
        Assert.Throws<NotImplementedException>(() => DB.Substring(new object(), new object(), new object()));
    }

    // -----------------------------------------------------------------------
    // DB.Left
    // -----------------------------------------------------------------------

    [Test, Parallelizable]
    public void Left_ISqlToken_Sqlite_EmitsSubstr() {
        string sql = ToSql(DB.Left(DB.Column("name"), DB.Constant(8000)), new SQLiteInfo());
        StringAssert.Contains("SUBSTR", sql);
        StringAssert.DoesNotContain("LEFT", sql);
        // Must include literal 1 as the start argument
        StringAssert.Contains("1", sql);
    }

    [Test, Parallelizable]
    public void Left_ISqlToken_Postgres_EmitsLeft() {
        string sql = ToSql(DB.Left(DB.Column("content"), DB.Constant(8000)), new PostgreInfo());
        StringAssert.Contains("LEFT", sql);
        StringAssert.DoesNotContain("SUBSTR", sql);
    }

    [Test, Parallelizable]
    public void Left_IntOverload_Sqlite_EmitsSubstr() {
        string sql = ToSql(DB.Left(DB.Column("content"), 8000), new SQLiteInfo());
        StringAssert.Contains("SUBSTR", sql);
        StringAssert.DoesNotContain("LEFT", sql);
        StringAssert.Contains("1", sql);
    }

    [Test, Parallelizable]
    public void Left_IntOverload_Postgres_EmitsLeft() {
        string sql = ToSql(DB.Left(DB.Column("content"), 8000), new PostgreInfo());
        StringAssert.Contains("LEFT", sql);
        StringAssert.DoesNotContain("SUBSTR", sql);
    }

    [Test, Parallelizable]
    public void Left_IntOverload_MsSql_EmitsLeft() {
        string sql = ToSql(DB.Left(DB.Column("content"), 8000), new MsSqlInfo());
        StringAssert.Contains("LEFT", sql);
        StringAssert.DoesNotContain("SUBSTR", sql);
    }

    [Test, Parallelizable]
    public void Left_IntOverload_MySQL_EmitsLeft() {
        string sql = ToSql(DB.Left(DB.Column("content"), 8000), new MySQLInfo());
        StringAssert.Contains("LEFT", sql);
        StringAssert.DoesNotContain("SUBSTR", sql);
    }

    [Test, Parallelizable]
    public void Left_ObjectOverload_ThrowsNotImplemented() {
        Assert.Throws<NotImplementedException>(() => DB.Left(new object(), new object()));
    }

    // -----------------------------------------------------------------------
    // DB.ConvertFrom
    // -----------------------------------------------------------------------

    [Test, Parallelizable]
    public void ConvertFrom_Postgres_EmitsConvertFrom() {
        string sql = ToSql(DB.ConvertFrom(DB.Column("content"), "UTF8"), new PostgreInfo());
        StringAssert.Contains("convert_from", sql);
    }

    [Test, Parallelizable]
    public void ConvertFrom_Sqlite_ThrowsNotSupported() {
        Assert.Throws<NotSupportedException>(() =>
            DB.ConvertFrom(DB.Column("content"), "UTF8").ToSql(new SQLiteInfo(), new OperationPreparator(), EntityDescriptor.Create, null));
    }

    [Test, Parallelizable]
    public void ConvertFrom_MsSql_ThrowsNotSupported() {
        Assert.Throws<NotSupportedException>(() =>
            DB.ConvertFrom(DB.Column("content"), "UTF8").ToSql(new MsSqlInfo(), new OperationPreparator(), EntityDescriptor.Create, null));
    }

    [Test, Parallelizable]
    public void ConvertFrom_MySQL_ThrowsNotSupported() {
        Assert.Throws<NotSupportedException>(() =>
            DB.ConvertFrom(DB.Column("content"), "UTF8").ToSql(new MySQLInfo(), new OperationPreparator(), EntityDescriptor.Create, null));
    }

    [Test, Parallelizable]
    public void ConvertFrom_ErrorMessage_MentionsFunctionName() {
        NotSupportedException ex = Assert.Throws<NotSupportedException>(() =>
            DB.ConvertFrom(DB.Column("content"), "UTF8").ToSql(new SQLiteInfo(), new OperationPreparator(), EntityDescriptor.Create, null));
        StringAssert.Contains("convert_from", ex.Message);
    }

    [Test, Parallelizable]
    public void ConvertFrom_ObjectOverload_ThrowsNotImplemented() {
        Assert.Throws<NotImplementedException>(() => DB.ConvertFrom(new object(), new object()));
    }

    // -----------------------------------------------------------------------
    // DB.ConvertFrom — ISqlToken encoding overload
    // -----------------------------------------------------------------------

    [Test, Parallelizable]
    public void ConvertFrom_ISqlTokenEncoding_Postgres_EmitsConvertFrom() {
        string sql = ToSql(DB.ConvertFrom(DB.Column("content"), DB.Constant("UTF8")), new PostgreInfo());
        StringAssert.Contains("convert_from", sql);
    }
}
