using System;
using System.Collections.Generic;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Clients.Tables;
using NightlyCode.Database.Entities.Operations.Prepared;

namespace NightlyCode.Database.Entities.Operations.Tables {

    /// <summary>
    /// operation used to load data from a table
    /// </summary>
    public class LoadDataOperation {
        readonly IDBClient client;
        readonly string tablename;
        string[] columns;

        /// <summary>
        /// creates a new <see cref="LoadDataOperation"/>
        /// </summary>
        /// <param name="client">access to database</param>
        /// <param name="tablename">name of table to load data from</param>
        public LoadDataOperation(IDBClient client, string tablename) {
            this.client = client;
            this.tablename = tablename;
        }

        /// <summary>
        /// specifies columns to load from table
        /// </summary>
        /// <param name="columnnames">name of columns</param>
        /// <returns>this operation for fluent behavior</returns>
        public LoadDataOperation Columns(params string[] columnnames) {
            columns = columnnames;
            return this;
        }

        /// <summary>
        /// executes the operation and loads a datatable as result
        /// </summary>
        /// <param name="transaction">transaction to use (optional)</param>
        /// <returns>datatable containing loaded data</returns>
        public DataTable Execute(Transaction transaction=null) {
            if (transaction == null)
                return Prepare().Execute();
            return Prepare().Execute(transaction);
        }

        /// <summary>
        /// executes the operation and loads a set of data as result
        /// </summary>
        /// <param name="transaction">transaction to use (optional)</param>
        /// <returns>loaded data</returns>
        public IEnumerable<T> ExecuteSet<T>(Transaction transaction = null)
        {
            if (transaction == null)
                return Prepare().ExecuteSet<T>();
            return Prepare().ExecuteSet<T>(transaction);
        }

        /// <summary>
        /// executes the operation and loads a scalar as result
        /// </summary>
        /// <param name="transaction">transaction to use (optional)</param>
        /// <returns>loaded scalar or default if no data was loaded</returns>
        public T ExecuteScalar<T>(Transaction transaction = null)
        {
            if (transaction == null)
                return Prepare().ExecuteScalar<T>();
            return Prepare().ExecuteScalar<T>(transaction);
        }

        /// <summary>
        /// executes the operation and loads a type as result
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="assignments">logic used to create entities</param>
        /// <param name="parameters">parameters to use</param>
        /// <returns>loaded entities</returns>
        public IEnumerable<T> ExecuteType<T>(Transaction transaction, Func<DataRow, T> assignments, params object[] parameters)
        {
            return Prepare().ExecuteType(transaction, assignments, parameters);
        }

        /// <summary>
        /// executes the operation and loads a type as result
        /// </summary>
        /// <param name="assignments">logic used to create entities</param>
        /// <param name="parameters">parameters to use</param>
        /// <returns>loaded entities</returns>
        public IEnumerable<T> ExecuteType<T>(Func<DataRow, T> assignments, params object[] parameters)
        {
            return Prepare().ExecuteType(assignments, parameters);
        }

        /// <summary>
        /// prepares the operation for execution
        /// </summary>
        /// <returns>operation to be executed</returns>
        public PreparedLoadValuesOperation Prepare() {
            OperationPreparator preparator = new OperationPreparator().AppendText("SELECT");

            bool flag = true;
            foreach (string column in columns)
            {
                if (flag) flag = false;
                else preparator.AppendText(",");
                preparator.AppendText(client.DBInfo.MaskColumn(column));
            }

            preparator.AppendText("FROM").AppendText(tablename);

            return preparator.GetLoadValuesOperation(client);
        }
    }
}