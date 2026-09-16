using Moq;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Errors;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Schemas;
using Pooshit.Ocelot.Tests.Data;

namespace Pooshit.Ocelot.Tests.Security;

[TestFixture, Parallelizable]
public class AlterTableColumnGuardTests {

    [Test, Parallelizable, Description("AlterTableOperation.Drop rejects an injected column name (rendered-only, Postgres; SQLite throws NotSupportedException for drop)")]
    public void AlterTableDropRejectsInjectedColumnName() {
        Mock<IDBClient> client = new();
        client.Setup(c => c.DBInfo).Returns(new PostgreInfo());
        Assert.Throws<InvalidIdentifierException>(() => new AlterTableOperation(client.Object, "t").Drop("a\"; DROP TABLE x; --").Prepare());
    }

    [Test, Parallelizable, Description("AlterTableOperation.Add rejects an injected column name reaching SQLiteInfo.AddColumn")]
    public void AlterTableAddRejectsInjectedColumnName() {
        IDBClient client = TestData.CreateDatabaseAccess();
        Assert.Throws<InvalidIdentifierException>(() => new AlterTableOperation(client, "t").Add(new ColumnDescriptor("a]; --") { Type = "TEXT" }).Prepare());
    }
}
