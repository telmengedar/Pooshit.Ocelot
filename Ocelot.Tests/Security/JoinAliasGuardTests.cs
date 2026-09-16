using Moq;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Errors;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Security.Models;
using Pooshit.Ocelot.Tokens;

namespace Pooshit.Ocelot.Tests.Security;

[TestFixture, Parallelizable]
public class JoinAliasGuardTests {

    [Test, Parallelizable, Description("MsSqlInfo.AppendJoin rejects an injected alias on the CROSS APPLY path, where no ON-criteria shadows the alias emission (rendered-only)")]
    public void MsSqlJoinAliasRejectsInjectedAlias() {
        MsSqlInfo dbInfo = new();
        Mock<IDBClient> client = new();
        client.Setup(c => c.DBInfo).Returns(dbInfo);

        LoadOperation<InjModel> inner = new(client.Object, EntityDescriptor.Create, DB.All);
        LoadOperation<InjModel> outer = new(client.Object, EntityDescriptor.Create, DB.All);
        Assert.Throws<InvalidIdentifierException>(() => outer.LateralJoin(inner, criteria: null, joinAlias: "x; --").Prepare());
    }

    [Test, Parallelizable, Description("DBInfo.AppendJoin rejects an injected alias on the lateral path with no ON-criteria to shadow it (rendered-only)")]
    public void BaseJoinAliasRejectsInjectedAliasWithoutCriteria() {
        PostgreInfo dbInfo = new();
        Mock<IDBClient> client = new();
        client.Setup(c => c.DBInfo).Returns(dbInfo);

        LoadOperation<InjModel> inner = new(client.Object, EntityDescriptor.Create, DB.All);
        LoadOperation<InjModel> outer = new(client.Object, EntityDescriptor.Create, DB.All);
        Assert.Throws<InvalidIdentifierException>(() => outer.LateralJoin(inner, criteria: null, joinAlias: "x; --").Prepare());
    }

    [Test, Parallelizable, Description("the untyped LoadOperation.Alias(string) rejects an injected alias")]
    public void UntypedLoadAliasRejectsInjectedAlias() {
        IDBClient client = TestData.CreateDatabaseAccess();
        Assert.Throws<InvalidIdentifierException>(() => new LoadOperation(client, EntityDescriptor.Create, DB.All).Alias("x; --").Prepare());
    }

    [Test, Parallelizable, Description("InsertData().PrepareBulk() rejects an injected table name")]
    public void InsertDataBulkRejectsInjectedTableName() {
        IDBClient client = TestData.CreateDatabaseAccess();
        Pooshit.Ocelot.Entities.EntityManager em = new(client);
        Assert.Throws<InvalidIdentifierException>(() => em.InsertData("t; --").Columns("a").PrepareBulk());
    }
}
