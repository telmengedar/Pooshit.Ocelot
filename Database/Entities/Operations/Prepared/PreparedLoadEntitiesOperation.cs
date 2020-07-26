using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using Converter = NightlyCode.Database.Extern.Converter;

namespace NightlyCode.Database.Entities.Operations.Prepared {

    /// <summary>
    /// load operation prepared to execute
    /// </summary>
    /// <typeparam name="T">type of entity created from result set</typeparam>
    public class PreparedLoadEntitiesOperation<T> : PreparedOperation {
        readonly EntityDescriptor descriptor;

        /// <summary>
        /// creates a new <see cref="PreparedLoadEntitiesOperation{T}"/>
        /// </summary>
        /// <param name="dbclient">access to database used for execution of operation</param>
        /// <param name="descriptor">descriptor used to create entities</param>
        /// <param name="commandtext">command text to execute</param>
        /// <param name="parameters">initial parameters of operation</param>
        public PreparedLoadEntitiesOperation(IDBClient dbclient, EntityDescriptor descriptor, string commandtext, params object[] parameters)
            : base(dbclient, commandtext, parameters)
        {
            this.descriptor = descriptor;
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <returns>entities created from result set</returns>
        public new virtual IEnumerable<T> Execute(params object[] parameters) {
            return Execute(null, parameters);
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <param name="transaction">transaction to use for execution</param>
        /// <param name="parameters">parameters to use for execution</param>
        /// <returns>entities created from result set</returns>
        public new virtual IEnumerable<T> Execute(Transaction transaction, params object[] parameters)
        {
            Clients.Tables.DataTable data = DBClient.Query(transaction, CommandText, ConstantParameters.Concat(parameters));
            return CreateObjects(data);
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <returns>entities created from result set</returns>
        public new virtual Task<T[]> ExecuteAsync(params object[] parameters)
        {
            return ExecuteAsync(null, parameters);
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <param name="transaction">transaction to use for execution</param>
        /// <param name="parameters">parameters to use for execution</param>
        /// <returns>entities created from result set</returns>
        public new virtual async Task<T[]> ExecuteAsync(Transaction transaction, params object[] parameters)
        {
            Clients.Tables.DataTable data = await DBClient.QueryAsync(transaction, CommandText, ConstantParameters.Concat(parameters));
            return CreateObjects(data).ToArray();
        }

        /// <summary>
        /// creates entities from table data
        /// </summary>
        /// <param name="dt">result table from which to create entities</param>
        /// <returns>enumeration of created entities</returns>
        protected IEnumerable<T> CreateObjects(Clients.Tables.DataTable dt) {
            foreach(Clients.Tables.DataRow row in dt.Rows) {
                T obj = (T)Activator.CreateInstance(typeof(T), true);
                foreach(string column in dt.Columns.Names) {
                    EntityColumnDescriptor pi = descriptor.GetColumn(column);

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
                yield return obj;
            }
        }
    }
}