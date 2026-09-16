using NUnit.Framework;
using Pooshit.Ocelot.Entities.Schema;
using Pooshit.Ocelot.Errors;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Tests.Security;

[TestFixture, Parallelizable]
public class MaskColumnGuardTests {

    static IDBInfo[] Dialects() => [new SQLiteInfo(), new PostgreInfo(), new MySQLInfo(), new MsSqlInfo()];

    [TestCaseSource(nameof(Dialects))]
    [Parallelizable]
    public void MaskColumnRejectsDelimiterOnEveryDialect(IDBInfo dbInfo) {
        Assert.Throws<InvalidIdentifierException>(() => dbInfo.MaskColumn("a]; DROP--"));
        Assert.Throws<InvalidIdentifierException>(() => dbInfo.MaskColumn("a\""));
        Assert.Throws<InvalidIdentifierException>(() => dbInfo.MaskColumn("a`"));
    }

    [Test, Parallelizable]
    public void MaskColumnStillQuotesValidNameOnEveryDialect() {
        Assert.That(new SQLiteInfo().MaskColumn("a"), Is.EqualTo("[a]"));
        Assert.That(new PostgreInfo().MaskColumn("a"), Is.EqualTo("\"a\""));
        Assert.That(new MySQLInfo().MaskColumn("a"), Is.EqualTo("`a`"));
        Assert.That(new MsSqlInfo().MaskColumn("a"), Is.EqualTo("\"a\""));
    }

    [TestCaseSource(nameof(Dialects))]
    [Parallelizable, Description("DropTable throws before the client is ever reached, reachable without a live connection")]
    public void DropTableRejectsInjectedName(IDBInfo dbInfo) {
        TableDescriptor descriptor = new("victim; DROP TABLE x; --");
        Assert.Throws<InvalidIdentifierException>(() => dbInfo.DropTable(null, descriptor));
    }

    [TestCaseSource(nameof(Dialects))]
    [Parallelizable, Description("DropView throws before the client is ever reached, reachable without a live connection")]
    public void DropViewRejectsInjectedName(IDBInfo dbInfo) {
        ViewDescriptor descriptor = new("victim; DROP TABLE x; --");
        Assert.Throws<InvalidIdentifierException>(() => dbInfo.DropView(null, descriptor));
    }

    [Test, Parallelizable, Description("the base Truncate implementation (used by MySQL/MSSQL) throws before it ever calls the client")]
    public void BaseTruncateRejectsInjectedTableBeforeClientCall() {
        Assert.Throws<InvalidIdentifierException>(() => new MySQLInfo().Truncate(null, "victim; DROP TABLE x; --").GetAwaiter().GetResult());
        Assert.Throws<InvalidIdentifierException>(() => new MsSqlInfo().Truncate(null, "victim; DROP TABLE x; --").GetAwaiter().GetResult());
    }

    [Test, Parallelizable]
    public void SqliteAddColumnRejectsInjectedTable() {
        SQLiteInfo dbInfo = new();
        Assert.Throws<InvalidIdentifierException>(() => dbInfo.AddColumn(null, "victim; DROP TABLE x; --", null));
    }
}
