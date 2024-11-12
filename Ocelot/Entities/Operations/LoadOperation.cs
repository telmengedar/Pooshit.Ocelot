using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Clients.Tables;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Expressions;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Tokens;
using Pooshit.Ocelot.Tokens.Values;

namespace Pooshit.Ocelot.Entities.Operations;

/// <summary>
/// operation used to load values of an entity based on a join operation
/// </summary>
/// <typeparam name="TLoad">type of initially loaded entity</typeparam>
/// <typeparam name="TJoin">type of joined entity</typeparam>
public class LoadOperation<TLoad, TJoin> : LoadOperation<TLoad> {

    internal LoadOperation(LoadOperation<TLoad> origin)
        : base(origin) {
    }

    /// <summary>
    /// specifies criterias for the operation
    /// </summary>
    /// <param name="criterias"></param>
    /// <returns></returns>
    public LoadOperation<TLoad, TJoin> Where(Expression<Func<TLoad, TJoin, bool>> criterias) {
        Criterias = criterias;
        return this;
    }

    /// <summary>
    /// specifies an order
    /// </summary>
    /// <param name="fields"></param>
    /// <returns></returns>
    public new LoadOperation<TLoad, TJoin> OrderBy(params OrderByCriteria[] fields) {
        base.OrderBy(fields);
        return this;
    }

    /// <summary>
    /// groups the results by the specified fields
    /// </summary>
    /// <param name="fields"></param>
    /// <returns></returns>
    public new LoadOperation<TLoad, TJoin> GroupBy(params IDBField[] fields) {
        base.GroupBy(fields);
        return this;
    }

    /// <summary>
    /// specifies a limited number of rows to return
    /// </summary>
    /// <param name="limit"></param>
    /// <returns></returns>
    public LoadOperation<TLoad, TJoin> Limit(int limit) {
        base.Limit(limit);
        return this;
    }

    /// <summary>
    /// specifies having criterias for the operation
    /// </summary>
    /// <param name="criterias"></param>
    /// <returns></returns>
    public LoadOperation<TLoad, TJoin> Having(Expression<Func<TLoad, TJoin, bool>> criterias) {
        Havings = criterias;
        return this;
    }

}

/// <summary>
/// operation used to load values of an entity
/// </summary>
/// <typeparam name="T">type of entity of which to load values</typeparam>
public class LoadOperation<T> : IDatabaseOperation {
    readonly IDBClient dbclient;
    readonly Func<Type, EntityDescriptor> descriptorgetter;
    readonly List<IDBField> columns= [];
    OrderByCriteria[] orderbycriterias;
    readonly List<IDBField> groupbycriterias= [];
    readonly List<JoinOperation> joinoperations = [];
    bool distinct;
    string alias;
    readonly List<IDatabaseOperation> unions= [];

    /// <summary>
    /// creates a new <see cref="LoadOperation{T}"/>
    /// </summary>
    /// <param name="origin">operation of which to copy existing specifications</param>
    internal LoadOperation(LoadOperation<T> origin)
        : this(origin.dbclient, origin.descriptorgetter, origin.columns) {
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
    /// creates a new <see cref="LoadOperation{T}"/>
    /// </summary>
    /// <param name="dbclient"> </param>
    /// <param name="fields">fields to load</param>
    /// <param name="descriptorgetter"></param>
    public LoadOperation(IDBClient dbclient, Func<Type, EntityDescriptor> descriptorgetter, params Expression<Func<T, object>>[] fields) {
        this.descriptorgetter = descriptorgetter;
        this.dbclient = dbclient;
        columns.AddRange(fields.Select(EntityField.Create));
    }

    /// <summary>
    /// creates a new <see cref="LoadOperation{T}"/>
    /// </summary>
    /// <param name="dbclient">client used for database access</param>
    /// <param name="fields">fields to load</param>
    /// <param name="descriptorgetter">func used to retrieve entity models</param>
    public LoadOperation(IDBClient dbclient, Func<Type, EntityDescriptor> descriptorgetter, params IDBField[] fields) {
        this.descriptorgetter = descriptorgetter;
        this.dbclient = dbclient;
        columns.AddRange(fields);
    }

    /// <summary>
    /// creates a new <see cref="LoadOperation{T}"/>
    /// </summary>
    /// <param name="dbclient">client used for database access</param>
    /// <param name="fields">fields to load</param>
    /// <param name="descriptorgetter">func used to retrieve entity models</param>
    public LoadOperation(IDBClient dbclient, Func<Type, EntityDescriptor> descriptorgetter, IEnumerable<IDBField> fields) {
        this.descriptorgetter = descriptorgetter;
        this.dbclient = dbclient;
        columns.AddRange(fields);
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
    public Clients.Tables.DataTable Execute(params object[] parameters) {
        return Prepare(false).Execute(parameters);
    }

    /// <summary>
    /// loads entities using the operation
    /// </summary>
    /// <returns></returns>
    public Clients.Tables.DataTable Execute(Transaction transaction, params object[] parameters) {
        return Prepare(false).Execute(transaction, parameters);
    }

    /// <summary>
    /// loads entities using the operation
    /// </summary>
    /// <returns></returns>
    public Task<Clients.Tables.DataTable> ExecuteAsync(params object[] parameters) {
        return Prepare(false).ExecuteAsync(parameters);
    }

    /// <summary>
    /// loads entities using the operation
    /// </summary>
    /// <returns></returns>
    public Task<Clients.Tables.DataTable> ExecuteAsync(Transaction transaction, params object[] parameters) {
        return Prepare(false).ExecuteAsync(transaction, parameters);
    }

    /// <summary>
    /// loads a value using the operation
    /// </summary>
    /// <typeparam name="TScalar">type of scalar to return</typeparam>
    /// <returns>resulting scalar of operation</returns>
    public TScalar ExecuteScalar<TScalar>(params object[] parameters) {
        return Prepare(false).ExecuteScalar<TScalar>(parameters);
    }

    /// <summary>
    /// loads a value using the operation
    /// </summary>
    /// <typeparam name="TScalar">type of scalar to return</typeparam>
    /// <param name="transaction">transaction to use (optional)</param>
    /// <param name="parameters">parameters for command</param>
    /// <returns>resulting scalar of operation</returns>
    public TScalar ExecuteScalar<TScalar>(Transaction transaction, params object[] parameters) {
        return Prepare(false).ExecuteScalar<TScalar>(transaction, parameters);
    }

    /// <summary>
    /// loads a value using the operation
    /// </summary>
    /// <typeparam name="TScalar">type of scalar to return</typeparam>
    /// <returns>resulting scalar of operation</returns>
    public Task<TScalar> ExecuteScalarAsync<TScalar>(params object[] parameters) {
        return Prepare(false).ExecuteScalarAsync<TScalar>(parameters);
    }

    /// <summary>
    /// loads a value using the operation
    /// </summary>
    /// <typeparam name="TScalar">type of scalar to return</typeparam>
    /// <param name="transaction">transaction to use (optional)</param>
    /// <param name="parameters">parameters for command</param>
    /// <returns>resulting scalar of operation</returns>
    public Task<TScalar> ExecuteScalarAsync<TScalar>(Transaction transaction, params object[] parameters) {
        return Prepare(false).ExecuteScalarAsync<TScalar>(transaction, parameters);
    }

    /// <summary>
    /// loads several values using the operation
    /// </summary>
    /// <typeparam name="TScalar">type of resulting set values</typeparam>
    /// <returns>resultset of operation</returns>
    public IEnumerable<TScalar> ExecuteSet<TScalar>(params object[] parameters) {
        return Prepare(false).ExecuteSet<TScalar>(parameters);
    }

    /// <summary>
    /// loads several values using the operation
    /// </summary>
    /// <typeparam name="TScalar">type of resulting set values</typeparam>
    /// <param name="transaction">transaction to use (optional)</param>
    /// <param name="parameters">parameters for command</param>
    /// <returns>resultset of operation</returns>
    public IEnumerable<TScalar> ExecuteSet<TScalar>(Transaction transaction, params object[] parameters) {
        return Prepare(false).ExecuteSet<TScalar>(transaction, parameters);
    }

    /// <summary>
    /// loads several values using the operation
    /// </summary>
    /// <typeparam name="TScalar">type of resulting set values</typeparam>
    /// <returns>resultset of operation</returns>
    public IAsyncEnumerable<TScalar> ExecuteSetAsync<TScalar>(params object[] parameters) {
        return Prepare(false).ExecuteSetAsync<TScalar>(parameters);
    }

    /// <summary>
    /// loads several values using the operation
    /// </summary>
    /// <typeparam name="TScalar">type of resulting set values</typeparam>
    /// <param name="transaction">transaction to use (optional)</param>
    /// <param name="parameters">parameters for command</param>
    /// <returns>resultset of operation</returns>
    public IAsyncEnumerable<TScalar> ExecuteSetAsync<TScalar>(Transaction transaction, params object[] parameters) {
        return Prepare(false).ExecuteSetAsync<TScalar>(transaction, parameters);
    }

    /// <summary>
    /// executes a query and stores the result in a custom result type
    /// </summary>
    /// <typeparam name="TType">type of result</typeparam>
    /// <param name="assignments">action used to assign values</param>
    /// <param name="parameters">parameters for command</param>
    /// <returns>enumeration of result types</returns>
    public IEnumerable<TType> ExecuteTypes<TType>(Func<Row, TType> assignments, params object[] parameters) {
        return Prepare(false).ExecuteTypes(assignments, parameters);
    }

    /// <summary>
    /// executes a query and stores the result in a custom result type
    /// </summary>
    /// <typeparam name="TType">type of result</typeparam>
    /// <param name="transaction">transaction to use for operation execution</param>
    /// <param name="assignments">action used to assign values</param>
    /// <param name="parameters">parameters for command</param>
    /// <returns>enumeration of result types</returns>
    public IEnumerable<TType> ExecuteTypes<TType>(Func<Row, TType> assignments, Transaction transaction, params object[] parameters) {
        return Prepare(false).ExecuteTypes(transaction, assignments, parameters);
    }

    /// <summary>
    /// executes a query and stores the result in a custom result type
    /// </summary>
    /// <typeparam name="TType">type of result</typeparam>
    /// <param name="assignments">action used to assign values</param>
    /// <param name="parameters">parameters for command</param>
    /// <returns>enumeration of result types</returns>
    public Task<TType> ExecuteTypeAsync<TType>(Func<Row, TType> assignments, params object[] parameters) {
        return Prepare(false).ExecuteTypeAsync(assignments, parameters);
    }

    /// <summary>
    /// executes a query and stores the result in a custom result type
    /// </summary>
    /// <typeparam name="TType">type of result</typeparam>
    /// <param name="transaction">transaction to use for operation execution</param>
    /// <param name="assignments">action used to assign values</param>
    /// <param name="parameters">parameters for command</param>
    /// <returns>enumeration of result types</returns>
    public Task<TType> ExecuteTypeAsync<TType>(Func<Row, TType> assignments, Transaction transaction, params object[] parameters) {
        return Prepare(false).ExecuteTypeAsync(transaction, assignments, parameters);
    }

    /// <summary>
    /// executes a query and stores the result in a custom result type
    /// </summary>
    /// <typeparam name="TType">type of result</typeparam>
    /// <param name="assignments">action used to assign values</param>
    /// <param name="parameters">parameters for command</param>
    /// <returns>enumeration of result types</returns>
    public IAsyncEnumerable<TType> ExecuteTypesAsync<TType>(Func<Row, TType> assignments, params object[] parameters) {
        return Prepare(false).ExecuteTypesAsync(assignments, parameters);
    }

    /// <summary>
    /// executes a query and stores the result in a custom result type
    /// </summary>
    /// <typeparam name="TType">type of result</typeparam>
    /// <param name="transaction">transaction to use for operation execution</param>
    /// <param name="assignments">action used to assign values</param>
    /// <param name="parameters">parameters for command</param>
    /// <returns>enumeration of result types</returns>
    public IAsyncEnumerable<TType> ExecuteTypesAsync<TType>(Func<Row, TType> assignments, Transaction transaction, params object[] parameters) {
        return Prepare(false).ExecuteTypesAsync(transaction, assignments, parameters);
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <typeparam name="TEntity">type of entities to create</typeparam>
    /// <param name="transaction">transaction to use</param>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public IEnumerable<TEntity> ExecuteEntities<TEntity>(Transaction transaction, params object[] parameters) {
        return Prepare(false).ExecuteEntities<TEntity>(transaction, parameters);
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <typeparam name="TEntity">type of entities to create</typeparam>
    /// <param name="transaction">transaction to use</param>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public TEntity ExecuteEntity<TEntity>(Transaction transaction, params object[] parameters) {
        if (LimitStatement?.Limit == null)
            Limit(1);
        return Prepare(false).ExecuteEntity<TEntity>(transaction, parameters);
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <typeparam name="TEntity">type of entities to create</typeparam>
    /// <param name="transaction">transaction to use</param>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public Task<TEntity> ExecuteEntityAsync<TEntity>(Transaction transaction, params object[] parameters) {
        if(LimitStatement?.Limit == null)
            Limit(1);
        return Prepare(false).ExecuteEntityAsync<TEntity>(transaction, parameters);
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <typeparam name="TEntity">type of entities to create</typeparam>
    /// <param name="transaction">transaction to use</param>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public IAsyncEnumerable<TEntity> ExecuteEntitiesAsync<TEntity>(Transaction transaction, params object[] parameters) {
        return Prepare(false).ExecuteEntitiesAsync<TEntity>(transaction, parameters);
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <typeparam name="TEntity">type of entities to create</typeparam>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public IAsyncEnumerable<TEntity> ExecuteEntitiesAsync<TEntity>(params object[] parameters) {
        return ExecuteEntitiesAsync<TEntity>(null, parameters);
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <typeparam name="TEntity">type of entities to create</typeparam>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public IEnumerable<TEntity> ExecuteEntities<TEntity>(params object[] parameters) {
        return ExecuteEntities<TEntity>(null, parameters);
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <typeparam name="TEntity">type of entities to create</typeparam>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public Task<TEntity> ExecuteEntityAsync<TEntity>(params object[] parameters) {
        return ExecuteEntityAsync<TEntity>(null, parameters);
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <typeparam name="TEntity">type of entities to create</typeparam>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public TEntity ExecuteEntity<TEntity>(params object[] parameters) {
        return ExecuteEntity<TEntity>(null, parameters);
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <param name="transaction">transaction to use</param>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public IEnumerable<T> ExecuteEntities(Transaction transaction, params object[] parameters) {
        return Prepare(false).ExecuteEntities<T>(transaction, parameters);
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <param name="transaction">transaction to use</param>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public T ExecuteEntity(Transaction transaction, params object[] parameters) {
        return Prepare(false).ExecuteEntity<T>(transaction, parameters);
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <param name="transaction">transaction to use</param>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public Task<T> ExecuteEntityAsync(Transaction transaction, params object[] parameters) {
        return Prepare(false).ExecuteEntityAsync<T>(transaction, parameters);
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <param name="transaction">transaction to use</param>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public IAsyncEnumerable<T> ExecuteEntitiesAsync(Transaction transaction, params object[] parameters) {
        return Prepare(false).ExecuteEntitiesAsync<T>(transaction, parameters);
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public IAsyncEnumerable<T> ExecuteEntitiesAsync(params object[] parameters) {
        return ExecuteEntitiesAsync<T>(null, parameters);
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public IEnumerable<T> ExecuteEntities(params object[] parameters) {
        return ExecuteEntities<T>(null, parameters);
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public Task<T> ExecuteEntityAsync(params object[] parameters) {
        return ExecuteEntityAsync<T>(null, parameters);
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public T ExecuteEntity(params object[] parameters) {
        return ExecuteEntity<T>(null, parameters);
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
    PreparedLoadOperation<T> Prepare(bool dbPrepare) {
        OperationPreparator preparator = new();
        ((IDatabaseOperation)this).Prepare(preparator);
        return preparator.GetLoadValuesOperation<T>(dbclient, descriptorgetter, dbPrepare);
    }
        
    /// <summary>
    /// prepares the operation for execution
    /// </summary>
    /// <returns>operation used to load data</returns>
    public PreparedLoadOperation<T> Prepare() {
        return Prepare(true);
    }

    /// <summary>
    /// specifies fields to be loaded
    /// </summary>
    /// <param name="fields">fields to be specified</param>
    /// <returns>this operation for fluent behavior</returns>
    public LoadOperation<T> Fields(params IDBField[] fields) {
        columns.AddRange(fields);
        return this;
    }

    /// <summary>
    /// specifies fields to be loaded
    /// </summary>
    /// <param name="fields">fields to be specified</param>
    /// <returns>this operation for fluent behavior</returns>
    public LoadOperation<T> Fields(params Expression<Func<T, object>>[] fields) {
        columns.AddRange(fields.Select(EntityField.Create));
        return this;
    }

    /// <summary>
    /// specifies fields to be loaded
    /// </summary>
    /// <param name="fields">fields to be specified</param>
    /// <returns>this operation for fluent behavior</returns>
    public LoadOperation<T> Fields<TEntity>(params Expression<Func<TEntity, object>>[] fields) {
        columns.AddRange(fields.Select(EntityField.Create));
        return this;
    }

    /// <summary>
    /// specifies fields to be loaded
    /// </summary>
    /// <param name="entityAlias">alias of table to load fields from</param>
    /// <param name="fields">fields to be specified</param>
    /// <returns>this operation for fluent behavior</returns>
    public LoadOperation<T> Fields<TEntity>(string entityAlias, params Expression<Func<TEntity, object>>[] fields) {
        columns.AddRange(fields.Select(f => DB.Property(f, entityAlias)));
        return this;
    }

    /// <summary>
    /// provides an alias to use for the operation
    /// </summary>
    /// <remarks>
    /// necessary to prevent conflicts if the loaded type is used multiple times in a complex query
    /// </remarks>
    /// <param name="tablealias">name of alias to use</param>
    /// <returns>this operation for fluent behavior</returns>
    public LoadOperation<T> Alias(string tablealias) {
        alias = tablealias;
        return this;
    }

    /// <summary>
    /// executes a union with another statement
    /// </summary>
    /// <param name="operation">operation to use as union</param>
    /// <returns>this operation for fluent behavior</returns>
    public LoadOperation<T> Union(IDatabaseOperation operation) {
        unions.Add(operation);
        return this;
    }

    /// <summary>
    /// specifies to only load rows with distinct values
    /// </summary>
    /// <returns></returns>
    public LoadOperation<T> Distinct() {
        distinct = true;
        return this;
    }

    /// <summary>
    /// specifies criterias for the operation
    /// </summary>
    /// <param name="criterias"></param>
    /// <returns></returns>
    public LoadOperation<T> Where(Expression<Func<T, bool>> criterias) {
        Criterias = criterias;
        return this;
    }

    /// <summary>
    /// specifies having criterias for the operation
    /// </summary>
    /// <param name="criterias"></param>
    /// <returns></returns>
    public LoadOperation<T> Having(Expression<Func<T, bool>> criterias) {
        Havings = criterias;
        return this;
    }

    /// <summary>
    /// specifies an order
    /// </summary>
    /// <param name="fields"></param>
    /// <returns></returns>
    public LoadOperation<T> OrderBy(params OrderByCriteria[] fields) {
        if(fields.Length == 0)
            throw new InvalidOperationException("at least one criteria has to be specified");

        orderbycriterias = fields;
        return this;
    }

    /// <summary>
    /// specifies sort criterias
    /// </summary>
    /// <param name="fields">fields to order the result set by</param>
    /// <returns>this operation for fluent behavior</returns>
    public LoadOperation<T> OrderBy(params Expression<Func<T, object>>[] fields) {
        OrderByCriteria[] criteriafields = fields.Select(f => new OrderByCriteria(Field.Property(f))).ToArray();
        return OrderBy(criteriafields);
    }

    /// <summary>
    /// specifies sort criterias
    /// </summary>
    /// <param name="fields">fields to order the result set by</param>
    /// <returns>this operation for fluent behavior</returns>
    public LoadOperation<T> OrderByDesc(params Expression<Func<T, object>>[] fields) {
        OrderByCriteria[] criteriafields = fields.Select(f => new OrderByCriteria(Field.Property(f), false)).ToArray();
        return OrderBy(criteriafields);
    }

    /// <summary>
    /// groups the results by the specified fields
    /// </summary>
    /// <param name="fields"></param>
    /// <returns></returns>
    public LoadOperation<T> GroupBy(params IDBField[] fields) {
        if(fields.Length == 0)
            throw new InvalidOperationException("at least one group criteria has to be specified");

        groupbycriterias.AddRange(fields);
        return this;
    }

    /// <summary>
    /// groups the results by the specified fields
    /// </summary>
    /// <param name="fields">fields by which to group operation</param>
    /// <returns>operation for fluid behavior</returns>
    public LoadOperation<T> GroupBy(params Expression<Func<T, object>>[] fields) {
        return GroupBy(fields.Select(Field.Property).Cast<IDBField>().ToArray());
    }

    /// <summary>
    /// specifies a limited number of rows to return
    /// </summary>
    /// <param name="limit">number of rows to return</param>
    public LoadOperation<T> Limit(long limit) {
        LimitStatement ??= new LimitField();
        LimitStatement.Limit = DB.Constant(limit);
        return this;
    }

    /// <summary>
    /// specifies a limited number of rows to return
    /// </summary>
    /// <param name="limit">number of rows to return</param>
    public LoadOperation<T> Limit(ISqlToken limit) {
        LimitStatement ??= new LimitField();
        LimitStatement.Limit = limit;
        return this;
    }

    /// <summary>
    /// specifies an offset from which on to return result rows
    /// </summary>
    /// <param name="offset">number of rows to skip</param>
    public LoadOperation<T> Offset(long offset) {
        LimitStatement ??= new LimitField();
        LimitStatement.Offset = new ConstantValue(offset);
        return this;
    }

    /// <summary>
    /// specifies an offset from which on to return result rows
    /// </summary>
    /// <param name="offset">number of rows to skip</param>
    public LoadOperation<T> Offset(ISqlToken offset) {
        LimitStatement ??= new LimitField();
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
    public LoadOperation<T, TJoin> Join<TJoin>(Expression<Func<T, TJoin, bool>> criteria, string alias = null) {
        joinoperations.Add(new JoinOperation(typeof(TJoin), criteria, JoinOp.Inner, null, alias));
        return new LoadOperation<T, TJoin>(this);
    }

    /// <summary>
    /// joins another type to the operation
    /// </summary>
    /// <typeparam name="TJoin">type to join</typeparam>
    /// <param name="criteria">join criteria</param>
    /// <param name="alias">alias to use</param>
    /// <returns>this load operation for fluent behavior</returns>
    public LoadOperation<T, TJoin> LeftJoin<TJoin>(Expression<Func<T, TJoin, bool>> criteria, string alias = null) {
        joinoperations.Add(new JoinOperation(typeof(TJoin), criteria, JoinOp.Left, null, alias));
        return new LoadOperation<T, TJoin>(this);
    }

    /// <inheritdoc />
    void IDatabaseOperation.Prepare(IOperationPreparator preparator) {
        if (columns.Count == 0)
            columns.AddRange(typeof(T).GetProperties().Select(p => Field.Property<T>(p.Name)));

        List<string> aliases = new();
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
            if (flag)
                flag = false;
            else preparator.AppendText(",");
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
        if(groupbycriterias.Count>0) {
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