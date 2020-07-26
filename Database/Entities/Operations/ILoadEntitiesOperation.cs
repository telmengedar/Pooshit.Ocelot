using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NightlyCode.Database.Clients;

namespace NightlyCode.Database.Entities.Operations {

    /// <summary>
    /// interface for a load entities operation
    /// </summary>
    public interface ILoadEntitiesOperation {

        /// <summary>
        /// loads entities from joined data
        /// </summary>
        /// <returns>all loaded entities</returns>
        IEnumerable<TEntity> Execute<TEntity>(params object[] parameters);

        /// <summary>
        /// loads entities from joined data
        /// </summary>
        /// <returns>all loaded entities</returns>
        Task<TEntity[]> ExecuteAsync<TEntity>(params object[] parameters);

        /// <summary>
        /// loads entities from joined data
        /// </summary>
        /// <returns>all loaded entities</returns>
        IEnumerable<TEntity> Execute<TEntity>(Transaction transaction, params object[] parameters);

        /// <summary>
        /// loads entities from joined data
        /// </summary>
        /// <returns>all loaded entities</returns>
        Task<TEntity[]> ExecuteAsync<TEntity>(Transaction transaction, params object[] parameters);

        /// <summary>
        /// specifies criterias for the operation
        /// </summary>
        /// <param name="criterias">filter criterias</param>
        /// <returns>this operation</returns>
        ILoadEntitiesOperation Where(Expression criterias);

        /// <summary>
        /// joins another type to the operation
        /// </summary>
        /// <param name="criteria">predicate for join criteria</param>
        /// <param name="additionalcriterias">additional criterias for join</param>
        /// <returns>this operation</returns>
        ILoadEntitiesOperation Join<TJoin>(Expression criteria, Expression additionalcriterias = null);
    }
}