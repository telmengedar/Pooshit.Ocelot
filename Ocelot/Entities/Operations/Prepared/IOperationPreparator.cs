using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Entities.Operations.Prepared {

    /// <summary>
    /// prepares operations to be executed on database
    /// </summary>
    public interface IOperationPreparator {

        /// <summary>
        /// tokens in operation
        /// </summary>
        IEnumerable<IOperationToken> Tokens { get; }

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
        /// <param name="dbPrepare">indicates whether to prepare statement in database</param>
        /// <returns>operation which can get executed</returns>
        PreparedOperation GetOperation(IDBClient dbclient, bool dbPrepare);

        /// <summary>
        /// create prepared operation
        /// </summary>
        /// <param name="dbclient">client used to execute operation</param>
        /// <param name="dbPrepare">indicates whether to prepare statement in database</param>
        /// <returns>operation which can get executed</returns>
        PreparedOperation GetReturnIdOperation(IDBClient dbclient, bool dbPrepare);

        /// <summary>
        /// create prepared operation
        /// </summary>
        /// <param name="dbclient">client used to execute operation</param>
        /// <param name="modelcache">cache for entity models</param>
        /// <param name="dbPrepare">indicates whether to prepare statement in database</param>
        /// <returns>operation which can get executed</returns>
        PreparedLoadOperation GetLoadValuesOperation(IDBClient dbclient, Func<Type, EntityDescriptor> modelcache, bool dbPrepare);

        /// <summary>
        /// appends a field to this preparator
        /// </summary>
        /// <param name="field">field to append</param>
        /// <param name="dbinfo">db info for database specific formatting</param>
        /// <param name="modelinfo">access to entity models</param>
        /// <param name="tablealias">alias to use when resolving properties</param>
        /// <returns>this preparator for fluent behavior</returns>
        IOperationPreparator AppendField(IDBField field, IDBInfo dbinfo, Func<Type, EntityDescriptor> modelinfo, string tablealias = null);

        /// <summary>
        /// appends an expression to the operation text
        /// </summary>
        /// <param name="expression">expression to append</param>
        /// <param name="dbinfo">database specific info</param>
        /// <param name="modelinfo">used to get model descriptors</param>
        /// <param name="tablealias">alias to use for current table</param>
        /// <returns>this preparator for fluent behavior</returns>
        IOperationPreparator AppendExpression(Expression expression, IDBInfo dbinfo, Func<Type, EntityDescriptor> modelinfo, string tablealias = null);
    }
}