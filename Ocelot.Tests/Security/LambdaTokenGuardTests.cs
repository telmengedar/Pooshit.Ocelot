using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Errors;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Models;
using Pooshit.Ocelot.Tokens;

namespace Pooshit.Ocelot.Tests.Security;

[TestFixture, Parallelizable]
public class LambdaTokenGuardTests {

    [Test, Parallelizable, Description("DB.As used inside a lambda field expression rejects an injected alias (DBInfo.Visit \"As\" case)")]
    public void LambdaAsRejectsInjectedAlias() {
        IDBClient client = TestData.CreateDatabaseAccess();
        EntityManager em = new(client);
        Assert.Throws<InvalidIdentifierException>(() => em.Load<ValueModel>(v => DB.As(v.Integer, "x; --")).Prepare());
    }

    [Test, Parallelizable, Description("DB.Field used inside a lambda field expression rejects an injected name (DBInfo.Visit \"Field\" case)")]
    public void LambdaFieldRejectsInjectedName() {
        IDBClient client = TestData.CreateDatabaseAccess();
        EntityManager em = new(client);
        Assert.Throws<InvalidIdentifierException>(() => em.Load<ValueModel>(v => DB.Field("x; --")).Prepare());
    }
}
