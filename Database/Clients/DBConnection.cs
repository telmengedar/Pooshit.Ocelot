using System.Data.Common;

namespace NightlyCode.Database.Clients {

    /// <inheritdoc />
    class DBConnection : IConnection {
        readonly bool disposeconnection;

        /// <summary>
        /// creates a new <see cref="DBConnection"/>
        /// </summary>
        /// <param name="connection">connection to contain</param>
        /// <param name="disposeconnection">determines whether connection is disposed when this connection is disposed</param>
        public DBConnection(DbConnection connection, bool disposeconnection) {
            this.disposeconnection = disposeconnection;
            Connection = connection;
        }

        /// <inheritdoc />
        public void Dispose() {
            if(disposeconnection) {
                Connection.Dispose();
            }
        }

        /// <inheritdoc />
        public DbConnection Connection { get; }
    }
}