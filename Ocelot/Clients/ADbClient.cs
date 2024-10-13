using System.Collections.Generic;
using System.Threading.Tasks;
using Pooshit.Ocelot.Clients.Tables;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Clients; 

/// <summary>
/// abstract implementation providing basic functionality to all db clients
/// </summary>
public abstract class ADbClient : IDBClient {
    /// <inheritdoc />
    public abstract IDBInfo DBInfo { get; }

    /// <inheritdoc />
    public abstract IConnectionProvider Connection { get; }

    /// <inheritdoc />
    public int NonQuery(string commandstring, params object[] parameters) => NonQuery(null, commandstring, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public int NonQuery(string commandstring, IEnumerable<object> parameters) => NonQuery(null, commandstring, parameters);

    /// <inheritdoc />
    public int NonQuery(Transaction transaction, string commandstring, params object[] parameters) => NonQuery(transaction, commandstring, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public abstract int NonQuery(Transaction transaction, string commandstring, IEnumerable<object> parameters);

    /// <inheritdoc />
    public DataTable Query(string query, params object[] parameters) => Query(null, query, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public DataTable Query(string query, IEnumerable<object> parameters) => Query(null, query, parameters);

    /// <inheritdoc />
    public DataTable Query(Transaction transaction, string query, params object[] parameters) => Query(transaction, query, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public abstract DataTable Query(Transaction transaction, string query, IEnumerable<object> parameters);

    /// <inheritdoc />
    public object Scalar(string query, params object[] parameters) => Scalar(null, query, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public object Scalar(string query, IEnumerable<object> parameters) => Scalar(null, query, parameters);

    /// <inheritdoc />
    public object Scalar(Transaction transaction, string query, params object[] parameters) => Scalar(transaction, query, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public abstract object Scalar(Transaction transaction, string query, IEnumerable<object> parameters);

    /// <inheritdoc />
    public IEnumerable<object> Set(string query, params object[] parameters) => Set(null, query, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public IEnumerable<object> Set(string query, IEnumerable<object> parameters) => Set(null, query, parameters);

    /// <inheritdoc />
    public IEnumerable<object> Set(Transaction transaction, string query, params object[] parameters) => Set(transaction, query, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public abstract IEnumerable<object> Set(Transaction transaction, string query, IEnumerable<object> parameters);

    /// <inheritdoc />
    public Task<int> NonQueryAsync(string commandstring, params object[] parameters) => NonQueryAsync(null, commandstring, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public Task<int> NonQueryAsync(string commandstring, IEnumerable<object> parameters) => NonQueryAsync(null, commandstring, parameters);

    /// <inheritdoc />
    public Task<int> NonQueryAsync(Transaction transaction, string commandstring, params object[] parameters) => NonQueryAsync(transaction, commandstring, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public abstract Task<int> NonQueryAsync(Transaction transaction, string commandstring, IEnumerable<object> parameters);

    /// <inheritdoc />
    public Task<DataTable> QueryAsync(string query, params object[] parameters) => QueryAsync(null, query, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public Task<DataTable> QueryAsync(string query, IEnumerable<object> parameters) => QueryAsync(null, query, parameters);

    /// <inheritdoc />
    public Task<DataTable> QueryAsync(Transaction transaction, string query, params object[] parameters) => QueryAsync(transaction, query, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public abstract Task<DataTable> QueryAsync(Transaction transaction, string query, IEnumerable<object> parameters);

    /// <inheritdoc />
    public Task<object> ScalarAsync(string query, params object[] parameters) => ScalarAsync(null, query, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public Task<object> ScalarAsync(string query, IEnumerable<object> parameters) => ScalarAsync(null, query, parameters);

    /// <inheritdoc />
    public Task<object> ScalarAsync(Transaction transaction, string query, params object[] parameters) => ScalarAsync(transaction, query, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public abstract Task<object> ScalarAsync(Transaction transaction, string query, IEnumerable<object> parameters);

    /// <inheritdoc />
    public Task<IEnumerable<object>> SetAsync(string query, params object[] parameters) => SetAsync(null, query, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public Task<IEnumerable<object>> SetAsync(string query, IEnumerable<object> parameters) => SetAsync(null, query, parameters);

    /// <inheritdoc />
    public Task<IEnumerable<object>> SetAsync(Transaction transaction, string query, params object[] parameters) => SetAsync(transaction, query, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public abstract Task<IEnumerable<object>> SetAsync(Transaction transaction, string query, IEnumerable<object> parameters);

    /// <inheritdoc />
    public abstract Transaction Transaction();

    /// <inheritdoc />
    public int NonQueryPrepared(string commandstring, params object[] parameters) => NonQueryPrepared(null, commandstring, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public int NonQueryPrepared(string commandstring, IEnumerable<object> parameters) => NonQueryPrepared(null, commandstring, parameters);

    /// <inheritdoc />
    public int NonQueryPrepared(Transaction transaction, string commandstring, params object[] parameters) => NonQueryPrepared(transaction, commandstring, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public abstract int NonQueryPrepared(Transaction transaction, string commandstring, IEnumerable<object> parameters);

    /// <inheritdoc />
    public DataTable QueryPrepared(string query, params object[] parameters) => QueryPrepared(null, query, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public DataTable QueryPrepared(string query, IEnumerable<object> parameters) => QueryPrepared(null, query, parameters);

    /// <inheritdoc />
    public DataTable QueryPrepared(Transaction transaction, string query, params object[] parameters) => QueryPrepared(transaction, query, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public abstract DataTable QueryPrepared(Transaction transaction, string query, IEnumerable<object> parameters);

    /// <inheritdoc />
    public object ScalarPrepared(string query, params object[] parameters) => ScalarPrepared(null, query, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public object ScalarPrepared(string query, IEnumerable<object> parameters) => ScalarPrepared(null, query, parameters);

    /// <inheritdoc />
    public object ScalarPrepared(Transaction transaction, string query, params object[] parameters) => ScalarPrepared(transaction, query, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public abstract object ScalarPrepared(Transaction transaction, string query, IEnumerable<object> parameters);

    /// <inheritdoc />
    public IEnumerable<object> SetPrepared(string query, params object[] parameters) => SetPrepared(null, query, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public IEnumerable<object> SetPrepared(string query, IEnumerable<object> parameters) => SetPrepared(null, query, parameters);

    /// <inheritdoc />
    public IEnumerable<object> SetPrepared(Transaction transaction, string query, params object[] parameters) => SetPrepared(transaction, query, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public abstract IEnumerable<object> SetPrepared(Transaction transaction, string query, IEnumerable<object> parameters);

    /// <inheritdoc />
    public Task<int> NonQueryPreparedAsync(string commandstring, params object[] parameters) => NonQueryPreparedAsync(null, commandstring, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public Task<int> NonQueryPreparedAsync(string commandstring, IEnumerable<object> parameters) => NonQueryPreparedAsync(null, commandstring, parameters);

    /// <inheritdoc />
    public Task<int> NonQueryPreparedAsync(Transaction transaction, string commandstring, params object[] parameters) => NonQueryPreparedAsync(transaction, commandstring, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public abstract Task<int> NonQueryPreparedAsync(Transaction transaction, string commandstring, IEnumerable<object> parameters);

    /// <inheritdoc />
    public Task<DataTable> QueryPreparedAsync(string query, params object[] parameters) => QueryPreparedAsync(null, query, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public Task<DataTable> QueryPreparedAsync(string query, IEnumerable<object> parameters) => QueryPreparedAsync(null, query, parameters);

    /// <inheritdoc />
    public Task<DataTable> QueryPreparedAsync(Transaction transaction, string query, params object[] parameters) => QueryPreparedAsync(transaction, query, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public abstract Task<DataTable> QueryPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters);

    /// <inheritdoc />
    public Task<object> ScalarPreparedAsync(string query, params object[] parameters) => ScalarPreparedAsync(null, query, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public Task<object> ScalarPreparedAsync(string query, IEnumerable<object> parameters) => ScalarPreparedAsync(null, query, parameters);

    /// <inheritdoc />
    public Task<object> ScalarPreparedAsync(Transaction transaction, string query, params object[] parameters) => ScalarPreparedAsync(transaction, query, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public abstract Task<object> ScalarPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters);

    /// <inheritdoc />
    public Task<IEnumerable<object>> SetPreparedAsync(string query, params object[] parameters) => SetPreparedAsync(null, query, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public Task<IEnumerable<object>> SetPreparedAsync(string query, IEnumerable<object> parameters) => SetPreparedAsync(null, query, parameters);

    /// <inheritdoc />
    public Task<IEnumerable<object>> SetPreparedAsync(Transaction transaction, string query, params object[] parameters) => SetPreparedAsync(transaction, query, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public abstract Task<IEnumerable<object>> SetPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters);

    /// <inheritdoc />
    public abstract Reader Reader(Transaction transaction, string command, IEnumerable<object> parameters);

    /// <inheritdoc />
    public Reader Reader(Transaction transaction, string command, params object[] parameters) => Reader(transaction, command, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public Reader Reader(string command, IEnumerable<object> parameters) => Reader(null, command, parameters);

    /// <inheritdoc />
    public Reader Reader(string command, params object[] parameters) => Reader(null, command, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public abstract Task<Reader> ReaderAsync(Transaction transaction, string command, IEnumerable<object> parameters);

    /// <inheritdoc />
    public Task<Reader> ReaderAsync(Transaction transaction, string command, params object[] parameters) => ReaderAsync(transaction, command, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public Task<Reader> ReaderAsync(string command, IEnumerable<object> parameters) => ReaderAsync(null, command, parameters);

    /// <inheritdoc />
    public Task<Reader> ReaderAsync(string command, params object[] parameters) => ReaderAsync(null, command, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public abstract Reader ReaderPrepared(Transaction transaction, string command, IEnumerable<object> parameters);

    /// <inheritdoc />
    public Reader ReaderPrepared(Transaction transaction, string command, params object[] parameters) => ReaderPrepared(transaction, command, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public Reader ReaderPrepared(string command, IEnumerable<object> parameters) => ReaderPrepared(null, command, parameters);

    /// <inheritdoc />
    public Reader ReaderPrepared(string command, params object[] parameters) => ReaderPrepared(null, command, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public abstract Task<Reader> ReaderPreparedAsync(Transaction transaction, string command, IEnumerable<object> parameters);

    /// <inheritdoc />
    public Task<Reader> ReaderPreparedAsync(Transaction transaction, string command, params object[] parameters) => ReaderPreparedAsync(transaction, command, (IEnumerable<object>)parameters);

    /// <inheritdoc />
    public Task<Reader> ReaderPreparedAsync(string command, IEnumerable<object> parameters) => ReaderPreparedAsync(null, command, parameters);

    /// <inheritdoc />
    public Task<Reader> ReaderPreparedAsync(string command, params object[] parameters) => ReaderPreparedAsync(null, command, (IEnumerable<object>)parameters);
}