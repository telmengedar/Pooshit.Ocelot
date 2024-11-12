using System;
using System.Collections.Generic;
using System.Data;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Clients.Tables;

namespace Pooshit.Ocelot.Extensions;

/// <summary>
/// extensions for <see cref="IDataReader"/>
/// </summary>
public static class ReaderExtensions {

    /// <summary>
    /// reads all rows of a data reader returning converted data
    /// </summary>
    /// <param name="reader">reader accessing database row</param>
    /// <param name="converter">converter for row data</param>
    /// <typeparam name="T">created type</typeparam>
    /// <returns>enumeration of created types</returns>
    public static IEnumerable<T> ReadTypes<T>(this Reader reader, Func<Row, T> converter) {
        using (reader) {
            Row row = new(reader);
            while (reader.Read())
                yield return converter(row);
        }
    }
    
    /// <summary>
    /// reads all rows of a data reader returning converted data
    /// </summary>
    /// <param name="reader">reader accessing database row</param>
    /// <param name="converter">converter for row data</param>
    /// <typeparam name="T">created type</typeparam>
    /// <returns>enumeration of created types</returns>
    public static async IAsyncEnumerable<T> ReadTypesAsync<T>(this Reader reader, Func<Row, T> converter) {
        using (reader) {
            Row row = new(reader);
            while (await reader.ReadAsync())
                yield return converter(row);
        }
    }

}