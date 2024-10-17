using System;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace Pooshit.Ocelot.Tests.Mocks {
    public class FakeReader : DbDataReader {
        int currentRow = -1;
        string[] names;
        object[][] data;

        public FakeReader(string[] names, object[][] data) {
            this.names = names;
            this.data = data;
        }

        public override bool GetBoolean(int i) {
            return (bool)data[currentRow][i];
        }

        public override byte GetByte(int i) {
            return (byte)data[currentRow][i];
        }

        public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) {
            throw new NotImplementedException();
        }

        public override char GetChar(int i) {
            return (char)data[currentRow][i];
        }

        public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i) {
            throw new NotImplementedException();
        }

        public override string GetDataTypeName(int i) {
            throw new NotImplementedException();
        }

        public override DateTime GetDateTime(int i) {
            return (DateTime)data[currentRow][i];
        }

        public override decimal GetDecimal(int i) {
            return (decimal)data[currentRow][i];
        }

        public override double GetDouble(int i) {
            return (double)data[currentRow][i];
        }

        public override IEnumerator GetEnumerator() {
            throw new NotImplementedException();
        }

        public override Type GetFieldType(int i) {
            return data[currentRow][i].GetType();
        }

        public override float GetFloat(int i) {
            return (float)data[currentRow][i];
        }

        public override Guid GetGuid(int i) {
            return (Guid)data[currentRow][i];
        }

        public override short GetInt16(int i) {
            return (short)data[currentRow][i];
        }

        public override int GetInt32(int i) {
            return (int)data[currentRow][i];
        }

        public override long GetInt64(int i) {
            return (long)data[currentRow][i];
        }

        public override string GetName(int i) {
            return names[i];
        }

        public override int GetOrdinal(string name) {
            throw new NotImplementedException();
        }

        public override string GetString(int i) {
            return (string)data[currentRow][i];
        }

        public override object GetValue(int i) {
            return data[currentRow][i];
        }

        public override int GetValues(object[] values) {
            throw new NotImplementedException();
        }

        public override bool IsDBNull(int i) {
            return data[currentRow][i] is DBNull;
        }

        public override int FieldCount => names.Length;
        public override bool HasRows { get; }

        public override object this[int i] => GetValue(i);

        public override object this[string name] => GetValue(Array.IndexOf(names, name));

        public void Dispose() {
        }

        public void Close() {
        }

        public DataTable GetSchemaTable() {
            throw new NotImplementedException();
        }

        public override bool NextResult() {
            return ++currentRow < data.Length;
        }

        public override bool Read() {
            return ++currentRow < data.Length;
        }

        public override int Depth { get; }
        public override bool IsClosed => false;
        public override int RecordsAffected { get; }
    }
}