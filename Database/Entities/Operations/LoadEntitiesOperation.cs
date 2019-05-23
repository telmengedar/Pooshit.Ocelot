﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Expressions;
using NightlyCode.Database.Entities.Operations.Fields;
using NightlyCode.Database.Entities.Operations.Prepared;

namespace NightlyCode.Database.Entities.Operations {
    /// <summary>
    /// operation used to load entities
    /// </summary>
    public class LoadEntitiesOperation<T> : ILoadEntitiesOperation {
        readonly IDBClient dbclient;
        readonly Func<Type, EntityDescriptor> descriptorgetter;
        OrderByCriteria[] orderbycriterias;
        IDBField[] groupbycriterias;
        readonly List<JoinOperation> joinoperations = new List<JoinOperation>();
 
        /// <summary>
        /// creates a new <see cref="LoadEntitiesOperation{T}"/>
        /// </summary>
        /// <param name="origin"></param>
        internal LoadEntitiesOperation(LoadEntitiesOperation<T> origin)
            : this(origin.dbclient, origin.descriptorgetter) {
            orderbycriterias = origin.orderbycriterias;
            groupbycriterias = origin.groupbycriterias;
            joinoperations = origin.joinoperations;
            LimitStatement = origin.LimitStatement;
            Criterias = origin.Criterias;
            Havings = origin.Havings;
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="dbclient"> </param>
        /// <param name="descriptorgetter"></param>
        public LoadEntitiesOperation(IDBClient dbclient, Func<Type, EntityDescriptor> descriptorgetter) {
            this.descriptorgetter = descriptorgetter;
            this.dbclient = dbclient;
        }

        /// <summary>
        /// result limit
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
        /// <returns>all loaded entities</returns>
        public IEnumerable<T> Execute(Transaction transaction = null) {
            return Prepare<T>().Execute(transaction);
        }

        /// <summary>
        /// loads entities using the operation
        /// </summary>
        /// <returns>all loaded entities</returns>
        public Task<IEnumerable<T>> ExecuteAsync(Transaction transaction = null)
        {
            return Prepare<T>().ExecuteAsync(transaction);
        }

        /// <summary>
        /// loads entities from joined data
        /// </summary>
        /// <returns>all loaded entities</returns>
        public IEnumerable<TEntity> Execute<TEntity>(Transaction transaction = null)
        {
            return Prepare<TEntity>().Execute(transaction);
        }

        /// <summary>
        /// loads entities from joined data
        /// </summary>
        /// <returns>all loaded entities</returns>
        public Task<IEnumerable<TEntity>> ExecuteAsync<TEntity>(Transaction transaction = null)
        {
            return Prepare<TEntity>().ExecuteAsync(transaction);
        }

        /// <summary>
        /// prepares the operation for execution
        /// </summary>
        /// <returns>prepared operation which can get reused</returns>
        public PreparedLoadEntitiesOperation<T> Prepare() {
            return Prepare<T>();
        }

        /// <summary>
        /// prepares the operation for execution
        /// </summary>
        /// <returns>prepared operation which can get reused</returns>
        public PreparedLoadEntitiesOperation<TEntity> Prepare<TEntity>() {
            OperationPreparator preparator = new OperationPreparator();
            preparator.AppendText("SELECT");

            string columnindicator = dbclient.DBInfo.ColumnIndicator;

            EntityDescriptor modeldescriptor = descriptorgetter(typeof(TEntity));
            if(typeof(TEntity)==typeof(T))
                // most simple case, entity is created from base table
                preparator.AppendText(string.Join(", ", modeldescriptor.Columns.Select(c => $"{columnindicator}{c.Name}{columnindicator}")));
            else {
                // entity is created from some joined table data
                JoinOperation entityjoin = JoinOperations.FirstOrDefault(o => o.JoinType == typeof(TEntity));
                if(entityjoin==null)
                    throw new InvalidOperationException("Unable to determine where to select entity values from (not selecting from base table and no join matches entity type)");
                preparator.AppendText(string.Join(", ", modeldescriptor.Columns.Select(c => $"{entityjoin.Alias}.{columnindicator}{c.Name}{columnindicator}")));
            }

            EntityDescriptor descriptor = descriptorgetter(typeof(T));
            preparator.AppendText("FROM").AppendText(descriptor.TableName);

            string tablealias = null;
            if(joinoperations.Count>0) {
                preparator.AppendText("AS t");
                tablealias = "t";
                foreach(JoinOperation operation in joinoperations) {
                    preparator.AppendText("INNER JOIN").AppendText(descriptorgetter(operation.JoinType).TableName);
                    if (!string.IsNullOrEmpty(operation.Alias))
                        preparator.AppendText("AS").AppendText(operation.Alias);
                    preparator.AppendText("ON");
                    CriteriaVisitor.GetCriteriaText(operation.Criterias, descriptorgetter, dbclient.DBInfo, preparator, tablealias, operation.Alias);
                    if (operation.AdditionalCriterias != null) {
                        preparator.AppendText("AND");
                        CriteriaVisitor.GetCriteriaText(operation.AdditionalCriterias, descriptorgetter, dbclient.DBInfo, preparator, operation.Alias);
                    }
                }
            }

            if(Criterias != null) {
                preparator.AppendText("WHERE");
                if(tablealias!=null)
                    CriteriaVisitor.GetCriteriaText(Criterias, descriptorgetter, dbclient.DBInfo, preparator, tablealias);
                else CriteriaVisitor.GetCriteriaText(Criterias, descriptorgetter, dbclient.DBInfo, preparator);
            }

            bool flag = true;
            if(groupbycriterias != null) {
                preparator.AppendText("GROUP BY");

                foreach(IDBField criteria in groupbycriterias) {
                    if(flag) flag = false;
                    else preparator.AppendText(",");
                    dbclient.DBInfo.Append(criteria, preparator, descriptorgetter);
                }
                
            }

            flag = true;
            if (orderbycriterias != null)
            {
                preparator.AppendText("ORDER BY");

                foreach (OrderByCriteria criteria in orderbycriterias)
                {
                    if (flag) flag = false;
                    else preparator.AppendText(",");
                    dbclient.DBInfo.Append(criteria.Field, preparator, descriptorgetter);

                    if (!criteria.Ascending)
                        preparator.AppendText("DESC");
                }
            }

            if (Havings != null) {
                preparator.AppendText("HAVING");
                if(tablealias!=null)
                    CriteriaVisitor.GetCriteriaText(Havings, descriptorgetter, dbclient.DBInfo, preparator, tablealias);
                else CriteriaVisitor.GetCriteriaText(Havings, descriptorgetter, dbclient.DBInfo, preparator);
            }

            if (!ReferenceEquals(LimitStatement, null)) {
                dbclient.DBInfo.Append(LimitStatement, preparator, descriptorgetter);
            }

            return preparator.GetLoadEntitiesOperation<TEntity>(dbclient, modeldescriptor);
        }

        /// <summary>
        /// specifies criterias for the operation
        /// </summary>
        /// <param name="criterias"></param>
        /// <returns></returns>
        public LoadEntitiesOperation<T> Where(Expression<Func<T, bool>> criterias) {
            Criterias = criterias;
            return this;
        }

        /// <inheritdoc />
        ILoadEntitiesOperation ILoadEntitiesOperation.Where(Expression criterias) {
            return Where(criterias);
        }

        /// <inheritdoc />
        ILoadEntitiesOperation ILoadEntitiesOperation.Join<TJoin>(Expression criterias, Expression additionalcriterias=null) {
            joinoperations.Add(new JoinOperation(typeof(TJoin), criterias, additionalcriterias, $"j{joinoperations.Count}"));
            return this;
        }

        /// <summary>
        /// specifies criterias for the operation
        /// </summary>
        /// <param name="criterias"></param>
        /// <returns></returns>
        public LoadEntitiesOperation<T> Where(Expression criterias)
        {
            Criterias = criterias;
            return this;
        }

        /// <summary>
        /// specifies having criterias for the operation
        /// </summary>
        /// <param name="criterias"></param>
        /// <returns></returns>
        public LoadEntitiesOperation<T> Having(Expression<Predicate<T>> criterias) {
            Havings = criterias;
            return this;
        }

        /// <summary>
        /// specifies an order
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public LoadEntitiesOperation<T> OrderBy(params OrderByCriteria[] fields) {
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
        public LoadEntitiesOperation<T> GroupBy(params IDBField[] fields) {
            if(fields.Length==0)
                throw new InvalidOperationException("at least one group criteria has to be specified");

            groupbycriterias = fields;
            return this;
        }

        /// <summary>
        /// specifies a limited number of rows to return
        /// </summary>
        /// <param name="limit">number of rows to return</param>
        public LoadEntitiesOperation<T> Limit(long limit) {
            if (ReferenceEquals(LimitStatement, null))
                LimitStatement = new LimitField();
            LimitStatement.Limit = limit;
            return this;
        }

        /// <summary>
        /// specifies an offset from which on to return result rows
        /// </summary>
        /// <param name="offset">number of rows to skip</param>
        public LoadEntitiesOperation<T> Offset(long offset)
        {
            if (ReferenceEquals(LimitStatement, null))
                LimitStatement = new LimitField();
            LimitStatement.Offset = offset;
            return this;
        }

        /// <summary>
        /// joins another type to the operation
        /// </summary>
        /// <typeparam name="TJoin">entity to join</typeparam>
        /// <param name="criteria">predicate for criteria</param>
        /// <param name="additionalcriterias">additional criterias for join</param>
        /// <returns></returns>
        public LoadEntitiesOperation<T, TJoin> Join<TJoin>(Expression<Func<T, TJoin, bool>> criteria, Expression<Func<TJoin, bool>> additionalcriterias=null) {
            ((ILoadEntitiesOperation) this).Join<TJoin>(criteria, additionalcriterias);
            return new LoadEntitiesOperation<T, TJoin>(this);
        }
    }

    /// <summary>
    /// operation used to load entities based on a join operation
    /// </summary>
    /// <typeparam name="TLoad"></typeparam>
    /// <typeparam name="TJoin"></typeparam>
    public class LoadEntitiesOperation<TLoad, TJoin> : LoadEntitiesOperation<TLoad> {

        internal LoadEntitiesOperation(LoadEntitiesOperation<TLoad> origin)
            : base(origin) {
        }

        /// <summary>
        /// specifies criterias for the operation
        /// </summary>
        /// <param name="criterias"></param>
        /// <returns></returns>
        public LoadEntitiesOperation<TLoad, TJoin> Where(Expression<Func<TLoad, TJoin, bool>> criterias) {
            Criterias = criterias;
            return this;
        }

        /// <summary>
        /// specifies an order
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public new LoadEntitiesOperation<TLoad, TJoin> OrderBy(params OrderByCriteria[] fields) {
            base.OrderBy(fields);
            return this;
        }

        /// <summary>
        /// groups the results by the specified fields
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public new LoadEntitiesOperation<TLoad, TJoin> GroupBy(params IDBField[] fields) {
            base.GroupBy(fields);
            return this;
        }

        /// <summary>
        /// specifies a limited number of rows to return
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public LoadEntitiesOperation<TLoad, TJoin> Limit(int limit) {
            base.Limit(limit);
            return this;
        }

        /// <summary>
        /// specifies having criterias for the operation
        /// </summary>
        /// <param name="criterias"></param>
        /// <returns></returns>
        public LoadEntitiesOperation<TLoad, TJoin> Having(Expression<Func<TLoad, TJoin, bool>> criterias) {
            Havings = criterias;
            return this;
        }

    }
}