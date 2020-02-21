using System;
using System.Data.Common;
using NightlyCode.Database.Info;

namespace NightlyCode.Database.Clients {

    /// <summary>
    /// factory used to create <see cref="IDBClient"/>s
    /// </summary>
    public static class ClientFactory {

        /// <summary>
        /// creates a new <see cref="IDBClient"/> used to access database
        /// </summary>
        /// <param name="connection">connection to use</param>
        /// <param name="info">database specific logic</param>
        /// <returns>created dbclient</returns>
        public static IDBClient Create(DbConnection connection, IDBInfo info) {
            IDBClient client = new DBClient(new ConnectionProvider(() => connection, false), info);
            if(connection.GetType().Name.ToLower().StartsWith("sqlite"))
                client = new LockedDBClient(client);
            return client;
        }

        /// <summary>
        /// creates a new <see cref="IDBClient"/> used to access database
        /// </summary>
        /// <param name="connectionprovider">provides connections to database to use</param>
        /// <param name="info">database specific logic</param>
        /// <param name="disposeconnection">determines whether to dispose the connection after usage (optional)</param>
        /// <param name="lockconnection">determines whether to lock use of the connection (optional)</param>
        /// <returns>created dbclient</returns>
        public static IDBClient Create(Func<DbConnection> connectionprovider, IDBInfo info, bool disposeconnection = false, bool lockconnection = false) {
            IDBClient client = new DBClient(new ConnectionProvider(connectionprovider, disposeconnection), info);
            if(lockconnection)
                client = new LockedDBClient(client);
            return client;
        }

    }
}