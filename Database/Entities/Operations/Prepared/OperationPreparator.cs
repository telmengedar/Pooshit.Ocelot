using System;
using System.Collections.Generic;
using System.Linq;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Info;

namespace NightlyCode.Database.Entities.Operations.Prepared {

    /// <summary>
    /// preparator for operations
    /// </summary>
    public class OperationPreparator {
        readonly List<IOperationToken> tokens = new List<IOperationToken>();

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
        public OperationPreparator AppendParameterIndex(int index)
        {
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

        bool PrepareParameters() {
            int index = 1;
            foreach(ParameterToken token in tokens.OfType<ParameterToken>().Where(o => !o.IsArray && o.IsConstant && o.Index==-1))
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
                        .Select(p => p.Value).ToArray(),
                    tokens.OfType<ParameterToken>()
                        .Where(p => p.IsConstant && p.IsArray)
                        .Select(p => p.Value).Cast<Array>().ToArray());

            return new PreparedOperation(dbclient,
                GetCommandText(dbclient.DBInfo),
                tokens.OfType<ParameterToken>()
                    .Where(p => p.IsConstant)
                    .Select(p => p.Value).ToArray());
        }

        /// <summary>
        /// create prepared operation
        /// </summary>
        /// <param name="dbclient">client used to execute operation</param>
        /// <returns>operation which can get executed</returns>
        public PreparedLoadValuesOperation GetLoadValuesOperation(IDBClient dbclient)
        {
            if (PrepareParameters())
                return new PreparedArrayLoadValuesOperation(dbclient,
                    GetCommandText(dbclient.DBInfo),
                    tokens.OfType<ParameterToken>()
                        .Where(p => p.IsConstant && !p.IsArray)
                        .Select(p => p.Value).ToArray(),
                    tokens.OfType<ParameterToken>()
                        .Where(p => p.IsConstant && p.IsArray)
                        .Select(p => p.Value).Cast<Array>().ToArray());

            return new PreparedLoadValuesOperation(dbclient,
                GetCommandText(dbclient.DBInfo),
                tokens.OfType<ParameterToken>()
                    .Where(p => p.IsConstant)
                    .Select(p => p.Value).ToArray());
        }

        /// <summary>
        /// create prepared operation
        /// </summary>
        /// <param name="dbclient">client used to execute operation</param>
        /// <returns>operation which can get executed</returns>
        public PreparedLoadEntitiesOperation<T> GetLoadEntitiesOperation<T>(IDBClient dbclient, EntityDescriptor descriptor)
        {
            if (PrepareParameters())
                return new PreparedArrayLoadEntitiesOperation<T>(dbclient,
                    descriptor,
                    GetCommandText(dbclient.DBInfo),
                    tokens.OfType<ParameterToken>()
                        .Where(p => p.IsConstant && !p.IsArray)
                        .Select(p => p.Value).ToArray(),
                    tokens.OfType<ParameterToken>()
                        .Where(p => p.IsConstant && p.IsArray)
                        .Select(p => p.Value).Cast<Array>().ToArray());

            return new PreparedLoadEntitiesOperation<T>(dbclient,
                descriptor,
                GetCommandText(dbclient.DBInfo),
                tokens.OfType<ParameterToken>()
                    .Where(p => p.IsConstant)
                    .Select(p => p.Value).ToArray());
        }
    }
}