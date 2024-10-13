using System.Threading.Tasks;
using Pooshit.Ocelot.Clients;

namespace Pooshit.Ocelot.Statements {

    /// <summary>
    /// statement to execute on a database
    /// </summary>
    public interface IStatement {

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <param name="parameters">parameters for statement</param>
        /// <returns>number of rows affected</returns>
        long Execute(params object[] parameters);

        /// <summary>
        /// executes the statement using an async operation
        /// </summary>
        /// <param name="parameters">parameters for statement</param>
        /// <returns>number of rows affected</returns>
        Task<long> ExecuteAsync(params object[] parameters);

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <param name="transaction">transaction to use for execution</param>
        /// <param name="parameters">parameters for statement</param>
        /// <returns>number of rows affected</returns>
        long Execute(Transaction transaction, params object[] parameters);

        /// <summary>
        /// executes the statement using an async operation
        /// </summary>
        /// <param name="transaction">transaction to use for execution</param>
        /// <param name="parameters">parameters for statement</param>
        /// <returns>number of rows affected</returns>
        Task<long> ExecuteAsync(Transaction transaction, params object[] parameters);
    }
}