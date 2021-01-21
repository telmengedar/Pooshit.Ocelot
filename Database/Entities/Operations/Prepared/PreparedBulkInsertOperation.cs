using System;
using System.Collections;
using System.Threading.Tasks;
using NightlyCode.Database.Clients;

namespace NightlyCode.Database.Entities.Operations.Prepared {
    
    /// <summary>
    /// a prepared db operation
    /// </summary>
    public class PreparedBulkInsertOperation {

        /// <summary>
        /// creates a new prepared operation
        /// </summary>
        /// <param name="dbclient">access to database</param>
        /// <param name="commandText">sql query text</param>
        public PreparedBulkInsertOperation(IDBClient dbclient, string commandText) {
            DBClient = dbclient;
            CommandText = commandText;
        }

        /// <summary>
        /// access to database
        /// </summary>
        /// <remarks>
        /// this usually is used to execute the operation
        /// </remarks>
        protected IDBClient DBClient { get; }

        /// <summary>
        /// text to execute
        /// </summary>
        public string CommandText { get; }

        PreparedOperation PrepareOperation(string basetext, IEnumerable data) {
            OperationPreparator preparator=new OperationPreparator();
            preparator.AppendText(basetext);

            bool firstrow = true;
            foreach (object row in data) {
                if (firstrow) firstrow = false;
                else preparator.AppendText(",");
                
                preparator.AppendText("(");
                if (row is IEnumerable rowdata) {
                    bool firstitem = true;
                    foreach (object item in rowdata) {
                        if (firstitem) firstitem = false;
                        else preparator.AppendText(",");

                        preparator.AppendParameter(item);
                    }
                }
                else throw new ArgumentException("Row to insert needs to be a series of values");
                preparator.AppendText(")");
            }
            
            return preparator.GetOperation(DBClient);
        }
        
        /// <summary>
        /// executes the operation using custom parameters
        /// </summary>
        /// <param name="parameters">parameters for operation</param>
        /// <returns>number of affected rows if applicable</returns>
        public virtual long Execute(IEnumerable[] parameters) {
            return Execute(null, parameters);
        }

        /// <summary>
        /// executes the operation using custom parameters
        /// </summary>
        /// <param name="parameters">parameters for operation</param>
        /// <returns>number of affected rows if applicable</returns>
        public virtual long Execute(IEnumerable parameters) {
            return Execute(null, parameters);
        }

        /// <summary>
        /// executes the operation using custom parameters
        /// </summary>
        /// <param name="transaction">transaction used to execute operation</param>
        /// <param name="parameters">parameters for operation</param>
        /// <returns>number of affected rows if applicable</returns>
        public virtual long Execute(Transaction transaction, IEnumerable[] parameters) {
            return PrepareOperation(CommandText, parameters).Execute(transaction);
        }

        /// <summary>
        /// executes the operation using custom parameters
        /// </summary>
        /// <param name="transaction">transaction used to execute operation</param>
        /// <param name="parameters">parameters for operation</param>
        /// <returns>number of affected rows if applicable</returns>
        public virtual long Execute(Transaction transaction, IEnumerable parameters) {
            return PrepareOperation(CommandText, parameters).Execute(transaction);
        }

        /// <summary>
        /// executes the operation using custom parameters
        /// </summary>
        /// <param name="parameters">parameters for operation</param>
        /// <returns>number of affected rows if applicable</returns>
        public virtual Task<long> ExecuteAsync(IEnumerable[] parameters) {
            return ExecuteAsync(null, parameters);
        }

        /// <summary>
        /// executes the operation using custom parameters
        /// </summary>
        /// <param name="parameters">parameters for operation</param>
        /// <returns>number of affected rows if applicable</returns>
        public virtual Task<long> ExecuteAsync(IEnumerable parameters) {
            return ExecuteAsync(null, parameters);
        }

        /// <summary>
        /// executes the operation using custom parameters
        /// </summary>
        /// <param name="transaction">transaction used to execute operation</param>
        /// <param name="parameters">parameters for operation</param>
        /// <returns>number of affected rows if applicable</returns>
        public virtual Task<long> ExecuteAsync(Transaction transaction, IEnumerable[] parameters) {
            return PrepareOperation(CommandText, parameters).ExecuteAsync(transaction);
        }

        /// <summary>
        /// executes the operation using custom parameters
        /// </summary>
        /// <param name="transaction">transaction used to execute operation</param>
        /// <param name="parameters">parameters for operation</param>
        /// <returns>number of affected rows if applicable</returns>
        public virtual Task<long> ExecuteAsync(Transaction transaction, IEnumerable parameters) {
            return PrepareOperation(CommandText, parameters).ExecuteAsync(transaction);
        }

        /// <inheritdoc/>
        public override string ToString() {
            return CommandText;
        }
    }
}