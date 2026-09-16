using NUnit.Framework;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Errors;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Tests.Models;
using Pooshit.Ocelot.Tokens;

namespace Pooshit.Ocelot.Tests.Security;

[TestFixture, Parallelizable]
public class PropNameGuardTests {

    [Test, Parallelizable, Description("DB.Property<T>(string, ignoreCase, alias) rejects an injected alias on the PropName.Alias branch")]
    public void PropNameAliasRejectsInjectedAlias() {
        OperationPreparator preparator = new();
        Assert.Throws<InvalidIdentifierException>(() => preparator.AppendField(DB.Property<ValueModel>("Integer", false, "x; --"), new SQLiteInfo(), EntityDescriptor.Create, null));
    }

    [Test, Parallelizable, Description("PropName.ToSql rejects an injected tablealias on the branch reached when no explicit Alias is set")]
    public void PropNameTableAliasRejectsInjectedAlias() {
        OperationPreparator preparator = new();
        Assert.Throws<InvalidIdentifierException>(() => preparator.AppendField(DB.Property(typeof(ValueModel), "Integer"), new SQLiteInfo(), EntityDescriptor.Create, "x; --"));
    }
}
