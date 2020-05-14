using System.Threading.Tasks;

namespace NightlyCode.Database.Clients {

    /// <summary>
    /// provides connections to the database
    /// </summary>
    public interface IConnectionProvider {

        /// <summary>
        /// creates a connection to the database
        /// </summary>
        /// <returns>created connection object</returns>
        IConnection Connect();

        /// <summary>
        /// creates a connection to the database
        /// </summary>
        /// <returns>created connection object</returns>
        Task<IConnection> ConnectAsync();
    }
}