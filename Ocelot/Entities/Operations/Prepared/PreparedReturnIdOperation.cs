using System.Linq;
using System.Threading.Tasks;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Extern;

namespace Pooshit.Ocelot.Entities.Operations.Prepared {

    /// <summary>
    /// prepared operation which is returning an id instead of number of affected rows
    /// </summary>
    public class PreparedReturnIdOperation : PreparedOperation {
        
        /// <summary>
        /// creates a new <see cref="PreparedReturnIdOperation"/>
        /// </summary>
        /// <param name="dbclient">access to database</param>
        /// <param name="commandText">command text to execute</param>
        /// <param name="constantparameters">constant command parameters</param>
        /// <param name="dbPrepare">indicates whether to prepare statement at db aswell</param>
        public PreparedReturnIdOperation(IDBClient dbclient, string commandText, object[] constantparameters, bool dbPrepare)
            : base(dbclient, commandText, constantparameters, dbPrepare) {
        }

        /// <inheritdoc />
        public override long Execute(Transaction transaction, params object[] parameters) {
            if(DBPrepare && DBClient.DBInfo.PreparationSupported)
                return Converter.Convert<long>(DBClient.ScalarPrepared(transaction, CommandText, ConstantParameters.Concat(parameters)), true);
            return Converter.Convert<long>(DBClient.Scalar(transaction, CommandText, ConstantParameters.Concat(parameters)), true);
        }

        /// <inheritdoc />
        public override long Execute(params object[] parameters) {
            return Execute(null, parameters);
        }

        /// <inheritdoc />
        public override Task<long> ExecuteAsync(params object[] parameters) {
            return ExecuteAsync(null, parameters);
        }

        /// <inheritdoc />
        public override async Task<long> ExecuteAsync(Transaction transaction, params object[] parameters) {
            object value;
            if(DBPrepare && DBClient.DBInfo.PreparationSupported)
                value = await DBClient.ScalarPreparedAsync(transaction, CommandText, ConstantParameters.Concat(parameters));
            else value = await DBClient.ScalarAsync(transaction, CommandText, ConstantParameters.Concat(parameters));
            return Converter.Convert<long>(value, true);
        }
    }
}