using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Clients.Tables;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Tokens;
using Pooshit.Ocelot.Tokens.Values;
using DataTable = Pooshit.Ocelot.Clients.Tables.DataTable;

namespace Pooshit.Ocelot.Entities.Operations.Tables {

    /// <summary>
    /// operation used to load data from a table
    /// </summary>
    public class LoadDataOperation : WhereTokenOperation {
        readonly IDBClient client;
        readonly Func<Type, EntityDescriptor> modelcache;
        readonly string tablename;
        DataField[] columns;

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
        /// adds a predicate for the operation
        /// </summary>
        /// <param name="predicate">predicate to append</param>
        /// <param name="mergeOp">operation to use when predicate is to be merged with an existing</param>
        public new LoadDataOperation Where(ISqlToken predicate, CriteriaOperator mergeOp = CriteriaOperator.AND) {
            base.Where(predicate, mergeOp);
            return this;
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
            columns = columnnames.Select(c => new DataField(c, true)).ToArray();
            return this;
        }

        /// <summary>
        /// specifies columns to load from table
        /// </summary>
        /// <param name="columnnames">name of columns</param>
        /// <returns>this operation for fluent behavior</returns>
        public LoadDataOperation Columns(params DataField[] columnnames) {
            columns = columnnames;
            return this;
        }
        
        /// <summary>
        /// specifies a limited number of rows to return
        /// </summary>
        /// <param name="limit">number of rows to return</param>
        public LoadDataOperation Limit(long limit) {
            LimitStatement ??= new LimitField();
            LimitStatement.Limit = DB.Constant(limit);
            return this;
        }

        /// <summary>
        /// specifies a limited number of rows to return
        /// </summary>
        /// <param name="limit">number of rows to return</param>
        public LoadDataOperation Limit(ISqlToken limit) {
            LimitStatement ??= new LimitField();
            LimitStatement.Limit = limit;
            return this;
        }

        /// <summary>
        /// specifies an offset from which on to return result rows
        /// </summary>
        /// <param name="offset">number of rows to skip</param>
        public LoadDataOperation Offset(long offset) {
            LimitStatement ??= new LimitField();
            LimitStatement.Offset = new ConstantValue(offset);
            return this;
        }

        /// <summary>
        /// specifies an offset from which on to return result rows
        /// </summary>
        /// <param name="offset">number of rows to skip</param>
        public LoadDataOperation Offset(ISqlToken offset) {
            LimitStatement ??= new LimitField();
            LimitStatement.Offset = offset;
            return this;
        }

        /// <summary>
        /// executes the operation and loads a datatable as result
        /// </summary>
        /// <param name="parameters">parameters for statement</param>
        /// <returns>datatable containing loaded data</returns>
        public DataTable Execute(params object[] parameters) {
            return Prepare(false).Execute(parameters);
        }

        /// <summary>
        /// executes the operation and loads a datatable as result
        /// </summary>
        /// <param name="transaction">transaction to use (optional)</param>
        /// <param name="parameters">parameters for statement</param>
        /// <returns>datatable containing loaded data</returns>
        public DataTable Execute(Transaction transaction, params object[] parameters) {
            return Prepare(false).Execute(transaction, parameters);
        }

        /// <summary>
        /// executes the operation and loads a datatable as result
        /// </summary>
        /// <param name="parameters">parameters for statement</param>
        /// <returns>datatable containing loaded data</returns>
        public Task<DataTable> ExecuteAsync(params object[] parameters) {
            return Prepare(false).ExecuteAsync(parameters);
        }

        /// <summary>
        /// executes the operation and loads a datatable as result
        /// </summary>
        /// <param name="transaction">transaction to use (optional)</param>
        /// <param name="parameters">parameters for statement</param>
        /// <returns>datatable containing loaded data</returns>
        public Task<DataTable> ExecuteAsync(Transaction transaction, params object[] parameters) {
            return Prepare(false).ExecuteAsync(transaction, parameters);
        }

        /// <summary>
        /// executes the operation and loads a set of data as result
        /// </summary>
        /// <param name="parameters">parameters for statement</param>
        /// <returns>loaded data</returns>
        public IEnumerable<T> ExecuteSet<T>(params object[] parameters) {
            return Prepare(false).ExecuteSet<T>(null, parameters);
        }

        /// <summary>
        /// executes the operation and loads a set of data as result
        /// </summary>
        /// <param name="transaction">transaction to use (optional)</param>
        /// <param name="parameters">parameters for statement</param>
        /// <returns>loaded data</returns>
        public IEnumerable<T> ExecuteSet<T>(Transaction transaction, params object[] parameters) {
            return Prepare(false).ExecuteSet<T>(transaction, parameters);
        }

        /// <summary>
        /// executes the operation and loads a set of data as result
        /// </summary>
        /// <param name="parameters">parameters for statement</param>
        /// <returns>loaded data</returns>
        public Task<IEnumerable<T>> ExecuteSetAsync<T>(params object[] parameters) {
            return Prepare(false).ExecuteSetAsync<T>(null, parameters);
        }

        /// <summary>
        /// executes the operation and loads a set of data as result
        /// </summary>
        /// <param name="transaction">transaction to use (optional)</param>
        /// <param name="parameters">parameters for statement</param>
        /// <returns>loaded data</returns>
        public Task<IEnumerable<T>> ExecuteSetAsync<T>(Transaction transaction, params object[] parameters) {
            return Prepare(false).ExecuteSetAsync<T>(transaction, parameters);
        }

        /// <summary>
        /// executes the operation and loads a scalar as result
        /// </summary>
        /// <param name="parameters">parameters for statement</param>
        /// <returns>loaded scalar or default if no data was loaded</returns>
        public T ExecuteScalar<T>(params object[] parameters) {
            return Prepare(false).ExecuteScalar<T>(parameters);
        }

        /// <summary>
        /// executes the operation and loads a scalar as result
        /// </summary>
        /// <param name="transaction">transaction to use (optional)</param>
        /// <param name="parameters">parameters for statement</param>
        /// <returns>loaded scalar or default if no data was loaded</returns>
        public T ExecuteScalar<T>(Transaction transaction, params object[] parameters) {
            return Prepare(false).ExecuteScalar<T>(transaction, parameters);
        }

        /// <summary>
        /// executes the operation and loads a scalar as result
        /// </summary>
        /// <param name="parameters">parameters for statement</param>
        /// <returns>loaded scalar or default if no data was loaded</returns>
        public Task<T> ExecuteScalarAsync<T>(params object[] parameters) {
            return Prepare(false).ExecuteScalarAsync<T>(parameters);
        }

        /// <summary>
        /// executes the operation and loads a scalar as result
        /// </summary>
        /// <param name="transaction">transaction to use (optional)</param>
        /// <param name="parameters">parameters for statement</param>
        /// <returns>loaded scalar or default if no data was loaded</returns>
        public Task<T> ExecuteScalarAsync<T>(Transaction transaction, params object[] parameters) {
            return Prepare(false).ExecuteScalarAsync<T>(transaction, parameters);
        }

        /// <summary>
        /// executes the operation and loads a type as result
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="assignments">logic used to create entities</param>
        /// <param name="parameters">parameters to use</param>
        /// <returns>loaded entities</returns>
        public T ExecuteType<T>(Transaction transaction, Func<Row, T> assignments, params object[] parameters)
        {
            return Prepare(false).ExecuteType(transaction, assignments, parameters);
        }

        /// <summary>
        /// executes the operation and loads a type as result
        /// </summary>
        /// <param name="assignments">logic used to create entities</param>
        /// <param name="parameters">parameters to use</param>
        /// <returns>loaded entities</returns>
        public T ExecuteType<T>(Func<Row, T> assignments, params object[] parameters)
        {
            return Prepare(false).ExecuteType(assignments, parameters);
        }

        /// <summary>
        /// executes the operation and loads a type as result
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="assignments">logic used to create entities</param>
        /// <param name="parameters">parameters to use</param>
        /// <returns>loaded entities</returns>
        public Task<T> ExecuteTypeAsync<T>(Transaction transaction, Func<Row, T> assignments, params object[] parameters)
        {
            return Prepare(false).ExecuteTypeAsync(transaction, assignments, parameters);
        }

        /// <summary>
        /// executes the operation and loads a type as result
        /// </summary>
        /// <param name="assignments">logic used to create entities</param>
        /// <param name="parameters">parameters to use</param>
        /// <returns>loaded entities</returns>
        public Task<T> ExecuteTypeAsync<T>(Func<Row, T> assignments, params object[] parameters)
        {
            return Prepare(false).ExecuteTypeAsync(assignments, parameters);
        }

        /// <summary>
        /// executes the operation and loads a type as result
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="assignments">logic used to create entities</param>
        /// <param name="parameters">parameters to use</param>
        /// <returns>loaded entities</returns>
        public IEnumerable<T> ExecuteTypes<T>(Transaction transaction, Func<Row, T> assignments, params object[] parameters)
        {
            return Prepare(false).ExecuteTypes(transaction, assignments, parameters);
        }

        /// <summary>
        /// executes the operation and loads a type as result
        /// </summary>
        /// <param name="assignments">logic used to create entities</param>
        /// <param name="parameters">parameters to use</param>
        /// <returns>loaded entities</returns>
        public IEnumerable<T> ExecuteTypes<T>(Func<Row, T> assignments, params object[] parameters)
        {
            return Prepare(false).ExecuteTypes(assignments, parameters);
        }

        /// <summary>
        /// executes the operation and loads a type as result
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="assignments">logic used to create entities</param>
        /// <param name="parameters">parameters to use</param>
        /// <returns>loaded entities</returns>
        public Task<IEnumerable<T>> ExecuteTypesAsync<T>(Transaction transaction, Func<Row, T> assignments, params object[] parameters)
        {
            return Prepare(false).ExecuteTypesAsync(transaction, assignments, parameters);
        }

        /// <summary>
        /// executes the operation and loads a type as result
        /// </summary>
        /// <param name="assignments">logic used to create entities</param>
        /// <param name="parameters">parameters to use</param>
        /// <returns>loaded entities</returns>
        public Task<IEnumerable<T>> ExecuteTypesAsync<T>(Func<Row, T> assignments, params object[] parameters)
        {
            return Prepare(false).ExecuteTypesAsync(assignments, parameters);
        }

        /// <summary>
        /// executes the operation returning a reader
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="parameters">parameters for operation</param>
        /// <returns>reader to use to read command result</returns>
        public IDataReader ExecuteReader(Transaction transaction, params object[] parameters) {
            return Prepare(false).ExecuteReader(transaction, parameters);
        }

        /// <summary>
        /// executes the operation returning a reader
        /// </summary>
        /// <param name="parameters">parameters for operation</param>
        /// <returns>reader to use to read command result</returns>
        public IDataReader ExecuteReader(params object[] parameters) {
            return ExecuteReader(null, parameters);
        }

        /// <summary>
        /// executes the operation returning a reader
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="parameters">parameters for operation</param>
        /// <returns>reader to use to read command result</returns>
        public Task<Reader> ExecuteReaderAsync(Transaction transaction, params object[] parameters) {
            return Prepare(false).ExecuteReaderAsync(transaction, parameters);
        }

        /// <summary>
        /// executes the operation returning a reader
        /// </summary>
        /// <param name="parameters">parameters for operation</param>
        /// <returns>reader to use to read command result</returns>
        public Task<Reader> ExecuteReaderAsync(params object[] parameters) {
            return ExecuteReaderAsync(null, parameters);
        }

        /// <summary>
        /// prepares the operation for execution
        /// </summary>
        /// <returns>operation to be executed</returns>
        public PreparedLoadOperation Prepare() {
            return Prepare(true);
        }
        
        PreparedLoadOperation Prepare(bool dbPrepare) {
            OperationPreparator preparator = new OperationPreparator().AppendText("SELECT");

            bool flag = true;
            if (columns != null) {
                foreach (DataField column in columns) {
                    if (flag) flag = false;
                    else preparator.AppendText(",");
                    if (column.IsColumn)
                        preparator.AppendText(client.DBInfo.MaskColumn(column.Name));
                    else preparator.AppendText(column.Name);
                }
            }
            else preparator.AppendText("*");

            preparator.AppendText("FROM").AppendText(tablename);

            AppendCriterias(client.DBInfo, preparator);
            
            if(!ReferenceEquals(LimitStatement, null))
                preparator.AppendField(LimitStatement, client.DBInfo, null, null);

            return preparator.GetLoadValuesOperation(client, modelcache, dbPrepare);
        }
    }
}