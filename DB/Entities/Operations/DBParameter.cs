using System;
using NightlyCode.DB.Entities.Descriptors;
using NightlyCode.DB.Info;

namespace NightlyCode.DB.Entities.Operations {
    
    /// <summary>
    /// parameter for statements
    /// </summary>
    public class DBParameter : DBField {
        readonly int index;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="index"></param>
        public DBParameter(int index) {
            this.index = index;
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public DBParameter(int index, object value) {
            this.index = index;
            Value = value;
        }

        /// <summary>
        /// index of the parameter
        /// </summary>
        public int Index { get { return index; } }

        /// <summary>
        /// property to use for comparision in expressions
        /// </summary>
        public object Value { get; set; }

        public override void PrepareCommand(OperationPreparator preparator, IDBInfo dbinfo, Func<Type, EntityDescriptor> descriptorgetter) {
            preparator.CommandBuilder.Append(dbinfo.Parameter + index);
        }
    }

    /// <summary>
    /// generic parameter for specific types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DBParameter<T> : DBParameter {

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="index"></param>
        public DBParameter(int index)
            : base(index) {}

        /// <summary>
        /// property to use for comparision in expressions
        /// </summary>
        public new T Value { get { return default(T); } }
    }
}