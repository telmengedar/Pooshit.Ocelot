using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace NightlyCode.Database.Clients {

    /// <inheritdoc />
    class ConnectionProvider : IConnectionProvider {
        readonly Func<DbConnection> provider;
        readonly bool disposeconnection;

        /// <summary>
        /// creates a new <see cref="ConnectionProvider"/>
        /// </summary>
        /// <param name="provider">provides underlying db connections</param>
        /// <param name="disposeconnection">determines whether to dispose the underlying connection after usage</param>
        public ConnectionProvider(Func<DbConnection> provider, bool disposeconnection) {
            this.provider = provider;
            this.disposeconnection = disposeconnection;
        }

        /// <inheritdoc />
        public IConnection Connect() {
            DBConnection connection = new DBConnection(provider(), disposeconnection);
            if(connection.Connection.State != ConnectionState.Open)
                connection.Connection.Open();
            return connection;
        }

        /// <inheritdoc />
        public async Task<IConnection> ConnectAsync() {
            DBConnection connection = new DBConnection(provider(), disposeconnection);
            if(connection.Connection.State != ConnectionState.Open)
                await connection.Connection.OpenAsync();
            return connection;
        }
    }
}