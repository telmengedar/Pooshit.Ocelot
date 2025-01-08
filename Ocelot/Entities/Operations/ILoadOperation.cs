using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Clients.Tables;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Tokens;

namespace Pooshit.Ocelot.Entities.Operations;

/// <summary>
/// operation used to load data from database
/// </summary>
public interface ILoadOperation : IDatabaseOperation {

	/// <summary>
	/// loads entities using the operation
	/// </summary>
	/// <returns>result table</returns>
	Clients.Tables.DataTable Execute(params object[] parameters);

	/// <summary>
	/// loads entities using the operation
	/// </summary>
	/// <returns>result table</returns>
	Clients.Tables.DataTable Execute(Transaction transaction, params object[] parameters);

	/// <summary>
	/// loads entities using the operation
	/// </summary>
	/// <returns>result table</returns>
	Task<Clients.Tables.DataTable> ExecuteAsync(params object[] parameters);

	/// <summary>
	/// loads entities using the operation
	/// </summary>
	/// <returns>result table</returns>
	Task<Clients.Tables.DataTable> ExecuteAsync(Transaction transaction, params object[] parameters);

	/// <summary>
	/// loads a value using the operation
	/// </summary>
	/// <typeparam name="TScalar">type of scalar to return</typeparam>
	/// <returns>resulting scalar of operation</returns>
	TScalar ExecuteScalar<TScalar>(params object[] parameters);

	/// <summary>
	/// loads a value using the operation
	/// </summary>
	/// <typeparam name="TScalar">type of scalar to return</typeparam>
	/// <param name="transaction">transaction to use (optional)</param>
	/// <param name="parameters">parameters for command</param>
	/// <returns>resulting scalar of operation</returns>
	TScalar ExecuteScalar<TScalar>(Transaction transaction, params object[] parameters);

	/// <summary>
	/// loads a value using the operation
	/// </summary>
	/// <typeparam name="TScalar">type of scalar to return</typeparam>
	/// <returns>resulting scalar of operation</returns>
	Task<TScalar> ExecuteScalarAsync<TScalar>(params object[] parameters);

	/// <summary>
	/// loads a value using the operation
	/// </summary>
	/// <typeparam name="TScalar">type of scalar to return</typeparam>
	/// <param name="transaction">transaction to use (optional)</param>
	/// <param name="parameters">parameters for command</param>
	/// <returns>resulting scalar of operation</returns>
	Task<TScalar> ExecuteScalarAsync<TScalar>(Transaction transaction, params object[] parameters);

	/// <summary>
	/// loads several values using the operation
	/// </summary>
	/// <typeparam name="TScalar">type of resulting set values</typeparam>
	/// <returns>resultset of operation</returns>
	IEnumerable<TScalar> ExecuteSet<TScalar>(params object[] parameters);

	/// <summary>
	/// loads several values using the operation
	/// </summary>
	/// <typeparam name="TScalar">type of resulting set values</typeparam>
	/// <param name="transaction">transaction to use (optional)</param>
	/// <param name="parameters">parameters for command</param>
	/// <returns>resultset of operation</returns>
	IEnumerable<TScalar> ExecuteSet<TScalar>(Transaction transaction, params object[] parameters);

	/// <summary>
	/// loads several values using the operation
	/// </summary>
	/// <typeparam name="TScalar">type of resulting set values</typeparam>
	/// <returns>resultset of operation</returns>
	IAsyncEnumerable<TScalar> ExecuteSetAsync<TScalar>(params object[] parameters);

	/// <summary>
	/// loads several values using the operation
	/// </summary>
	/// <typeparam name="TScalar">type of resulting set values</typeparam>
	/// <param name="transaction">transaction to use (optional)</param>
	/// <param name="parameters">parameters for command</param>
	/// <returns>resultset of operation</returns>
	IAsyncEnumerable<TScalar> ExecuteSetAsync<TScalar>(Transaction transaction, params object[] parameters);

	/// <summary>
	/// executes a query and stores the result in a custom result type
	/// </summary>
	/// <typeparam name="TType">type of result</typeparam>
	/// <param name="assignments">action used to assign values</param>
	/// <param name="parameters">parameters for command</param>
	/// <returns>enumeration of result types</returns>
	IEnumerable<TType> ExecuteTypes<TType>(Func<Row, TType> assignments, params object[] parameters);

	/// <summary>
	/// executes a query and stores the result in a custom result type
	/// </summary>
	/// <typeparam name="TType">type of result</typeparam>
	/// <param name="transaction">transaction to use for operation execution</param>
	/// <param name="assignments">action used to assign values</param>
	/// <param name="parameters">parameters for command</param>
	/// <returns>enumeration of result types</returns>
	IEnumerable<TType> ExecuteTypes<TType>(Func<Row, TType> assignments, Transaction transaction, params object[] parameters);

	/// <summary>
	/// executes a query and stores the result in a custom result type
	/// </summary>
	/// <typeparam name="TType">type of result</typeparam>
	/// <param name="assignments">action used to assign values</param>
	/// <param name="parameters">parameters for command</param>
	/// <returns>enumeration of result types</returns>
	Task<TType> ExecuteTypeAsync<TType>(Func<Row, TType> assignments, params object[] parameters);

	/// <summary>
	/// executes a query and stores the result in a custom result type
	/// </summary>
	/// <typeparam name="TType">type of result</typeparam>
	/// <param name="transaction">transaction to use for operation execution</param>
	/// <param name="assignments">action used to assign values</param>
	/// <param name="parameters">parameters for command</param>
	/// <returns>enumeration of result types</returns>
	Task<TType> ExecuteTypeAsync<TType>(Func<Row, TType> assignments, Transaction transaction, params object[] parameters);

	/// <summary>
	/// executes a query and stores the result in a custom result type
	/// </summary>
	/// <typeparam name="TType">type of result</typeparam>
	/// <param name="assignments">action used to assign values</param>
	/// <param name="parameters">parameters for command</param>
	/// <returns>enumeration of result types</returns>
	IAsyncEnumerable<TType> ExecuteTypesAsync<TType>(Func<Row, TType> assignments, params object[] parameters);

	/// <summary>
	/// executes a query and stores the result in a custom result type
	/// </summary>
	/// <typeparam name="TType">type of result</typeparam>
	/// <param name="transaction">transaction to use for operation execution</param>
	/// <param name="assignments">action used to assign values</param>
	/// <param name="parameters">parameters for command</param>
	/// <returns>enumeration of result types</returns>
	IAsyncEnumerable<TType> ExecuteTypesAsync<TType>(Func<Row, TType> assignments, Transaction transaction, params object[] parameters);

	/// <summary>
	/// executes the operation and creates entities from the result
	/// </summary>
	/// <typeparam name="TEntity">type of entities to create</typeparam>
	/// <param name="transaction">transaction to use</param>
	/// <param name="parameters">parameters for execution</param>
	/// <returns>created entities</returns>
	IEnumerable<TEntity> ExecuteEntities<TEntity>(Transaction transaction, params object[] parameters);

	/// <summary>
	/// executes the operation and creates entities from the result
	/// </summary>
	/// <typeparam name="TEntity">type of entities to create</typeparam>
	/// <param name="transaction">transaction to use</param>
	/// <param name="parameters">parameters for execution</param>
	/// <returns>created entities</returns>
	TEntity ExecuteEntity<TEntity>(Transaction transaction, params object[] parameters);

	/// <summary>
	/// executes the operation and creates entities from the result
	/// </summary>
	/// <typeparam name="TEntity">type of entities to create</typeparam>
	/// <param name="transaction">transaction to use</param>
	/// <param name="parameters">parameters for execution</param>
	/// <returns>created entities</returns>
	Task<TEntity> ExecuteEntityAsync<TEntity>(Transaction transaction, params object[] parameters);

	/// <summary>
	/// executes the operation and creates entities from the result
	/// </summary>
	/// <typeparam name="TEntity">type of entities to create</typeparam>
	/// <param name="transaction">transaction to use</param>
	/// <param name="parameters">parameters for execution</param>
	/// <returns>created entities</returns>
	IAsyncEnumerable<TEntity> ExecuteEntitiesAsync<TEntity>(Transaction transaction, params object[] parameters);

	/// <summary>
	/// executes the operation and creates entities from the result
	/// </summary>
	/// <typeparam name="TEntity">type of entities to create</typeparam>
	/// <param name="parameters">parameters for execution</param>
	/// <returns>created entities</returns>
	IAsyncEnumerable<TEntity> ExecuteEntitiesAsync<TEntity>(params object[] parameters);

	/// <summary>
	/// executes the operation and creates entities from the result
	/// </summary>
	/// <typeparam name="TEntity">type of entities to create</typeparam>
	/// <param name="parameters">parameters for execution</param>
	/// <returns>created entities</returns>
	IEnumerable<TEntity> ExecuteEntities<TEntity>(params object[] parameters);

	/// <summary>
	/// executes the operation and creates entities from the result
	/// </summary>
	/// <typeparam name="TEntity">type of entities to create</typeparam>
	/// <param name="parameters">parameters for execution</param>
	/// <returns>created entities</returns>
	Task<TEntity> ExecuteEntityAsync<TEntity>(params object[] parameters);

	/// <summary>
	/// executes the operation and creates entities from the result
	/// </summary>
	/// <typeparam name="TEntity">type of entities to create</typeparam>
	/// <param name="parameters">parameters for execution</param>
	/// <returns>created entities</returns>
	TEntity ExecuteEntity<TEntity>(params object[] parameters);

	/// <summary>
	/// executes the operation returning a reader
	/// </summary>
	/// <param name="transaction">transaction to use</param>
	/// <param name="parameters">parameters for operation</param>
	/// <returns>reader to use to read command result</returns>
	IDataReader ExecuteReader(Transaction transaction, params object[] parameters);

	/// <summary>
	/// executes the operation returning a reader
	/// </summary>
	/// <param name="parameters">parameters for operation</param>
	/// <returns>reader to use to read command result</returns>
	IDataReader ExecuteReader(params object[] parameters);

	/// <summary>
	/// executes the operation returning a reader
	/// </summary>
	/// <param name="transaction">transaction to use</param>
	/// <param name="parameters">parameters for operation</param>
	/// <returns>reader to use to read command result</returns>
	Task<Reader> ExecuteReaderAsync(Transaction transaction, params object[] parameters);

	/// <summary>
	/// executes the operation returning a reader
	/// </summary>
	/// <param name="parameters">parameters for operation</param>
	/// <returns>reader to use to read command result</returns>
	Task<Reader> ExecuteReaderAsync(params object[] parameters);

	/// <summary>
	/// prepares the operation for execution
	/// </summary>
	/// <returns>operation used to load data</returns>
	PreparedLoadOperation Prepare();

	/// <summary>
	/// specifies fields to be loaded
	/// </summary>
	/// <param name="fields">fields to be specified</param>
	/// <returns>this operation for fluent behavior</returns>
	ILoadOperation Fields(params IDBField[] fields);

	/// <summary>
	/// specifies fields to be loaded
	/// </summary>
	/// <param name="fields">fields to be specified</param>
	/// <returns>this operation for fluent behavior</returns>
	ILoadOperation Fields(params Expression<Func<object>>[] fields);

	/// <summary>
	/// specifies fields to be loaded
	/// </summary>
	/// <param name="fields">fields to be specified</param>
	/// <returns>this operation for fluent behavior</returns>
	ILoadOperation Fields<TEntity>(params Expression<Func<TEntity, object>>[] fields);

	/// <summary>
	/// specifies fields to be loaded
	/// </summary>
	/// <param name="entityAlias">alias of table to load fields from</param>
	/// <param name="fields">fields to be specified</param>
	/// <returns>this operation for fluent behavior</returns>
	ILoadOperation Fields<TEntity>(string entityAlias, params Expression<Func<TEntity, object>>[] fields);

	/// <summary>
	/// provides an alias to use for the operation
	/// </summary>
	/// <remarks>
	/// necessary to prevent conflicts if the loaded type is used multiple times in a complex query
	/// </remarks>
	/// <param name="tablealias">name of alias to use</param>
	/// <returns>this operation for fluent behavior</returns>
	ILoadOperation Alias(string tablealias);

	/// <summary>
	/// executes a union with another statement
	/// </summary>
	/// <param name="operation">operation to use as union</param>
	/// <returns>this operation for fluent behavior</returns>
	ILoadOperation Union(IDatabaseOperation operation);

	/// <summary>
	/// specifies to only load rows with distinct values
	/// </summary>
	/// <returns>this operation for fluent behavior</returns>
	ILoadOperation Distinct();

	/// <summary>
	/// specifies criterias for the operation
	/// </summary>
	/// <param name="criterias"></param>
	/// <returns>this operation for fluent behavior</returns>
	ILoadOperation Where(Expression<Func<bool>> criterias);

	/// <summary>
	/// specifies having criterias for the operation
	/// </summary>
	/// <param name="criterias"></param>
	/// <returns>this operation for fluent behavior</returns>
	ILoadOperation Having(Expression<Func<bool>> criterias);

	/// <summary>
	/// specifies an order
	/// </summary>
	/// <param name="fields"></param>
	/// <returns>this operation for fluent behavior</returns>
	ILoadOperation OrderBy(params OrderByCriteria[] fields);

	/// <summary>
	/// specifies sort criterias
	/// </summary>
	/// <param name="fields">fields to order the result set by</param>
	/// <returns>this operation for fluent behavior</returns>
	ILoadOperation OrderBy(params Expression<Func<object>>[] fields);

	/// <summary>
	/// specifies sort criterias
	/// </summary>
	/// <param name="fields">fields to order the result set by</param>
	/// <returns>this operation for fluent behavior</returns>
	ILoadOperation OrderByDesc(params Expression<Func<object>>[] fields);

	/// <summary>
	/// groups the results by the specified fields
	/// </summary>
	/// <param name="fields"></param>
	/// <returns></returns>
	ILoadOperation GroupBy(params IDBField[] fields);

	/// <summary>
	/// groups the results by the specified fields
	/// </summary>
	/// <param name="fields">fields by which to group operation</param>
	/// <returns>operation for fluid behavior</returns>
	ILoadOperation GroupBy(params Expression<Func<object>>[] fields);

	/// <summary>
	/// specifies a limited number of rows to return
	/// </summary>
	/// <param name="limit">number of rows to return</param>
	ILoadOperation Limit(long limit);

	/// <summary>
	/// specifies a limited number of rows to return
	/// </summary>
	/// <param name="limit">number of rows to return</param>
	ILoadOperation Limit(ISqlToken limit);

	/// <summary>
	/// specifies an offset from which on to return result rows
	/// </summary>
	/// <param name="offset">number of rows to skip</param>
	ILoadOperation Offset(long offset);

	/// <summary>
	/// specifies an offset from which on to return result rows
	/// </summary>
	/// <param name="offset">number of rows to skip</param>
	ILoadOperation Offset(ISqlToken offset);

	/// <summary>
	/// joins another type to the operation
	/// </summary>
	/// <typeparam name="TJoin">type to join</typeparam>
	/// <param name="criteria">join criteria</param>
	/// <param name="joinAlias">alias to use</param>
	/// <returns>this load operation for fluent behavior</returns>
	ILoadOperation Join<TJoin>(Expression<Func<TJoin, bool>> criteria, string joinAlias = null);

	/// <summary>
	/// joins another type to the operation
	/// </summary>
	/// <param name="operation">operation to join</param>
	/// <param name="criteria">join criteria</param>
	/// <param name="joinAlias">alias to use</param>
	/// <returns>this load operation for fluent behavior</returns>
	ILoadOperation Join(IDatabaseOperation operation, Expression<Func<bool>> criteria, string joinAlias = null);

	/// <summary>
	/// joins another type to the operation
	/// </summary>
	/// <typeparam name="TJoin">type to join</typeparam>
	/// <param name="criteria">join criteria</param>
	/// <param name="joinAlias">alias to use</param>
	/// <returns>this load operation for fluent behavior</returns>
	ILoadOperation LeftJoin<TJoin>(Expression<Func<TJoin, bool>> criteria, string joinAlias = null);

	/// <summary>
	/// joins another type to the operation
	/// </summary>
	/// <param name="operation">operation to join</param>
	/// <param name="criteria">join criteria</param>
	/// <param name="joinAlias">alias to use</param>
	/// <returns>this load operation for fluent behavior</returns>
	ILoadOperation LeftJoin(IDatabaseOperation operation, Expression<Func<bool>> criteria, string joinAlias = null);
}