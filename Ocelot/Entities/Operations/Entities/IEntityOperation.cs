using System.Collections.Generic;
using Pooshit.Ocelot.Clients;

namespace Pooshit.Ocelot.Entities.Operations.Entities {

    /// <summary>
    /// operation to be executed on entities
    /// </summary>
    /// <typeparam name="T">type of entities</typeparam>
    public interface IEntityOperation<T> {

        /// <summary>
        /// executes the operation
        /// </summary>
        /// <param name="entities">entities on which to operate</param>
        /// <returns>number of affected rows</returns>
        long Execute(params T[] entities);

        /// <summary>
        /// executes the operation using a transaction
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="entities">entities on which to operate</param>
        /// <returns>number of affected rows</returns>
        long Execute(Transaction transaction, params T[] entities);

        /// <summary>
        /// executes the operation
        /// </summary>
        /// <param name="entities">entities on which to operate</param>
        /// <returns>number of affected rows</returns>
        long Execute(IEnumerable<T> entities);

        /// <summary>
        /// executes the operation using a transaction
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="entities">entities on which to operate</param>
        /// <returns>number of affected rows</returns>
        long Execute(Transaction transaction, IEnumerable<T> entities);
    }
}