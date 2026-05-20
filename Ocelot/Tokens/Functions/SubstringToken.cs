using System;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Tokens.Functions;

/// <summary>
/// SQL SUBSTRING(expr, start, length) function.
/// Emits <c>SUBSTRING(expr, start, length)</c> on Postgres, MySQL and MSSQL;
/// falls back to <c>SUBSTR(expr, start, length)</c> on SQLite.
/// </summary>
public class SubstringToken : SqlToken {

    /// <summary>
    /// creates a new <see cref="SubstringToken"/>
    /// </summary>
    /// <param name="expr">string expression to substring</param>
    /// <param name="start">1-based start position</param>
    /// <param name="length">number of characters to extract</param>
    internal SubstringToken(ISqlToken expr, ISqlToken start, ISqlToken length) {
        Expr = expr;
        Start = start;
        Length = length;
    }

    /// <summary>string expression to substring</summary>
    public ISqlToken Expr { get; }

    /// <summary>1-based start position</summary>
    public ISqlToken Start { get; }

    /// <summary>number of characters to extract</summary>
    public ISqlToken Length { get; }

    /// <inheritdoc />
    public override void ToSql(IDBInfo dbinfo, IOperationPreparator preparator, Func<Type, EntityDescriptor> models, string tablealias) {
        // SQLite uses SUBSTR; all other supported dialects use SUBSTRING
        string functionName = dbinfo is SQLiteInfo ? "SUBSTR" : "SUBSTRING";
        preparator.AppendText(functionName);
        preparator.AppendText("(");
        Expr.ToSql(dbinfo, preparator, models, tablealias);
        preparator.AppendText(",");
        Start.ToSql(dbinfo, preparator, models, tablealias);
        preparator.AppendText(",");
        Length.ToSql(dbinfo, preparator, models, tablealias);
        preparator.AppendText(")");
    }
}
