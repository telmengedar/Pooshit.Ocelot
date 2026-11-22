using System;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Tokens;

namespace Pooshit.Ocelot.Tests.Operations;

internal sealed class CteLiteralToken(string literal) : SqlToken {
    /// <inheritdoc />
    public override void ToSql(IDBInfo dbinfo, IOperationPreparator preparator, Func<Type, EntityDescriptor> models, string tablealias) {
        preparator.AppendText(literal);
    }
}
