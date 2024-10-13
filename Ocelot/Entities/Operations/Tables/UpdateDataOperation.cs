using System;
using System.Linq;
using System.Threading.Tasks;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Tokens;

namespace Pooshit.Ocelot.Entities.Operations.Tables {

    /// <summary>
    /// updates data of a table
    /// </summary>
    public class UpdateDataOperation : WhereTokenOperation {
        readonly IDBClient dbclient;
        readonly string tablename;

        string[] columns;
        object[] values;

        /// <summary>
        /// creates a new <see cref="UpdateDataOperation"/>
        /// </summary>
        /// <param name="dbclient">access to database</param>
        /// <param name="tablename">name of table to update</param>
        public UpdateDataOperation(IDBClient dbclient, string tablename) {
            this.dbclient = dbclient;
            this.tablename = tablename;
        }

        /// <summary>
        /// adds a predicate for the operation
        /// </summary>
        /// <param name="predicate">predicate to append</param>
        /// <param name="mergeOp">operation to use when predicate is to be merged with an existing</param>
        public new UpdateDataOperation Where(ISqlToken predicate, CriteriaOperator mergeOp = CriteriaOperator.AND) {
            base.Where(predicate, mergeOp);
            return this;
        }

        /// <summary>
        /// specifies a value to update with the operation
        /// </summary>
        /// <param name="columnnames">column to update</param>
        /// <returns>this operation for fluent behavior</returns>
        public UpdateDataOperation Set(params string[] columnnames) {
            columns = columnnames;
            return this;
        }

        /// <summary>
        /// specifies values to use for update operation
        /// </summary>
        /// <param name="values">values to use when executing operation</param>
        /// <returns>this operation for fluent behavior</returns>
        public UpdateDataOperation Values(params object[] values) {
            this.values = values;
            return this;
        }
        
        /// <summary>
        /// prepares the operation for execution
        /// </summary>
        /// <returns></returns>
        public PreparedOperation Prepare() {
            return Prepare(true);
        }
        
        PreparedOperation Prepare(bool dbPrepare) {
            if((columns?.Length ?? 0) == 0)
                throw new InvalidOperationException("No columns to update specified");

            OperationPreparator preparator = new OperationPreparator();
            preparator.AppendText($"UPDATE {tablename} SET ");

            if(values != null) {
                if(values.Length != columns.Length)
                    throw new InvalidOperationException("Value count does not match column count");
                preparator.AppendText($"{dbclient.DBInfo.ColumnIndicator}{columns.First()}{dbclient.DBInfo.ColumnIndicator}=");
                preparator.AppendParameter(values[0]);
                for(int i = 1; i < columns.Length; ++i) {
                    preparator.AppendText($",{dbclient.DBInfo.ColumnIndicator}{columns[i]}{dbclient.DBInfo.ColumnIndicator}=");
                    preparator.AppendParameter(values[i]);
                }
            }
            else {
                preparator.AppendText($"{dbclient.DBInfo.ColumnIndicator}{columns.First()}{dbclient.DBInfo.ColumnIndicator}=");
                preparator.AppendParameter();
                foreach(string column in columns.Skip(1)) {
                    preparator.AppendText($",{dbclient.DBInfo.ColumnIndicator}{column}{dbclient.DBInfo.ColumnIndicator}=");
                    preparator.AppendParameter();
                }
            }

            AppendCriterias(dbclient.DBInfo, preparator);

            return preparator.GetOperation(dbclient, dbPrepare);
        }

        /// <summary>
        /// executes the operation using a transaction
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="parameters">parameters to use for operation</param>
        /// <returns>number of affected rows</returns>
        public long Execute(Transaction transaction, params object[] parameters) {
            return Prepare(false).Execute(transaction, parameters);
        }

        /// <summary>
        /// executes the operation using a transaction
        /// </summary>
        /// <param name="parameters">parameters to use for operation</param>
        /// <returns>number of affected rows</returns>
        public long Execute(params object[] parameters) {
            return Prepare(false).Execute(parameters);
        }

        /// <summary>
        /// executes the operation using a transaction
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="parameters">parameters to use for operation</param>
        /// <returns>number of affected rows</returns>
        public Task<long> ExecuteAsync(Transaction transaction, params object[] parameters) {
            return Prepare(false).ExecuteAsync(transaction, parameters);
        }

        /// <summary>
        /// executes the operation using a transaction
        /// </summary>
        /// <param name="parameters">parameters to use for operation</param>
        /// <returns>number of affected rows</returns>
        public Task<long> ExecuteAsync(params object[] parameters) {
            return Prepare(false).ExecuteAsync(parameters);
        }

    }
}