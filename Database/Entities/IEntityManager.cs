using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Database.Clients;
using Database.Entities.Descriptors;
using Database.Entities.Operations;

namespace Database.Entities {

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
        /// inserts entities to the db
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        void InsertEntities<T>(params T[] entities);

        /// <summary>
        /// inserts entities to the db
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        void InsertEntities<T>(IEnumerable<T> entities);

        /// <summary>
        /// gets an operation which allows to update the values of an entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        UpdateValuesOperation<T> Update<T>();

        /// <summary>
        /// updates entities in db
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        void UpdateEntities<T>(params T[] entities);

        /// <summary>
        /// updates entities in db
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        void UpdateEntities<T>(IEnumerable<T> entities);

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
        /// get a load operation for the specified entity type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        LoadEntitiesOperation<T> LoadEntities<T>();

        /// <summary>
        /// get a load operation to use to load values of an entity from the database
        /// </summary>
        /// <typeparam name="T">type of entity</typeparam>
        /// <param name="fields">fields to load from the db</param>
        /// <returns></returns>
        LoadValuesOperation<T> Load<T>(params IDBField[] fields);

        /// <summary>
        /// delete entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        void DeleteEntities<T>(params T[] entities);

        DeleteOperation<T> Delete<T>();

        /// <summary>
        /// get a load operation to use to load values of an entity from the database
        /// </summary>
        LoadValuesOperation<T> Load<T>(params Expression<Func<T, object>>[] fields);

        /// <summary>
        /// gets an operation which allows to insert entities to database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        InsertValuesOperation<T> Insert<T>();

        /// <summary>
        /// updates the schema of the specified type
        /// </summary>
        /// <remarks>
        /// currently this is only implemented for sqlite databases
        /// </remarks>
        /// <typeparam name="T">type of which to update schema</typeparam>
        void UpdateSchema<T>();

        /// <summary>
        /// executes a prepared operation without result
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="values"></param>
        int Execute(PreparedOperation operation, params object[] values);

        /// <summary>
        /// executes a prepared operation without result
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="values"></param>
        long ExecuteID<T>(PreparedOperation operation, params object[] values);

        /// <summary>
        /// get access to an entity model
        /// </summary>
        /// <typeparam name="T">type of entity of which to access model</typeparam>
        EntityDescriptorAccess<T> Model<T>();
    }
}