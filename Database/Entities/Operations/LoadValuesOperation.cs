using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Expressions;
using NightlyCode.Database.Entities.Operations.Fields;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Fields;

namespace NightlyCode.Database.Entities.Operations {
    
    /// <summary>
    /// operation used to load values of an entity based on a join operation
    /// </summary>
    /// <typeparam name="TLoad">type of initially loaded entity</typeparam>
    /// <typeparam name="TJoin">type of joined entity</typeparam>
    public class LoadValuesOperation<TLoad, TJoin> : LoadValuesOperation<TLoad> {

        internal LoadValuesOperation(LoadValuesOperation<TLoad> origin)
            : base(origin) {
        }

        /// <summary>
        /// specifies criterias for the operation
        /// </summary>
        /// <param name="criterias"></param>
        /// <returns></returns>
        public LoadValuesOperation<TLoad, TJoin> Where(Expression<Func<TLoad, TJoin, bool>> criterias) {
            Criterias = criterias;
            return this;
        }

        /// <summary>
        /// specifies an order
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public new LoadValuesOperation<TLoad, TJoin> OrderBy(params OrderByCriteria[] fields) {
            base.OrderBy(fields);
            return this;
        }

        /// <summary>
        /// groups the results by the specified fields
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public new LoadValuesOperation<TLoad, TJoin> GroupBy(params IDBField[] fields) {
            base.GroupBy(fields);
            return this;
        }

        /// <summary>
        /// specifies a limited number of rows to return
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public LoadValuesOperation<TLoad, TJoin> Limit(int limit) {
            base.Limit(limit);
            return this;
        }

        /// <summary>
        /// specifies having criterias for the operation
        /// </summary>
        /// <param name="criterias"></param>
        /// <returns></returns>
        public LoadValuesOperation<TLoad, TJoin> Having(Expression<Func<TLoad, TJoin, bool>> criterias) {
            Havings = criterias;
            return this;
        }

    }

    /// <summary>
    /// operation used to load values of an entity
    /// </summary>
    /// <typeparam name="T">type of entity of which to load values</typeparam>
    public class LoadValuesOperation<T> : IDatabaseOperation {
        readonly IDBClient dbclient;
        readonly Func<Type, EntityDescriptor> descriptorgetter;
        readonly IDBField[] columns;
        OrderByCriteria[] orderbycriterias;
        IDBField[] groupbycriterias;
        readonly List<JoinOperation> joinoperations = new List<JoinOperation>();
        bool distinct;
        string alias;
        readonly List<IDatabaseOperation> unions=new List<IDatabaseOperation>();

        /// <summary>
        /// creates a new <see cref="LoadValuesOperation{T}"/>
        /// </summary>
        /// <param name="origin">operation of which to copy existing specifications</param>
        internal LoadValuesOperation(LoadValuesOperation<T> origin)
            : this(origin.dbclient, origin.columns, origin.descriptorgetter) {
            orderbycriterias = origin.orderbycriterias;
            groupbycriterias = origin.groupbycriterias;
            joinoperations = origin.joinoperations;
            LimitStatement = origin.LimitStatement;
            Criterias = origin.Criterias;
            Havings = origin.Havings;
            distinct = origin.distinct;
            alias = origin.alias;
        }

        /// <summary>
        /// creates a new <see cref="LoadValuesOperation{T}"/>
        /// </summary>
        /// <param name="dbclient"> </param>
        /// <param name="fields">fields to load</param>
        /// <param name="descriptorgetter"></param>
        public LoadValuesOperation(IDBClient dbclient, Func<Type, EntityDescriptor> descriptorgetter, params Expression<Func<T, object>>[] fields) {
            this.descriptorgetter = descriptorgetter;
            this.dbclient = dbclient;
            columns = fields.Select(EntityField.Create).Cast<IDBField>().ToArray();
        }

        /// <summary>
        /// creates a new <see cref="LoadValuesOperation{T}"/>
        /// </summary>
        /// <param name="dbclient"> </param>
        /// <param name="fields">fields to load</param>
        /// <param name="descriptorgetter"></param>
        public LoadValuesOperation(IDBClient dbclient, IDBField[] fields, Func<Type, EntityDescriptor> descriptorgetter) {
            this.descriptorgetter = descriptorgetter;
            this.dbclient = dbclient;
            columns = fields;
        }

        /// <summary>
        /// limit to use when loading
        /// </summary>
        protected LimitField LimitStatement { get; set; }

        /// <summary>
        /// operations to join
        /// </summary>
        protected internal List<JoinOperation> JoinOperations => joinoperations;

        /// <summary>
        /// criterias to use when loading
        /// </summary>
        protected Expression Criterias { get; set; }

        /// <summary>
        /// having criterias
        /// </summary>
        protected Expression Havings { get; set; }

        /// <summary>
        /// loads entities using the operation
        /// </summary>
        /// <returns></returns>
        public Clients.Tables.DataTable Execute(Transaction transaction = null) {
            return Prepare().Execute(transaction);
        }

        /// <summary>
        /// loads entities using the operation
        /// </summary>
        /// <returns></returns>
        public Task<Clients.Tables.DataTable> ExecuteAsync(Transaction transaction = null) {
            return Prepare().ExecuteAsync(transaction);
        }

        /// <summary>
        /// loads a value using the operation
        /// </summary>
        /// <typeparam name="TScalar">type of scalar to return</typeparam>
        /// <param name="transaction">transaction to use (optional)</param>
        /// <returns>resulting scalar of operation</returns>
        public TScalar ExecuteScalar<TScalar>(Transaction transaction = null) {
            return Prepare().ExecuteScalar<TScalar>(transaction);
        }

        /// <summary>
        /// loads a value using the operation
        /// </summary>
        /// <typeparam name="TScalar">type of scalar to return</typeparam>
        /// <param name="transaction">transaction to use (optional)</param>
        /// <returns>resulting scalar of operation</returns>
        public Task<TScalar> ExecuteScalarAsync<TScalar>(Transaction transaction = null) {
            return Prepare().ExecuteScalarAsync<TScalar>(transaction);
        }

        /// <summary>
        /// loads several values using the operation
        /// </summary>
        /// <typeparam name="TScalar">type of resulting set values</typeparam>
        /// <param name="transaction">transaction to use (optional)</param>
        /// <returns>resultset of operation</returns>
        public IEnumerable<TScalar> ExecuteSet<TScalar>(Transaction transaction = null) {
            return Prepare().ExecuteSet<TScalar>(transaction);
        }

        /// <summary>
        /// loads several values using the operation
        /// </summary>
        /// <typeparam name="TScalar">type of resulting set values</typeparam>
        /// <param name="transaction">transaction to use (optional)</param>
        /// <returns>resultset of operation</returns>
        public Task<TScalar[]> ExecuteSetAsync<TScalar>(Transaction transaction = null) {
            return Prepare().ExecuteSetAsync<TScalar>(transaction);
        }

        /// <summary>
        /// executes a query and stores the result in a custom result type
        /// </summary>
        /// <typeparam name="TType">type of result</typeparam>
        /// <param name="transaction">transaction to use for operation execution</param>
        /// <param name="assignments">action used to assign values</param>
        /// <returns>enumeration of result types</returns>
        public IEnumerable<TType> ExecuteType<TType>(Func<Clients.Tables.DataRow, TType> assignments, Transaction transaction = null) {
            return Prepare().ExecuteType(transaction, assignments);
        }

        /// <summary>
        /// executes a query and stores the result in a custom result type
        /// </summary>
        /// <typeparam name="TType">type of result</typeparam>
        /// <param name="transaction">transaction to use for operation execution</param>
        /// <param name="assignments">action used to assign values</param>
        /// <returns>enumeration of result types</returns>
        public Task<TType[]> ExecuteTypeAsync<TType>(Func<Clients.Tables.DataRow, TType> assignments, Transaction transaction = null) {
            return Prepare().ExecuteTypeAsync(transaction, assignments);
        }

        /// <summary>
        /// prepares the operation for execution
        /// </summary>
        /// <returns></returns>
        public PreparedLoadValuesOperation Prepare() {
            OperationPreparator preparator = new OperationPreparator();
            ((IDatabaseOperation)this).Prepare(preparator);
            return preparator.GetLoadValuesOperation(dbclient);
        }

        /// <summary>
        /// provides an alias to use for the operation
        /// </summary>
        /// <remarks>
        /// necessary to prevent conflicts if the loaded type is used multiple times in a complex query
        /// </remarks>
        /// <param name="tablealias">name of alias to use</param>
        /// <returns>this operation for fluent behavior</returns>
        public LoadValuesOperation<T> Alias(string tablealias) {
            alias = tablealias;
            return this;
        }

        /// <summary>
        /// executes a union with another statement
        /// </summary>
        /// <param name="operation">operation to use as union</param>
        /// <returns>this operation for fluent behavior</returns>
        public LoadValuesOperation<T> Union(IDatabaseOperation operation) {
            unions.Add(operation);
            return this;
        }

        /// <summary>
        /// specifies to only load rows with distinct values
        /// </summary>
        /// <returns></returns>
        public LoadValuesOperation<T> Distinct() {
            distinct = true;
            return this;
        }

        /// <summary>
        /// specifies criterias for the operation
        /// </summary>
        /// <param name="criterias"></param>
        /// <returns></returns>
        public LoadValuesOperation<T> Where(Expression<Func<T, bool>> criterias) {
            Criterias = criterias;
            return this;
        }

        /// <summary>
        /// specifies having criterias for the operation
        /// </summary>
        /// <param name="criterias"></param>
        /// <returns></returns>
        public LoadValuesOperation<T> Having(Expression<Func<T, bool>> criterias) {
            Havings = criterias;
            return this;
        }

        /// <summary>
        /// specifies an order
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public LoadValuesOperation<T> OrderBy(params OrderByCriteria[] fields) {
            if(fields.Length == 0)
                throw new InvalidOperationException("at least one criteria has to be specified");

            orderbycriterias = fields;
            return this;
        }

        /// <summary>
        /// groups the results by the specified fields
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public LoadValuesOperation<T> GroupBy(params IDBField[] fields) {
            if(fields.Length == 0)
                throw new InvalidOperationException("at least one group criteria has to be specified");

            groupbycriterias = fields;
            return this;
        }

        /// <summary>
        /// specifies a limited number of rows to return
        /// </summary>
        /// <param name="limit">number of rows to return</param>
        public LoadValuesOperation<T> Limit(long limit) {
            if(ReferenceEquals(LimitStatement, null))
                LimitStatement = new LimitField();
            LimitStatement.Limit = limit;
            return this;
        }

        /// <summary>
        /// specifies an offset from which on to return result rows
        /// </summary>
        /// <param name="offset">number of rows to skip</param>
        public LoadValuesOperation<T> Offset(long offset) {
            if(ReferenceEquals(LimitStatement, null))
                LimitStatement = new LimitField();
            LimitStatement.Offset = offset;
            return this;
        }

        /// <summary>
        /// joins another type to the operation
        /// </summary>
        /// <typeparam name="TJoin">type to join</typeparam>
        /// <param name="criteria">join criteria</param>
        /// <param name="alias">alias to use</param>
        /// <returns>this load operation for fluent behavior</returns>
        public LoadValuesOperation<T, TJoin> Join<TJoin>(Expression<Func<T, TJoin, bool>> criteria, string alias = null) {
            joinoperations.Add(new JoinOperation(typeof(TJoin), criteria, JoinOp.Inner, null, alias));
            return new LoadValuesOperation<T, TJoin>(this);
        }

        /// <summary>
        /// joins another type to the operation
        /// </summary>
        /// <typeparam name="TJoin">type to join</typeparam>
        /// <param name="criteria">join criteria</param>
        /// <param name="alias">alias to use</param>
        /// <returns>this load operation for fluent behavior</returns>
        public LoadValuesOperation<T, TJoin> LeftJoin<TJoin>(Expression<Func<T, TJoin, bool>> criteria, string alias = null) {
            joinoperations.Add(new JoinOperation(typeof(TJoin), criteria, JoinOp.Left, null, alias));
            return new LoadValuesOperation<T, TJoin>(this);
        }

        /// <inheritdoc />
        void IDatabaseOperation.Prepare(IOperationPreparator preparator) {
            List<string> aliases = new List<string>();
            string tablealias = null;

            if (!string.IsNullOrEmpty(alias)) {
                tablealias = alias;
                aliases.Add(tablealias);
            }
            else if(joinoperations.Count > 0) {
                tablealias = "t";
                aliases.Add(tablealias);
            }

            preparator.AppendText("SELECT");
            
            if(distinct)
                preparator.AppendText("DISTINCT");

            EntityDescriptor descriptor = typeof(T) == typeof(object) ? null : descriptorgetter(typeof(T));

            bool flag = true;
            foreach(IDBField criteria in columns) {
                if(flag)
                    flag = false;
                else
                    preparator.AppendText(",");
                preparator.AppendField(criteria, dbclient.DBInfo, descriptorgetter, tablealias);
            }

            if(descriptor != null)
                preparator.AppendText("FROM").AppendText(descriptor.TableName);

            if(!string.IsNullOrEmpty(tablealias))
                preparator.AppendText("AS").AppendText(tablealias);

            if(joinoperations.Count > 0) {
                foreach(JoinOperation operation in joinoperations) {
                    preparator.AppendText($"{operation.Operation.ToString().ToUpper()} JOIN")
                        .AppendText(descriptorgetter(operation.JoinType).TableName);
                    if(!string.IsNullOrEmpty(operation.Alias))
                        preparator.AppendText("AS").AppendText(operation.Alias);
                    preparator.AppendText("ON");
                    CriteriaVisitor.GetCriteriaText(operation.Criterias, descriptorgetter, dbclient.DBInfo, preparator, tablealias, operation.Alias);
                    aliases.Add(operation.Alias);
                }
            }

            if(Criterias != null) {
                preparator.AppendText("WHERE");
                CriteriaVisitor.GetCriteriaText(Criterias, descriptorgetter, dbclient.DBInfo, preparator, aliases.ToArray());
            }

            flag = true;
            if(groupbycriterias != null) {
                preparator.AppendText("GROUP BY");

                foreach(IDBField criteria in groupbycriterias) {
                    if(flag)
                        flag = false;
                    else
                        preparator.AppendText(",");
                    preparator.AppendField(criteria, dbclient.DBInfo, descriptorgetter, tablealias);
                }
            }

            flag = true;
            if(orderbycriterias != null) {
                preparator.AppendText("ORDER BY");

                foreach(OrderByCriteria criteria in orderbycriterias) {
                    if(flag)
                        flag = false;
                    else
                        preparator.AppendText(",");
                    preparator.AppendField(criteria.Field, dbclient.DBInfo, descriptorgetter, tablealias);

                    if(!criteria.Ascending)
                        preparator.AppendText("DESC");
                }
            }

            if(Havings != null) {
                preparator.AppendText("HAVING");
                CriteriaVisitor.GetCriteriaText(Havings, descriptorgetter, dbclient.DBInfo, preparator, aliases.ToArray());
            }

            if(!ReferenceEquals(LimitStatement, null))
                preparator.AppendField(LimitStatement, dbclient.DBInfo, descriptorgetter, tablealias);

            foreach(IDatabaseOperation union in unions) {
                preparator.AppendText("UNION ALL");
                union.Prepare(preparator);
            }
        }
    }
}