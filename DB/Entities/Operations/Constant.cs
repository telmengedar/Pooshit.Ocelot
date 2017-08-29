using System;
using NightlyCode.DB.Entities.Descriptors;
using NightlyCode.DB.Info;

namespace NightlyCode.DB.Entities.Operations {

    /// <summary>
    /// constant value
    /// </summary>
    public class Constant : DBField {

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="value"></param>
        Constant(object value) {
            Value = value;
        }

        /// <summary>
        /// value
        /// </summary>
        public object Value { get; private set; }

        public override void PrepareCommand(OperationPreparator preparator, IDBInfo dbinfo, Func<Type, EntityDescriptor> descriptorgetter) {
            if(Value==null)
                preparator.CommandBuilder.Append("NULL");
            else preparator.AppendParameter(Value);
        }

        /// <summary>
        /// creates a new constant
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Constant Create(object value) {
            return new Constant(value);
        }
    }
}