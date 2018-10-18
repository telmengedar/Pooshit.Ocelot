using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations;
using NightlyCode.Database.Entities.Operations.Entities;
using NightlyCode.Database.Entities.Operations.Fields;

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
        /// inserts entities to the db
        /// </summary>
        /// <typeparam name="T">type of entities to insert</typeparam>
        IEntityOperation<T> InsertEntities<T>();

        /// <summary>
        /// get a load operation for the specified entity type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        LoadEntitiesOperation<T> LoadEntities<T>();

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
        /// get a load operation to use to load values of an entity from the database
        /// </summary>
        /// <typeparam name="T">type of entity</typeparam>
        /// <param name="fields">fields to load from the db</param>
        /// <returns></returns>
        LoadValuesOperation<T> Load<T>(params IDBField[] fields);

        /// <summary>
        /// get an operation used to delete data from database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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
        /// get access to an entity model
        /// </summary>
        /// <typeparam name="T">type of entity of which to access model</typeparam>
        EntityDescriptorAccess<T> Model<T>();
    }
}