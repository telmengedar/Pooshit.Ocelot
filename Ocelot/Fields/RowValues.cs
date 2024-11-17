using System;
using Pooshit.Ocelot.Clients.Tables;

namespace Pooshit.Ocelot.Fields;

/// <inheritdoc />
public class RowValues : IRowValues {
    readonly DataRow row;
    readonly string[] fields;
    readonly Func<string[], string, int> fieldIndex;

    /// <summary>
    /// creates new <see cref="RowValues"/>
    /// </summary>
    /// <param name="row">row from which to read values</param>
    /// <param name="fields">fields of which to read index</param>
    /// <param name="fieldIndex">func used to retrieve field index</param>
    public RowValues(DataRow row, string[] fields, Func<string[], string, int> fieldIndex) {
        this.row = row;
        this.fields = fields;
        this.fieldIndex = fieldIndex;
    }

    /// <inheritdoc />
    public object GetFieldValue(string name) {
        int index = fieldIndex(fields, name);
        return index < 0 ? null : row.GetValue<object>(index);
    }

    /// <inheritdoc />
    public T GetFieldValue<T>(string name) {
        int index = fieldIndex(fields, name);
        return index < 0 ? default : row.GetValue<T>(index);
    }
}