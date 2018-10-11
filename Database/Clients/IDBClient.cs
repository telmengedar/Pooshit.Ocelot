using System.Collections.Generic;
using NightlyCode.Database.Info;

namespace NightlyCode.Database.Clients
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
        /// <param name="commandstring">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        int NonQuery(string commandstring, params object[] parameters);

        /// <summary>
        /// executes a non query
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="commandstring">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        int NonQuery(Transaction transaction, string commandstring, params object[] parameters);

        /// <summary>
        /// executes a query
        /// </summary>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>table containing result data</returns>
        Tables.DataTable Query(string query, params object[] parameters);

        /// <summary>
        /// executes a query
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>table containing result data</returns>
        Tables.DataTable Query(Transaction transaction, string query, params object[] parameters);

        /// <summary>
        /// executes a query returning a scalar
        /// </summary>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting scalar</returns>
        object Scalar(string query, params object[] parameters);

        /// <summary>
        /// executes a query returning a scalar
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting scalar</returns>
        object Scalar(Transaction transaction, string query, params object[] parameters);

        /// <summary>
        /// executes a query returning a set of values
        /// </summary>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting set of values</returns>
        IEnumerable<object> Set(string query, params object[] parameters);

        /// <summary>
        /// executes a query returning a set of values
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting set of values</returns>
        IEnumerable<object> Set(Transaction transaction, string query, params object[] parameters);

        /// <summary>
        /// begins a transaction
        /// </summary>
        /// <returns>Transaction object to use</returns>
        Transaction BeginTransaction();
    }
}
