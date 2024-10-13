using System;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Tokens.Values;

/// <summary>
/// field which references a column directly
/// </summary>
public class ColumnToken : SqlToken {

    /// <summary>
    /// creates a new <see cref="ColumnToken"/>
    /// </summary>
    /// <param name="name">name of column</param>
    public ColumnToken(string name)
        : this(null, name)
    {
    }

    /// <summary>
    /// creates a new <see cref="ColumnToken"/>
    /// </summary>
    /// <param name="table">table/view/alias of which to reference column</param>
    /// <param name="name">name of column</param>
    public ColumnToken(string table, string name) {
        Table = table;
        Name = name;
    }

    /// <summary>
    /// name of table
    /// </summary>
    public string Table { get; }

    /// <summary>
    /// name of column
    /// </summary>
    public string Name { get; }

    /// <inheritdoc />
    public override void ToSql(IDBInfo dbinfo, IOperationPreparator preparator, Func<Type, EntityDescriptor> models, string tablealias) {
        if(!string.IsNullOrEmpty(Table))
            preparator.AppendText($"{Table}.{dbinfo.MaskColumn(Name)}");
        else preparator.AppendText(dbinfo.MaskColumn(Name));
    }
}