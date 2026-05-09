using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Clients.Tables;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Tokens;
using Pooshit.Ocelot.Tokens.Partitions;

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
    /// <param name="postprocess">action to execute after entity was filled with data</param>
    public FieldMapper(FieldMapping<TModel>[] mappings, Action<TModel, string[], IRowValues> initializer=null, Action<TModel, string[]> postprocess=null) 
    : this((IEnumerable<FieldMapping<TModel>>)mappings, initializer, postprocess)
    {
    }

    /// <summary>
    /// creates a new <see cref="FieldMapper{TEntity}"/>
    /// </summary>
    /// <param name="initializer">initializer used to initialize entities before processing mappings</param>
    /// <param name="mappings">mappings to initialize mapper with</param>
    /// <param name="postprocess">action to execute after entity was filled with data</param>
    public FieldMapper(IEnumerable<FieldMapping<TModel>> mappings, Action<TModel, string[], IRowValues> initializer=null, Action<TModel, string[]> postprocess=null) {
        this.mappings.AddRange(mappings);
        InitializeEntity = initializer;
        PostProcessEntity = postprocess;
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

    /// <inheritdoc />
    public IEnumerable<string> FieldNames => fieldLookup.Keys;
    
    Action<TModel, string[], IRowValues> InitializeEntity { get; }
    
    Action<TModel, string[]> PostProcessEntity { get; }
    
    void BuildFieldLookup() {
        fieldLookup = new();
        foreach (FieldMapping<TModel> field in mappings)
            fieldLookup[field.Name] = field;
    }

    /// <inheritdoc />
    public IEnumerable<IDBField> DbFieldsFromNames(params string[] names) {
        if ((names?.Length??0) == 0)
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

        PostProcessEntity?.Invoke(entity, fields);
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
        if (!await reader.ReadAsync())
            return default;
        return EntityFromCurrentRow(reader, fields);
    }

    /// <inheritdoc />
    public TModel EntityFromCurrentRow(Reader reader, params string[] fields) {
        TModel entity = Activator.CreateInstance<TModel>();
        InitializeEntity?.Invoke(entity, fields, new ReaderValues(reader, fields, IndexOf));
        int index = 0;
        if (fields?.Length > 0) {
            foreach (string field in fields)
                this[field].SetValue(entity, reader.GetValue<object>(index++));
        }
        else {
            foreach (FieldMapping<TModel> field in mappings)
                field.SetValue(entity, reader.GetValue<object>(index++));
        }
        PostProcessEntity?.Invoke(entity, fields);
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
            yield return EntityFromCurrentRow(reader, fields);
    }

    /// <inheritdoc />
    public TModel EntityFromTable(DataTable table, params string[] fields) {
        return table.Rows.Select(r => EntityFromRow(r, fields)).FirstOrDefault();
    }

    /// <inheritdoc />
    public Task<WindowResult<TModel, TWindow>> WindowedFromOperation<TWindow>(LoadOperation<TModel> operation, WindowedAggregate windowedAggregate, CancellationToken cancellationToken = default, params string[] fields) {
        return WindowedFromOperation<TWindow, TModel>(operation, windowedAggregate, cancellationToken, fields);
    }

    /// <inheritdoc />
    public async Task<WindowResult<TModel, TWindow>> WindowedFromOperation<TWindow, TLoad>(LoadOperation<TLoad> operation, WindowedAggregate windowedAggregate, CancellationToken cancellationToken = default, params string[] fields) {
        if (windowedAggregate == null)
            throw new ArgumentNullException(nameof(windowedAggregate));

        cancellationToken.ThrowIfCancellationRequested();

        // Resolve fields: if none supplied, use all mapper field names for deterministic positional mapping
        string[] resolvedFields = fields?.Length > 0 ? fields : FieldNames.ToArray();

        PreparedLoadOperation<TLoad> prepared = operation.Prepare(false);
        WindowReader<TWindow> windowReader = await prepared.ExecuteWindowedReaderAsync<TWindow>(windowedAggregate, cancellationToken);

        // Eagerly read row 1 so the WindowedReader proxy can resolve WindowValue and WindowOrdinal
        bool hasFirstRow;
        try {
            hasFirstRow = await windowReader.Reader.ReadAsync(cancellationToken);
        }
        catch {
            windowReader.Reader.Dispose();
            throw;
        }

        // Zero-row branch
        if (!hasFirstRow) {
            windowReader.Reader.Dispose();
            return new WindowResult<TModel, TWindow>(EmptySequence(), windowReader.WindowValue);
        }

        // Materialize first row (WindowedReader has already resolved WindowOrdinal)
        TModel firstEntity = EntityFromCurrentRow(windowReader.Reader, resolvedFields);

        // SQLite (single-connection) buffering branch
        if (!prepared.DBClient.DBInfo.MultipleConnectionsSupported) {
            List<TModel> buffer = [firstEntity];
            try {
                while (await windowReader.Reader.ReadAsync(cancellationToken))
                    buffer.Add(EntityFromCurrentRow(windowReader.Reader, resolvedFields));
            }
            catch {
                windowReader.Reader.Dispose();
                throw;
            }

            windowReader.Reader.Dispose();
            return new WindowResult<TModel, TWindow>(BufferToAsyncEnumerable(buffer), windowReader.WindowValue);
        }

        // Multi-connection streaming branch
        return new WindowResult<TModel, TWindow>(
            StreamRemaining(windowReader.Reader, firstEntity, resolvedFields, cancellationToken),
            windowReader.WindowValue);
    }

    /// <inheritdoc />
    public Task<WindowResult<TModel, long>> PagedFromOperation(LoadOperation<TModel> operation, int limit, int offset, CancellationToken cancellationToken = default, params string[] fields) {
        return PagedFromOperation<TModel>(operation, limit, offset, cancellationToken, fields);
    }

    /// <inheritdoc />
    public Task<WindowResult<TModel, long>> PagedFromOperation<TLoad>(LoadOperation<TLoad> operation, int limit, int offset, CancellationToken cancellationToken = default, params string[] fields) {
        if (limit < 0)
            throw new ArgumentOutOfRangeException(nameof(limit), "limit must be >= 0");
        if (offset < 0)
            throw new ArgumentOutOfRangeException(nameof(offset), "offset must be >= 0");
        operation.Limit(limit).Offset(offset);
        return WindowedFromOperation<long, TLoad>(operation, DB.CountOver(), cancellationToken, fields);
    }

    static async IAsyncEnumerable<TModel> EmptySequence() {
        await Task.CompletedTask;
        yield break;
    }

    static async IAsyncEnumerable<TModel> BufferToAsyncEnumerable(List<TModel> items) {
        foreach (TModel item in items)
            yield return item;
        await Task.CompletedTask;
    }

    async IAsyncEnumerable<TModel> StreamRemaining(Reader reader, TModel firstEntity, string[] fields, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken) {
        using (reader) {
            yield return firstEntity;
            while (await reader.ReadAsync(cancellationToken)) {
                cancellationToken.ThrowIfCancellationRequested();
                yield return EntityFromCurrentRow(reader, fields);
            }
        }
    }

    /// <inheritdoc />
    public LoadOperation<TModel> CreateOperation(IEntityManager database) {
        return CreateOperation(database, DbFields.ToArray());
    }

    /// <inheritdoc />
    public LoadOperation<TModel> CreateOperation(IEntityManager database, params string[] fields) {
        return CreateOperation(database, DbFieldsFromNames(fields).ToArray());
    }
    
    /// <inheritdoc />
    public virtual LoadOperation<TModel> CreateOperation(IEntityManager database, params IDBField[] fields) {
        return database.Load<TModel>(fields);
    }

    /// <inheritdoc />
    public virtual string[] DefaultFields => [];

    /// <inheritdoc />
    public virtual string[] DefaultListFields => [];
}

/// <inheritdoc cref="IFieldMapper{TModel,TEntity}"/> />
public abstract class FieldMapper<TModel, TEntity> : FieldMapper<TModel>, IFieldMapper<TModel, TEntity> {
    
    /// <inheritdoc />
    protected FieldMapper(params FieldMapping<TModel>[] mappings) : base(mappings) { }

    /// <inheritdoc />
    protected FieldMapper(FieldMapping<TModel>[] mappings, Action<TModel, string[], IRowValues> initializer = null, Action<TModel, string[]> postprocess=null) 
        : base(mappings, initializer, postprocess) { }

    /// <inheritdoc />
    protected FieldMapper(IEnumerable<FieldMapping<TModel>> mappings, Action<TModel, string[], IRowValues> initializer = null, Action<TModel, string[]> postprocess=null) 
        : base(mappings, initializer, postprocess) { }

    /// <inheritdoc />
    public new LoadOperation<TEntity> CreateOperation(IEntityManager database) {
        return CreateOperation(database,  DbFields.ToArray());
    }

    /// <inheritdoc />
    public new LoadOperation<TEntity> CreateOperation(IEntityManager database, params string[] fields) {
        return CreateOperation(database, DbFieldsFromNames(fields).ToArray());
    }

    /// <inheritdoc />
    public new virtual LoadOperation<TEntity> CreateOperation(IEntityManager database, params IDBField[] fields) {
        return database.Load<TEntity>(fields);
    }
}