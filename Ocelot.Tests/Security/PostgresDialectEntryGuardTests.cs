using NUnit.Framework;
using Pooshit.Ocelot.Errors;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Tests.Security;

[TestFixture, Parallelizable]
public class PostgresDialectEntryGuardTests {

    [Test, Parallelizable, Description("PostgreInfo.Truncate rejects an injected table name at the method's own entry guard, not only via MaskColumn downstream")]
    public void PostgresTruncateRejectsInjectedTableBeforeClientCall() {
        PostgreInfo dbInfo = new();
        InvalidIdentifierException exception = Assert.Throws<InvalidIdentifierException>(() => _ = dbInfo.Truncate(null, "victim; DROP TABLE x; --"));
        Assert.That(exception.Role, Is.EqualTo("table"));
    }

    [Test, Parallelizable, Description("PostgreInfo.GenerateCreateStatement rejects an injected table name at the method's own entry guard before the client is reached")]
    public void PostgresGenerateCreateStatementRejectsInjectedTableBeforeClientCall() {
        PostgreInfo dbInfo = new();
        InvalidIdentifierException exception = Assert.ThrowsAsync<InvalidIdentifierException>(() => dbInfo.GenerateCreateStatement(null, "victim; DROP TABLE x; --"));
        Assert.That(exception.Role, Is.EqualTo("table"));
    }
}
