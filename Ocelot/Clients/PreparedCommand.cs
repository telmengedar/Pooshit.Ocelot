using System;
using System.Data.Common;

namespace Pooshit.Ocelot.Clients {
    
    /// <summary>
    /// command prepared to be sent to database
    /// </summary>
    public class PreparedCommand : IDisposable {
        
        /// <summary>
        /// creates a new <see cref="PreparedCommand"/>
        /// </summary>
        /// <param name="connection">connection used to send command</param>
        /// <param name="command">connection used to send command</param>
        public PreparedCommand(IConnection connection, DbCommand command) {
            Connection = connection;
            Command = command;
        }

        /// <summary>
        /// connection used to send command
        /// </summary>
        public IConnection Connection { get; }
        
        /// <summary>
        /// command to be sent
        /// </summary>
        public DbCommand Command { get; }

        /// <inheritdoc />
        public void Dispose() {
            Connection.Dispose();
            Command.Dispose();
        }
    }
}