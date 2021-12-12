using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NightlyCode.Database.Info;
using DataTable = NightlyCode.Database.Clients.Tables.DataTable;

namespace NightlyCode.Database.Clients {

    /// <summary>
    /// db client which locks calls to db because connection does not support concurrency (SQLite)
    /// </summary>
    public class LockedDBClient : ADbClient {
        readonly IDBClient baseclient;
        readonly SemaphoreSlim connectionlock = new SemaphoreSlim(1, 1);
        readonly SemaphoreSlim transactionlock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// creates a new <see cref="LockedDBClient"/>
        /// </summary>
        /// <param name="baseclient">client to wrap</param>
        internal LockedDBClient(IDBClient baseclient) {
            this.baseclient = baseclient;
        }

        /// <inheritdoc />
        public override IDBInfo DBInfo => baseclient.DBInfo;

        /// <inheritdoc />
        public override IConnectionProvider Connection => baseclient.Connection;
        
        /// <inheritdoc />
        public override int NonQuery(Transaction transaction, string commandstring, IEnumerable<object> parameters) {
            if(transaction == null) {
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
        public override DataTable Query(Transaction transaction, string query, IEnumerable<object> parameters) {
            if(transaction == null) {
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
        public override object Scalar(Transaction transaction, string query, IEnumerable<object> parameters) {
            if(transaction == null) {
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
        public override IEnumerable<object> Set(Transaction transaction, string query, IEnumerable<object> parameters) {
            if(transaction == null) {
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
        public override async Task<int> NonQueryAsync(Transaction transaction, string commandstring, IEnumerable<object> parameters) {
            if(transaction == null) {
                await connectionlock.WaitAsync();
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
        public override async Task<DataTable> QueryAsync(Transaction transaction, string query, IEnumerable<object> parameters) {
            if(transaction == null) {
                await connectionlock.WaitAsync();
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
        public override async Task<object> ScalarAsync(Transaction transaction, string query, IEnumerable<object> parameters) {
            if(transaction == null) {
                await connectionlock.WaitAsync();
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
        public override async Task<object[]> SetAsync(Transaction transaction, string query, IEnumerable<object> parameters) {
            if(transaction == null) {
                await connectionlock.WaitAsync();
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
        public override Transaction Transaction() {
            return new Transaction(DBInfo, Connection.Connect(), transactionlock);
        }

        /// <inheritdoc />
        public override int NonQueryPrepared(Transaction transaction, string commandstring, IEnumerable<object> parameters) {
            if(transaction == null) {
                connectionlock.Wait();
                try {
                    return baseclient.NonQueryPrepared(null, commandstring, parameters);
                }
                finally {
                    connectionlock.Release();
                }
            }

            return baseclient.NonQueryPrepared(transaction, commandstring, parameters);
        }

        /// <inheritdoc />
        public override DataTable QueryPrepared(Transaction transaction, string query, IEnumerable<object> parameters) {
            if(transaction == null) {
                connectionlock.Wait();
                try {
                    return baseclient.QueryPrepared(null, query, parameters);
                }
                finally {
                    connectionlock.Release();
                }
            }

            return baseclient.QueryPrepared(transaction, query, parameters);
        }

        /// <inheritdoc />
        public override object ScalarPrepared(Transaction transaction, string query, IEnumerable<object> parameters) {
            if(transaction == null) {
                connectionlock.Wait();
                try {
                    return baseclient.ScalarPrepared(null, query, parameters);
                }
                finally {
                    connectionlock.Release();
                }
            }

            return baseclient.ScalarPrepared(transaction, query, parameters);
        }

        /// <inheritdoc />
        public override IEnumerable<object> SetPrepared(Transaction transaction, string query, IEnumerable<object> parameters) {
            if(transaction == null) {
                connectionlock.Wait();
                try {
                    return baseclient.SetPrepared(null, query, parameters);
                }
                finally {
                    connectionlock.Release();
                }
            }

            return baseclient.SetPrepared(transaction, query, parameters);
        }

        /// <inheritdoc />
        public override async Task<int> NonQueryPreparedAsync(Transaction transaction, string commandstring, IEnumerable<object> parameters) {
            if(transaction == null) {
                await connectionlock.WaitAsync();
                try {
                    return await baseclient.NonQueryPreparedAsync(null, commandstring, parameters);
                }
                finally {
                    connectionlock.Release();
                }
            }

            return await baseclient.NonQueryPreparedAsync(transaction, commandstring, parameters);
        }

        /// <inheritdoc />
        public override async Task<DataTable> QueryPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters) {
            if(transaction == null) {
                await connectionlock.WaitAsync();
                try {
                    return await baseclient.QueryPreparedAsync(null, query, parameters);
                }
                finally {
                    connectionlock.Release();
                }
            }

            return await baseclient.QueryPreparedAsync(transaction, query, parameters);
        }

        /// <inheritdoc />
        public override async Task<object> ScalarPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters) {
            if(transaction == null) {
                await connectionlock.WaitAsync();
                try {
                    return await baseclient.ScalarPreparedAsync(null, query, parameters);
                }
                finally {
                    connectionlock.Release();
                }
            }

            return await baseclient.ScalarPreparedAsync(transaction, query, parameters);
        }

        /// <inheritdoc />
        public override async Task<object[]> SetPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters) {
            if(transaction == null) {
                await connectionlock.WaitAsync();
                try {
                    return await baseclient.SetPreparedAsync(null, query, parameters);
                }
                finally {
                    connectionlock.Release();
                }
            }

            return await baseclient.SetPreparedAsync(transaction, query, parameters);
        }
    }
}