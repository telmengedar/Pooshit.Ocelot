using System;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Fields;
using NightlyCode.Database.Info;

namespace NightlyCode.Database.Tokens.Functions {
    
    /// <summary>
    /// aggregate function
    /// </summary>
    public class AggregateFunction : SqlToken {
        
        /// <summary>
        /// creates a new <see cref="Aggregate"/>
        /// </summary>
        /// <param name="method">aggregate method</param>
        /// <param name="arguments">arguments for method</param>
        internal AggregateFunction(string method, params ISqlToken[] arguments) {
            Method = method;
            Arguments = arguments;
        }

        /// <summary>
        /// method name
        /// </summary>
        public string Method { get; }

        /// <summary>
        /// content of the function
        /// </summary>
        public ISqlToken[] Arguments { get; }

        /// <inheritdoc />
        public override void ToSql(IDBInfo dbinfo, IOperationPreparator preparator, Func<Type, EntityDescriptor> models, string tablealias) {
            preparator.AppendText(Method);
            preparator.AppendText("(");
            bool first = true;
            foreach (ISqlToken argument in Arguments) {
                if (first)
                    first = false;
                else preparator.AppendText(",");
                argument.ToSql(dbinfo, preparator, models, tablealias);
            }
            preparator.AppendText(")");
        }
    }
}