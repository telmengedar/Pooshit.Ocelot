using Pooshit.Ocelot.Entities.Operations.Prepared;

namespace Pooshit.Ocelot.Entities.Operations;

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