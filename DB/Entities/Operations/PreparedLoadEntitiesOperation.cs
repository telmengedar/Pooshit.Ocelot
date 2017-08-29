using System.Linq;
using NightlyCode.DB.Clients;

namespace NightlyCode.DB.Entities.Operations {

    /// <summary>
    /// load operation prepared to execute
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PreparedDeleteOperation<T> {
        readonly IDBClient dbclient;
        readonly PreparedOperation operation;

        /// <summary>
        /// creates a new prepared delete operation
        /// </summary>
        /// <param name="dbclient"></param>
        /// <param name="statement"></param>
        public PreparedDeleteOperation(IDBClient dbclient, PreparedOperation statement) {
            this.dbclient = dbclient;
            operation = statement;
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <returns></returns>
        public int Execute() {
            return dbclient.NonQuery(operation.CommandText, operation.Parameters.Select(p => p.Value).ToArray());
        }

        public override string ToString() {
            return operation.CommandText;
        }
    }
}