using System.Collections.Generic;
using System.Threading.Tasks;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Models;

namespace Pooshit.Ocelot.Schemas {
    
    /// <summary>
    /// service handling database schemas
    /// </summary>
    public interface ISchemaService {

        /// <summary>
        /// database used by schema service
        /// </summary>
        public IDBClient Database { get; }
        
        /// <summary>
        /// creates a new table
        /// </summary>
        /// <param name="schema">schema to create</param>
        /// <param name="transaction">transaction to use (optional)</param>
        Task CreateSchema(Schema schema, Transaction transaction = null);

        /// <summary>
        /// creates a new schema
        /// </summary>
        /// <param name="transaction">transaction to use (optional)</param>
        Task CreateSchema<T>(Transaction transaction = null);

        /// <summary>
        /// creates a new schema if it is not already in database, otherwise the existing one is updated
        /// </summary>
        /// <param name="transaction">transaction to use (optional)</param>
        Task CreateOrUpdateSchema<T>(Transaction transaction = null);

        /// <summary>
        /// checks whether a schema exists in database
        /// </summary>
        /// <param name="name">name of schema to check</param>
        /// <param name="transaction">transaction to use</param>
        /// <returns>true if schema exists in database, false otherwise</returns>
        Task<bool> ExistsSchema(string name, Transaction transaction=null);

        /// <summary>
        /// checks whether a type exists in database
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <typeparam name="T">type to check</typeparam>
        /// <returns>true if schema exists in database, false otherwise</returns>
        Task<bool> ExistsSchema<T>(Transaction transaction=null);
        
        /// <summary>
        /// updates a table schema
        /// </summary>
        /// <param name="name">name of schema to update</param>
        /// <param name="schema">schema to create</param>
        /// <param name="transaction">transaction to use (optional)</param>
        Task UpdateSchema(string name, Schema schema, Transaction transaction = null);

        /// <summary>
        /// updates a table schema
        /// </summary>
        /// <param name="transaction">transaction to use (optional)</param>
        Task UpdateSchema<T>(Transaction transaction = null);

        /// <summary>
        /// lists schemata (tables and views) in database
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="options">options to apply</param>
        /// <returns>list of schemata in database</returns>
        Task<IEnumerable<Schema>> ListSchemata(PageOptions options = null, Transaction transaction = null);
        
        /// <summary>
        /// get structure of a schema in database
        /// </summary>
        /// <param name="name">name of schema to get</param>
        /// <param name="transaction">transaction to use (optional)</param>
        Task<Schema> GetSchema(string name, Transaction transaction = null);

        /// <summary>
        /// removes a schema from database
        /// </summary>
        /// <param name="name">name of schema to remove</param>
        /// <param name="transaction">transaction to use (optional)</param>
        Task RemoveSchema(string name, Transaction transaction = null);

        /// <summary>
        /// removes a schema from database
        /// </summary>
        /// <param name="transaction">transaction to use (optional)</param>
        /// <typeparam name="T">type of model specifying schema to remove</typeparam>
        Task RemoveSchema<T>(Transaction transaction = null);
        
        /// <summary>
        /// get type of an existing schema
        /// </summary>
        /// <param name="name">name of schema of which to get type</param>
        /// <param name="transaction">transaction to use (optional)</param>
        /// <returns>type of schema</returns>
        Task<SchemaType> GetSchemaType(string name, Transaction transaction = null);
    }
}