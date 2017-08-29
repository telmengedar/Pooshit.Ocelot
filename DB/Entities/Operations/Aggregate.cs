using System;
using NightlyCode.DB.Entities.Descriptors;
using NightlyCode.DB.Info;

namespace NightlyCode.DB.Entities.Operations {

    /// <summary>
    /// aggregate function
    /// </summary>
    public class Aggregate : DBField {
        static int id;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="type">type of function</param>
        /// <param name="field">field to use</param>
        internal Aggregate(AggregateType type, IDBField field) {
            Type = type;
            Alias = "agg" + id++;
            Field = field;
        }

        /// <summary>
        /// type of aggregate function
        /// </summary>
        public AggregateType Type { get; private set; }

        /// <summary>
        /// content of the function
        /// </summary>
        public IDBField Field { get; private set; }

        /// <summary>
        /// alias to use
        /// </summary>
        public string Alias { get; private set; }

        string Method {
            get {
                switch(Type) {
                case AggregateType.Sum:
                    return "sum";
                case AggregateType.Max:
                    return "max";
                default:
                    throw new NotImplementedException();
                }
            }
        }

        public override void PrepareCommand(OperationPreparator preparator, IDBInfo dbinfo, Func<Type, EntityDescriptor> descriptorgetter) {
            preparator.CommandBuilder.Append(Method).Append("(");
            Field.PrepareCommand(preparator, dbinfo, descriptorgetter);
            preparator.CommandBuilder.Append(")");
        }

        /// <summary>
        /// sums up a field in db
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Aggregate Sum<T>(IDBField field) {
            return new Aggregate(AggregateType.Sum, field);
        }

        /// <summary>
        /// maximum value of a field
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static Aggregate Max(IDBField field) {
            return new Aggregate(AggregateType.Max, field);
        }
    }
}