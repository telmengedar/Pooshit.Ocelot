using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Entities.Operations.Entities;
using Pooshit.Ocelot.Entities.Operations.Tables;
using Pooshit.Ocelot.Entities.Schema;
using Pooshit.Ocelot.Extern;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Statements;

namespace Pooshit.Ocelot.Entities {

    /// <summary>
    /// manages entities in db
    /// </summary>
    public class EntityManager : IEntityManager {
        readonly SchemaCreator creator;
        readonly SchemaUpdater updater;
        readonly EntityDescriptorCache modelcache=new();

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

        /// <inheritdoc />
        public void Drop<T>() {
            SchemaDescriptor schema = DBClient.DBInfo.GetSchema(DBClient, modelcache.Get<T>().TableName);
            if (schema is ViewDescriptor descriptor)
                DBClient.DBInfo.DropView(DBClient, descriptor);
            else if (schema is TableDescriptor tableDescriptor)
                DBClient.DBInfo.DropTable(DBClient, tableDescriptor);
            else
                throw new Exception("Invalid descriptor type");
        }

        /// <inheritdoc />
        public Task Truncate<T>(TruncateOptions options=null) {
            EntityDescriptor model=modelcache.Get<T>();
            return DBClient.DBInfo.Truncate(DBClient, model.TableName, options);
        }

        /// <inheritdoc />
        public CreateTableOperation CreateTable(string tablename) {
            return new CreateTableOperation(DBClient, tablename);
        }

        /// <inheritdoc />
        public InsertDataOperation InsertData(string table) {
            return new InsertDataOperation(DBClient, table);
        }

        /// <inheritdoc />
        public UpdateDataOperation UpdateData(string table) {
            return new UpdateDataOperation(DBClient, table);
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
        /// <typeparam name="T">type of entities to insert</typeparam>
        public IEntityOperation<T> InsertEntities<T>() {
            return new InsertEntitiesOperation<T>(this, modelcache.Get);
        }

        /// <summary>
        /// updates entities in db
        /// </summary>
        /// <typeparam name="T">type of entity to update</typeparam>
        public IEntityOperation<T> UpdateEntities<T>()
        {
            return new UpdateEntitiesOperation<T>(DBClient, modelcache.Get);
        }

        /// <summary>
        /// delete entities
        /// </summary>
        /// <typeparam name="T">type of entities to delete</typeparam>
        public IEntityOperation<T> DeleteEntities<T>()
        {
            return new DeleteEntitiesOperation<T>(DBClient, modelcache.Get);
        }

        /// <summary>
        /// gets an operation which allows to update the values of an entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public UpdateValuesOperation<T> Update<T>() {
            return new(DBClient, modelcache.Get);
        }

        /// <inheritdoc />
        public LoadDataOperation LoadData(string tablename) {
            return new(DBClient, modelcache.Get, tablename);
        }

        /// <inheritdoc />
        public InsertValuesOperation<T> Insert<T>() {
            return new(DBClient, modelcache.Get);
        }

        /// <inheritdoc />
        public DeleteOperation<T> Delete<T>() {
            return new(DBClient, modelcache.Get);
        }

        /// <inheritdoc />
        public DeleteOperation Delete(string table) {
            return new(DBClient, table);
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
            List<T> toinsert = new();
            List<T> toupdate = new();

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
                InsertEntities<T>().Execute(toinsert.ToArray());
            if(toupdate.Count>0)
                UpdateEntities<T>().Execute(toupdate.ToArray());
        }

        bool PrimaryKeyEquals(object lhs, object rhs) {
            if(lhs == null) return rhs == null;
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// get a load operation to use to load values of an entity from the database
        /// </summary>
        public LoadOperation Load() {
            return new(DBClient, modelcache.Get, Array.Empty<IDBField>());
        }

        /// <summary>
        /// get a load operation to use to load values of an entity from the database
        /// </summary>
        public LoadOperation Load(params IDBField[] fields) {
            return new(DBClient, modelcache.Get, fields);
        }

        /// <summary>
        /// get a load operation to use to load values of an entity from the database
        /// </summary>
        public LoadOperation Load(params Expression<Func<object>>[] fields) {
            return new(DBClient, modelcache.Get, fields.Select(EntityField.Create).Cast<IDBField>().ToArray());
        }

        /// <summary>
        /// get a load operation to use to load values of an entity from the database
        /// </summary>
        public LoadOperation<T> Load<T>() {
            return new(DBClient, modelcache.Get, Array.Empty<IDBField>());
        }

        /// <summary>
        /// get a load operation to use to load values of an entity from the database
        /// </summary>
        public LoadOperation<T> Load<T>(params IDBField[] fields) {
            return new(DBClient, modelcache.Get, fields);
        }

        /// <summary>
        /// get a load operation to use to load values of an entity from the database
        /// </summary>
        public LoadOperation<T> Load<T>(params Expression<Func<T, object>>[] fields) {
            return new(DBClient, modelcache.Get, fields.Select(EntityField.Create).Cast<IDBField>().ToArray());
        }

        /// <summary>
        /// get access to an entity model
        /// </summary>
        /// <typeparam name="T">type of entity of which to access model</typeparam>
        public EntityDescriptorAccess<T> Model<T>() {
            return new(modelcache.Get<T>());
        }

        /// <inheritdoc />
        public bool Exists<T>() {
            return Exists(modelcache.Get<T>().TableName);
        }

        /// <inheritdoc />
        public bool Exists(string table) {
            return DBClient.DBInfo.CheckIfTableExists(DBClient, table);
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
            return DBClient.Transaction();
        }
    }
}