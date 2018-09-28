using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace NightlyCode.DB.Clients.Tables
{
    public class DataTable
    {
        DataTableColumns columns;
        DataRow[] rows;

        /// <summary>
        /// creates a new datatable
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="rows"></param>
        public DataTable(DataTableColumns columns, DataRow[] rows)
        {
            this.columns = columns;
            this.rows = rows;
        }

        /// <summary>
        /// column information
        /// </summary>
        public DataTableColumns Columns => columns;

        /// <summary>
        /// rows containing data
        /// </summary>
        public DataRow[] Rows => rows;

        static IEnumerable<object> ReadRow(IDataReader reader)
        {
            for (int i = 0; i < reader.FieldCount; ++i)
                yield return reader.GetValue(i);
        }

        static IEnumerable<DataRow> ReadRows(IDataReader reader, DataTableColumns columns)
        {
            while(reader.Read())
                yield return new DataRow(ReadRow(reader).ToArray(), columns);
        }

        public static DataTable FromReader(IDataReader reader)
        {
            DataTableColumns columns = new DataTableColumns();
            for (int i = 0; i < reader.FieldCount; ++i)
                columns[reader.GetName(i)] = i;

            return new DataTable(columns, ReadRows(reader, columns).ToArray());
        }
    }
}
