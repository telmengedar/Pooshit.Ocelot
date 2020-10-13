using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Clients.Tables;
using NightlyCode.Database.Entities.Descriptors;
using Converter = NightlyCode.Database.Extern.Converter;

namespace NightlyCode.Database.Entities.Operations.Prepared {

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
        public PreparedLoadOperation(IDBClient dbclient, Func<Type, EntityDescriptor> modelcache, string commandtext, params object[] parameters)
            : base(dbclient, commandtext, parameters) {
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
            foreach(object value in DBClient.Set(transaction, CommandText, ConstantParameters.Concat(parameters)))
                yield return Converter.Convert<TScalar>(value, true);
        }

        /// <summary>
        /// executes a query and stores the result in a custom result type
        /// </summary>
        /// <typeparam name="TType">type of result</typeparam>
        /// <param name="assignments">action used to assign values</param>
        /// <param name="parameters">custom parameters for query execution</param>
        /// <returns>enumeration of result types</returns>
        public virtual IEnumerable<TType> ExecuteType<TType>(Func<DataRow, TType> assignments, params object[] parameters) {
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
        public virtual IEnumerable<TType> ExecuteType<TType>(Transaction transaction, Func<DataRow, TType> assignments, params object[] parameters) {
            DataTable table = Execute(transaction, parameters);
            return table.Rows.Select(assignments);
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
            return Converter.Convert<TScalar>(await DBClient.ScalarAsync(transaction, CommandText, ConstantParameters.Concat(parameters)), true);
        }

        /// <summary>
        /// executes the statement returning a set of scalars
        /// </summary>
        /// <typeparam name="TScalar">type of scalar to return</typeparam>
        /// <returns>values of first column of result set converted to TScalar</returns>
        public virtual Task<TScalar[]> ExecuteSetAsync<TScalar>(params object[] parameters)
        {
            return ExecuteSetAsync<TScalar>(null, parameters);
        }

        /// <summary>
        /// executes the statement returning a set of scalars
        /// </summary>
        /// <typeparam name="TScalar">type of scalar to return</typeparam>
        /// <returns>values of first column of result set converted to TScalar</returns>
        public virtual async Task<TScalar[]> ExecuteSetAsync<TScalar>(Transaction transaction, params object[] parameters) {
            return (await DBClient.SetAsync(transaction, CommandText, ConstantParameters.Concat(parameters))).Select(v => Converter.Convert<TScalar>(v, true)).ToArray();
        }

        /// <summary>
        /// executes a query and stores the result in a custom result type
        /// </summary>
        /// <typeparam name="TType">type of result</typeparam>
        /// <param name="assignments">action used to assign values</param>
        /// <param name="parameters">custom parameters for query execution</param>
        /// <returns>enumeration of result types</returns>
        public virtual Task<TType[]> ExecuteTypeAsync<TType>(Func<DataRow, TType> assignments, params object[] parameters)
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
        public virtual async Task<TType[]> ExecuteTypeAsync<TType>(Transaction transaction, Func<DataRow, TType> assignments, params object[] parameters)
        {
            DataTable table = await ExecuteAsync(transaction, parameters);
            return table.Rows.Select(assignments).ToArray();
        }

        /// <summary>
        /// executes the operation and creates entities from the result
        /// </summary>
        /// <typeparam name="T">type of entities to create</typeparam>
        /// <param name="transaction">transaction to use</param>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>created entities</returns>
        public virtual IEnumerable<T> ExecuteEntities<T>(Transaction transaction, params object[] parameters) {
            DataTable table = Execute(transaction, parameters);
            return CreateObjects<T>(table);
        }

        /// <summary>
        /// executes the operation and creates entities from the result
        /// </summary>
        /// <typeparam name="T">type of entities to create</typeparam>
        /// <param name="transaction">transaction to use</param>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>created entities</returns>
        public virtual T ExecuteEntity<T>(Transaction transaction, params object[] parameters) {
            DataTable table = Execute(transaction, parameters);
            if (table.Rows.Length == 0)
                return default;

            return ToObject<T>(table.Rows.First(), table.Columns, EntityDescriptor.Create(typeof(T)));
        }

        /// <summary>
        /// executes the operation and creates entities from the result
        /// </summary>
        /// <typeparam name="T">type of entities to create</typeparam>
        /// <param name="transaction">transaction to use</param>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>created entities</returns>
        public virtual async Task<T> ExecuteEntityAsync<T>(Transaction transaction, params object[] parameters) {
            DataTable table = await ExecuteAsync(transaction, parameters);
            if(table.Rows.Length == 0)
                return default;

            return ToObject<T>(table.Rows.First(), table.Columns, EntityDescriptor.Create(typeof(T)));
        }

        /// <summary>
        /// executes the operation and creates entities from the result
        /// </summary>
        /// <typeparam name="T">type of entities to create</typeparam>
        /// <param name="transaction">transaction to use</param>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>created entities</returns>
        public virtual async Task<T[]> ExecuteEntitiesAsync<T>(Transaction transaction, params object[] parameters) {
            DataTable table = await ExecuteAsync(transaction, parameters);
            return await CreateObjectsAsync<T>(table);
        }

        /// <summary>
        /// executes the operation and creates entities from the result
        /// </summary>
        /// <typeparam name="T">type of entities to create</typeparam>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>created entities</returns>
        public virtual Task<T[]> ExecuteEntitiesAsync<T>(params object[] parameters) {
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
        /// creates entities from table data
        /// </summary>
        /// <param name="dt">result table from which to create entities</param>
        /// <returns>enumeration of created entities</returns>
        protected IEnumerable<T> CreateObjects<T>(DataTable dt) {
            EntityDescriptor descriptor=modelcache(typeof(T));
            foreach(DataRow row in dt.Rows) {
                yield return ToObject<T>(row, dt.Columns, descriptor);
            }
        }

        /// <summary>
        /// creates entities from table data
        /// </summary>
        /// <typeparam name="T">type of entity to create</typeparam>
        /// <param name="dt">table to create entities from</param>
        /// <returns>created entities</returns>
        protected Task<T[]> CreateObjectsAsync<T>(DataTable dt) {
            EntityDescriptor descriptor = modelcache(typeof(T));
            DataTableColumns columns = dt.Columns;
            return Task.WhenAll(dt.Rows.Select(r => Task.Run(() => ToObject<T>(r, columns, descriptor))));
        }

        /// <summary>
        /// converts a data row to an object
        /// </summary>
        /// <typeparam name="T">type of entity to which to convert data row</typeparam>
        /// <param name="row">row containing entity data</param>
        /// <param name="columns">columns in result set</param>
        /// <param name="descriptor">descriptor containing model information</param>
        /// <returns>created entity</returns>
        protected T ToObject<T>(DataRow row, DataTableColumns columns, EntityDescriptor descriptor) {
            T obj = (T)Activator.CreateInstance(typeof(T), true);
            foreach(string column in columns.Names) {
                EntityColumnDescriptor pi = descriptor.TryGetColumn(column);
                if (pi == null)
                    continue;

                object dbvalue = row[column];

                if(DBConverterCollection.ContainsConverter(pi.Property.PropertyType)) {
                    pi.Property.SetValue(obj, DBConverterCollection.FromDBValue(pi.Property.PropertyType, dbvalue), null);
                }
                else if(pi.Property.PropertyType.IsEnum) {
                    int index = Converter.Convert<int>(dbvalue, true);
                    object value = Enum.ToObject(pi.Property.PropertyType, index);
                    pi.Property.SetValue(obj, value, null);
                }
                else if(dbvalue.GetType() == pi.Property.PropertyType)
                    pi.Property.SetValue(obj, dbvalue, null);
                else if(dbvalue is DBNull) {
                    if(!pi.Property.PropertyType.IsValueType)
                        pi.Property.SetValue(obj, null, null);
                }
                else {
                    pi.Property.SetValue(obj, Converter.Convert(dbvalue, pi.Property.PropertyType), null);
                }
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
        public PreparedLoadOperation(IDBClient dbclient, Func<Type, EntityDescriptor> modelcache, string commandtext, params object[] parameters) 
            : base(dbclient, modelcache, commandtext, parameters) {
        }

        /// <summary>
        /// executes the operation and creates entities from the result
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>created entities</returns>
        public virtual IEnumerable<T> ExecuteEntities(Transaction transaction, params object[] parameters) {
            DataTable table = Execute(transaction, parameters);
            return CreateObjects<T>(table);
        }

        /// <summary>
        /// executes the operation and creates entities from the result
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>created entities</returns>
        public virtual T ExecuteEntity(Transaction transaction, params object[] parameters) {
            DataTable table = Execute(transaction, parameters);
            if(table.Rows.Length == 0)
                return default;

            return ToObject<T>(table.Rows.First(), table.Columns, EntityDescriptor.Create(typeof(T)));
        }

        /// <summary>
        /// executes the operation and creates entities from the result
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>created entities</returns>
        public virtual async Task<T> ExecuteEntityAsync(Transaction transaction, params object[] parameters) {
            DataTable table = await ExecuteAsync(transaction, parameters);
            if(table.Rows.Length == 0)
                return default;

            return ToObject<T>(table.Rows.First(), table.Columns, EntityDescriptor.Create(typeof(T)));
        }

        /// <summary>
        /// executes the operation and creates entities from the result
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>created entities</returns>
        public virtual async Task<T[]> ExecuteEntitiesAsync(Transaction transaction, params object[] parameters) {
            DataTable table = await ExecuteAsync(transaction, parameters);
            return await CreateObjectsAsync<T>(table);
        }

        /// <summary>
        /// executes the operation and creates entities from the result
        /// </summary>
        /// <param name="parameters">parameters for execution</param>
        /// <returns>created entities</returns>
        public virtual Task<T[]> ExecuteEntitiesAsync(params object[] parameters) {
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
}