using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Clients.Tables;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Extensions;
using Pooshit.Ocelot.Extern;
using Converter = Pooshit.Ocelot.Extern.Converter;
using DataTable = Pooshit.Ocelot.Clients.Tables.DataTable;

namespace Pooshit.Ocelot.Entities.Operations.Prepared;

/// <summary>
/// a prepared load values operation
/// </summary>
public class PreparedLoadOperation : PreparedOperation {
    readonly Func<Type, EntityDescriptor> modelcache;

    /// <summary>
    /// access to the entity model cache (for use by typed subclasses)
    /// </summary>
    protected Func<Type, EntityDescriptor> ModelDescriptorCache => modelcache;

    /// <summary>
    /// creates a new <see cref="PreparedLoadOperation"/>
    /// </summary>
    /// <param name="dbclient">database access</param>
    /// <param name="modelcache">cache of entity models</param>
    /// <param name="commandtext">command text</param>
    /// <param name="parameters">parameters for operation</param>
    /// <param name="dbprepare">indicates whether to prepare statement at db aswell</param>
    public PreparedLoadOperation(IDBClient dbclient, Func<Type, EntityDescriptor> modelcache, string commandtext, object[] parameters, bool dbprepare)
        : base(dbclient, commandtext, parameters, dbprepare) {
        this.modelcache = modelcache;
    }

    /// <summary>
    /// executes the statement
    /// </summary>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>data table containing results</returns>
    public new virtual DataTable Execute(params object[] parameters) {
        return Execute(null, parameters);
    }

    /// <summary>
    /// executes the statement
    /// </summary>
    /// <param name="transaction">transaction used to execute operation</param>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>data table containing results</returns>
    public new virtual DataTable Execute(Transaction transaction, params object[] parameters) {
        if (DBPrepare && DBClient.DBInfo.PreparationSupported)
            return DBClient.QueryPrepared(transaction, CommandText, ConstantParameters.Concat(parameters));
        return DBClient.Query(transaction, CommandText, ConstantParameters.Concat(parameters));
    }

    /// <summary>
    /// executes the statement returning a scalar
    /// </summary>
    /// <returns>first value of the result set or default of TScalar</returns>
    public virtual TScalar ExecuteScalar<TScalar>(params object[] parameters) {
        return ExecuteScalar<TScalar>(null, parameters);
    }

    /// <summary>
    /// executes the statement returning a scalar
    /// </summary>
    /// <returns>first value of the result set or default of TScalar</returns>
    public virtual TScalar ExecuteScalar<TScalar>(Transaction transaction, params object[] parameters) {
        if(DBPrepare && DBClient.DBInfo.PreparationSupported)
            return Converter.Convert<TScalar>(DBClient.ScalarPrepared(transaction, CommandText, ConstantParameters.Concat(parameters)), true);
        return Converter.Convert<TScalar>(DBClient.Scalar(transaction, CommandText, ConstantParameters.Concat(parameters)), true);
    }

    /// <summary>
    /// executes the statement returning a set of scalars
    /// </summary>
    /// <typeparam name="TScalar">type of scalar to return</typeparam>
    /// <returns>values of first column of result set converted to TScalar</returns>
    public virtual IEnumerable<TScalar> ExecuteSet<TScalar>(params object[] parameters) {
        return ExecuteSet<TScalar>(null, parameters);
    }

    /// <summary>
    /// executes the statement returning a set of scalars
    /// </summary>
    /// <typeparam name="TScalar">type of scalar to return</typeparam>
    /// <returns>values of first column of result set converted to TScalar</returns>
    public virtual IEnumerable<TScalar> ExecuteSet<TScalar>(Transaction transaction, params object[] parameters) {
        IEnumerable<object> result;
        if (DBPrepare && DBClient.DBInfo.PreparationSupported)
            result = DBClient.SetPrepared(transaction, CommandText, ConstantParameters.Concat(parameters));
        else result = DBClient.Set(transaction, CommandText, ConstantParameters.Concat(parameters));
            
        foreach(object value in result)
            yield return Converter.Convert<TScalar>(value, true);
    }

    /// <summary>
    /// executes a query and stores the result in a custom result type
    /// </summary>
    /// <typeparam name="TType">type of result</typeparam>
    /// <param name="assignments">action used to assign values</param>
    /// <param name="parameters">custom parameters for query execution</param>
    /// <returns>enumeration of result types</returns>
    public virtual IEnumerable<TType> ExecuteTypes<TType>(Func<Row, TType> assignments, params object[] parameters) {
        return ExecuteTypes(null, assignments, parameters);
    }

    /// <summary>
    /// executes a query and stores the result in a custom result type
    /// </summary>
    /// <typeparam name="TType">type of result</typeparam>
    /// <param name="assignments">action used to assign values</param>
    /// <param name="transaction">transaction to use for execution</param>
    /// <param name="parameters">custom parameters for query execution</param>
    /// <returns>enumeration of result types</returns>
    public virtual IEnumerable<TType> ExecuteTypes<TType>(Transaction transaction, Func<Row, TType> assignments, params object[] parameters) {
        Reader reader = ExecuteReader(transaction, parameters);
        if(DBClient.DBInfo.MultipleConnectionsSupported)
            return reader.ReadTypes(assignments);
        return reader.ReadTypes(assignments).ToArray();
    }

    /// <summary>
    /// executes a query and stores the result in a custom result type
    /// </summary>
    /// <typeparam name="TType">type of result</typeparam>
    /// <param name="assignments">action used to assign values</param>
    /// <param name="parameters">custom parameters for query execution</param>
    /// <returns>enumeration of result types</returns>
    public virtual TType ExecuteType<TType>(Func<Row, TType> assignments, params object[] parameters) {
        return ExecuteType(null, assignments, parameters);
    }

    /// <summary>
    /// executes a query and stores the result in a custom result type
    /// </summary>
    /// <typeparam name="TType">type of result</typeparam>
    /// <param name="assignments">action used to assign values</param>
    /// <param name="transaction">transaction to use for execution</param>
    /// <param name="parameters">custom parameters for query execution</param>
    /// <returns>enumeration of result types</returns>
    public virtual TType ExecuteType<TType>(Transaction transaction, Func<Row, TType> assignments, params object[] parameters) {
        return ExecuteTypes(transaction, assignments, parameters).FirstOrDefault();
    }

    /// <summary>
    /// executes the statement
    /// </summary>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>data table containing results</returns>
    public new virtual Task<DataTable> ExecuteAsync(params object[] parameters)
    {
        return ExecuteAsync(null, parameters);
    }

    /// <summary>
    /// executes the statement
    /// </summary>
    /// <param name="transaction">transaction used to execute operation</param>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>data table containing results</returns>
    public new virtual Task<DataTable> ExecuteAsync(Transaction transaction, params object[] parameters)
    {
        if(DBPrepare && DBClient.DBInfo.PreparationSupported)
            return DBClient.QueryPreparedAsync(transaction, CommandText, ConstantParameters.Concat(parameters));
        return DBClient.QueryAsync(transaction, CommandText, ConstantParameters.Concat(parameters));
    }

    /// <summary>
    /// executes the statement returning a scalar
    /// </summary>
    /// <returns>first value of the result set or default of TScalar</returns>
    public virtual Task<TScalar> ExecuteScalarAsync<TScalar>(params object[] parameters)
    {
        return ExecuteScalarAsync<TScalar>(null, parameters);
    }

    /// <summary>
    /// executes the statement returning a scalar
    /// </summary>
    /// <returns>first value of the result set or default of TScalar</returns>
    public virtual async Task<TScalar> ExecuteScalarAsync<TScalar>(Transaction transaction, params object[] parameters)
    {
        if(DBPrepare && DBClient.DBInfo.PreparationSupported)
            return Converter.Convert<TScalar>(await DBClient.ScalarPreparedAsync(transaction, CommandText, ConstantParameters.Concat(parameters)), true);
        return Converter.Convert<TScalar>(await DBClient.ScalarAsync(transaction, CommandText, ConstantParameters.Concat(parameters)), true);
    }

    /// <summary>
    /// executes the statement returning a set of scalars
    /// </summary>
    /// <typeparam name="TScalar">type of scalar to return</typeparam>
    /// <returns>values of first column of result set converted to TScalar</returns>
    public virtual IAsyncEnumerable<TScalar> ExecuteSetAsync<TScalar>(params object[] parameters)
    {
        return ExecuteSetAsync<TScalar>(null, parameters);
    }

    /// <summary>
    /// executes the statement returning a set of scalars
    /// </summary>
    /// <typeparam name="TScalar">type of scalar to return</typeparam>
    /// <returns>values of first column of result set converted to TScalar</returns>
    public virtual async IAsyncEnumerable<TScalar> ExecuteSetAsync<TScalar>(Transaction transaction, params object[] parameters) {
        IAsyncEnumerable<object> result;
        if (DBPrepare && DBClient.DBInfo.PreparationSupported)
            result = DBClient.SetPreparedAsync(transaction, CommandText, ConstantParameters.Concat(parameters));
        else result = DBClient.SetAsync(transaction, CommandText, ConstantParameters.Concat(parameters));

        await foreach (object value in result)
            yield return Converter.Convert<TScalar>(value, true);
    }

    /// <summary>
    /// executes a query and stores the result in a custom result type
    /// </summary>
    /// <typeparam name="TType">type of result</typeparam>
    /// <param name="assignments">action used to assign values</param>
    /// <param name="parameters">custom parameters for query execution</param>
    /// <returns>enumeration of result types</returns>
    public virtual IAsyncEnumerable<TType> ExecuteTypesAsync<TType>(Func<Row, TType> assignments, params object[] parameters)
    {
        return ExecuteTypesAsync(null, assignments, parameters);
    }

    /// <summary>
    /// executes a query and stores the result in a custom result type
    /// </summary>
    /// <typeparam name="TType">type of result</typeparam>
    /// <param name="assignments">action used to assign values</param>
    /// <param name="transaction">transaction to use for execution</param>
    /// <param name="parameters">custom parameters for query execution</param>
    /// <returns>enumeration of result types</returns>
    public virtual async IAsyncEnumerable<TType> ExecuteTypesAsync<TType>(Transaction transaction, Func<Row, TType> assignments, params object[] parameters)
    {
        Reader reader = await ExecuteReaderAsync(transaction, parameters);
        if (DBClient.DBInfo.MultipleConnectionsSupported) {
            await foreach (TType item in reader.ReadTypesAsync(assignments))
                yield return item;
            yield break;
        }

        List<TType> buffer = [];
        await foreach (TType item in reader.ReadTypesAsync(assignments))
            buffer.Add(item);

        foreach (TType item in buffer)
            yield return item;
    }

    /// <summary>
    /// executes a query and stores the result in a custom result type
    /// </summary>
    /// <typeparam name="TType">type of result</typeparam>
    /// <param name="assignments">action used to assign values</param>
    /// <param name="parameters">custom parameters for query execution</param>
    /// <returns>enumeration of result types</returns>
    public virtual Task<TType> ExecuteTypeAsync<TType>(Func<Row, TType> assignments, params object[] parameters)
    {
        return ExecuteTypeAsync(null, assignments, parameters);
    }

    /// <summary>
    /// executes a query and stores the result in a custom result type
    /// </summary>
    /// <typeparam name="TType">type of result</typeparam>
    /// <param name="assignments">action used to assign values</param>
    /// <param name="transaction">transaction to use for execution</param>
    /// <param name="parameters">custom parameters for query execution</param>
    /// <returns>enumeration of result types</returns>
    public virtual async Task<TType> ExecuteTypeAsync<TType>(Transaction transaction, Func<Row, TType> assignments, params object[] parameters)
    {
        Reader reader = await ExecuteReaderAsync(transaction, parameters);
        return reader.ReadTypes(assignments).FirstOrDefault();
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <typeparam name="T">type of entities to create</typeparam>
    /// <param name="transaction">transaction to use</param>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public virtual IEnumerable<T> ExecuteEntities<T>(Transaction transaction, params object[] parameters) {
        Reader reader = ExecuteReader(transaction, parameters);
        if (DBClient.DBInfo.MultipleConnectionsSupported)
            return CreateObjects<T>(reader);
        return CreateObjects<T>(reader).ToArray();
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <typeparam name="T">type of entities to create</typeparam>
    /// <param name="transaction">transaction to use</param>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public virtual T ExecuteEntity<T>(Transaction transaction, params object[] parameters) {
        return ExecuteEntities<T>(transaction, parameters).FirstOrDefault();
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <typeparam name="T">type of entities to create</typeparam>
    /// <param name="transaction">transaction to use</param>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public virtual async Task<T> ExecuteEntityAsync<T>(Transaction transaction, params object[] parameters) {
        await foreach (T item in ExecuteEntitiesAsync<T>(transaction, parameters))
            return item;
        return default;
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <typeparam name="T">type of entities to create</typeparam>
    /// <param name="transaction">transaction to use</param>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public virtual async IAsyncEnumerable<T> ExecuteEntitiesAsync<T>(Transaction transaction, params object[] parameters) {
        Reader reader = await ExecuteReaderAsync(transaction, parameters);
        if (DBClient.DBInfo.MultipleConnectionsSupported) {
            await foreach (T item in CreateObjectsAsync<T>(reader))
                yield return item;
            yield break;
        }

        List<T> buffer = [];
        await foreach (T item in CreateObjectsAsync<T>(reader))
            buffer.Add(item);

        foreach (T item in buffer)
            yield return item;
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <typeparam name="T">type of entities to create</typeparam>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public virtual IAsyncEnumerable<T> ExecuteEntitiesAsync<T>(params object[] parameters) {
        return ExecuteEntitiesAsync<T>(null, parameters);
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <typeparam name="T">type of entities to create</typeparam>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public virtual IEnumerable<T> ExecuteEntities<T>(params object[] parameters) {
        return ExecuteEntities<T>(null, parameters);
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <typeparam name="T">type of entities to create</typeparam>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public virtual Task<T> ExecuteEntityAsync<T>(params object[] parameters) {
        return ExecuteEntityAsync<T>(null, parameters);
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <typeparam name="T">type of entities to create</typeparam>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public virtual T ExecuteEntity<T>(params object[] parameters) {
        return ExecuteEntity<T>(null, parameters);
    }

    /// <summary>
    /// executes the operation returning a reader
    /// </summary>
    /// <param name="transaction">transaction to use</param>
    /// <param name="parameters">parameters for command</param>
    /// <returns>data reader used to read results</returns>
    public virtual Reader ExecuteReader(Transaction transaction, params object[] parameters) {
        if (DBPrepare && DBClient.DBInfo.PreparationSupported)
            return DBClient.ReaderPrepared(transaction, CommandText, ConstantParameters.Concat(parameters));
        return DBClient.Reader(transaction, CommandText, ConstantParameters.Concat(parameters));
    }

    /// <summary>
    /// executes the operation returning a reader
    /// </summary>
    /// <param name="parameters">parameters for command</param>
    /// <returns>data reader used to read results</returns>
    public virtual IDataReader ExecuteReader(params object[] parameters) {
        return ExecuteReader(null, parameters);
    }

    /// <summary>
    /// executes the operation returning a reader
    /// </summary>
    /// <param name="transaction">transaction to use</param>
    /// <param name="parameters">parameters for command</param>
    /// <returns>data reader used to read results</returns>
    public virtual async Task<Reader> ExecuteReaderAsync(Transaction transaction, params object[] parameters) {
        if (DBPrepare && DBClient.DBInfo.PreparationSupported)
            return await DBClient.ReaderPreparedAsync(transaction, CommandText, ConstantParameters.Concat(parameters));
        return await DBClient.ReaderAsync(transaction, CommandText, ConstantParameters.Concat(parameters));
    }

    /// <summary>
    /// executes the operation returning a reader
    /// </summary>
    /// <param name="parameters">parameters for command</param>
    /// <returns>data reader used to read results</returns>
    public virtual Task<Reader> ExecuteReaderAsync(params object[] parameters) {
        return ExecuteReaderAsync(null, parameters);
    }
        
    /// <summary>
    /// creates entities from table data
    /// </summary>
    /// <typeparam name="T">type of entity to create</typeparam>
    /// <param name="reader">reader used to retrieve data rows from database</param>
    /// <returns>created entities</returns>
    protected IEnumerable<T> CreateObjects<T>(Reader reader) {
        if (reader == null)
            yield break;
            
        using (reader) {
            EntityDescriptor descriptor = modelcache(typeof(T));

            List<PropertyInfo> setters = [];
            for (int i = 0; i < reader.FieldCount; ++i) {
                string columnname = reader.GetName(i);
                EntityColumnDescriptor column = descriptor.TryGetColumn(columnname);
                setters.Add(column?.Property);
            }

            while (reader.Read())
                yield return ToObject<T>(reader, setters);
        }
    }

    /// <summary>
    /// creates entities from table data
    /// </summary>
    /// <typeparam name="T">type of entity to create</typeparam>
    /// <param name="reader">reader used to retrieve data rows from database</param>
    /// <returns>created entities</returns>
    protected async IAsyncEnumerable<T> CreateObjectsAsync<T>(Reader reader) {
        if (reader == null)
            yield break;
            
        using (reader) {
            EntityDescriptor descriptor = modelcache(typeof(T));

            List<PropertyInfo> setters = [];
            for (int i = 0; i < reader.FieldCount; ++i) {
                string columnname = reader.GetName(i);
                EntityColumnDescriptor column = descriptor.TryGetColumn(columnname);
                setters.Add(column?.Property);
            }

            while (await reader.ReadAsync())
                yield return await ToObjectAsync<T>(reader, setters);
        }
    }

    /// <summary>
    /// converts a data row to an object
    /// </summary>
    /// <typeparam name="T">type of entity to which to convert data row</typeparam>
    /// <param name="reader">reader used to retrieve field values</param>
    /// <param name="properties">properties to assign values to</param>
    /// <returns>created entity</returns>
    T ToObject<T>(Reader reader, List<PropertyInfo> properties) {
        T obj = (T)Activator.CreateInstance(typeof(T), true);
            
        for(int i=0;i<reader.FieldCount;++i) {
            PropertyInfo pi = properties[i];
            if (pi == null)
                // property for column not found
                continue;

            object dbvalue;
            try {
                dbvalue = DBClient.DBInfo.ValueFromReader(reader, i, pi.PropertyType);
            }
            catch (Exception e) {
                Logger.Warning(this,$"Unable to read property '{pi.Name}'", e.ToString());
                continue;
            }
                
            if (dbvalue is null or DBNull)
                continue;
                
            if(pi.PropertyType.IsEnum) {
                int index = Converter.Convert<int>(dbvalue, true);
                object value = Enum.ToObject(pi.PropertyType, index);
                pi.SetValue(obj, value, null);
            }
            else if(dbvalue.GetType() == pi.PropertyType)
                pi.SetValue(obj, dbvalue, null);
            else
                pi.SetValue(obj, Converter.Convert(dbvalue, pi.PropertyType), null);
        }

        return obj;
    }
        
    /// <summary>
    /// converts a data row to an object
    /// </summary>
    /// <typeparam name="T">type of entity to which to convert data row</typeparam>
    /// <param name="reader">reader used to retrieve field values</param>
    /// <param name="properties">properties to assign values to</param>
    /// <returns>created entity</returns>
    async Task<T> ToObjectAsync<T>(Reader reader, List<PropertyInfo> properties) {
        T obj = (T)Activator.CreateInstance(typeof(T), true);
            
        for(int i=0;i<reader.FieldCount;++i) {
            PropertyInfo pi = properties[i];
            if (pi == null)
                // property for column not found
                continue;

            object dbvalue;
            try {
                dbvalue = await DBClient.DBInfo.ValueFromReaderAsync(reader, i, pi.PropertyType);
            }
            catch (Exception e) {
                Logger.Warning(this,$"Unable to read property '{pi.Name}'", e.ToString());
                continue;
            }
                
            if (dbvalue is null or DBNull)
                continue;
                
            if(pi.PropertyType.IsEnum) {
                int index = Converter.Convert<int>(dbvalue, true);
                object value = Enum.ToObject(pi.PropertyType, index);
                pi.SetValue(obj, value, null);
            }
            else if(dbvalue.GetType() == pi.PropertyType)
                pi.SetValue(obj, dbvalue, null);
            else
                pi.SetValue(obj, Converter.Convert(dbvalue, pi.PropertyType), null);
        }

        return obj;
    }

}

/// <summary>
/// operation prepared to load values from the database
/// </summary>
/// <typeparam name="T">type of entity for which operation is prepared</typeparam>
public class PreparedLoadOperation<T> : PreparedLoadOperation {
        
    /// <summary>
    /// creates a new <see cref="PreparedLoadOperation"/>
    /// </summary>
    /// <param name="dbclient">database access</param>
    /// <param name="modelcache">cache of entity models</param>
    /// <param name="commandtext">command text</param>
    /// <param name="parameters">parameters for operation</param>
    /// <param name="dbprepare">indicated whether to prepare statement at db aswell</param>
    public PreparedLoadOperation(IDBClient dbclient, Func<Type, EntityDescriptor> modelcache, string commandtext, object[] parameters, bool dbprepare) 
        : base(dbclient, modelcache, commandtext, parameters, dbprepare) {
    }
        
    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <param name="transaction">transaction to use</param>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public virtual IEnumerable<T> ExecuteEntities(Transaction transaction, params object[] parameters) {
        Reader reader = ExecuteReader(transaction, parameters);
        if (DBClient.DBInfo.MultipleConnectionsSupported)
            return CreateObjects<T>(reader);
        return CreateObjects<T>(reader).ToArray();
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <param name="transaction">transaction to use</param>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public virtual T ExecuteEntity(Transaction transaction, params object[] parameters) {
        return ExecuteEntities<T>(transaction, parameters).FirstOrDefault();
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <param name="transaction">transaction to use</param>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public virtual async Task<T> ExecuteEntityAsync(Transaction transaction, params object[] parameters) {
        await foreach (T item in ExecuteEntitiesAsync<T>(transaction, parameters))
            return item;
        return default;
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <param name="transaction">transaction to use</param>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public virtual async Task<IEnumerable<T>> ExecuteEntitiesAsync(Transaction transaction, params object[] parameters) {
        Reader reader = await ExecuteReaderAsync(transaction, parameters);
        if (DBClient.DBInfo.MultipleConnectionsSupported)
            return CreateObjects<T>(reader);
        return CreateObjects<T>(reader).ToArray();
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public virtual IAsyncEnumerable<T> ExecuteEntitiesAsync(params object[] parameters) {
        return ExecuteEntitiesAsync<T>(null, parameters);
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public virtual IEnumerable<T> ExecuteEntities(params object[] parameters) {
        return ExecuteEntities<T>(null, parameters);
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public virtual Task<T> ExecuteEntityAsync(params object[] parameters) {
        return ExecuteEntityAsync<T>(null, parameters);
    }

    /// <summary>
    /// executes the operation and creates entities from the result
    /// </summary>
    /// <param name="parameters">parameters for execution</param>
    /// <returns>created entities</returns>
    public virtual T ExecuteEntity(params object[] parameters) {
        return ExecuteEntity<T>(null, parameters);
    }

    /// <summary>
    /// executes the operation as a single-statement paged load, returning both the page items and the total matching count
    /// without a second SQL round trip.
    /// </summary>
    /// <remarks>
    /// Injects <c>COUNT(*) OVER () AS __total</c> into the projection. Requires the underlying database to support
    /// <c>COUNT(*) OVER ()</c>: SQLite 3.25+, PostgreSQL 8.4+, MSSQL 2005+, MySQL 8.0+ / MariaDB 10.2+.
    /// Older engines will fail with a <see cref="Pooshit.Ocelot.Errors.StatementException"/> wrapping a SQL-syntax error.
    /// The alias <c>__total</c> is reserved; entity properties with that name will receive the windowed count value.
    /// Any prior <c>.Limit()</c> / <c>.Offset()</c> on the operation are overridden by <paramref name="limit"/> and <paramref name="offset"/>.
    /// </remarks>
    /// <param name="limit">number of rows to return (must be &gt;= 0)</param>
    /// <param name="offset">number of rows to skip (must be &gt;= 0)</param>
    /// <param name="cancellationToken">token used to cancel the operation</param>
    /// <returns>
    /// a <see cref="PagedResult{T}"/> whose <see cref="PagedResult{T}.Total"/> is already resolved
    /// and whose <see cref="PagedResult{T}.Items"/> is ready to iterate when the returned task completes
    /// </returns>
    public virtual Task<PagedResult<T>> ExecutePagedAsync(int limit, int offset, CancellationToken cancellationToken = default) {
        return ExecutePagedAsync(null, limit, offset, cancellationToken);
    }

    /// <summary>
    /// executes the operation as a single-statement paged load, returning both the page items and the total matching count
    /// without a second SQL round trip.
    /// </summary>
    /// <remarks>
    /// Injects <c>COUNT(*) OVER () AS __total</c> into the projection. Requires the underlying database to support
    /// <c>COUNT(*) OVER ()</c>: SQLite 3.25+, PostgreSQL 8.4+, MSSQL 2005+, MySQL 8.0+ / MariaDB 10.2+.
    /// Older engines will fail with a <see cref="Pooshit.Ocelot.Errors.StatementException"/> wrapping a SQL-syntax error.
    /// The alias <c>__total</c> is reserved; entity properties with that name will receive the windowed count value.
    /// Any prior <c>.Limit()</c> / <c>.Offset()</c> on the operation are overridden by <paramref name="limit"/> and <paramref name="offset"/>.
    /// </remarks>
    /// <param name="transaction">transaction to use (optional)</param>
    /// <param name="limit">number of rows to return (must be &gt;= 0)</param>
    /// <param name="offset">number of rows to skip (must be &gt;= 0)</param>
    /// <param name="cancellationToken">token used to cancel the operation</param>
    /// <returns>
    /// a <see cref="PagedResult{T}"/> whose <see cref="PagedResult{T}.Total"/> is already resolved
    /// and whose <see cref="PagedResult{T}.Items"/> is ready to iterate when the returned task completes
    /// </returns>
    public virtual async Task<PagedResult<T>> ExecutePagedAsync(Transaction transaction, int limit, int offset, CancellationToken cancellationToken = default) {
        if (limit < 0)
            throw new ArgumentOutOfRangeException(nameof(limit), "limit must be >= 0");
        if (offset < 0)
            throw new ArgumentOutOfRangeException(nameof(offset), "offset must be >= 0");

        cancellationToken.ThrowIfCancellationRequested();

        // Build a new SQL string injecting COUNT(*) OVER() AS __total and overriding LIMIT/OFFSET.
        string pagedCommandText = InjectTotalColumn(CommandText, limit, offset, DBClient.DBInfo.GetType().Name);

        object[] allParams = ConstantParameters.ToArray();

        Reader reader;
        if (DBPrepare && DBClient.DBInfo.PreparationSupported)
            reader = await DBClient.ReaderPreparedAsync(transaction, pagedCommandText, allParams, cancellationToken);
        else
            reader = await DBClient.ReaderAsync(transaction, pagedCommandText, allParams, cancellationToken);

        // Read first row eagerly so Total resolves before Task<PagedResult<T>> returns
        TaskCompletionSource<long> totalTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        EntityDescriptor descriptor = ModelDescriptorCache(typeof(T));

        List<PropertyInfo> setters = null;
        int totalOrdinal = -1;

        T firstRow = default;
        bool hasFirstRow = false;

        try {
            if (await reader.ReadAsync(cancellationToken)) {
                // Initialize column mapping on first row (includes __total column)
                setters = BuildSetters(reader, descriptor, out totalOrdinal);
                long total = ExtractTotal(reader, totalOrdinal);
                totalTcs.TrySetResult(total);
                firstRow = await ToObjectPagedAsync(reader, setters, totalOrdinal);
                hasFirstRow = true;
            }
            else {
                // Zero rows — total is 0, stream is empty
                setters = [];
                totalOrdinal = -1;
                totalTcs.TrySetResult(0L);
            }
        }
        catch (OperationCanceledException ex) {
            totalTcs.TrySetException(ex);
            reader.Dispose();
            throw;
        }
        catch (Exception ex) {
            totalTcs.TrySetException(ex);
            reader.Dispose();
            throw;
        }

        // On single-connection dialects (SQLite) buffer all remaining rows immediately, then release the semaphore
        if (!DBClient.DBInfo.MultipleConnectionsSupported) {
            List<T> buffer = [];
            if (hasFirstRow)
                buffer.Add(firstRow);

            try {
                while (await reader.ReadAsync(cancellationToken)) {
                    cancellationToken.ThrowIfCancellationRequested();
                    buffer.Add(await ToObjectPagedAsync(reader, setters, totalOrdinal));
                }
            }
            catch (OperationCanceledException) {
                reader.Dispose();
                throw;
            }
            catch {
                reader.Dispose();
                throw;
            }

            reader.Dispose();

            return new PagedResult<T>(ToAsyncEnumerable(buffer), totalTcs.Task);
        }

        // Multi-connection path: stream rows through the open reader
        return new PagedResult<T>(StreamRemaining(reader, firstRow, hasFirstRow, setters, totalOrdinal, cancellationToken), totalTcs.Task);
    }

    static string InjectTotalColumn(string commandText, int limit, int offset, string dialectTypeName) {
        // Insert ", COUNT(*) OVER() AS __total" just before the first "FROM" keyword (case-insensitive, word boundary).
        // Strip any existing LIMIT/OFFSET and re-append the caller-supplied values in dialect-appropriate syntax.
        // This works because LoadOperation<T>.Prepare always produces "SELECT ... FROM ..." shape.
        int fromIdx = FindFromKeyword(commandText);
        string baseText;
        if (fromIdx < 0)
            baseText = commandText + ", COUNT(*) OVER() AS __total";
        else
            baseText = commandText[..fromIdx].TrimEnd() + ", COUNT(*) OVER() AS __total " + commandText[fromIdx..];

        // Strip trailing LIMIT/OFFSET clause that may have been baked in already,
        // then re-append with the caller-supplied values.
        baseText = StripLimitOffset(baseText);

        // MSSQL uses OFFSET x ROWS FETCH NEXT y ROWS ONLY; all others use LIMIT y OFFSET x
        if (dialectTypeName == "MsSqlInfo") {
            if (!baseText.Contains("ORDER BY", StringComparison.OrdinalIgnoreCase))
                baseText += " ORDER BY(SELECT NULL)";
            return baseText + $" OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY";
        }

        return baseText + $" LIMIT {limit} OFFSET {offset}";
    }

    static string StripLimitOffset(string sql) {
        // Find the last top-level LIMIT or OFFSET keyword and strip from there
        int lastLimit = FindLastTopLevelKeyword(sql, "LIMIT");
        int lastOffset = FindLastTopLevelKeyword(sql, "OFFSET");
        int cutAt = -1;
        if (lastLimit >= 0) cutAt = lastLimit;
        if (lastOffset >= 0 && (cutAt < 0 || lastOffset < cutAt)) cutAt = lastOffset;
        if (cutAt < 0) return sql;
        return sql[..cutAt].TrimEnd();
    }

    static int FindLastTopLevelKeyword(string sql, string keyword) {
        int result = -1;
        int depth = 0;
        for (int i = 0; i < sql.Length; i++) {
            if (sql[i] == '(') { depth++; continue; }
            if (sql[i] == ')') { depth--; continue; }
            if (depth > 0) continue;
            if (i + keyword.Length <= sql.Length && string.Compare(sql, i, keyword, 0, keyword.Length, StringComparison.OrdinalIgnoreCase) == 0) {
                bool afterOk = i + keyword.Length >= sql.Length || !char.IsLetterOrDigit(sql[i + keyword.Length]);
                bool beforeOk = i == 0 || char.IsWhiteSpace(sql[i - 1]);
                if (beforeOk && afterOk)
                    result = i;
            }
        }
        return result;
    }

    static int FindFromKeyword(string sql) {
        // Walk from left to find first top-level FROM not inside parentheses
        int depth = 0;
        for (int i = 0; i < sql.Length; i++) {
            if (sql[i] == '(') { depth++; continue; }
            if (sql[i] == ')') { depth--; continue; }
            if (depth > 0) continue;
            if (i + 4 <= sql.Length && string.Compare(sql, i, "FROM", 0, 4, StringComparison.OrdinalIgnoreCase) == 0) {
                // Ensure it's a word boundary
                bool afterOk = i + 4 >= sql.Length || !char.IsLetterOrDigit(sql[i + 4]);
                bool beforeOk = i == 0 || char.IsWhiteSpace(sql[i - 1]);
                if (beforeOk && afterOk)
                    return i;
            }
        }
        return -1;
    }

    static List<PropertyInfo> BuildSetters(Reader reader, EntityDescriptor descriptor, out int totalOrdinal) {
        List<PropertyInfo> setters = [];
        totalOrdinal = -1;
        for (int i = 0; i < reader.FieldCount; ++i) {
            string colName = reader.GetName(i);
            if (colName == "__total") {
                totalOrdinal = i;
                setters.Add(null); // skip mapping for __total
            }
            else {
                EntityColumnDescriptor column = descriptor.TryGetColumn(colName);
                setters.Add(column?.Property);
            }
        }
        return setters;
    }

    static long ExtractTotal(Reader reader, int ordinal) {
        if (ordinal < 0) return 0L;
        object raw = reader.GetValue(ordinal);
        return Converter.Convert<long>(raw, true);
    }

    async Task<T> ToObjectPagedAsync(Reader reader, List<PropertyInfo> properties, int totalOrdinal) {
        T obj = (T)Activator.CreateInstance(typeof(T), true);
        for (int i = 0; i < reader.FieldCount; ++i) {
            if (i == totalOrdinal) continue; // skip __total
            PropertyInfo pi = properties[i];
            if (pi == null) continue;

            object dbvalue;
            try {
                dbvalue = await DBClient.DBInfo.ValueFromReaderAsync(reader, i, pi.PropertyType);
            }
            catch (Exception e) {
                Logger.Warning(this, $"Unable to read property '{pi.Name}'", e.ToString());
                continue;
            }

            if (dbvalue is null or DBNull) continue;

            if (pi.PropertyType.IsEnum) {
                int index = Converter.Convert<int>(dbvalue, true);
                pi.SetValue(obj, Enum.ToObject(pi.PropertyType, index), null);
            }
            else if (dbvalue.GetType() == pi.PropertyType)
                pi.SetValue(obj, dbvalue, null);
            else
                pi.SetValue(obj, Converter.Convert(dbvalue, pi.PropertyType), null);
        }
        return obj;
    }

    static async IAsyncEnumerable<T> ToAsyncEnumerable(List<T> items) {
        foreach (T item in items)
            yield return item;
        await Task.CompletedTask;
    }

    async IAsyncEnumerable<T> StreamRemaining(Reader reader, T firstRow, bool hasFirstRow, List<PropertyInfo> setters, int totalOrdinal, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken) {
        using (reader) {
            if (hasFirstRow)
                yield return firstRow;
            while (await reader.ReadAsync(cancellationToken)) {
                cancellationToken.ThrowIfCancellationRequested();
                yield return await ToObjectPagedAsync(reader, setters, totalOrdinal);
            }
        }
    }
}