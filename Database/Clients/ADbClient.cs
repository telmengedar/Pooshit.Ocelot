using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NightlyCode.Database.Clients.Tables;
using NightlyCode.Database.Info;

namespace NightlyCode.Database.Clients {
    
    /// <summary>
    /// abstract implementation providing basic functionality to all db clients
    /// </summary>
    public abstract class ADbClient : IDBClient {
        
        /// <inheritdoc />
        public abstract IDBInfo DBInfo { get; }

        /// <inheritdoc />
        public abstract IConnectionProvider Connection { get; }

        /// <inheritdoc />
        public int NonQuery(string commandstring, params object[] parameters) {
            return NonQuery(null, commandstring, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public int NonQuery(string commandstring, IEnumerable<object> parameters) {
            return NonQuery(null, commandstring, parameters);
        }

        /// <inheritdoc />
        public int NonQuery(Transaction transaction, string commandstring, params object[] parameters) {
            return NonQuery(transaction, commandstring, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public abstract int NonQuery(Transaction transaction, string commandstring, IEnumerable<object> parameters);

        /// <inheritdoc />
        public DataTable Query(string query, params object[] parameters) {
            return Query(null, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public DataTable Query(string query, IEnumerable<object> parameters) {
            return Query(null, query, parameters);
        }

        /// <inheritdoc />
        public DataTable Query(Transaction transaction, string query, params object[] parameters) {
            return Query(transaction, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public abstract DataTable Query(Transaction transaction, string query, IEnumerable<object> parameters);

        /// <inheritdoc />
        public object Scalar(string query, params object[] parameters) {
            return Scalar(null, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public object Scalar(string query, IEnumerable<object> parameters) {
            return Scalar(null, query, parameters);
        }

        /// <inheritdoc />
        public object Scalar(Transaction transaction, string query, params object[] parameters) {
            return Scalar(transaction, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public abstract object Scalar(Transaction transaction, string query, IEnumerable<object> parameters);

        /// <inheritdoc />
        public IEnumerable<object> Set(string query, params object[] parameters) {
            return Set(null, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public IEnumerable<object> Set(string query, IEnumerable<object> parameters) {
            return Set(null, query, parameters);
        }

        /// <inheritdoc />
        public IEnumerable<object> Set(Transaction transaction, string query, params object[] parameters) {
            return Set(transaction, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public abstract IEnumerable<object> Set(Transaction transaction, string query, IEnumerable<object> parameters);

        /// <inheritdoc />
        public Task<int> NonQueryAsync(string commandstring, params object[] parameters) {
            return NonQueryAsync(null, commandstring, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public Task<int> NonQueryAsync(string commandstring, IEnumerable<object> parameters) {
            return NonQueryAsync(null, commandstring, parameters);
        }

        /// <inheritdoc />
        public Task<int> NonQueryAsync(Transaction transaction, string commandstring, params object[] parameters) {
            return NonQueryAsync(transaction, commandstring, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public abstract Task<int> NonQueryAsync(Transaction transaction, string commandstring, IEnumerable<object> parameters);

        /// <inheritdoc />
        public Task<DataTable> QueryAsync(string query, params object[] parameters) {
            return QueryAsync(null, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public Task<DataTable> QueryAsync(string query, IEnumerable<object> parameters) {
            return QueryAsync(null, query, parameters);
        }

        /// <inheritdoc />
        public Task<DataTable> QueryAsync(Transaction transaction, string query, params object[] parameters) {
            return QueryAsync(transaction, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public abstract Task<DataTable> QueryAsync(Transaction transaction, string query, IEnumerable<object> parameters);

        /// <inheritdoc />
        public Task<object> ScalarAsync(string query, params object[] parameters) {
            return ScalarAsync(null, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public Task<object> ScalarAsync(string query, IEnumerable<object> parameters) {
            return ScalarAsync(null, query, parameters);
        }

        /// <inheritdoc />
        public Task<object> ScalarAsync(Transaction transaction, string query, params object[] parameters) {
            return ScalarAsync(transaction, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public abstract Task<object> ScalarAsync(Transaction transaction, string query, IEnumerable<object> parameters);

        /// <inheritdoc />
        public Task<object[]> SetAsync(string query, params object[] parameters) {
            return SetAsync(null, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public Task<object[]> SetAsync(string query, IEnumerable<object> parameters) {
            return SetAsync(null, query, parameters);
        }

        /// <inheritdoc />
        public Task<object[]> SetAsync(Transaction transaction, string query, params object[] parameters) {
            return SetAsync(transaction, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public abstract Task<object[]> SetAsync(Transaction transaction, string query, IEnumerable<object> parameters);

        /// <inheritdoc />
        public abstract Transaction Transaction();

        /// <inheritdoc />
        public int NonQueryPrepared(string commandstring, params object[] parameters) {
            return NonQueryPrepared(null, commandstring, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public int NonQueryPrepared(string commandstring, IEnumerable<object> parameters) {
            return NonQueryPrepared(null, commandstring, parameters);
        }

        /// <inheritdoc />
        public int NonQueryPrepared(Transaction transaction, string commandstring, params object[] parameters) {
            return NonQueryPrepared(transaction, commandstring, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public abstract int NonQueryPrepared(Transaction transaction, string commandstring, IEnumerable<object> parameters);

        /// <inheritdoc />
        public DataTable QueryPrepared(string query, params object[] parameters) {
            return QueryPrepared(null, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public DataTable QueryPrepared(string query, IEnumerable<object> parameters) {
            return QueryPrepared(null, query, parameters);
        }

        /// <inheritdoc />
        public DataTable QueryPrepared(Transaction transaction, string query, params object[] parameters) {
            return QueryPrepared(transaction, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public abstract DataTable QueryPrepared(Transaction transaction, string query, IEnumerable<object> parameters);

        /// <inheritdoc />
        public object ScalarPrepared(string query, params object[] parameters) {
            return ScalarPrepared(null, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public object ScalarPrepared(string query, IEnumerable<object> parameters) {
            return ScalarPrepared(null, query, parameters);
        }

        /// <inheritdoc />
        public object ScalarPrepared(Transaction transaction, string query, params object[] parameters) {
            return ScalarPrepared(transaction, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public abstract object ScalarPrepared(Transaction transaction, string query, IEnumerable<object> parameters);

        /// <inheritdoc />
        public IEnumerable<object> SetPrepared(string query, params object[] parameters) {
            return SetPrepared(null, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public IEnumerable<object> SetPrepared(string query, IEnumerable<object> parameters) {
            return SetPrepared(null, query, parameters);
        }

        /// <inheritdoc />
        public IEnumerable<object> SetPrepared(Transaction transaction, string query, params object[] parameters) {
            return SetPrepared(transaction, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public abstract IEnumerable<object> SetPrepared(Transaction transaction, string query, IEnumerable<object> parameters);

        /// <inheritdoc />
        public Task<int> NonQueryPreparedAsync(string commandstring, params object[] parameters) {
            return NonQueryPreparedAsync(null, commandstring, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public Task<int> NonQueryPreparedAsync(string commandstring, IEnumerable<object> parameters) {
            return NonQueryPreparedAsync(null, commandstring, parameters);
        }

        /// <inheritdoc />
        public Task<int> NonQueryPreparedAsync(Transaction transaction, string commandstring, params object[] parameters) {
            return NonQueryPreparedAsync(transaction, commandstring, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public abstract Task<int> NonQueryPreparedAsync(Transaction transaction, string commandstring, IEnumerable<object> parameters);

        /// <inheritdoc />
        public Task<DataTable> QueryPreparedAsync(string query, params object[] parameters) {
            return QueryPreparedAsync(null, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public Task<DataTable> QueryPreparedAsync(string query, IEnumerable<object> parameters) {
            return QueryPreparedAsync(null, query, parameters);
        }

        /// <inheritdoc />
        public Task<DataTable> QueryPreparedAsync(Transaction transaction, string query, params object[] parameters) {
            return QueryPreparedAsync(transaction, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public abstract Task<DataTable> QueryPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters);

        /// <inheritdoc />
        public Task<object> ScalarPreparedAsync(string query, params object[] parameters) {
            return ScalarPreparedAsync(null, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public Task<object> ScalarPreparedAsync(string query, IEnumerable<object> parameters) {
            return ScalarPreparedAsync(null, query, parameters);
        }

        /// <inheritdoc />
        public Task<object> ScalarPreparedAsync(Transaction transaction, string query, params object[] parameters) {
            return ScalarPreparedAsync(transaction, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public abstract Task<object> ScalarPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters);

        /// <inheritdoc />
        public Task<object[]> SetPreparedAsync(string query, params object[] parameters) {
            return SetPreparedAsync(null, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public Task<object[]> SetPreparedAsync(string query, IEnumerable<object> parameters) {
            return SetPreparedAsync(null, query, parameters);
        }

        /// <inheritdoc />
        public Task<object[]> SetPreparedAsync(Transaction transaction, string query, params object[] parameters) {
            return SetPreparedAsync(transaction, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public abstract Task<object[]> SetPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters);
    }
}