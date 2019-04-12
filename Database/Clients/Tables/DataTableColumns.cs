using System.Collections.Generic;

namespace NightlyCode.Database.Clients.Tables
{

    /// <summary>
    /// column information for a <see cref="DataTable"/>
    /// </summary>
    public class DataTableColumns
    {
        readonly Dictionary<string, int> indices = new Dictionary<string, int>();

        /// <summary>
        /// indexer for column index information
        /// </summary>
        /// <param name="column">column name</param>
        /// <returns>index information for column name</returns>
        public int this[string column]
        {
            get => GetIndex(column);
            set => indices[column] = value;
        }

        /// <summary>
        /// get index for the column
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public int GetIndex(string column) {
            if (!indices.TryGetValue(column, out int index))
                throw new KeyNotFoundException($"'{column}' not found in column information. Available columns:\n{string.Join("\n", indices.Keys)}");
            return index;
        }

        /// <summary>
        /// names of columns
        /// </summary>
        public IEnumerable<string> Names => indices.Keys;
    }
}
