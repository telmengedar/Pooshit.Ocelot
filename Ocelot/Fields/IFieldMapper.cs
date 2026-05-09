using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Clients.Tables;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Tokens.Partitions;

namespace Pooshit.Ocelot.Fields;

/// <summary>
/// mapper for fields used to load entities from database
/// </summary>
public interface IFieldMapper<TModel> {
	
	/// <summary>
	/// access to fields by name
	/// </summary>
	/// <param name="name">name of field to get</param>
	FieldMapping<TModel> this[string name] { get; }

	/// <summary>
	/// referenced db fields of contained field mappings
	/// </summary>
	IEnumerable<IDBField> DbFields { get; }

	/// <summary>
	/// names of fields
	/// </summary>
	IEnumerable<string> FieldNames { get; }
	
	/// <summary>
	/// get fields from names
	/// </summary>
	/// <param name="names">field names</param>
	/// <returns>db fields</returns>
	IEnumerable<IDBField> DbFieldsFromNames(params string[] names);

	/// <summary>
	/// get fields from names
	/// </summary>
	/// <param name="names">field names</param>
	/// <returns>db fields</returns>
	IEnumerable<IDBField> DbFieldsFromNames(IEnumerable<string> names);

	/// <summary>
	/// creates an entity from a loaded row
	/// </summary>
	/// <param name="row">database row loaded from field mapping of this mapper</param>
	/// <param name="fields">expected fields in row</param>
	/// <returns>created entity</returns>
	TModel EntityFromRow(DataRow row, params string[] fields);

	/// <summary>
	/// create entities from table
	/// </summary>
	/// <param name="table">table from which to create entities</param>
	/// <param name="fields">expected fields in rows (optional)</param>
	/// <returns>enumeration of entities</returns>
	IEnumerable<TModel> EntitiesFromTable(DataTable table, params string[] fields);

	/// <summary>
	/// creates entities from an operation
	/// </summary>
	/// <param name="operation">operation of which to create entities</param>
	/// <param name="fields">expected fields in rows (optional)</param>
	/// <returns>enumeration of entities</returns>
	IAsyncEnumerable<TModel> EntitiesFromOperation(LoadOperation<TModel> operation, params string[] fields);

	/// <summary>
	/// creates entities from an operation
	/// </summary>
	/// <param name="operation">operation of which to create entities</param>
	/// <param name="fields">expected fields in rows (optional)</param>
	/// <returns>enumeration of entities</returns>
	IAsyncEnumerable<TModel> EntitiesFromOperation<TLoad>(LoadOperation<TLoad> operation, params string[] fields);

	/// <summary>
	/// creates entities from an operation
	/// </summary>
	/// <param name="reader">dataset result reader</param>
	/// <param name="fields">expected fields in rows (optional)</param>
	/// <returns>enumeration of entities</returns>
	IAsyncEnumerable<TModel> EntitiesFromReader(Reader reader, params string[] fields);

	/// <summary>
	/// create entities from table
	/// </summary>
	/// <param name="table">table from which to create entities</param>
	/// <param name="fields">expected fields in rows (optional)</param>
	/// <returns>enumeration of entities</returns>
	TModel EntityFromTable(DataTable table, params string[] fields);

	/// <summary>
	/// creates entities from an operation
	/// </summary>
	/// <param name="operation">operation of which to create entities</param>
	/// <param name="fields">expected fields in rows (optional)</param>
	/// <returns>enumeration of entities</returns>
	Task<TModel> EntityFromOperation(LoadOperation<TModel> operation, params string[] fields);

	/// <summary>
	/// creates entities from an operation
	/// </summary>
	/// <param name="operation">operation of which to create entities</param>
	/// <param name="fields">expected fields in rows (optional)</param>
	/// <returns>enumeration of entities</returns>
	Task<TModel> EntityFromOperation<TLoad>(LoadOperation<TLoad> operation, params string[] fields);

	/// <summary>
	/// creates entities from an operation
	/// </summary>
	/// <param name="reader">dataset result reader</param>
	/// <param name="fields">expected fields in rows (optional)</param>
	/// <returns>enumeration of entities</returns>
	Task<TModel> EntityFromReader(Reader reader, params string[] fields);

	/// <summary>
	/// materializes an entity from the reader's current row position without advancing the reader.
	/// The caller must have already called <c>ReadAsync</c> and confirmed a row is available.
	/// </summary>
	/// <param name="reader">reader already positioned on a valid row</param>
	/// <param name="fields">expected fields in the row (optional; uses all mapper fields if empty)</param>
	/// <returns>the entity built from the current row</returns>
	TModel EntityFromCurrentRow(Reader reader, params string[] fields);

	/// <summary>
	/// loads entities and a windowed aggregate value from a single SQL statement.
	/// </summary>
	/// <typeparam name="TWindow">type of the windowed aggregate value</typeparam>
	/// <param name="operation">the load operation to execute</param>
	/// <param name="windowedAggregate">windowed aggregate to inject into the projection</param>
	/// <param name="cancellationToken">token used to cancel the operation</param>
	/// <param name="fields">expected fields in rows (optional; uses all mapper fields if empty)</param>
	/// <returns>
	/// a <see cref="WindowResult{TModel,TWindow}"/> whose <see cref="WindowResult{TModel,TWindow}.WindowValue"/>
	/// resolves without a second SQL round trip
	/// </returns>
	Task<WindowResult<TModel, TWindow>> WindowedFromOperation<TWindow>(LoadOperation<TModel> operation, WindowedAggregate windowedAggregate, CancellationToken cancellationToken = default, params string[] fields);

	/// <summary>
	/// loads entities and a windowed aggregate value from a single SQL statement.
	/// </summary>
	/// <typeparam name="TWindow">type of the windowed aggregate value</typeparam>
	/// <typeparam name="TLoad">entity type used for the load operation</typeparam>
	/// <param name="operation">the load operation to execute</param>
	/// <param name="windowedAggregate">windowed aggregate to inject into the projection</param>
	/// <param name="cancellationToken">token used to cancel the operation</param>
	/// <param name="fields">expected fields in rows (optional; uses all mapper fields if empty)</param>
	/// <returns>
	/// a <see cref="WindowResult{TModel,TWindow}"/> whose <see cref="WindowResult{TModel,TWindow}.WindowValue"/>
	/// resolves without a second SQL round trip
	/// </returns>
	Task<WindowResult<TModel, TWindow>> WindowedFromOperation<TWindow, TLoad>(LoadOperation<TLoad> operation, WindowedAggregate windowedAggregate, CancellationToken cancellationToken = default, params string[] fields);

	/// <summary>
	/// loads a page of entities and the total unfiltered count from a single SQL statement.
	/// Sugar over <see cref="WindowedFromOperation{TWindow}"/> using <c>DB.CountOver()</c>.
	/// </summary>
	/// <param name="operation">the load operation to execute</param>
	/// <param name="limit">number of rows to return (must be &gt;= 0)</param>
	/// <param name="offset">number of rows to skip (must be &gt;= 0)</param>
	/// <param name="cancellationToken">token used to cancel the operation</param>
	/// <param name="fields">expected fields in rows (optional; uses all mapper fields if empty)</param>
	/// <returns>
	/// a <see cref="WindowResult{TModel,long}"/> whose <see cref="WindowResult{TModel,long}.WindowValue"/>
	/// resolves to the total unfiltered row count
	/// </returns>
	Task<WindowResult<TModel, long>> PagedFromOperation(LoadOperation<TModel> operation, int limit, int offset, CancellationToken cancellationToken = default, params string[] fields);

	/// <summary>
	/// loads a page of entities and the total unfiltered count from a single SQL statement.
	/// Sugar over <see cref="WindowedFromOperation{TWindow,TLoad}"/> using <c>DB.CountOver()</c>.
	/// </summary>
	/// <typeparam name="TLoad">entity type used for the load operation</typeparam>
	/// <param name="operation">the load operation to execute</param>
	/// <param name="limit">number of rows to return (must be &gt;= 0)</param>
	/// <param name="offset">number of rows to skip (must be &gt;= 0)</param>
	/// <param name="cancellationToken">token used to cancel the operation</param>
	/// <param name="fields">expected fields in rows (optional; uses all mapper fields if empty)</param>
	/// <returns>
	/// a <see cref="WindowResult{TModel,long}"/> whose <see cref="WindowResult{TModel,long}.WindowValue"/>
	/// resolves to the total unfiltered row count
	/// </returns>
	Task<WindowResult<TModel, long>> PagedFromOperation<TLoad>(LoadOperation<TLoad> operation, int limit, int offset, CancellationToken cancellationToken = default, params string[] fields);

	/// <summary>
	/// creates a load operation to be used to load the entities from database
	/// </summary>
	/// <param name="database">database used to generate load operation</param>
	/// <returns>load operation to use to load entities</returns>
	LoadOperation<TModel> CreateOperation(IEntityManager database);

	/// <summary>
	/// creates a load operation to be used to load the entities from database
	/// </summary>
	/// <param name="database">database used to generate load operation</param>
	/// <param name="fields">selection of fields to load</param>
	/// <returns>load operation to use to load entities</returns>
	LoadOperation<TModel> CreateOperation(IEntityManager database, params string[] fields);

	/// <summary>
	/// creates a load operation to be used to load the entities from database
	/// </summary>
	/// <param name="database">database used to generate load operation</param>
	/// <param name="fields">selection of fields to load</param>
	/// <returns>load operation to use to load entities</returns>
	LoadOperation<TModel> CreateOperation(IEntityManager database, params IDBField[] fields);

	/// <summary>
	/// default fields to load when loading single items
	/// </summary>
	public string[] DefaultFields { get; }

	/// <summary>
	/// default fields to load when loading list items
	/// </summary>
	public string[] DefaultListFields { get; }
}

/// <summary>
/// mapper for fields used to load entities from database
/// </summary>
public interface IFieldMapper<TModel, TEntity> : IFieldMapper<TModel> {

	/// <summary>
	/// creates a load operation to be used to load the entities from database
	/// </summary>
	/// <param name="database">database used to generate load operation</param>
	/// <returns>load operation to use to load entities</returns>
	new LoadOperation<TEntity> CreateOperation(IEntityManager database);

	/// <summary>
	/// creates a load operation to be used to load the entities from database
	/// </summary>
	/// <param name="database">database used to generate load operation</param>
	/// <param name="fields">selection of fields to load</param>
	/// <returns>load operation to use to load entities</returns>
	new LoadOperation<TEntity> CreateOperation(IEntityManager database, params string[] fields);

	/// <summary>
	/// creates a load operation to be used to load the entities from database
	/// </summary>
	/// <param name="database">database used to generate load operation</param>
	/// <param name="fields">selection of fields to load</param>
	/// <returns>load operation to use to load entities</returns>
	new LoadOperation<TEntity> CreateOperation(IEntityManager database, params IDBField[] fields);
}