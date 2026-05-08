using System;
using System.Linq;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Tokens.Partitions;

/// <summary>
/// generic windowed aggregate token that emits &lt;aggregate&gt; OVER ([PARTITION BY ...] [ORDER BY ...]) [AS alias]
/// </summary>
/// <remarks>
/// Use the static factories on <see cref="DB"/> (e.g. <c>DB.CountOver()</c>, <c>DB.SumOver(...)</c>) instead of constructing
/// this type directly. All four target databases support window functions from: SQLite 3.25+, PostgreSQL 8.4+,
/// MSSQL 2005+, MySQL 8.0+ / MariaDB 10.2+.
/// </remarks>
public class WindowedAggregate : SqlToken {

    /// <summary>
    /// creates a new <see cref="WindowedAggregate"/> token
    /// </summary>
    /// <param name="aggregateExpression">
    /// the aggregate expression emitted before OVER — e.g. a <c>DB.Count(DB.All)</c> token, a column reference, etc.
    /// </param>
    /// <param name="partitionBy">optional field to partition the window by</param>
    /// <param name="orderBy">optional order-by criterion for the window</param>
    /// <param name="alias">optional SQL alias emitted as AS &lt;alias&gt; after the OVER clause</param>
    public WindowedAggregate(ISqlToken aggregateExpression, IDBField partitionBy = null, OrderByCriteria orderBy = null, string alias = null) {
        AggregateExpression = aggregateExpression;
        PartitionBy = partitionBy;
        OrderBy = orderBy;
        Alias = alias;
    }

    /// <summary>
    /// the aggregate expression emitted before OVER
    /// </summary>
    public ISqlToken AggregateExpression { get; }

    /// <summary>
    /// optional field to partition by
    /// </summary>
    public IDBField PartitionBy { get; }

    /// <summary>
    /// optional order-by criterion for the window frame
    /// </summary>
    public OrderByCriteria OrderBy { get; }

    /// <summary>
    /// optional alias emitted as AS &lt;alias&gt; after the OVER clause
    /// </summary>
    public string Alias { get; }

    /// <summary>
    /// renders an ISqlToken into a compact text string using a temporary preparator,
    /// avoiding the space-joining that occurs when tokens are emitted into the real preparator
    /// </summary>
    static string RenderCompact(ISqlToken token, IDBInfo dbinfo, Func<Type, EntityDescriptor> models, string tablealias) {
        OperationPreparator temp = new();
        token.ToSql(dbinfo, temp, models, tablealias);
        // Collect text tokens and join without spaces to produce compact SQL (e.g. "COUNT(*)" not "COUNT ( * )")
        return string.Concat(temp.Tokens.Select(t => t.GetText(dbinfo)));
    }

    /// <summary>
    /// renders an IDBField into a compact text string using a temporary preparator
    /// </summary>
    static string RenderFieldCompact(IDBField field, IDBInfo dbinfo, Func<Type, EntityDescriptor> models, string tablealias) {
        OperationPreparator temp = new();
        temp.AppendField(field, dbinfo, models, tablealias);
        return string.Concat(temp.Tokens.Select(t => t.GetText(dbinfo)));
    }

    /// <inheritdoc />
    public override void ToSql(IDBInfo dbinfo, IOperationPreparator preparator, Func<Type, EntityDescriptor> models, string tablealias) {
        // Render aggregate expression compactly (e.g. "COUNT(*)" not "COUNT ( * )")
        string aggregateSql = RenderCompact(AggregateExpression, dbinfo, models, tablealias);

        // Build OVER(...) clause
        string overContent = string.Empty;

        if (PartitionBy != null) {
            string pbSql = RenderFieldCompact(PartitionBy, dbinfo, models, tablealias);
            overContent += "PARTITION BY " + pbSql;
        }

        if (OrderBy != null) {
            if (overContent.Length > 0) overContent += " ";
            string obSql = RenderFieldCompact(OrderBy.Field, dbinfo, models, tablealias);
            overContent += "ORDER BY " + obSql;
            if (!OrderBy.Ascending)
                overContent += " DESC";
        }

        // Emit the whole windowed expression as a single token to avoid unwanted spaces
        string windowedSql = aggregateSql + " OVER(" + overContent + ")";
        preparator.AppendText(windowedSql);

        if (!string.IsNullOrEmpty(Alias)) {
            preparator.AppendText("AS");
            preparator.AppendText(Alias);
        }
    }
}
