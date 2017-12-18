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
        internal Aggregate(AggregateType type, params IDBField[] field) {
            Type = type;
            Alias = "agg" + id++;
            Arguments = field;
        }

        /// <summary>
        /// type of aggregate function
        /// </summary>
        public AggregateType Type { get; }

        /// <summary>
        /// content of the function
        /// </summary>
        public IDBField[] Arguments { get; }

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
                case AggregateType.Average:
                    return "avg";
                default:
                    throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// text used to represent the field
        /// </summary>
        public override void PrepareCommand(OperationPreparator preparator, IDBInfo dbinfo, Func<Type, EntityDescriptor> descriptorgetter) {
            bool second = false;
            preparator.CommandBuilder.Append(Method).Append("(");
            foreach(IDBField field in Arguments) {
                if(second)
                    preparator.CommandBuilder.Append(", ");

                field.PrepareCommand(preparator, dbinfo, descriptorgetter);
                second = true;
            }
            preparator.CommandBuilder.Append(")");
        }

        /// <summary>
        /// sums up a field in db
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Aggregate Sum<T>(params IDBField[] field) {
            return new Aggregate(AggregateType.Sum, field);
        }

        /// <summary>
        /// maximum value of a field or multiple values
        /// </summary>
        /// <param name="field">fields of which to select maximum value</param>
        /// <returns>aggregate field</returns>
        public static Aggregate Max(params IDBField[] field) {
            return new Aggregate(AggregateType.Max, field);
        }

        /// <summary>
        /// average value of a field or multiple values
        /// </summary>
        /// <param name="field">fields of which to select average value</param>
        /// <returns>aggregate field</returns>
        public static Aggregate Average(params IDBField[] field) {
            return new Aggregate(AggregateType.Average, field);
        }
    }
}