using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
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
}