using System;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Tokens.Partitions;

/// <summary>
/// row number token
/// </summary>
public class RowNumberOver : SqlToken {
	
	/// <summary>
	/// creates a new <see cref="RowNumberOver"/> token
	/// </summary>
	/// <param name="partitionBy">field to partition result by</param>
	/// <param name="orderBy">field to order result by</param>
	public RowNumberOver(IDBField partitionBy, OrderByCriteria orderBy) {
		PartitionBy = partitionBy;
		OrderBy = orderBy;
	}

	/// <summary>
	/// creates a new <see cref="RowNumberOver"/> token
	/// </summary>
	/// <param name="orderBy">field to order result by</param>
	public RowNumberOver(OrderByCriteria orderBy) {
		OrderBy = orderBy;
	}

	/// <summary>
	/// field to partition rows by
	/// </summary>
	public IDBField PartitionBy { get; set; }
	
	/// <summary>
	/// order of result rows
	/// </summary>
	public OrderByCriteria OrderBy { get; set; }
	
	public override void ToSql(IDBInfo dbinfo, IOperationPreparator preparator, Func<Type, EntityDescriptor> models, string tablealias) {
		preparator.AppendText("ROW_NUMBER() OVER(");
		if (PartitionBy != null)
			preparator.AppendText("PARTITION BY").AppendField(PartitionBy, dbinfo, models, tablealias);
		if (OrderBy != null) {
			preparator.AppendText("ORDER BY").AppendField(OrderBy.Field, dbinfo, models, tablealias);
			if (!OrderBy.Ascending)
				preparator.AppendText("DESC");
		}

		preparator.AppendText(")");
	}
}