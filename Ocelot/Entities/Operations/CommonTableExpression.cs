using System;
using System.Collections.Generic;
using System.Linq;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Entities.Operations;

/// <summary>A named nonrecursive load definition and reference.</summary>
public sealed class CommonTableExpression : IDatabaseOperation {
    readonly IDatabaseOperation body;
    readonly IDBInfo dbInfo;

    internal CommonTableExpression(string name, IDatabaseOperation body, IDBInfo dbInfo) {
        Name = IdentifierGuard.Simple(name, "common table expression");
        this.body = body;
        this.dbInfo = dbInfo;
    }

    /// <summary>The statement-local name.</summary>
    public string Name { get; }

    /// <inheritdoc />
    void IDatabaseOperation.Prepare(IOperationPreparator preparator) {
        preparator.AppendText("SELECT * FROM");
        preparator.AppendText(dbInfo.MaskColumn(Name));
    }

    internal static void AddDefinitions(List<CommonTableExpression> target, CommonTableExpression[] definitions) {
        HashSet<string> names = new(target.Select(expression => expression.Name), StringComparer.OrdinalIgnoreCase);
        foreach(CommonTableExpression expression in definitions)
            if(!names.Add(expression.Name))
                throw new ArgumentException("Common table expression names must be unique.", nameof(definitions));
        target.AddRange(definitions);
    }

    internal static void PrepareDefinitions(List<CommonTableExpression> expressions, IOperationPreparator preparator) {
        if(expressions.Count == 0)
            return;
        preparator.AppendText("WITH");
        for(int i = 0; i < expressions.Count; i++) {
            if(i > 0)
                preparator.AppendText(",");
            CommonTableExpression expression = expressions[i];
            preparator.AppendText(expression.dbInfo.MaskColumn(expression.Name));
            preparator.AppendText("AS (");
            expression.body.Prepare(preparator);
            preparator.AppendText(")");
        }
    }
}
