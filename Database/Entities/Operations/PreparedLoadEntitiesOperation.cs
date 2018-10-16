using System;
using System.Collections.Generic;
using System.Linq;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Fields;
using Converter = NightlyCode.Database.Extern.Converter;

namespace NightlyCode.Database.Entities.Operations {

    /// <summary>
    /// load operation prepared to execute
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PreparedLoadEntitiesOperation<T> : PreparedOperation {
        readonly IDBClient dbclient;
        readonly EntityDescriptor descriptor;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="dbclient"></param>
        /// <param name="descriptor"></param>
        /// <param name="commandtext"></param>
        /// <param name="parameters"></param>
        public PreparedLoadEntitiesOperation(IDBClient dbclient, EntityDescriptor descriptor, string commandtext, params object[] parameters)
            : base(dbclient, commandtext, parameters)
        {
            this.dbclient = dbclient;
            this.descriptor = descriptor;
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        public IEnumerable<T> Execute()
        {
            return Execute(Parameters);
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> Execute(Transaction transaction, params object[] parameters)
        {
            Clients.Tables.DataTable data = dbclient.Query(transaction, CommandText, parameters);
            return CreateObjects(data);
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> Execute(params object[] parameters) {
            Clients.Tables.DataTable data = dbclient.Query(CommandText, parameters);
            return CreateObjects(data);            
        }

        IEnumerable<T> CreateObjects(Clients.Tables.DataTable dt) {
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

        public override string ToString() {
            return CommandText;
        }
    }
}