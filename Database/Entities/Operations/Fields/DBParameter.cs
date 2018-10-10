using System;
using Database.Entities.Descriptors;
using Database.Info;

namespace Database.Entities.Operations {
    
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
        public int Index => index;

        /// <summary>
        /// property to use for comparision in expressions
        /// </summary>
        public object Value { get; set; }

        #region expression fields

        /// <summary>
        /// field to use in expressions when referencing a <see cref="Guid"/> parameter
        /// </summary>
        public static Guid Guid => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");

        /// <summary>
        /// field to use in expressions when referencing a <see cref="int"/> parameter
        /// </summary>
        public static int Int32 => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");

        /// <summary>
        /// field to use in expressions when referencing a <see cref="long"/> parameter
        /// </summary>
        public static long Int64 => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");

        /// <summary>
        /// field to use in expressions when referencing a <see cref="string"/> parameter
        /// </summary>
        public static string String => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");

        /// <summary>
        /// field to use in expressions when referencing a <see cref="string"/> parameter
        /// </summary>
        public static float Single => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");

        /// <summary>
        /// field to use in expressions when referencing a <see cref="string"/> parameter
        /// </summary>
        public static double Double => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");

        /// <summary>
        /// field to use in expressions when referencing a <see cref="T:byte[]"/> parameter
        /// </summary>
        public static byte[] Blob => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");
        #endregion
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