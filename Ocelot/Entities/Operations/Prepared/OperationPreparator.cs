using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Expressions;
using Pooshit.Ocelot.Extern;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Tokens;

namespace Pooshit.Ocelot.Entities.Operations.Prepared {
    /// <summary>
    /// preparator for operations
    /// </summary>
    public class OperationPreparator : IOperationPreparator {
        readonly List<IOperationToken> tokens = new List<IOperationToken>();

        /// <inheritdoc />
        public IEnumerable<IOperationToken> Tokens => tokens;

        /// <summary>
        /// appends a custom array parameter to the command
        /// </summary>
        public OperationPreparator AppendArrayParameter() {
            tokens.Add(new ParameterToken(true));
            return this;
        }

        /// <summary>
        /// appends a custom array parameter to the command
        /// </summary>
        public OperationPreparator AppendArrayParameterIndex(int index) {
            tokens.Add(new ParameterToken(true) {
                Index = index
            });
            return this;
        }

        /// <summary>
        /// appends a reference to a parameter index to the command
        /// </summary>
        public OperationPreparator AppendParameter() {
            tokens.Add(new ParameterToken(false));
            return this;
        }

        /// <summary>
        /// appends a reference to a parameter index to the command
        /// </summary>
        public OperationPreparator AppendParameterIndex(int index) {
            tokens.Add(new ParameterToken(false) {
                Index = index
            });
            return this;
        }

        /// <summary>
        /// appends a parameter to the command
        /// </summary>
        /// <param name="value">value of parameter (optional)</param>
        public OperationPreparator AppendParameter(object value) {
            tokens.Add(new ParameterToken(value));
            return this;
        }

        /// <summary>
        /// appends a raw command text to the operation
        /// </summary>
        /// <param name="text"></param>
        public OperationPreparator AppendText(string text) {
            tokens.Add(new CommandTextToken(text));
            return this;
        }

        /// <summary>
        /// appends a field to this preparator
        /// </summary>
        /// <param name="field">field to append</param>
        /// <param name="dbinfo">db info for database specific formatting</param>
        /// <param name="modelinfo">access to entity models</param>
        /// <param name="tablealias">alias to use when resolving properties</param>
        /// <returns>this preparator for fluent behavior</returns>
        public IOperationPreparator AppendField(IDBField field, IDBInfo dbinfo, Func<Type, EntityDescriptor> modelinfo, string tablealias = null) {
            dbinfo.Append(field, this, modelinfo, tablealias);
            return this;
        }

        /// <inheritdoc />
        public IOperationPreparator AppendExpression(Expression expression, IDBInfo dbinfo, Func<Type, EntityDescriptor> modelinfo, string tablealias = null) {
            CriteriaVisitor.GetCriteriaText(expression, modelinfo, dbinfo, this, tablealias);
            return this;
        }

        bool PrepareParameters(IDBClient client) {
            
            int index = 1;
            if (client.DBInfo.SupportsArrayParameters) {
                foreach(ParameterToken token in tokens.OfType<ParameterToken>().Where(o => o.IsConstant && o.Index == -1))
                    token.Index = index++;
                foreach(ParameterToken token in tokens.OfType<ParameterToken>().Where(o => !o.IsConstant && o.Index == -1))
                    token.Index = index++;
                return false;
            }

            foreach (ParameterToken token in tokens.OfType<ParameterToken>().Where(o => !o.IsArray && o.IsConstant && o.Index == -1))
                token.Index = index++;
            foreach (ParameterToken token in tokens.OfType<ParameterToken>().Where(o => !o.IsArray && !o.IsConstant && o.Index == -1))
                token.Index = index++;

            index = 0;
            foreach(ParameterToken token in tokens.OfType<ParameterToken>().Where(o => o.IsArray && o.IsConstant && o.Index == -1))
                token.Index = index++;
            foreach(ParameterToken token in tokens.OfType<ParameterToken>().Where(o => o.IsArray && !o.IsConstant && o.Index == -1))
                token.Index = index++;
            return index > 0;
        }

        string GetCommandText(IDBInfo dbinfo) {
            return string.Join(" ", tokens.Select(t => t.GetText(dbinfo)));
        }

        object GetParameterValue(ParameterToken parameter) {
            if(parameter.Value is Enum enumvalue)
                return Converter.Convert(enumvalue, Enum.GetUnderlyingType(enumvalue.GetType()), true);

            return parameter.Value;
        }

        /// <inheritdoc />
        public PreparedOperation GetOperation(IDBClient dbclient, bool dbPrepare) {
            if(PrepareParameters(dbclient))
                return new PreparedArrayOperation(dbclient,
                    GetCommandText(dbclient.DBInfo),
                    tokens.OfType<ParameterToken>()
                        .Where(p => p.IsConstant && !p.IsArray)
                        .Select(GetParameterValue).ToArray(),
                    tokens.OfType<ParameterToken>()
                        .Where(p => p.IsConstant && p.IsArray)
                        .Select(GetParameterValue).Cast<Array>().ToArray());

            return new PreparedOperation(dbclient,
                GetCommandText(dbclient.DBInfo),
                tokens.OfType<ParameterToken>()
                    .Where(p => p.IsConstant)
                    .Select(GetParameterValue).ToArray(), dbPrepare);
        }


        /// <inheritdoc />
        public PreparedOperation GetReturnIdOperation(IDBClient dbclient, bool dbPrepare) {
            if(PrepareParameters(dbclient))
                return new PreparedArrayOperation(dbclient,
                    GetCommandText(dbclient.DBInfo),
                    tokens.OfType<ParameterToken>()
                        .Where(p => p.IsConstant && !p.IsArray)
                        .Select(GetParameterValue).ToArray(),
                    tokens.OfType<ParameterToken>()
                        .Where(p => p.IsConstant && p.IsArray)
                        .Select(GetParameterValue).Cast<Array>().ToArray());

            return new PreparedReturnIdOperation(dbclient,
                GetCommandText(dbclient.DBInfo),
                tokens.OfType<ParameterToken>()
                    .Where(p => p.IsConstant)
                    .Select(GetParameterValue).ToArray(), dbPrepare);
        }

        /// <inheritdoc />
        public PreparedLoadOperation GetLoadValuesOperation(IDBClient dbclient, Func<Type, EntityDescriptor> modelcache, bool dbPrepare) {
            if(PrepareParameters(dbclient))
                return new PreparedArrayLoadOperation(dbclient, modelcache,
                    GetCommandText(dbclient.DBInfo),
                    tokens.OfType<ParameterToken>()
                        .Where(p => p.IsConstant && !p.IsArray)
                        .Select(GetParameterValue).ToArray(),
                    tokens.OfType<ParameterToken>()
                        .Where(p => p.IsConstant && p.IsArray)
                        .Select(GetParameterValue).Cast<Array>().ToArray());

            return new PreparedLoadOperation(dbclient, modelcache,
                GetCommandText(dbclient.DBInfo),
                tokens.OfType<ParameterToken>()
                    .Where(p => p.IsConstant)
                    .Select(GetParameterValue).ToArray(), dbPrepare);
        }

        /// <summary>
        /// create prepared operation
        /// </summary>
        /// <param name="dbclient">client used to execute operation</param>
        /// <param name="modelcache">cache for entity models</param>
        /// <param name="dbPrepare">indicates whether to prepare statement in database</param>
        /// <returns>operation which can get executed</returns>
        public PreparedLoadOperation<T> GetLoadValuesOperation<T>(IDBClient dbclient, Func<Type, EntityDescriptor> modelcache, bool dbPrepare) {
            if(PrepareParameters(dbclient))
                return new PreparedArrayLoadOperation<T>(dbclient, modelcache,
                    GetCommandText(dbclient.DBInfo),
                    tokens.OfType<ParameterToken>()
                        .Where(p => p.IsConstant && !p.IsArray)
                        .Select(GetParameterValue).ToArray(),
                    tokens.OfType<ParameterToken>()
                        .Where(p => p.IsConstant && p.IsArray)
                        .Select(GetParameterValue).Cast<Array>().ToArray());

            return new PreparedLoadOperation<T>(dbclient, modelcache,
                GetCommandText(dbclient.DBInfo),
                tokens.OfType<ParameterToken>()
                    .Where(p => p.IsConstant)
                    .Select(GetParameterValue).ToArray(), dbPrepare);
        }

        /// <summary>
        /// create bulk insert operation from prepared data
        /// </summary>
        /// <param name="dbclient">client used to execute operation</param>
        /// <returns>operation to use to insert bulk data</returns>
        public PreparedBulkInsertOperation GetBulkInsertOperation(IDBClient dbclient) {
            return new PreparedBulkInsertOperation(dbclient, GetCommandText(dbclient.DBInfo));
        }
    }
}