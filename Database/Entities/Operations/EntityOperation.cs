using System;
using System.Collections;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using Converter = NightlyCode.Database.Extern.Converter;

namespace NightlyCode.Database.Entities.Operations {

    /// <summary>
    /// updates entities in db
    /// </summary>
    public abstract class EntityOperation {
        readonly Func<Type, EntityDescriptor> descriptor;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="dbclient"></param>
        /// <param name="entities"></param>
        /// <param name="descriptor"></param>
        protected EntityOperation(IDBClient dbclient, IEnumerable entities, Func<Type, EntityDescriptor> descriptor) {
            this.DBClient = dbclient;
            this.Entities = entities;
            this.descriptor = descriptor;
        }

        /// <summary>
        /// dbclient used to access db
        /// </summary>
        protected IDBClient DBClient { get; }

        /// <summary>
        /// entities to process
        /// </summary>
        protected IEnumerable Entities { get; }

        /// <summary>
        /// get the entity descriptor for the specified type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected EntityDescriptor GetDescriptor(Type type) {
            return descriptor(type);
        }

        /// <summary>
        /// get value of entity which is compatible with db
        /// </summary>
        /// <param name="column"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected object GetValue(EntityColumnDescriptor column, object entity) {
            return Converter.Convert(column.GetValue(entity), DBClient.DBInfo.GetDBRepresentation(column.Property.PropertyType));
        }

        /// <summary>
        /// executes the operation
        /// </summary>
        /// <returns></returns>
        public abstract int Execute();

    }
}