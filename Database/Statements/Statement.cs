using System.Threading.Tasks;
using NightlyCode.Database.Clients;

namespace NightlyCode.Database.Statements {

    /// <inheritdoc />
    public class Statement : IStatement {
        readonly IDBClient dbclient;
        readonly string commandtext;

        /// <summary>
        /// creates a new <see cref="Statement"/>
        /// </summary>
        /// <param name="dbclient">client to use for execution</param>
        /// <param name="commandtext">command text to execute</param>
        public Statement(IDBClient dbclient, string commandtext) {
            this.dbclient = dbclient;
            this.commandtext = commandtext;
        }

        /// <inheritdoc />
        public long Execute(params object[] parameters) {
            return dbclient.NonQuery(commandtext, parameters);
        }

        /// <inheritdoc />
        public async Task<long> ExecuteAsync(params object[] parameters) {
            return await dbclient.NonQueryAsync(commandtext, parameters);
        }

        /// <inheritdoc />
        public long Execute(Transaction transaction, params object[] parameters) {
            return dbclient.NonQuery(transaction, commandtext, parameters);
        }

        /// <inheritdoc />
        public async Task<long> ExecuteAsync(Transaction transaction, params object[] parameters) {
            return await dbclient.NonQueryAsync(transaction, commandtext, parameters);
        }
    }
}