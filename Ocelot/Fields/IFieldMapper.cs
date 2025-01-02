﻿using System.Collections.Generic;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Clients.Tables;
using Pooshit.Ocelot.Entities.Operations;

namespace Pooshit.Ocelot.Fields;

/// <summary>
/// mapper for fields used to load entities from database
/// </summary>
public interface IFieldMapper<TEntity> {
	
	/// <summary>
	/// access to fields by name
	/// </summary>
	/// <param name="name">name of field to get</param>
	FieldMapping<TEntity> this[string name] { get; }

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
	TEntity EntityFromRow(DataRow row, params string[] fields);

	/// <summary>
	/// create entities from table
	/// </summary>
	/// <param name="table">table from which to create entities</param>
	/// <param name="fields">expected fields in rows (optional)</param>
	/// <returns>enumeration of entities</returns>
	IEnumerable<TEntity> EntitiesFromTable(DataTable table, params string[] fields);

	/// <summary>
	/// creates entities from an operation
	/// </summary>
	/// <param name="operation">operation of which to create entities</param>
	/// <param name="fields">expected fields in rows (optional)</param>
	/// <returns>enumeration of entities</returns>
	IAsyncEnumerable<TEntity> EntitiesFromOperation(LoadOperation<TEntity> operation, params string[] fields);

	/// <summary>
	/// creates entities from an operation
	/// </summary>
	/// <param name="operation">operation of which to create entities</param>
	/// <param name="fields">expected fields in rows (optional)</param>
	/// <returns>enumeration of entities</returns>
	IAsyncEnumerable<TEntity> EntitiesFromOperation<TLoad>(LoadOperation<TLoad> operation, params string[] fields);

	/// <summary>
	/// creates entities from an operation
	/// </summary>
	/// <param name="reader">dataset result reader</param>
	/// <param name="fields">expected fields in rows (optional)</param>
	/// <returns>enumeration of entities</returns>
	IAsyncEnumerable<TEntity> EntitiesFromReader(Reader reader, params string[] fields);

	/// <summary>
	/// create entities from table
	/// </summary>
	/// <param name="table">table from which to create entities</param>
	/// <param name="fields">expected fields in rows (optional)</param>
	/// <returns>enumeration of entities</returns>
	TEntity EntityFromTable(DataTable table, params string[] fields);
}