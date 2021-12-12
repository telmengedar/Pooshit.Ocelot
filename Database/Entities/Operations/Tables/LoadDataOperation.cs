using System;
using System.Collections.Generic;
using System.Linq;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Clients.Tables;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Fields;
using NightlyCode.Database.Tokens;
using NightlyCode.Database.Tokens.Values;

namespace NightlyCode.Database.Entities.Operations.Tables {

    /// <summary>
    /// operation used to load data from a table
    /// </summary>
    public class LoadDataOperation {
        readonly IDBClient client;
        readonly Func<Type, EntityDescriptor> modelcache;
        readonly string tablename;
        string[] columns;
        readonly List<LoadCriteria> criterias=new List<LoadCriteria>();

        /// <summary>
        /// creates a new <see cref="LoadDataOperation"/>
        /// </summary>
        /// <param name="client">access to database</param>
        /// <param name="modelcache">access to entity models</param>
        /// <param name="tablename">name of table to load data from</param>
        public LoadDataOperation(IDBClient client, Func<Type, EntityDescriptor> modelcache, string tablename) {
            this.client = client;
            this.modelcache = modelcache;
            this.tablename = tablename;
        }

        /// <summary>
        /// limit to use when loading
        /// </summary>
        protected LimitField LimitStatement { get; set; }

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
        /// where criteria for load operation
        /// </summary>
        /// <param name="column">column for criteria</param>
        /// <param name="op">operator used to compare column to</param>
        /// <param name="value">value to compare against</param>
        /// <param name="type">type how criteria is linked</param>
        /// <returns>this operation for fluent behavior</returns>
        public LoadDataOperation Where(string column, string op, string value, CriteriaOperator type = CriteriaOperator.AND) {
            criterias.Add(new LoadCriteria(column, op, value, type));
            return this;
        }

        /// <summary>
        /// specifies a limited number of rows to return
        /// </summary>
        /// <param name="limit">number of rows to return</param>
        public LoadDataOperation Limit(long limit) {
            if(ReferenceEquals(LimitStatement, null))
                LimitStatement = new LimitField();
            LimitStatement.Limit = DB.Constant(limit);
            return this;
        }

        /// <summary>
        /// specifies a limited number of rows to return
        /// </summary>
        /// <param name="limit">number of rows to return</param>
        public LoadDataOperation Limit(ISqlToken limit) {
            if(ReferenceEquals(LimitStatement, null))
                LimitStatement = new LimitField();
            LimitStatement.Limit = limit;
            return this;
        }

        /// <summary>
        /// specifies an offset from which on to return result rows
        /// </summary>
        /// <param name="offset">number of rows to skip</param>
        public LoadDataOperation Offset(long offset) {
            if(ReferenceEquals(LimitStatement, null))
                LimitStatement = new LimitField();
            LimitStatement.Offset = new ConstantValue(offset);
            return this;
        }

        /// <summary>
        /// specifies an offset from which on to return result rows
        /// </summary>
        /// <param name="offset">number of rows to skip</param>
        public LoadDataOperation Offset(ISqlToken offset) {
            if(ReferenceEquals(LimitStatement, null))
                LimitStatement = new LimitField();
            LimitStatement.Offset = offset;
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
        public T ExecuteType<T>(Transaction transaction, Func<DataRow, T> assignments, params object[] parameters)
        {
            return Prepare().ExecuteType(transaction, assignments, parameters);
        }

        /// <summary>
        /// executes the operation and loads a type as result
        /// </summary>
        /// <param name="assignments">logic used to create entities</param>
        /// <param name="parameters">parameters to use</param>
        /// <returns>loaded entities</returns>
        public T ExecuteType<T>(Func<DataRow, T> assignments, params object[] parameters)
        {
            return Prepare().ExecuteType(assignments, parameters);
        }

        /// <summary>
        /// executes the operation and loads a type as result
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="assignments">logic used to create entities</param>
        /// <param name="parameters">parameters to use</param>
        /// <returns>loaded entities</returns>
        public IEnumerable<T> ExecuteTypes<T>(Transaction transaction, Func<DataRow, T> assignments, params object[] parameters)
        {
            return Prepare().ExecuteTypes(transaction, assignments, parameters);
        }

        /// <summary>
        /// executes the operation and loads a type as result
        /// </summary>
        /// <param name="assignments">logic used to create entities</param>
        /// <param name="parameters">parameters to use</param>
        /// <returns>loaded entities</returns>
        public IEnumerable<T> ExecuteTypes<T>(Func<DataRow, T> assignments, params object[] parameters)
        {
            return Prepare().ExecuteTypes(assignments, parameters);
        }

        /// <summary>
        /// prepares the operation for execution
        /// </summary>
        /// <returns>operation to be executed</returns>
        public PreparedLoadOperation Prepare() {
            OperationPreparator preparator = new OperationPreparator().AppendText("SELECT");

            bool flag = true;
            if (columns != null) {
                foreach (string column in columns) {
                    if (flag) flag = false;
                    else preparator.AppendText(",");
                    preparator.AppendText(client.DBInfo.MaskColumn(column));
                }
            }
            else preparator.AppendText("*");

            preparator.AppendText("FROM").AppendText(tablename);

            if (criterias.Any()) {
                preparator.AppendText("WHERE");
                flag = true;
                foreach (LoadCriteria criteria in criterias) {
                    if (flag) flag = false;
                    else preparator.AppendText(criteria.Type.ToString());

                    preparator.AppendText(client.DBInfo.MaskColumn(criteria.Column));
                    preparator.AppendText(criteria.Operator);
                    preparator.AppendParameter(criteria.Value);
                }
            }

            if(!ReferenceEquals(LimitStatement, null))
                preparator.AppendField(LimitStatement, client.DBInfo, null, null);

            return preparator.GetLoadValuesOperation(client, modelcache);
        }
    }
}