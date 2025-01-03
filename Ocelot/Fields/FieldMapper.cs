using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Clients.Tables;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations;

namespace Pooshit.Ocelot.Fields;

/// <inheritdoc />
public class FieldMapper<TModel> : IFieldMapper<TModel> {
    readonly List<FieldMapping<TModel>> mappings = [];
    Dictionary<string, FieldMapping<TModel>> fieldLookup;

    /// <summary>
    /// creates a new <see cref="FieldMapper{TEntity}"/>
    /// </summary>
    /// <param name="mappings">mappings to initialize mapper with</param>
    public FieldMapper(params FieldMapping<TModel>[] mappings) 
    : this((IEnumerable<FieldMapping<TModel>>)mappings)
    {
    }

    /// <summary>
    /// creates a new <see cref="FieldMapper{TEntity}"/>
    /// </summary>
    /// <param name="mappings">mappings to initialize mapper with</param>
    /// <param name="initializer">initializer used to initialize entities before processing mappings</param>
    public FieldMapper(FieldMapping<TModel>[] mappings, Action<TModel, string[], IRowValues> initializer=null) 
    : this((IEnumerable<FieldMapping<TModel>>)mappings, initializer)
    {
    }

    /// <summary>
    /// creates a new <see cref="FieldMapper{TEntity}"/>
    /// </summary>
    /// <param name="initializer">initializer used to initialize entities before processing mappings</param>
    /// <param name="mappings">mappings to initialize mapper with</param>
    public FieldMapper(IEnumerable<FieldMapping<TModel>> mappings, Action<TModel, string[], IRowValues> initializer=null) {
        this.mappings.AddRange(mappings);
        InitializeEntity = initializer;
        BuildFieldLookup();
    }

    /// <inheritdoc />
    public FieldMapping<TModel> this[string name] {
        get {
            if (!fieldLookup.TryGetValue(name, out FieldMapping<TModel> field))
                fieldLookup[name] = field = fieldLookup[name.ToLower()];
            return field;
        }
    }

    /// <inheritdoc />
    public IEnumerable<IDBField> DbFields => mappings.Select(m => m.Field);

    Action<TModel, string[], IRowValues> InitializeEntity { get; }
    
    void BuildFieldLookup() {
        fieldLookup = new();
        foreach (FieldMapping<TModel> field in mappings)
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
            foreach (FieldMapping<TModel> field in mappings) {
                if (field.Name == fieldName)
                    return index;
                ++index;
            }
        }

        return -1;
    }

    /// <inheritdoc />
    public TModel EntityFromRow(DataRow row, params string[] fields) {
        TModel entity = Activator.CreateInstance<TModel>();
        InitializeEntity?.Invoke(entity, fields, new RowValues(row, fields, IndexOf));
        int index = 0;
        if (fields?.Length > 0) {
            foreach(string field in fields)
                this[field].SetValue(entity, row.GetValue<object>(index++));
        }
        else {
            foreach (FieldMapping<TModel> field in mappings)
                field.SetValue(entity, row.GetValue<object>(index++));
        }

        return entity;
    }

    /// <inheritdoc />
    public async Task<TModel> EntityFromOperation(LoadOperation<TModel> operation, params string[] fields) {
        using Reader reader = await operation.ExecuteReaderAsync();
        return await EntityFromReader(reader, fields);
    }

    /// <inheritdoc />
    public async Task<TModel> EntityFromOperation<TLoad>(LoadOperation<TLoad> operation, params string[] fields) {
        using Reader reader = await operation.ExecuteReaderAsync();
        return await EntityFromReader(reader, fields);
    }

    /// <inheritdoc />
    public async Task<TModel> EntityFromReader(Reader reader, params string[] fields) {
        if(!await reader.ReadAsync())
            return default;
        return CreateEntityFromReader(reader, fields);
    }
    
    TModel CreateEntityFromReader(Reader reader, params string[] fields) {
        TModel entity = Activator.CreateInstance<TModel>();
        InitializeEntity?.Invoke(entity, fields, new ReaderValues(reader, fields, IndexOf));
        int index = 0;
        if (fields?.Length > 0) {
            foreach(string field in fields)
                this[field].SetValue(entity, reader.GetValue<object>(index++));
        }
        else {
            foreach (FieldMapping<TModel> field in mappings)
                field.SetValue(entity, reader.GetValue<object>(index++));
        }

        return entity;
    }

    /// <inheritdoc />
    public IEnumerable<TModel> EntitiesFromTable(DataTable table, params string[] fields) {
        return table.Rows.Select(r=>EntityFromRow(r, fields));
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<TModel> EntitiesFromOperation(LoadOperation<TModel> operation, params string[] fields) {
        using Reader reader = await operation.ExecuteReaderAsync();
        await foreach (TModel item in EntitiesFromReader(reader, fields))
            yield return item;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<TModel> EntitiesFromOperation<TLoad>(LoadOperation<TLoad> operation, params string[] fields) {
        using Reader reader = await operation.ExecuteReaderAsync();
        await foreach (TModel item in EntitiesFromReader(reader, fields))
            yield return item;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<TModel> EntitiesFromReader(Reader reader, params string[] fields) {
        while (await reader.ReadAsync())
            yield return CreateEntityFromReader(reader, fields);
    }

    /// <inheritdoc />
    public TModel EntityFromTable(DataTable table, params string[] fields) {
        return table.Rows.Select(r => EntityFromRow(r, fields)).FirstOrDefault();
    }

    /// <inheritdoc />
    public virtual LoadOperation<TModel> CreateOperation(IEntityManager database, params string[] fields) {
        return database.Load<TModel>(DbFieldsFromNames(fields).ToArray());
    }
}

/// <inheritdoc cref="IFieldMapper{TModel,TEntity}"/> />
public abstract class FieldMapper<TModel, TEntity> : FieldMapper<TModel>, IFieldMapper<TModel, TEntity> {
    
    /// <inheritdoc />
    protected FieldMapper(params FieldMapping<TModel>[] mappings) : base(mappings) { }

    /// <inheritdoc />
    protected FieldMapper(FieldMapping<TModel>[] mappings, Action<TModel, string[], IRowValues> initializer = null) : base(mappings, initializer) { }

    /// <inheritdoc />
    protected FieldMapper(IEnumerable<FieldMapping<TModel>> mappings, Action<TModel, string[], IRowValues> initializer = null) : base(mappings, initializer) { }

    /// <inheritdoc />
    public new abstract LoadOperation<TEntity> CreateOperation(IEntityManager database, params string[] fields);
}