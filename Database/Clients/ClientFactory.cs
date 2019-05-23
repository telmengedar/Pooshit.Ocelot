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
        /// <returns></returns>
        public static IDBClient Create(DbConnection connection, IDBInfo info) {
            IDBClient client = new DBClient(connection, info);
            if (connection.GetType().Name.ToLower().StartsWith("sqlite"))
                client= new LockedDBClient(client);
            return client;
        }
    }
}