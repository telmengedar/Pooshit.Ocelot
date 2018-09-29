using System;
using NightlyCode.DB.Entities.Descriptors;
using NightlyCode.DB.Info;

namespace NightlyCode.DB.Entities.Operations.Aggregates {

    /// <summary>
    /// aggregate function
    /// </summary>
    public abstract class Aggregate : DBField {
        readonly string method;

        /// <summary>
        /// creates a new <see cref="Aggregate"/>
        /// </summary>
        /// <param name="method">aggregate method</param>
        protected Aggregate(string method) {
            this.method = method;
        }

        /// <summary>
        /// appends field arguments for aggregate method to operation
        /// </summary>
        /// <param name="preparator">database operation</param>
        /// <param name="dbinfo">database info</param>
        /// <param name="descriptorgetter">access to model schemata</param>
        protected abstract void AppendFields(OperationPreparator preparator, IDBInfo dbinfo, Func<Type, EntityDescriptor> descriptorgetter);

        /// <summary>
        /// text used to represent the field
        /// </summary>
        public override void PrepareCommand(OperationPreparator preparator, IDBInfo dbinfo, Func<Type, EntityDescriptor> descriptorgetter) {
            preparator.CommandBuilder.Append(method).Append("(");
            AppendFields(preparator, dbinfo, descriptorgetter);
            preparator.CommandBuilder.Append(")");
        }
    }
}