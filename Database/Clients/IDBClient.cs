using System.Collections.Generic;
using System.Threading.Tasks;
using NightlyCode.Database.Info;

namespace NightlyCode.Database.Clients {

    /// <summary>
    /// interface for clients which provide access to a database
    /// </summary>
    public interface IDBClient {
        
        /// <summary>
        /// info about db connection
        /// </summary>
        IDBInfo DBInfo { get; }

        /// <summary>
        /// underlying connection
        /// </summary>
        IConnectionProvider Connection { get; }

        /// <summary>
        /// executes a non query
        /// </summary>
        /// <param name="commandstring">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        int NonQuery(string commandstring, params object[] parameters);

        /// <summary>
        /// executes a non query
        /// </summary>
        /// <param name="commandstring">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        int NonQuery(string commandstring, IEnumerable<object> parameters);

        /// <summary>
        /// executes a non query
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="commandstring">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        int NonQuery(Transaction transaction, string commandstring, params object[] parameters);

        /// <summary>
        /// executes a non query
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="commandstring">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        int NonQuery(Transaction transaction, string commandstring, IEnumerable<object> parameters);

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
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>table containing result data</returns>
        Tables.DataTable Query(string query, IEnumerable<object> parameters);

        /// <summary>
        /// executes a query
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>table containing result data</returns>
        Tables.DataTable Query(Transaction transaction, string query, params object[] parameters);

        /// <summary>
        /// executes a query
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>table containing result data</returns>
        Tables.DataTable Query(Transaction transaction, string query, IEnumerable<object> parameters);

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
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting scalar</returns>
        object Scalar(string query, IEnumerable<object> parameters);

        /// <summary>
        /// executes a query returning a scalar
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting scalar</returns>
        object Scalar(Transaction transaction, string query, params object[] parameters);

        /// <summary>
        /// executes a query returning a scalar
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting scalar</returns>
        object Scalar(Transaction transaction, string query, IEnumerable<object> parameters);

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
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting set of values</returns>
        IEnumerable<object> Set(string query, IEnumerable<object> parameters);

        /// <summary>
        /// executes a query returning a set of values
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting set of values</returns>
        IEnumerable<object> Set(Transaction transaction, string query, params object[] parameters);

        /// <summary>
        /// executes a query returning a set of values
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting set of values</returns>
        IEnumerable<object> Set(Transaction transaction, string query, IEnumerable<object> parameters);

        /// <summary>
        /// executes a non query
        /// </summary>
        /// <param name="commandstring">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        Task<int> NonQueryAsync(string commandstring, params object[] parameters);

        /// <summary>
        /// executes a non query
        /// </summary>
        /// <param name="commandstring">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        Task<int> NonQueryAsync(string commandstring, IEnumerable<object> parameters);

        /// <summary>
        /// executes a non query
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="commandstring">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        Task<int> NonQueryAsync(Transaction transaction, string commandstring, params object[] parameters);

        /// <summary>
        /// executes a non query
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="commandstring">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        Task<int> NonQueryAsync(Transaction transaction, string commandstring, IEnumerable<object> parameters);

        /// <summary>
        /// executes a query
        /// </summary>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>table containing result data</returns>
        Task<Tables.DataTable> QueryAsync(string query, params object[] parameters);

        /// <summary>
        /// executes a query
        /// </summary>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>table containing result data</returns>
        Task<Tables.DataTable> QueryAsync(string query, IEnumerable<object> parameters);

        /// <summary>
        /// executes a query
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>table containing result data</returns>
        Task<Tables.DataTable> QueryAsync(Transaction transaction, string query, params object[] parameters);

        /// <summary>
        /// executes a query
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>table containing result data</returns>
        Task<Tables.DataTable> QueryAsync(Transaction transaction, string query, IEnumerable<object> parameters);

        /// <summary>
        /// executes a query returning a scalar
        /// </summary>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting scalar</returns>
        Task<object> ScalarAsync(string query, params object[] parameters);

        /// <summary>
        /// executes a query returning a scalar
        /// </summary>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting scalar</returns>
        Task<object> ScalarAsync(string query, IEnumerable<object> parameters);

        /// <summary>
        /// executes a query returning a scalar
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting scalar</returns>
        Task<object> ScalarAsync(Transaction transaction, string query, params object[] parameters);

        /// <summary>
        /// executes a query returning a scalar
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting scalar</returns>
        Task<object> ScalarAsync(Transaction transaction, string query, IEnumerable<object> parameters);

        /// <summary>
        /// executes a query returning a set of values
        /// </summary>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting set of values</returns>
        Task<object[]> SetAsync(string query, params object[] parameters);

        /// <summary>
        /// executes a query returning a set of values
        /// </summary>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting set of values</returns>
        Task<object[]> SetAsync(string query, IEnumerable<object> parameters);

        /// <summary>
        /// executes a query returning a set of values
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting set of values</returns>
        Task<object[]> SetAsync(Transaction transaction, string query, params object[] parameters);

        /// <summary>
        /// executes a query returning a set of values
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting set of values</returns>
        Task<object[]> SetAsync(Transaction transaction, string query, IEnumerable<object> parameters);

        /// <summary>
        /// begins a transaction
        /// </summary>
        /// <returns>Transaction object to use</returns>
        Transaction Transaction();
        
                /// <summary>
        /// executes a non query
        /// </summary>
        /// <param name="commandstring">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        int NonQueryPrepared(string commandstring, params object[] parameters);

        /// <summary>
        /// executes a non query
        /// </summary>
        /// <param name="commandstring">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        int NonQueryPrepared(string commandstring, IEnumerable<object> parameters);

        /// <summary>
        /// executes a non query
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="commandstring">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        int NonQueryPrepared(Transaction transaction, string commandstring, params object[] parameters);

        /// <summary>
        /// executes a non query
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="commandstring">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        int NonQueryPrepared(Transaction transaction, string commandstring, IEnumerable<object> parameters);

        /// <summary>
        /// executes a query
        /// </summary>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>table containing result data</returns>
        Tables.DataTable QueryPrepared(string query, params object[] parameters);

        /// <summary>
        /// executes a query
        /// </summary>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>table containing result data</returns>
        Tables.DataTable QueryPrepared(string query, IEnumerable<object> parameters);

        /// <summary>
        /// executes a query
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>table containing result data</returns>
        Tables.DataTable QueryPrepared(Transaction transaction, string query, params object[] parameters);

        /// <summary>
        /// executes a query
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>table containing result data</returns>
        Tables.DataTable QueryPrepared(Transaction transaction, string query, IEnumerable<object> parameters);

        /// <summary>
        /// executes a query returning a scalar
        /// </summary>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting scalar</returns>
        object ScalarPrepared(string query, params object[] parameters);

        /// <summary>
        /// executes a query returning a scalar
        /// </summary>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting scalar</returns>
        object ScalarPrepared(string query, IEnumerable<object> parameters);

        /// <summary>
        /// executes a query returning a scalar
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting scalar</returns>
        object ScalarPrepared(Transaction transaction, string query, params object[] parameters);

        /// <summary>
        /// executes a query returning a scalar
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting scalar</returns>
        object ScalarPrepared(Transaction transaction, string query, IEnumerable<object> parameters);

        /// <summary>
        /// executes a query returning a set of values
        /// </summary>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting set of values</returns>
        IEnumerable<object> SetPrepared(string query, params object[] parameters);

        /// <summary>
        /// executes a query returning a set of values
        /// </summary>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting set of values</returns>
        IEnumerable<object> SetPrepared(string query, IEnumerable<object> parameters);

        /// <summary>
        /// executes a query returning a set of values
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting set of values</returns>
        IEnumerable<object> SetPrepared(Transaction transaction, string query, params object[] parameters);

        /// <summary>
        /// executes a query returning a set of values
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting set of values</returns>
        IEnumerable<object> SetPrepared(Transaction transaction, string query, IEnumerable<object> parameters);

        /// <summary>
        /// executes a non query
        /// </summary>
        /// <param name="commandstring">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        Task<int> NonQueryPreparedAsync(string commandstring, params object[] parameters);

        /// <summary>
        /// executes a non query
        /// </summary>
        /// <param name="commandstring">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        Task<int> NonQueryPreparedAsync(string commandstring, IEnumerable<object> parameters);

        /// <summary>
        /// executes a non query
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="commandstring">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        Task<int> NonQueryPreparedAsync(Transaction transaction, string commandstring, params object[] parameters);

        /// <summary>
        /// executes a non query
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="commandstring">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        Task<int> NonQueryPreparedAsync(Transaction transaction, string commandstring, IEnumerable<object> parameters);

        /// <summary>
        /// executes a query
        /// </summary>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>table containing result data</returns>
        Task<Tables.DataTable> QueryPreparedAsync(string query, params object[] parameters);

        /// <summary>
        /// executes a query
        /// </summary>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>table containing result data</returns>
        Task<Tables.DataTable> QueryPreparedAsync(string query, IEnumerable<object> parameters);

        /// <summary>
        /// executes a query
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>table containing result data</returns>
        Task<Tables.DataTable> QueryPreparedAsync(Transaction transaction, string query, params object[] parameters);

        /// <summary>
        /// executes a query
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>table containing result data</returns>
        Task<Tables.DataTable> QueryPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters);

        /// <summary>
        /// executes a query returning a scalar
        /// </summary>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting scalar</returns>
        Task<object> ScalarPreparedAsync(string query, params object[] parameters);

        /// <summary>
        /// executes a query returning a scalar
        /// </summary>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting scalar</returns>
        Task<object> ScalarPreparedAsync(string query, IEnumerable<object> parameters);

        /// <summary>
        /// executes a query returning a scalar
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting scalar</returns>
        Task<object> ScalarPreparedAsync(Transaction transaction, string query, params object[] parameters);

        /// <summary>
        /// executes a query returning a scalar
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting scalar</returns>
        Task<object> ScalarPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters);

        /// <summary>
        /// executes a query returning a set of values
        /// </summary>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting set of values</returns>
        Task<object[]> SetPreparedAsync(string query, params object[] parameters);

        /// <summary>
        /// executes a query returning a set of values
        /// </summary>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting set of values</returns>
        Task<object[]> SetPreparedAsync(string query, IEnumerable<object> parameters);

        /// <summary>
        /// executes a query returning a set of values
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting set of values</returns>
        Task<object[]> SetPreparedAsync(Transaction transaction, string query, params object[] parameters);

        /// <summary>
        /// executes a query returning a set of values
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="query">command text to execute</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns>resulting set of values</returns>
        Task<object[]> SetPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters);

    }
}
