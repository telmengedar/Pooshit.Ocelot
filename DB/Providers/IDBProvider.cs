using System.Data;
using NightlyCode.DB.Info;

namespace NightlyCode.DB.Providers {

    /// <summary>
    /// provides database specific data
    /// </summary>
    public interface IDBProvider {

        /// <summary>
        /// creates a new connection to the database
        /// </summary>
        /// <returns></returns>
        IDbConnection CreateConnection(string connectionstring);

        /// <summary>
        /// provider specific info
        /// </summary>
        IDBInfo DatabaseInfo { get; }
    }
}