namespace NightlyCode.Database.Clients.Tables
{
    /// <summary>
    /// row of a database table
    /// </summary>
    public class DataRow
    {
        DataTableColumns columninformation;
        object[] data;

        /// <summary>
        /// creates a new <see cref="DataRow"/>
        /// </summary>
        /// <param name="data">data of the <see cref="DataRow"/></param>
        /// <param name="columns">column information of <see cref="DataTable"/></param>
        public DataRow(object[] data, DataTableColumns columns=null)
        {
            this.data = data;
            columninformation = columns;
        }

        /// <summary>
        /// indexer for row data
        /// </summary>
        /// <param name="index">index at which to get value</param>
        /// <returns>value at the specified index</returns>
        public object this[int index]=>data[index];

        /// <summary>
        /// indexer for row data
        /// </summary>
        /// <param name="column">name of column which to return</param>
        /// <returns>value of specified column</returns>
        public object this[string column]=>this[columninformation[column]];
    }
}
