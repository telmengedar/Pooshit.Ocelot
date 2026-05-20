using System;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Tokens.Functions;

/// <summary>
/// Postgres-specific <c>convert_from(bytea, encoding)</c> function.
/// Throws <see cref="NotSupportedException"/> on all other dialects.
/// </summary>
public class ConvertFromToken : SqlToken {

    /// <summary>
    /// creates a new <see cref="ConvertFromToken"/>
    /// </summary>
    /// <param name="bytes">bytea expression to convert</param>
    /// <param name="encoding">encoding name token (e.g. a constant 'UTF8')</param>
    internal ConvertFromToken(ISqlToken bytes, ISqlToken encoding) {
        Bytes = bytes;
        Encoding = encoding;
    }

    /// <summary>bytea expression to convert</summary>
    public ISqlToken Bytes { get; }

    /// <summary>encoding name</summary>
    public ISqlToken Encoding { get; }

    /// <inheritdoc />
    public override void ToSql(IDBInfo dbinfo, IOperationPreparator preparator, Func<Type, EntityDescriptor> models, string tablealias) {
        if (dbinfo is not PostgreInfo)
            throw new NotSupportedException("DB.ConvertFrom is only supported on PostgreSQL. Other dialects do not have an equivalent to convert_from(bytea, text).");

        preparator.AppendText("convert_from(");
        Bytes.ToSql(dbinfo, preparator, models, tablealias);
        preparator.AppendText(",");
        Encoding.ToSql(dbinfo, preparator, models, tablealias);
        preparator.AppendText(")");
    }
}
