﻿using Pooshit.Ocelot.Extern;

namespace Pooshit.Ocelot.Clients.Tables; 

/// <summary>
/// row of a database table
/// </summary>
public class DataRow {
    readonly DataTableColumns columninformation;

    /// <summary>
    /// creates a new <see cref="DataRow" />
    /// </summary>
    /// <param name="data">data of the <see cref="DataRow" /></param>
    /// <param name="columns">column information of <see cref="DataTable" /></param>
    public DataRow(object[] data, DataTableColumns columns = null) {
        this.Cells = data;
        columninformation = columns;
    }

    /// <summary>
    /// data cells in row
    /// </summary>
    public object[] Cells { get; }

    /// <summary>
    /// indexer for row data
    /// </summary>
    /// <param name="index">index at which to get value</param>
    /// <returns>value at the specified index</returns>
    public object this[int index] => Cells[index];

    /// <summary>
    /// indexer for row data
    /// </summary>
    /// <param name="column">name of column which to return</param>
    /// <returns>value of specified column</returns>
    public object this[string column] => this[columninformation[column]];

    /// <summary>
    /// get typed value from row
    /// </summary>
    /// <typeparam name="T">type of value to get</typeparam>
    /// <param name="column">name of column of which to read value</param>
    /// <returns>converted value</returns>
    public T GetValue<T>(string column) => Converter.Convert<T>(this[column], true);

    /// <summary>
    /// get typed value from row
    /// </summary>
    /// <typeparam name="T">type of value to get</typeparam>
    /// <param name="index">index of column of which to read value</param>
    /// <returns>converted value</returns>
    public T GetValue<T>(int index) => Converter.Convert<T>(this[index], true);
}