using System;
using System.Collections.Generic;
using System.Linq;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Extern;
using NightlyCode.Database.Fields;
using NightlyCode.Database.Info;
using NightlyCode.Database.Tokens;

namespace NightlyCode.Database.Entities.Operations.Prepared {
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
            if(field is ISqlToken sqlfield)
                sqlfield.ToSql(dbinfo, this, modelinfo, tablealias);
            else
                dbinfo.Append(field, this, modelinfo, tablealias);
            return this;
        }

        bool PrepareParameters() {
            int index = 1;
            foreach(ParameterToken token in tokens.OfType<ParameterToken>().Where(o => !o.IsArray && o.IsConstant && o.Index == -1))
                token.Index = index++;
            foreach(ParameterToken token in tokens.OfType<ParameterToken>().Where(o => !o.IsArray && !o.IsConstant && o.Index == -1))
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
        /// <summary>
        /// create prepared operation
        /// </summary>
        /// <param name="dbclient">client used to execute operation</param>
        /// <returns>operation which can get executed</returns>
        public PreparedOperation GetOperation(IDBClient dbclient) {
            if(PrepareParameters())
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
                    .Select(GetParameterValue).ToArray());
        }

        /// <summary>
        /// create prepared operation
        /// </summary>
        /// <param name="dbclient">client used to execute operation</param>
        /// <returns>operation which can get executed</returns>
        public PreparedOperation GetReturnIdOperation(IDBClient dbclient) {
            if(PrepareParameters())
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
                    .Select(GetParameterValue).ToArray());
        }

        /// <summary>
        /// create prepared operation
        /// </summary>
        /// <param name="dbclient">client used to execute operation</param>
        /// <param name="modelcache">cached entity models</param>
        /// <returns>operation which can get executed</returns>
        public PreparedLoadOperation GetLoadValuesOperation(IDBClient dbclient, Func<Type, EntityDescriptor> modelcache) {
            if(PrepareParameters())
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
                    .Select(GetParameterValue).ToArray());
        }

        /// <summary>
        /// create prepared operation
        /// </summary>
        /// <param name="dbclient">client used to execute operation</param>
        /// <param name="modelcache">cached entity models</param>
        /// <returns>operation which can get executed</returns>
        public PreparedLoadOperation<T> GetLoadValuesOperation<T>(IDBClient dbclient, Func<Type, EntityDescriptor> modelcache) {
            if(PrepareParameters())
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
                    .Select(GetParameterValue).ToArray());
        }
    }
}