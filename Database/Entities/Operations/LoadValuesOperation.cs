using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Expressions;
using NightlyCode.Database.Entities.Operations.Fields;

namespace NightlyCode.Database.Entities.Operations {
    /// <summary>
    /// operation used to load values of an entity based on a join operation
    /// </summary>
    /// <typeparam name="TLoad"></typeparam>
    /// <typeparam name="TJoin"></typeparam>
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
    /// <typeparam name="T"></typeparam>
    public class LoadValuesOperation<T> {
        readonly IDBClient dbclient;
        readonly Func<Type, EntityDescriptor> descriptorgetter;
        readonly IDBField[] columns;
        OrderByCriteria[] orderbycriterias;
        IDBField[] groupbycriterias;
        readonly List<JoinOperation> joinoperations = new List<JoinOperation>();

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="origin"></param>
        internal LoadValuesOperation(LoadValuesOperation<T> origin)
            : this(origin.dbclient, origin.columns, origin.descriptorgetter) {
            orderbycriterias = origin.orderbycriterias;
            groupbycriterias = origin.groupbycriterias;
            joinoperations = origin.joinoperations;
        }

        /// <summary>
        /// ctor
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
        protected List<JoinOperation> JoinOperations => joinoperations;

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
        public Clients.Tables.DataTable Execute(Transaction transaction)
        {
            return Prepare().Execute(transaction);
        }

        /// <summary>
        /// loads entities using the operation
        /// </summary>
        /// <returns></returns>
        public Clients.Tables.DataTable Execute() {
            return Prepare().Execute();
        }

        /// <summary>
        /// loads a value using the operation
        /// </summary>
        /// <typeparam name="TScalar"></typeparam>
        /// <returns></returns>
        public TScalar ExecuteScalar<TScalar>(Transaction transaction)
        {
            return Prepare().ExecuteScalar<TScalar>(transaction);
        }

        /// <summary>
        /// loads a value using the operation
        /// </summary>
        /// <typeparam name="TScalar"></typeparam>
        /// <returns></returns>
        public TScalar ExecuteScalar<TScalar>() {
            return Prepare().ExecuteScalar<TScalar>();
        }

        /// <summary>
        /// loads several values using the operation
        /// </summary>
        /// <typeparam name="TScalar"></typeparam>
        /// <returns></returns>
        public IEnumerable<TScalar> ExecuteSet<TScalar>(Transaction transaction)
        {
            return Prepare().ExecuteSet<TScalar>(transaction);
        }

        /// <summary>
        /// loads several values using the operation
        /// </summary>
        /// <typeparam name="TScalar"></typeparam>
        /// <returns></returns>
        public IEnumerable<TScalar> ExecuteSet<TScalar>() {
            return Prepare().ExecuteSet<TScalar>();
        }

        /// <summary>
        /// executes a query and stores the result in a custom result type
        /// </summary>
        /// <typeparam name="TType">type of result</typeparam>
        /// <param name="assignments">action used to assign values</param>
        /// <returns>enumeration of result types</returns>
        public IEnumerable<TType> ExecuteType<TType>(Action<Clients.Tables.DataRow, TType> assignments)
            where TType : new()
        {
            return Prepare().ExecuteType(assignments);
        }

        /// <summary>
        /// prepares the operation for execution
        /// </summary>
        /// <returns></returns>
        public PreparedLoadValuesOperation<T> Prepare() {
            OperationPreparator preparator = new OperationPreparator(dbclient.DBInfo);
            preparator.CommandBuilder.Append("SELECT ");

            EntityDescriptor descriptor = descriptorgetter(typeof(T));

            bool flag = true;
            foreach(IDBField criteria in columns) {
                if(flag) flag = false;
                else preparator.CommandBuilder.Append(", ");
                dbclient.DBInfo.Append(criteria, preparator, descriptorgetter);
            }

            preparator.CommandBuilder.Append(" FROM ").Append(descriptor.TableName);

            if(joinoperations.Count > 0) {
                foreach(JoinOperation operation in joinoperations) {
                    preparator.CommandBuilder.Append(" INNER JOIN ").Append(descriptorgetter(operation.JoinType).TableName).Append(" ON ");
                    CriteriaVisitor.GetCriteriaText(operation.Criterias, descriptorgetter, dbclient.DBInfo, preparator);
                }
            }

            if(Criterias != null) {
                preparator.CommandBuilder.Append(" WHERE ");
                CriteriaVisitor.GetCriteriaText(Criterias, descriptorgetter, dbclient.DBInfo, preparator);
            }

            flag = true;
            if(groupbycriterias != null) {
                preparator.CommandBuilder.Append(" GROUP BY ");

                foreach(IDBField criteria in groupbycriterias) {
                    if(flag) flag = false;
                    else preparator.CommandBuilder.Append(", ");
                    dbclient.DBInfo.Append(criteria, preparator, descriptorgetter);
                }
            }

            flag = true;
            if (orderbycriterias != null)
            {
                preparator.CommandBuilder.Append(" ORDER BY ");

                foreach (OrderByCriteria criteria in orderbycriterias)
                {
                    if (flag) flag = false;
                    else preparator.CommandBuilder.Append(", ");
                    dbclient.DBInfo.Append(criteria.Field, preparator, descriptorgetter);

                    if (!criteria.Ascending)
                        preparator.CommandBuilder.Append(" DESC");
                }
            }

            if (Havings != null) {
                preparator.CommandBuilder.Append(" HAVING ");
                CriteriaVisitor.GetCriteriaText(Havings, descriptorgetter, dbclient.DBInfo, preparator);
            }

            if (!ReferenceEquals(LimitStatement, null)) {
                preparator.CommandBuilder.Append(" ");
                dbclient.DBInfo.Append(LimitStatement, preparator, descriptorgetter);
            }

            return new PreparedLoadValuesOperation<T>(dbclient, preparator.GetOperation());
        }

        /// <summary>
        /// specifies criterias for the operation
        /// </summary>
        /// <param name="criterias"></param>
        /// <returns></returns>
        public LoadValuesOperation<T> Where(Expression<Func<T,bool>> criterias) {
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
        public LoadValuesOperation<T> Limit(long limit)
        {
            if (ReferenceEquals(LimitStatement, null))
                LimitStatement = new LimitField();
            LimitStatement.Limit = limit;
            return this;
        }

        /// <summary>
        /// specifies an offset from which on to return result rows
        /// </summary>
        /// <param name="offset">number of rows to skip</param>
        public LoadValuesOperation<T> Offset(long offset)
        {
            if (ReferenceEquals(LimitStatement, null))
                LimitStatement = new LimitField();
            LimitStatement.Offset = offset;
            return this;
        }

        /// <summary>
        /// joins another type to the operation
        /// </summary>
        /// <typeparam name="TJoin"></typeparam>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public LoadValuesOperation<T, TJoin> Join<TJoin>(Expression<Func<T, TJoin, bool>> criteria) {
            joinoperations.Add(new JoinOperation(typeof(TJoin), criteria));
            return new LoadValuesOperation<T, TJoin>(this);
        }
    }
}