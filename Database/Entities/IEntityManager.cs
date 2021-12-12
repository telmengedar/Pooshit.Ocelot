using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations;
using NightlyCode.Database.Entities.Operations.Entities;
using NightlyCode.Database.Entities.Operations.Tables;
using NightlyCode.Database.Fields;
using NightlyCode.Database.Statements;

namespace NightlyCode.Database.Entities {

    /// <summary>
    /// interface for an entity manager
    /// </summary>
    public interface IEntityManager {

        /// <summary>
        /// client used to access db
        /// </summary>
        IDBClient DBClient { get; }

        /// <summary>
        /// starts a transaction
        /// </summary>
        /// <returns>Transaction object to use</returns>
        Transaction Transaction();

        /// <summary>
        /// creates the table for the entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void Create<T>();

        /// <summary>
        /// creates the tables for the entities
        /// </summary>
        /// <param name="types"></param>
        void Create(params Type[] types);

        /// <summary>
        /// drops tables or views of an entity
        /// </summary>
        /// <typeparam name="T">type of entity of which to drop table/view</typeparam>
        void Drop<T>();

        /// <summary>
        /// truncates data of a table
        /// </summary>
        /// <typeparam name="T">type to truncate</typeparam>
        Task Truncate<T>(TruncateOptions options=null);
        
        /// <summary>
        /// get an operation used to create tables
        /// </summary>
        /// <returns>operation to execute</returns>
        CreateTableOperation CreateTable(string tablename);

        /// <summary>
        /// inserts entities to the db
        /// </summary>
        /// <typeparam name="T">type of entities to insert</typeparam>
        IEntityOperation<T> InsertEntities<T>();

        /// <summary>
        /// updates entities in db
        /// </summary>
        /// <typeparam name="T">type of entities to update</typeparam>
        IEntityOperation<T> UpdateEntities<T>();

        /// <summary>
        /// delete entities
        /// </summary>
        /// <typeparam name="T">type of entities to delete</typeparam>
        IEntityOperation<T> DeleteEntities<T>();

        /// <summary>
        /// gets an operation which allows to update the values of an entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        UpdateValuesOperation<T> Update<T>();

        /// <summary>
        /// inserts or updates the specified entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        void Save<T>(params T[] entities);

        /// <summary>
        /// inserts or updates the specified entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        void Save<T>(IEnumerable<T> entities);

        /// <summary>
        /// get an operation used to delete data from database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        DeleteOperation<T> Delete<T>();

        /// <summary>
        /// get a load operation to use to load values of an entity from the database
        /// </summary>
        LoadOperation<T> Load<T>();

        /// <summary>
        /// get a load operation to use to load values of an entity from the database
        /// </summary>
        LoadOperation<T> Load<T>(params IDBField[] fields);

        /// <summary>
        /// get a load operation to use to load values of an entity from the database
        /// </summary>
        LoadOperation<T> Load<T>(params Expression<Func<T, object>>[] fields);

        /// <summary>
        /// loads data from a table
        /// </summary>
        /// <param name="tablename">name of table to load data from</param>
        /// <returns>operation to use</returns>
        LoadDataOperation LoadData(string tablename);

        /// <summary>
        /// gets an operation which allows to insert entities to database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        InsertValuesOperation<T> Insert<T>();

        /// <summary>
        /// inserts data into a table
        /// </summary>
        /// <param name="table">table to insert data into</param>
        /// <returns>operation used to insert data</returns>
        InsertDataOperation InsertData(string table);

        /// <summary>
        /// updates data of a table
        /// </summary>
        /// <param name="table">table to update data of</param>
        /// <returns>operation to be used to update data</returns>
        UpdateDataOperation UpdateData(string table);

        /// <summary>
        /// updates the schema of the specified type
        /// </summary>
        /// <remarks>
        /// currently this is only implemented for sqlite databases
        /// </remarks>
        /// <typeparam name="T">type of which to update schema</typeparam>
        void UpdateSchema<T>();

        /// <summary>
        /// get access to an entity model
        /// </summary>
        /// <typeparam name="T">type of entity of which to access model</typeparam>
        EntityDescriptorAccess<T> Model<T>();

        /// <summary>
        /// determines whether an entity exists in the database
        /// </summary>
        /// <typeparam name="T">type of entity to check</typeparam>
        /// <returns>true if table for entity exists, false otherwise</returns>
        bool Exists<T>();

        /// <summary>
        /// determines whether a table exists in the database
        /// </summary>
        /// <param name="table">name of table to check for</param>
        /// <returns>true if table exists, false otherwise</returns>
        bool Exists(string table);
    }
}