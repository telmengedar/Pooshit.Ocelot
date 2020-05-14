using System;
using System.Data.Common;

namespace NightlyCode.Database.Clients {

    /// <summary>
    /// a connection wrapper used to communicate with a database
    /// </summary>
    public interface IConnection : IDisposable {

        /// <summary>
        /// connection used to send statements and receive results
        /// </summary>
        DbConnection Connection { get; }
    }
}