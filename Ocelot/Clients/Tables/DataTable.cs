using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Pooshit.Ocelot.Clients.Tables; 

/// <summary>
/// table containing data fields
/// </summary>
public class DataTable {
    /// <summary>
    /// creates a new datatable
    /// </summary>
    /// <param name="columns">column descriptions</param>
    /// <param name="rows">rows in table</param>
    public DataTable(DataTableColumns columns, DataRow[] rows) {
        Columns = columns;
        Rows = rows;
    }

    /// <summary>
    /// column information
    /// </summary>
    public DataTableColumns Columns { get; }

    /// <summary>
    /// rows containing data
    /// </summary>
    public DataRow[] Rows { get; }

    static IEnumerable<object> ReadRow(IDataReader reader) {
        for (int i = 0; i < reader.FieldCount; ++i)
            yield return reader.GetValue(i);
    }

    static IEnumerable<DataRow> ReadRows(IDataReader reader, DataTableColumns columns) {
        while (reader.Read())
            yield return new DataRow(ReadRow(reader).ToArray(), columns);
    }

    /// <summary>
    /// creates a <see cref="DataTable" /> from query result
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    public static DataTable FromReader(IDataReader reader) {
        DataTableColumns columns = new();
        for (int i = 0; i < reader.FieldCount; ++i)
            columns[reader.GetName(i)] = i;

        return new DataTable(columns, ReadRows(reader, columns).ToArray());
    }
}