using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations;
using NightlyCode.Database.Entities.Operations.Fields;
using NightlyCode.Database.Entities.Schema;
using NightlyCode.Database.Extern;
using Converter = NightlyCode.Database.Extern.Converter;

namespace NightlyCode.Database.Entities {

    /// <summary>
    /// manages entities in db
    /// </summary>
    public class EntityManager : IEntityManager {
        readonly SchemaCreator creator;
        readonly SchemaUpdater updater;
        readonly EntityDescriptorCache modelcache = new EntityDescriptorCache();

        /// <summary>
        /// creates a new <see cref="EntityManager"/>
        /// </summary>
        /// <param name="dbclient">access to database</param>
        public EntityManager(IDBClient dbclient) {
            DBClient = dbclient;
            creator = new SchemaCreator(modelcache);
            updater = new SchemaUpdater(modelcache);
        }

        /// <summary>
        /// client used to access db
        /// </summary>
        public IDBClient DBClient { get; }

        /// <summary>
        /// creates the table for the entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Create<T>() {
            CreateSingle(typeof(T));
        }

        /// <summary>
        /// creates the tables for the entities
        /// </summary>
        /// <param name="types"></param>
        public void Create(params Type[] types) {
            foreach(Type type in types)
                CreateSingle(type);
        }

        /// <summary>
        /// updates the schema of the specified type
        /// </summary>
        /// <remarks>
        /// currently this is only implemented for sqlite databases
        /// </remarks>
        /// <typeparam name="T">type of which to update schema</typeparam>
        public void UpdateSchema<T>() {
            EntityDescriptor descriptor = modelcache.Get<T>();

            if(!DBClient.DBInfo.CheckIfTableExists(DBClient, descriptor.TableName)) {
                Logger.Info(this, $"Creating new table for '{typeof(T).Name}");
                Create<T>();
                return;
            }

            updater.Update<T>(DBClient);
        }

        void CreateSingle(Type type) {
            creator.Create(type, DBClient);
        }

        /// <summary>
        /// inserts entities to the db
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        public void InsertEntities<T>(params T[] entities) {
            InsertEntities((IEnumerable<T>)entities);
        }

        /// <summary>
        /// inserts entities to the db
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        public void InsertEntities<T>(IEnumerable<T> entities) {
            new InsertEntitiesOperation(DBClient, entities, modelcache.Get).Execute();
        }

        /// <summary>
        /// gets an operation which allows to update the values of an entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public UpdateValuesOperation<T> Update<T>() {
            return new UpdateValuesOperation<T>(DBClient, modelcache.Get);
        }

        /// <summary>
        /// gets an operation which allows to insert entities to database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public InsertValuesOperation<T> Insert<T>() {
            return new InsertValuesOperation<T>(DBClient, modelcache.Get);
        }

        /// <summary>
        /// delete entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        public void DeleteEntities<T>(params T[] entities) {
            new DeleteEntitiesOperation(DBClient, entities, modelcache.Get).Execute();
        }

        public DeleteOperation<T> Delete<T>() {
            return new DeleteOperation<T>(DBClient, modelcache.Get);
        }

        /// <summary>
        /// updates entities in db
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        public void UpdateEntities<T>(params T[] entities) {
            UpdateEntities((IEnumerable<T>)entities);
        }

        /// <summary>
        /// updates entities in db
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        public void UpdateEntities<T>(IEnumerable<T> entities) {
            new UpdateEntitiesOperation(DBClient, entities, modelcache.Get).Execute();
        }

        /// <summary>
        /// inserts or updates the specified entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        public void Save<T>(params T[] entities) {
            Save((IEnumerable<T>)entities);
        }

        /// <summary>
        /// inserts or updates the specified entities
        /// </summary>
        /// <remarks>
        /// this only works with entities with a primary key and autoincrement or no primary key at all
        /// when used with an entity with primary key the primary key column has to be left untouched
        /// else this won't work either.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        public void Save<T>(IEnumerable<T> entities) {
            EntityDescriptor descriptor = modelcache.Get<T>();
            List<T> toinsert = new List<T>();
            List<T> toupdate = new List<T>();

            if(descriptor.PrimaryKeyColumn != null) {
                if(!descriptor.PrimaryKeyColumn.AutoIncrement)
                    throw new InvalidOperationException("Primary Key Column must be auto incremented for this method to work");

                object defaultvalue = GetDefault(descriptor.PrimaryKeyColumn.Property.PropertyType);

                foreach(T entity in entities) {
                    if(PrimaryKeyEquals(descriptor.PrimaryKeyColumn.GetValue(entity), defaultvalue))
                        toinsert.Add(entity);
                    else toupdate.Add(entity);
                }
            }
            else toinsert.AddRange(entities);

            if(toinsert.Count>0)
                InsertEntities<T>(toinsert);
            if(toupdate.Count>0)
                UpdateEntities<T>(toupdate);
        }

        bool PrimaryKeyEquals(object lhs, object rhs) {
            if(lhs == null) return rhs == null;
            return lhs.Equals(rhs);
        }
        /// <summary>
        /// get a load operation for the specified entity type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public LoadEntitiesOperation<T> LoadEntities<T>() {
            return new LoadEntitiesOperation<T>(DBClient, modelcache.Get);
        }

        /// <summary>
        /// get a load operation to use to load values of an entity from the database
        /// </summary>
        /// <typeparam name="T">type of entity</typeparam>
        /// <param name="fields">fields to load from the db</param>
        /// <returns></returns>
        public LoadValuesOperation<T> Load<T>(params IDBField[] fields) {
            return new LoadValuesOperation<T>(DBClient, fields, modelcache.Get);
        }

        /// <summary>
        /// get a load operation to use to load values of an entity from the database
        /// </summary>
        public LoadValuesOperation<T> Load<T>(params Expression<Func<T, object>>[] fields) {
            return Load<T>(fields.Select(EntityField.Create).Cast<IDBField>().ToArray());
        }

        /// <summary>
        /// get access to an entity model
        /// </summary>
        /// <typeparam name="T">type of entity of which to access model</typeparam>
        public EntityDescriptorAccess<T> Model<T>() {
            return new EntityDescriptorAccess<T>(modelcache.Get<T>());
        }

        static object GetDefault(Type type) {
            if(type.IsValueType) {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        /// <summary>
        /// starts a transaction
        /// </summary>
        /// <returns>Transaction object to use</returns>
        public Transaction Transaction()
        {
            return DBClient.BeginTransaction();
        }
    }
}