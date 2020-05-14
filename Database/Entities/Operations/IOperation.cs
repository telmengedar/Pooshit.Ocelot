using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Statements;

namespace NightlyCode.Database.Entities.Operations {

    /// <summary>
    /// operation to prepare for execution
    /// </summary>
    public interface IOperation {

        /// <summary>
        /// prepares the operation for execution
        /// </summary>
        /// <returns></returns>
        PreparedOperation Prepare();
    }
}