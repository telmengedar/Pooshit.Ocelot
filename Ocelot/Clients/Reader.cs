using System;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Pooshit.Ocelot.Extern;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Clients;

/// <summary>
/// reader which locks access to database until it is disposed
/// </summary>
public class Reader : IDataReader {
    readonly DbDataReader reader;
    readonly PreparedCommand command;
    readonly IDBInfo dbInfo;
    readonly MethodInfo fieldValue;

    /// <summary>
    /// creates a new <see cref="Reader"/>
    /// </summary>
    /// <param name="reader">reader used to read data</param>
    /// <param name="command">command used to create reader</param>
    /// <param name="dbInfo">db specific info</param>
    public Reader(DbDataReader reader, PreparedCommand command, IDBInfo dbInfo) {
        this.reader = reader;
        this.command = command;
        this.dbInfo = dbInfo;
        fieldValue = reader.GetType().GetMethod("GetFieldValue");
    }

    /// <summary>
    /// semaphore which locks the reading process
    /// </summary>
    public SemaphoreSlim Semaphore { get; internal set; }
        
    /// <inheritdoc />
    public bool GetBoolean(int i) {
        return reader.GetBoolean(i);
    }

    /// <inheritdoc />
    public byte GetByte(int i) {
        return reader.GetByte(i);
    }

    /// <inheritdoc />
    public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) {
        return reader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
    }

    /// <inheritdoc />
    public char GetChar(int i) {
        return reader.GetChar(i);
    }

    /// <inheritdoc />
    public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) {
        return reader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
    }

    /// <inheritdoc />
    public IDataReader GetData(int i) {
        return reader.GetData(i);
    }

    /// <inheritdoc />
    public string GetDataTypeName(int i) {
        return reader.GetDataTypeName(i);
    }

    /// <inheritdoc />
    public DateTime GetDateTime(int i) {
        return reader.GetDateTime(i);
    }

    /// <inheritdoc />
    public decimal GetDecimal(int i) {
        return reader.GetDecimal(i);
    }

    /// <inheritdoc />
    public double GetDouble(int i) {
        return reader.GetDouble(i);
    }

    /// <inheritdoc />
    public Type GetFieldType(int i) {
        return reader.GetFieldType(i);
    }

    /// <inheritdoc />
    public float GetFloat(int i) {
        return reader.GetFloat(i);
    }

    /// <inheritdoc />
    public Guid GetGuid(int i) {
        return reader.GetGuid(i);
    }

    /// <inheritdoc />
    public short GetInt16(int i) {
        return reader.GetInt16(i);
    }

    /// <inheritdoc />
    public int GetInt32(int i) {
        return reader.GetInt32(i);
    }

    /// <inheritdoc />
    public long GetInt64(int i) {
        return reader.GetInt64(i);
    }

    /// <inheritdoc />
    public string GetName(int i) {
        return reader.GetName(i);
    }

    /// <inheritdoc />
    public int GetOrdinal(string name) {
        return reader.GetOrdinal(name);
    }

    /// <inheritdoc />
    public string GetString(int i) {
        return reader.GetString(i);
    }

    /// <inheritdoc />
    public object GetValue(int i) {
        return reader.GetValue(i);
    }
    
    /// <summary>
    /// get a value of a specific type from reader
    /// </summary>
    /// <param name="i">index of field</param>
    /// <typeparam name="T">type of value to get</typeparam>
    /// <returns>value at the specified index</returns>
    public T FieldValue<T>(int i) {
        return reader.GetFieldValue<T>(i);
    }

    /// <summary>
    /// get a value of a specific type from reader
    /// </summary>
    /// <param name="i">index of field</param>
    /// <typeparam name="T">type of value to get</typeparam>
    /// <returns>value at the specified index</returns>
    public Task<T> FieldValueAsync<T>(int i) {
        return reader.GetFieldValueAsync<T>(i);
    }

    /// <summary>
    /// get a value of a specific type from reader
    /// </summary>
    /// <param name="i">index of field</param>
    /// <typeparam name="T">type of value to get</typeparam>
    /// <returns>value at the specified index</returns>
    public T GetValue<T>(int i) {
        return Converter.Convert<T>(dbInfo.ValueFromReader(this, i, typeof(T)), true);
    }

    /// <summary>
    /// get a value of a specific type from reader
    /// </summary>
    /// <param name="i">index of field</param>
    /// <param name="fieldType">type of value to get</param>
    /// <returns>value at the specified index</returns>
    public object GetValue(int i, Type fieldType) {
        return fieldValue.MakeGenericMethod(fieldType).Invoke(reader, [i]);
    }

    /// <inheritdoc />
    public int GetValues(object[] values) {
        return reader.GetValues(values);
    }

    /// <inheritdoc />
    public bool IsDBNull(int i) {
        return reader.IsDBNull(i);
    }

    /// <inheritdoc />
    public int FieldCount => reader.FieldCount;

    /// <inheritdoc />
    public object this[int i] => reader[i];

    /// <inheritdoc />
    public object this[string name] => reader[name];

    /// <inheritdoc />
    public void Dispose() {
        reader.Dispose();
        command?.Dispose();
        Semaphore?.Release();
    }

    /// <inheritdoc />
    public void Close() {
        reader.Close();
    }

    /// <summary>
    /// closes the reader
    /// </summary>
    public Task CloseAsync() {
        return reader.CloseAsync();
    }

    /// <inheritdoc />
    public DataTable GetSchemaTable() {
        return reader.GetSchemaTable();
    }

    /// <inheritdoc />
    public bool NextResult() {
        return reader.NextResult();
    }

    /// <inheritdoc />
    public bool Read() {
        return reader.Read();
    }

    /// <summary>
    /// fetches the next row in an async read
    /// </summary>
    /// <returns>
    /// true if there are more rows; otherwise, false.
    /// </returns>
    public Task<bool> ReadAsync() {
        return reader.ReadAsync();
    }
    
    /// <inheritdoc />
    public int Depth => reader.Depth;

    /// <inheritdoc />
    public bool IsClosed => reader.IsClosed;

    /// <inheritdoc />
    public int RecordsAffected => reader.RecordsAffected;
}