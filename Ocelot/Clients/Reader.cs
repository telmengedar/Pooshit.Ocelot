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

    // Optional inner reader for proxy subclasses (e.g. WindowedReader) that wrap an existing Reader.
    // When set, all virtual members delegate to this instead of the raw DbDataReader fields above.
    readonly Reader inner;

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
    /// creates a proxy <see cref="Reader"/> subclass that delegates all operations to the supplied
    /// <paramref name="inner"/> reader. Subclasses that intercept reads (e.g. <c>WindowedReader</c>)
    /// use this constructor and override only the methods they need to intercept.
    /// </summary>
    /// <param name="inner">the reader to delegate to</param>
    protected Reader(Reader inner) {
        this.inner = inner;
    }

    /// <summary>
    /// semaphore which locks the reading process
    /// </summary>
    public SemaphoreSlim Semaphore { get; internal set; }

    /// <inheritdoc />
    public bool GetBoolean(int i) {
        if (inner != null) return inner.GetBoolean(i);
        return reader.GetBoolean(i);
    }

    /// <inheritdoc />
    public byte GetByte(int i) {
        if (inner != null) return inner.GetByte(i);
        return reader.GetByte(i);
    }

    /// <inheritdoc />
    public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) {
        if (inner != null) return inner.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        return reader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
    }

    /// <inheritdoc />
    public char GetChar(int i) {
        if (inner != null) return inner.GetChar(i);
        return reader.GetChar(i);
    }

    /// <inheritdoc />
    public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) {
        if (inner != null) return inner.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        return reader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
    }

    /// <inheritdoc />
    public IDataReader GetData(int i) {
        if (inner != null) return inner.GetData(i);
        return reader.GetData(i);
    }

    /// <inheritdoc />
    public string GetDataTypeName(int i) {
        if (inner != null) return inner.GetDataTypeName(i);
        return reader.GetDataTypeName(i);
    }

    /// <inheritdoc />
    public DateTime GetDateTime(int i) {
        if (inner != null) return inner.GetDateTime(i);
        return reader.GetDateTime(i);
    }

    /// <inheritdoc />
    public decimal GetDecimal(int i) {
        if (inner != null) return inner.GetDecimal(i);
        return reader.GetDecimal(i);
    }

    /// <inheritdoc />
    public double GetDouble(int i) {
        if (inner != null) return inner.GetDouble(i);
        return reader.GetDouble(i);
    }

    /// <inheritdoc />
    public Type GetFieldType(int i) {
        if (inner != null) return inner.GetFieldType(i);
        return reader.GetFieldType(i);
    }

    /// <inheritdoc />
    public float GetFloat(int i) {
        if (inner != null) return inner.GetFloat(i);
        return reader.GetFloat(i);
    }

    /// <inheritdoc />
    public Guid GetGuid(int i) {
        if (inner != null) return inner.GetGuid(i);
        return reader.GetGuid(i);
    }

    /// <inheritdoc />
    public short GetInt16(int i) {
        if (inner != null) return inner.GetInt16(i);
        return reader.GetInt16(i);
    }

    /// <inheritdoc />
    public int GetInt32(int i) {
        if (inner != null) return inner.GetInt32(i);
        return reader.GetInt32(i);
    }

    /// <inheritdoc />
    public long GetInt64(int i) {
        if (inner != null) return inner.GetInt64(i);
        return reader.GetInt64(i);
    }

    /// <inheritdoc />
    public string GetName(int i) {
        if (inner != null) return inner.GetName(i);
        return reader.GetName(i);
    }

    /// <inheritdoc />
    public int GetOrdinal(string name) {
        if (inner != null) return inner.GetOrdinal(name);
        return reader.GetOrdinal(name);
    }

    /// <inheritdoc />
    public string GetString(int i) {
        if (inner != null) return inner.GetString(i);
        return reader.GetString(i);
    }

    /// <inheritdoc />
    public object GetValue(int i) {
        if (inner != null) return inner.GetValue(i);
        return reader.GetValue(i);
    }

    /// <summary>
    /// get a value of a specific type from reader
    /// </summary>
    /// <param name="i">index of field</param>
    /// <typeparam name="T">type of value to get</typeparam>
    /// <returns>value at the specified index</returns>
    public T FieldValue<T>(int i) {
        if (inner != null) return inner.FieldValue<T>(i);
        return reader.GetFieldValue<T>(i);
    }

    /// <summary>
    /// get a value of a specific type from reader
    /// </summary>
    /// <param name="i">index of field</param>
    /// <typeparam name="T">type of value to get</typeparam>
    /// <returns>value at the specified index</returns>
    public Task<T> FieldValueAsync<T>(int i) {
        if (inner != null) return inner.FieldValueAsync<T>(i);
        return reader.GetFieldValueAsync<T>(i);
    }

    /// <summary>
    /// get a value of a specific type from reader
    /// </summary>
    /// <param name="i">index of field</param>
    /// <typeparam name="T">type of value to get</typeparam>
    /// <returns>value at the specified index</returns>
    public T GetValue<T>(int i) {
        if (inner != null) return inner.GetValue<T>(i);
        return Converter.Convert<T>(dbInfo.ValueFromReader(this, i, typeof(T)), true);
    }

    /// <summary>
    /// get a value of a specific type from reader
    /// </summary>
    /// <param name="i">index of field</param>
    /// <param name="fieldType">type of value to get</param>
    /// <returns>value at the specified index</returns>
    public object GetValue(int i, Type fieldType) {
        if (inner != null) return inner.GetValue(i, fieldType);
        return fieldValue.MakeGenericMethod(fieldType).Invoke(reader, [i]);
    }

    /// <inheritdoc />
    public int GetValues(object[] values) {
        if (inner != null) return inner.GetValues(values);
        return reader.GetValues(values);
    }

    /// <inheritdoc />
    public bool IsDBNull(int i) {
        if (inner != null) return inner.IsDBNull(i);
        return reader.IsDBNull(i);
    }

    /// <inheritdoc />
    public int FieldCount => inner != null ? inner.FieldCount : reader.FieldCount;

    /// <inheritdoc />
    public object this[int i] => inner != null ? inner[i] : reader[i];

    /// <inheritdoc />
    public object this[string name] => inner != null ? inner[name] : reader[name];

    /// <inheritdoc />
    public void Dispose() {
        if (inner != null) {
            inner.Dispose();
            Semaphore?.Release();
            return;
        }
        reader.Dispose();
        command?.Dispose();
        Semaphore?.Release();
    }

    /// <inheritdoc />
    public void Close() {
        if (inner != null) { inner.Close(); return; }
        reader.Close();
    }

    /// <summary>
    /// closes the reader
    /// </summary>
    public Task CloseAsync() {
        if (inner != null) return inner.CloseAsync();
        return reader.CloseAsync();
    }

    /// <inheritdoc />
    public DataTable GetSchemaTable() {
        if (inner != null) return inner.GetSchemaTable();
        return reader.GetSchemaTable();
    }

    /// <inheritdoc />
    public bool NextResult() {
        if (inner != null) return inner.NextResult();
        return reader.NextResult();
    }

    /// <inheritdoc />
    public virtual bool Read() {
        if (inner != null) return inner.Read();
        return reader.Read();
    }

    /// <summary>
    /// fetches the next row in an async read
    /// </summary>
    /// <returns>
    /// true if there are more rows; otherwise, false.
    /// </returns>
    public virtual Task<bool> ReadAsync() {
        if (inner != null) return inner.ReadAsync();
        return reader.ReadAsync();
    }

    /// <summary>
    /// fetches the next row in an async read with cancellation support
    /// </summary>
    /// <param name="cancellationToken">token used to cancel the operation</param>
    /// <returns>
    /// true if there are more rows; otherwise, false.
    /// </returns>
    public virtual Task<bool> ReadAsync(CancellationToken cancellationToken) {
        if (inner != null) return inner.ReadAsync(cancellationToken);
        return reader.ReadAsync(cancellationToken);
    }

    /// <inheritdoc />
    public int Depth => inner != null ? inner.Depth : reader.Depth;

    /// <inheritdoc />
    public bool IsClosed => inner != null ? inner.IsClosed : reader.IsClosed;

    /// <inheritdoc />
    public int RecordsAffected => inner != null ? inner.RecordsAffected : reader.RecordsAffected;
}
