using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NightlyCode.DB.Clients;
using NightlyCode.DB.Entities.Descriptors;
using Converter = NightlyCode.DB.Extern.Converter;

namespace NightlyCode.DB.Entities.Operations {

    /// <summary>
    /// load operation prepared to execute
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PreparedLoadEntitiesOperation<T> {
        readonly IDBClient dbclient;
        readonly EntityDescriptor descriptor;
        readonly PreparedOperation operation;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="dbclient"></param>
        /// <param name="descriptor"></param>
        /// <param name="statement"></param>
        public PreparedLoadEntitiesOperation(IDBClient dbclient, EntityDescriptor descriptor, PreparedOperation statement) {
            this.dbclient = dbclient;
            this.descriptor = descriptor;
            operation = statement;
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> Execute() {
            DataTable data = dbclient.Query(operation.CommandText, operation.Parameters.Select(p => p.Value).ToArray());
            return CreateObjects(data);            
        }

        IEnumerable<T> CreateObjects(DataTable dt) {
            foreach(DataRow row in dt.Rows) {
                T obj = (T)Activator.CreateInstance(typeof(T), true);
                foreach(DataColumn column in dt.Columns) {
                    EntityColumnDescriptor pi = descriptor.GetColumn(column.ColumnName);

                    object dbvalue = row[column.ColumnName];

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

        public override string ToString() {
            return operation.CommandText;
        }
    }
}