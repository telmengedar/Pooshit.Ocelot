using System;
using System.Collections.Generic;
using System.Linq;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Clients.Tables;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations;

namespace Pooshit.Ocelot.Fields;

/// <inheritdoc />
public class FieldMapper<TEntity> : IFieldMapper<TEntity> {
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
    public FieldMapper(FieldMapping<TEntity>[] mappings, Action<TEntity, string[], IRowValues> initializer=null) 
    : this((IEnumerable<FieldMapping<TEntity>>)mappings, initializer)
    {
    }

    /// <summary>
    /// creates a new <see cref="FieldMapper{TEntity}"/>
    /// </summary>
    /// <param name="initializer">initializer used to initialize entities before processing mappings</param>
    /// <param name="mappings">mappings to initialize mapper with</param>
    public FieldMapper(IEnumerable<FieldMapping<TEntity>> mappings, Action<TEntity, string[], IRowValues> initializer=null) {
        this.mappings.AddRange(mappings);
        InitializeEntity = initializer;
        BuildFieldLookup();
    }

    /// <inheritdoc />
    public FieldMapping<TEntity> this[string name] {
        get {
            if (!fieldLookup.TryGetValue(name, out FieldMapping<TEntity> field))
                fieldLookup[name] = field = fieldLookup[name.ToLower()];
            return field;
        }
    }

    /// <inheritdoc />
    public IEnumerable<IDBField> DbFields => mappings.Select(m => m.Field);

    Action<TEntity, string[], IRowValues> InitializeEntity { get; }
    
    void BuildFieldLookup() {
        fieldLookup = new();
        foreach (FieldMapping<TEntity> field in mappings)
            fieldLookup[field.Name] = field;
    }

    /// <inheritdoc />
    public IEnumerable<IDBField> DbFieldsFromNames(params string[] names) {
        if (names.Length == 0)
            return DbFields;
        return DbFieldsFromNames((IEnumerable<string>)names);
    }

    /// <inheritdoc />
    public IEnumerable<IDBField> DbFieldsFromNames(IEnumerable<string> names) {
        return names.Select(n => this[n].Field);
    }

    int IndexOf(string[] fields, string fieldName) {
        int index = 0;
        if (fields?.Length > 0) {
            foreach (string field in fields) {
                if (field == fieldName)
                    return index;
                ++index;
            }
        }
        else {
            foreach (FieldMapping<TEntity> field in mappings) {
                if (field.Name == fieldName)
                    return index;
                ++index;
            }
        }

        return -1;
    }

    /// <inheritdoc />
    public TEntity EntityFromRow(DataRow row, params string[] fields) {
        TEntity entity = Activator.CreateInstance<TEntity>();
        InitializeEntity?.Invoke(entity, fields, new RowValues(row, fields, IndexOf));
        int index = 0;
        if (fields?.Length > 0) {
            foreach(string field in fields)
                this[field].SetValue(entity, row.GetValue<object>(index++));
        }
        else {
            foreach (FieldMapping<TEntity> field in mappings)
                field.SetValue(entity, row.GetValue<object>(index++));
        }

        return entity;
    }

    /// <summary>
    /// creates an entity from a loaded row
    /// </summary>
    /// <param name="reader">database result reader</param>
    /// <param name="fields">expected fields in row</param>
    /// <returns>created entity</returns>
    TEntity EntityFromReader(Reader reader, params string[] fields) {
        TEntity entity = Activator.CreateInstance<TEntity>();
        InitializeEntity?.Invoke(entity, fields, new ReaderValues(reader, fields, IndexOf));
        int index = 0;
        if (fields?.Length > 0) {
            foreach(string field in fields)
                this[field].SetValue(entity, reader.GetValue<object>(index++));
        }
        else {
            foreach (FieldMapping<TEntity> field in mappings)
                field.SetValue(entity, reader.GetValue<object>(index++));
        }

        return entity;
    }

    /// <inheritdoc />
    public IEnumerable<TEntity> EntitiesFromTable(DataTable table, params string[] fields) {
        return table.Rows.Select(r=>EntityFromRow(r, fields));
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<TEntity> EntitiesFromOperation(LoadOperation<TEntity> operation, params string[] fields) {
        using Reader reader = await operation.ExecuteReaderAsync();
        await foreach (TEntity item in EntitiesFromReader(reader, fields))
            yield return item;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<TEntity> EntitiesFromOperation<TLoad>(LoadOperation<TLoad> operation, params string[] fields) {
        using Reader reader = await operation.ExecuteReaderAsync();
        await foreach (TEntity item in EntitiesFromReader(reader, fields))
            yield return item;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<TEntity> EntitiesFromReader(Reader reader, params string[] fields) {
        while (await reader.ReadAsync())
            yield return EntityFromReader(reader, fields);
    }

    /// <inheritdoc />
    public TEntity EntityFromTable(DataTable table, params string[] fields) {
        return table.Rows.Select(r => EntityFromRow(r, fields)).FirstOrDefault();
    }

    /// <inheritdoc />
    public virtual LoadOperation<TEntity> CreateOperation(IEntityManager database, params string[] fields) {
        return database.Load<TEntity>(DbFieldsFromNames(fields).ToArray());
    }
}