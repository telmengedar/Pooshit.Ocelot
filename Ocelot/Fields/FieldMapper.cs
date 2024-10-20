using System;
using System.Collections.Generic;
using System.Linq;
using Pooshit.Ocelot.Clients.Tables;

namespace Pooshit.Ocelot.Fields;

/// <summary>
/// mapper for fields
/// </summary>
public class FieldMapper<TEntity> {
    readonly List<FieldMapping<TEntity>> mappings = [];
    Dictionary<string, FieldMapping<TEntity>> fieldLookup;

    /// <summary>
    /// creates a new <see cref="FieldMapper{TEntity}"/>
    /// </summary>
    /// <param name="mappings">mappings to initialize mapper with</param>
    public FieldMapper(params FieldMapping<TEntity>[] mappings) 
    : this((IEnumerable<FieldMapping<TEntity>>)mappings)
    {
    }

    /// <summary>
    /// creates a new <see cref="FieldMapper{TEntity}"/>
    /// </summary>
    /// <param name="mappings">mappings to initialize mapper with</param>
    /// <param name="initializer">initializer used to initialize entities before processing mappings</param>
    public FieldMapper(FieldMapping<TEntity>[] mappings, Action<TEntity, string[], Func<string, object>> initializer=null) 
    : this((IEnumerable<FieldMapping<TEntity>>)mappings, initializer)
    {
    }

    /// <summary>
    /// creates a new <see cref="FieldMapper{TEntity}"/>
    /// </summary>
    /// <param name="initializer">initializer used to initialize entities before processing mappings</param>
    /// <param name="mappings">mappings to initialize mapper with</param>
    public FieldMapper(IEnumerable<FieldMapping<TEntity>> mappings, Action<TEntity, string[], Func<string, object>> initializer=null) {
        this.mappings.AddRange(mappings);
        InitializeEntity = initializer;
        BuildFieldLookup();
    }
    
    /// <summary>
    /// access to fields by name
    /// </summary>
    /// <param name="name"></param>
    public FieldMapping<TEntity> this[string name] => fieldLookup[name];

    /// <summary>
    /// referenced db fields of contained field mappings
    /// </summary>
    public IEnumerable<IDBField> DbFields => mappings.Select(m => m.Field);

    Action<TEntity, string[], Func<string,object>> InitializeEntity { get; }
    
    void BuildFieldLookup() {
        fieldLookup = new();
        foreach (FieldMapping<TEntity> field in mappings)
            fieldLookup[field.Name] = field;
    }

    /// <summary>
    /// get fields from names
    /// </summary>
    /// <param name="names">field names</param>
    /// <returns>db fields</returns>
    public IEnumerable<IDBField> DbFieldsFromNames(params string[] names) {
        return DbFieldsFromNames((IEnumerable<string>)names);
    }

    /// <summary>
    /// get fields from names
    /// </summary>
    /// <param name="names">field names</param>
    /// <returns>db fields</returns>
    public IEnumerable<IDBField> DbFieldsFromNames(IEnumerable<string> names) {
        return names.Select(n => this[n].Field);
    }

    int IndexOf(string[] fields, string fieldName) {
        int index = 0;
        foreach (string field in fields) {
            if (field == fieldName)
                return index;
            ++index;
        }

        return -1;
    }
    
    /// <summary>
    /// creates an entity from a loaded row
    /// </summary>
    /// <param name="row">database row loaded from field mapping of this mapper</param>
    /// <param name="fields">expected fields in row</param>
    /// <returns>created entity</returns>
    public TEntity EntityFromRow(DataRow row, params string[] fields) {
        TEntity entity = Activator.CreateInstance<TEntity>();
        InitializeEntity?.Invoke(entity, fields, fieldName => row.GetValue<object>(IndexOf(fields, fieldName)));
        int index = 0;
        if (fields.Length == 0) {
            foreach (FieldMapping<TEntity> field in mappings)
                field.SetValue(entity, row.GetValue<object>(index++));
        }
        else {
            foreach(string field in fields)
                this[field].SetValue(entity, row.GetValue<object>(index++));
        }

        return entity;
    }

    /// <summary>
    /// create entities from table
    /// </summary>
    /// <param name="table">table from which to create entities</param>
    /// <param name="fields">expected fields in rows (optional)</param>
    /// <returns>enumeration of entities</returns>
    public IEnumerable<TEntity> EntitiesFromTable(DataTable table, params string[] fields) {
        return table.Rows.Select(r=>EntityFromRow(r, fields));
    }

    /// <summary>
    /// create entities from table
    /// </summary>
    /// <param name="table">table from which to create entities</param>
    /// <param name="fields">expected fields in rows (optional)</param>
    /// <returns>enumeration of entities</returns>
    public TEntity EntityFromTable(DataTable table, params string[] fields) {
        return table.Rows.Select(r => EntityFromRow(r, fields)).FirstOrDefault();
    }

}