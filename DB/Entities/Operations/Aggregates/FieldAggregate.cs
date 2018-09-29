using System;
using System.Linq;
using NightlyCode.DB.Entities.Descriptors;
using NightlyCode.DB.Info;

namespace NightlyCode.DB.Entities.Operations.Aggregates {

    /// <summary>
    /// aggregate with <see cref="IDBField"/> arguments
    /// </summary>
    public class FieldAggregate : Aggregate {

        /// <summary>
        /// creates a new <see cref="FieldAggregate"/>
        /// </summary>
        /// <param name="method">aggregate method</param>
        /// <param name="arguments">arguments for method</param>
        internal FieldAggregate(string method, params IDBField[] arguments)
            : base(method) {
            Arguments = arguments;
        }

        /// <summary>
        /// content of the function
        /// </summary>
        public IDBField[] Arguments { get; }

        protected override void AppendFields(OperationPreparator preparator, IDBInfo dbinfo, Func<Type, EntityDescriptor> descriptorgetter) {
            if(Arguments.Length == 0)
                return;

            Arguments[0].PrepareCommand(preparator, dbinfo, descriptorgetter);
            foreach (IDBField field in Arguments.Skip(1))
            {
                preparator.CommandBuilder.Append(", ");
                field.PrepareCommand(preparator, dbinfo, descriptorgetter);
            }
        }
    }
}