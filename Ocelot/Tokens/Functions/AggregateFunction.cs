using System;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Tokens.Functions;

/// <summary>
/// aggregate function
/// </summary>
public class AggregateFunction : SqlToken {
        
    /// <summary>
    /// creates a new <see cref="Aggregate"/>
    /// </summary>
    /// <param name="method">aggregate method</param>
    /// <param name="arguments">arguments for method</param>
    internal AggregateFunction(string method, params ISqlToken[] arguments) {
        Method = method;
        Arguments = arguments;
    }

    /// <summary>
    /// method name
    /// </summary>
    public string Method { get; }

    /// <summary>
    /// content of the function
    /// </summary>
    public ISqlToken[] Arguments { get; }

    /// <inheritdoc />
    public override void ToSql(IDBInfo dbinfo, IOperationPreparator preparator, Func<Type, EntityDescriptor> models, string tablealias) {
        preparator.AppendText(Method);
        preparator.AppendText("(");
        bool first = true;
        foreach (ISqlToken argument in Arguments) {
            if (first)
                first = false;
            else preparator.AppendText(",");
            argument.ToSql(dbinfo, preparator, models, tablealias);
        }
        preparator.AppendText(")");
    }
}