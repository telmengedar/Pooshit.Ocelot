using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Entities.Operations.Prepared;

/// <summary>
/// token in <see cref="OperationPreparator"/>
/// </summary>
public interface IOperationToken {

    /// <summary>
    /// get text for database command
    /// </summary>
    /// <param name="dbinfo">database specific information</param>
    /// <returns>text representing this token</returns>
    string GetText(IDBInfo dbinfo);
}