using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using NightlyCode.Database.Info;
using DataTable = NightlyCode.Database.Clients.Tables.DataTable;

namespace NightlyCode.Database.Clients {

    /// <summary>
    /// db client which locks calls to db because connection does not support concurrency (SQLite)
    /// </summary>
    public class LockedDBClient : IDBClient {
        readonly IDBClient baseclient;
        readonly SemaphoreSlim connectionlock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// creates a new <see cref="LockedDBClient"/>
        /// </summary>
        /// <param name="baseclient">client to wrap</param>
        internal LockedDBClient(IDBClient baseclient) {
            this.baseclient = baseclient;
        }

        /// <inheritdoc />
        public IDBInfo DBInfo => baseclient.DBInfo;

        /// <inheritdoc />
        public DbConnection Connection => baseclient.Connection;

        /// <inheritdoc />
        public int NonQuery(string commandstring, params object[] parameters) {
            return NonQuery(null, commandstring, parameters);
        }

        /// <inheritdoc />
        public int NonQuery(string commandstring, IEnumerable<object> parameters) {
            return NonQuery(null, commandstring, parameters);
        }

        /// <inheritdoc />
        public int NonQuery(Transaction transaction, string commandstring, params object[] parameters) {
            if (transaction == null) {
                connectionlock.Wait();
                try {
                    return baseclient.NonQuery(null, commandstring, parameters);
                }
                finally {
                    connectionlock.Release();
                }
            }

            return baseclient.NonQuery(transaction, commandstring, parameters);
        }

        /// <inheritdoc />
        public int NonQuery(Transaction transaction, string commandstring, IEnumerable<object> parameters) {
            if (transaction == null) {
                connectionlock.Wait();
                try {
                    return baseclient.NonQuery(null, commandstring, parameters);
                }
                finally {
                    connectionlock.Release();
                }
            }

            return baseclient.NonQuery(transaction, commandstring, parameters);
        }

        /// <inheritdoc />
        public DataTable Query(string query, params object[] parameters) {
            return Query(null, query, parameters);
        }

        /// <inheritdoc />
        public DataTable Query(string query, IEnumerable<object> parameters) {
            return Query(null, query, parameters);
        }

        /// <inheritdoc />
        public DataTable Query(Transaction transaction, string query, params object[] parameters) {
            if (transaction == null) {
                connectionlock.Wait();
                try {
                    return baseclient.Query(null, query, parameters);
                }
                finally {
                    connectionlock.Release();
                }
            }

            return baseclient.Query(transaction, query, parameters);
        }

        /// <inheritdoc />
        public DataTable Query(Transaction transaction, string query, IEnumerable<object> parameters) {
            if (transaction == null) {
                connectionlock.Wait();
                try {
                    return baseclient.Query(null, query, parameters);
                }
                finally {
                    connectionlock.Release();
                }
            }

            return baseclient.Query(transaction, query, parameters);
        }

        /// <inheritdoc />
        public object Scalar(string query, params object[] parameters) {
            return Scalar(null, query, parameters);
        }

        /// <inheritdoc />
        public object Scalar(string query, IEnumerable<object> parameters) {
            return Scalar(null, query, parameters);
        }

        /// <inheritdoc />
        public object Scalar(Transaction transaction, string query, params object[] parameters) {
            if (transaction == null) {
                connectionlock.Wait();
                try {
                    return baseclient.Scalar(null, query, parameters);
                }
                finally {
                    connectionlock.Release();
                }
            }

            return baseclient.Scalar(transaction, query, parameters);
        }

        /// <inheritdoc />
        public object Scalar(Transaction transaction, string query, IEnumerable<object> parameters) {
            if (transaction == null) {
                connectionlock.Wait();
                try {
                    return baseclient.Scalar(null, query, parameters);
                }
                finally {
                    connectionlock.Release();
                }
            }

            return baseclient.Scalar(transaction, query, parameters);
        }

        /// <inheritdoc />
        public IEnumerable<object> Set(string query, params object[] parameters) {
            return Set(null, query, parameters);
        }

        /// <inheritdoc />
        public IEnumerable<object> Set(string query, IEnumerable<object> parameters) {
            return Set(null, query, parameters);
        }

        /// <inheritdoc />
        public IEnumerable<object> Set(Transaction transaction, string query, params object[] parameters) {
            if (transaction == null) {
                connectionlock.Wait();
                try {
                    return baseclient.Set(null, query, parameters);
                }
                finally {
                    connectionlock.Release();
                }
            }

            return baseclient.Set(transaction, query, parameters);
        }

        /// <inheritdoc />
        public IEnumerable<object> Set(Transaction transaction, string query, IEnumerable<object> parameters) {
            if (transaction == null) {
                connectionlock.Wait();
                try {
                    return baseclient.Set(null, query, parameters);
                }
                finally {
                    connectionlock.Release();
                }
            }

            return baseclient.Set(transaction, query, parameters);
        }

        /// <inheritdoc />
        public Task<int> NonQueryAsync(string commandstring, params object[] parameters) {
            return NonQueryAsync(null, commandstring, parameters);
        }

        /// <inheritdoc />
        public Task<int> NonQueryAsync(string commandstring, IEnumerable<object> parameters) {
            return NonQueryAsync(null, commandstring, parameters);
        }

        /// <inheritdoc />
        public async Task<int> NonQueryAsync(Transaction transaction, string commandstring, params object[] parameters) {
            if (transaction == null) {
                connectionlock.Wait();
                try {
                    return await baseclient.NonQueryAsync(null, commandstring, parameters);
                }
                finally {
                    connectionlock.Release();
                }
            }

            return await baseclient.NonQueryAsync(transaction, commandstring, parameters);
        }

        /// <inheritdoc />
        public async Task<int> NonQueryAsync(Transaction transaction, string commandstring, IEnumerable<object> parameters) {
            if (transaction == null) {
                connectionlock.Wait();
                try {
                    return await baseclient.NonQueryAsync(null, commandstring, parameters);
                }
                finally {
                    connectionlock.Release();
                }
            }

            return await baseclient.NonQueryAsync(transaction, commandstring, parameters);
        }

        /// <inheritdoc />
        public Task<DataTable> QueryAsync(string query, params object[] parameters) {
            return QueryAsync(null, query, parameters);
        }

        /// <inheritdoc />
        public Task<DataTable> QueryAsync(string query, IEnumerable<object> parameters) {
            return QueryAsync(null, query, parameters);
        }

        /// <inheritdoc />
        public async Task<DataTable> QueryAsync(Transaction transaction, string query, params object[] parameters) {
            if (transaction == null) {
                connectionlock.Wait();
                try {
                    return await baseclient.QueryAsync(null, query, parameters);
                }
                finally {
                    connectionlock.Release();
                }
            }

            return await baseclient.QueryAsync(transaction, query, parameters);
        }

        /// <inheritdoc />
        public async Task<DataTable> QueryAsync(Transaction transaction, string query, IEnumerable<object> parameters) {
            if (transaction == null) {
                connectionlock.Wait();
                try {
                    return await baseclient.QueryAsync(null, query, parameters);
                }
                finally {
                    connectionlock.Release();
                }
            }

            return await baseclient.QueryAsync(transaction, query, parameters);
        }

        /// <inheritdoc />
        public Task<object> ScalarAsync(string query, params object[] parameters) {
            return ScalarAsync(null, query, parameters);
        }

        /// <inheritdoc />
        public Task<object> ScalarAsync(string query, IEnumerable<object> parameters) {
            return ScalarAsync(null, query, parameters);
        }

        /// <inheritdoc />
        public async Task<object> ScalarAsync(Transaction transaction, string query, params object[] parameters) {
            if (transaction == null) {
                connectionlock.Wait();
                try {
                    return await baseclient.ScalarAsync(null, query, parameters);
                }
                finally {
                    connectionlock.Release();
                }
            }

            return await baseclient.ScalarAsync(transaction, query, parameters);
        }

        /// <inheritdoc />
        public async Task<object> ScalarAsync(Transaction transaction, string query, IEnumerable<object> parameters) {
            if (transaction == null) {
                connectionlock.Wait();
                try {
                    return await baseclient.ScalarAsync(null, query, parameters);
                }
                finally {
                    connectionlock.Release();
                }
            }

            return await baseclient.ScalarAsync(transaction, query, parameters);
        }

        /// <inheritdoc />
        public Task<IEnumerable<object>> SetAsync(string query, params object[] parameters) {
            return SetAsync(null, query, parameters);
        }

        /// <inheritdoc />
        public Task<IEnumerable<object>> SetAsync(string query, IEnumerable<object> parameters) {
            return SetAsync(null, query, parameters);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<object>> SetAsync(Transaction transaction, string query, params object[] parameters) {
            if (transaction == null) {
                connectionlock.Wait();
                try {
                    return await baseclient.SetAsync(null, query, parameters);
                }
                finally {
                    connectionlock.Release();
                }
            }

            return await baseclient.SetAsync(transaction, query, parameters);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<object>> SetAsync(Transaction transaction, string query, IEnumerable<object> parameters) {
            if (transaction == null) {
                connectionlock.Wait();
                try {
                    return await baseclient.SetAsync(null, query, parameters);
                }
                finally {
                    connectionlock.Release();
                }
            }

            return await baseclient.SetAsync(transaction, query, parameters);
        }

        /// <inheritdoc />
        public Transaction Transaction() {
            if (Connection.State != ConnectionState.Open)
                Connection.Open();
            return new Transaction(DBInfo, Connection, connectionlock);
        }
    }
}