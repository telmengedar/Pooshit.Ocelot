using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;

namespace NightlyCode.Database.Entities.Operations.Prepared {

    /// <summary>
    /// prepares operations to be executed on database
    /// </summary>
    public interface IOperationPreparator {

        /// <summary>
        /// appends a custom array parameter to the command
        /// </summary>
        OperationPreparator AppendArrayParameter();

        /// <summary>
        /// appends a custom array parameter to the command
        /// </summary>
        OperationPreparator AppendArrayParameterIndex(int index);

        /// <summary>
        /// appends a reference to a parameter index to the command
        /// </summary>
        OperationPreparator AppendParameter();

        /// <summary>
        /// appends a reference to a parameter index to the command
        /// </summary>
        OperationPreparator AppendParameterIndex(int index);

        /// <summary>
        /// appends a parameter to the command
        /// </summary>
        /// <param name="value">value of parameter (optional)</param>
        OperationPreparator AppendParameter(object value);

        /// <summary>
        /// appends a raw command text to the operation
        /// </summary>
        /// <param name="text"></param>
        OperationPreparator AppendText(string text);

        /// <summary>
        /// create prepared operation
        /// </summary>
        /// <param name="dbclient">client used to execute operation</param>
        /// <returns>operation which can get executed</returns>
        PreparedOperation GetOperation(IDBClient dbclient);

        /// <summary>
        /// create prepared operation
        /// </summary>
        /// <param name="dbclient">client used to execute operation</param>
        /// <returns>operation which can get executed</returns>
        PreparedOperation GetReturnIdOperation(IDBClient dbclient);

        /// <summary>
        /// create prepared operation
        /// </summary>
        /// <param name="dbclient">client used to execute operation</param>
        /// <returns>operation which can get executed</returns>
        PreparedLoadValuesOperation GetLoadValuesOperation(IDBClient dbclient);

        /// <summary>
        /// create prepared operation
        /// </summary>
        /// <param name="dbclient">client used to execute operation</param>
        /// <returns>operation which can get executed</returns>
        PreparedLoadEntitiesOperation<T> GetLoadEntitiesOperation<T>(IDBClient dbclient, EntityDescriptor descriptor);
    }
}