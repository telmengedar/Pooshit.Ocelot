using System;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Tokens.Functions;

/// <summary>
/// SQL LEFT(expr, length) function.
/// Emits <c>LEFT(expr, length)</c> on Postgres, MySQL and MSSQL;
/// falls back to <c>SUBSTR(expr, 1, length)</c> on SQLite which has no native LEFT.
/// </summary>
public class LeftToken : SqlToken {

    /// <summary>
    /// creates a new <see cref="LeftToken"/>
    /// </summary>
    /// <param name="expr">string expression</param>
    /// <param name="length">number of characters to take from the left</param>
    internal LeftToken(ISqlToken expr, ISqlToken length) {
        Expr = expr;
        Length = length;
    }

    /// <summary>string expression</summary>
    public ISqlToken Expr { get; }

    /// <summary>number of characters to take from the left</summary>
    public ISqlToken Length { get; }

    /// <inheritdoc />
    public override void ToSql(IDBInfo dbinfo, IOperationPreparator preparator, Func<Type, EntityDescriptor> models, string tablealias) {
        if (dbinfo is SQLiteInfo) {
            // SQLite has no LEFT(); fall back to SUBSTR(expr, 1, length)
            preparator.AppendText("SUBSTR(");
            Expr.ToSql(dbinfo, preparator, models, tablealias);
            preparator.AppendText(",");
            preparator.AppendText("1");
            preparator.AppendText(",");
            Length.ToSql(dbinfo, preparator, models, tablealias);
            preparator.AppendText(")");
        }
        else {
            preparator.AppendText("LEFT(");
            Expr.ToSql(dbinfo, preparator, models, tablealias);
            preparator.AppendText(",");
            Length.ToSql(dbinfo, preparator, models, tablealias);
            preparator.AppendText(")");
        }
    }
}
