using System;
using Pooshit.Ocelot.Clients;

namespace Pooshit.Ocelot.Fields;

/// <inheritdoc />
public class ReaderValues : IRowValues {
    readonly Reader reader;
    readonly string[] fields;
    readonly Func<string[], string, int> fieldIndex;

    /// <summary>
    /// creates new <see cref="ReaderValues"/>
    /// </summary>
    /// <param name="reader">reader of which to get values</param>
    /// <param name="fields">fields of which to read index</param>
    /// <param name="fieldIndex">func used to retrieve field index</param>
    public ReaderValues(Reader reader, string[] fields, Func<string[], string, int> fieldIndex) {
        this.reader = reader;
        this.fields = fields;
        this.fieldIndex = fieldIndex;
    }

    /// <inheritdoc />
    public object GetFieldValue(string name) {
        int index = fieldIndex(fields, name);
        return index < 0 ? null : reader[index];
    }

    /// <inheritdoc />
    public T GetFieldValue<T>(string name) {
        int index = fieldIndex(fields, name);
        return index < 0 ? default : reader.GetValue<T>(index);
    }
}