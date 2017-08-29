using System.Collections.Generic;
using System.Data;
using NightlyCode.DB.Info;

namespace NightlyCode.DB.Clients
{

    /// <summary>
    /// interface for clients which provide access to a database
    /// </summary>
    public interface IDBClient
    {
        /// <summary>
        /// info about db connection
        /// </summary>
        IDBInfo DBInfo { get; }

        /// <summary>
        /// executes a non query
        /// </summary>
        /// <param name="commandstring"></param>
        /// <param name="parameters"></param>
        int NonQuery(string commandstring, params object[] parameters);

        /// <summary>
        /// executes a query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        DataTable Query(string query, params object[] parameters);

        /// <summary>
        /// executes a query returning a scalar
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        object Scalar(string query, params object[] parameters);

        /// <summary>
        /// executes a query returning a set of values
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IEnumerable<object> Set(string query, params object[] parameters);

        /// <summary>
        /// begins a transaction
        /// </summary>
        /// <returns></returns>
        Transaction BeginTransaction();
    }
}
