using System.Collections.Generic;
using System.Threading.Tasks;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Clients.Tables;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations;

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
	TModel EntityFromReader(Reader reader, params string[] fields);
	
	/// <summary>
	/// creates a load operation to be used to load the entities from database
	/// </summary>
	/// <param name="database">database used to generate load operation</param>
	/// <param name="fields">selection of fields to load</param>
	/// <returns>load operation to use to load entities</returns>
	LoadOperation<TModel> CreateOperation(IEntityManager database, params string[] fields);
}

/// <summary>
/// mapper for fields used to load entities from database
/// </summary>
public interface IFieldMapper<TModel, TEntity> : IFieldMapper<TModel> {
	
	/// <summary>
	/// creates a load operation to be used to load the entities from database
	/// </summary>
	/// <param name="database">database used to generate load operation</param>
	/// <param name="fields">selection of fields to load</param>
	/// <returns>load operation to use to load entities</returns>
	new LoadOperation<TEntity> CreateOperation(IEntityManager database, params string[] fields);
}