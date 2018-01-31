using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using NightlyCode.DB.Clients;
using NightlyCode.DB.Entities.Descriptors;

namespace NightlyCode.DB.Entities.Operations {
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
        public new LoadValuesOperation<TLoad, TJoin> Limit(int limit) {
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
        protected int? LoadLimit { get; set; }

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
        public DataTable Execute() {
            return Prepare().Execute();
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
        public IEnumerable<TScalar> ExecuteSet<TScalar>() {
            return Prepare().ExecuteSet<TScalar>();
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
                criteria.PrepareCommand(preparator, dbclient.DBInfo, descriptorgetter);
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
            if(orderbycriterias != null) {
                preparator.CommandBuilder.Append(" ORDER BY ");

                foreach (OrderByCriteria criteria in orderbycriterias)
                {
                    if (flag) flag = false;
                    else preparator.CommandBuilder.Append(", ");
                    criteria.Field.PrepareCommand(preparator, dbclient.DBInfo, descriptorgetter);
                    if (!criteria.Ascending)
                        preparator.CommandBuilder.Append(" DESC");
                }
            }

            flag = true;
            if(groupbycriterias != null) {
                preparator.CommandBuilder.Append(" GROUP BY ");

                foreach(IDBField criteria in groupbycriterias) {
                    if(flag) flag = false;
                    else preparator.CommandBuilder.Append(", ");
                    criteria.PrepareCommand(preparator, dbclient.DBInfo, descriptorgetter);
                }

            }
            if(Havings != null) {
                preparator.CommandBuilder.Append(" HAVING ");
                CriteriaVisitor.GetCriteriaText(Havings, descriptorgetter, dbclient.DBInfo, preparator);
            }

            if(LoadLimit.HasValue) {
                switch(dbclient.DBInfo.Type) {
                case DBType.SQLite:
                case DBType.Postgre:
                    preparator.CommandBuilder.Append(" LIMIT ").Append(LoadLimit.Value);
                    break;
                default:
                    throw new NotImplementedException();
                }
            }
            return new PreparedLoadValuesOperation<T>(dbclient, preparator.GetOperation());
        }

        /// <summary>
        /// specifies criterias for the operation
        /// </summary>
        /// <param name="criterias"></param>
        /// <returns></returns>
        public LoadValuesOperation<T> Where(Expression<Predicate<T>> criterias) {
            Criterias = criterias;
            return this;
        }

        /// <summary>
        /// specifies having criterias for the operation
        /// </summary>
        /// <param name="criterias"></param>
        /// <returns></returns>
        public LoadValuesOperation<T> Having(Expression<Predicate<T>> criterias) {
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
        /// <param name="limit"></param>
        /// <returns></returns>
        public LoadValuesOperation<T> Limit(int limit) {
            LoadLimit = limit;
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