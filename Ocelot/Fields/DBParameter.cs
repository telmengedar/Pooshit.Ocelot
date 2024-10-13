using System;

namespace Pooshit.Ocelot.Fields {
    
    /// <summary>
    /// parameter for statements
    /// </summary>
    public class DBParameter : DBField {

        internal DBParameter() { }

        /// <summary>
        /// creates a reference to a parameter
        /// </summary>
        /// <param name="index">index of parameter</param>
        /// <returns>field to use in expressions</returns>
        public static DBParameter Index(int index) {
            throw new NotImplementedException("Method has no implementation since it is only used for typed expressions");
        }

        #region expression fields

        /// <summary>
        /// field used for lambda operations
        /// </summary>
        public new static object Value => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");

        /// <summary>
        /// field to use in expressions when referencing a <see cref="bool"/> parameter
        /// </summary>
        public new static bool Bool => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");

        /// <summary>
        /// field to use in expressions when referencing a <see cref="Guid"/> parameter
        /// </summary>
        public new static Guid Guid => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");

        /// <summary>
        /// field to use in expressions when referencing a <see cref="int"/> parameter
        /// </summary>
        public new static int Int32 => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");

        /// <summary>
        /// field to use in expressions when referencing a <see cref="long"/> parameter
        /// </summary>
        public new static long Int64 => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");

        /// <summary>
        /// field to use in expressions when referencing a <see cref="string"/> parameter
        /// </summary>
        public new static string String => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");

        /// <summary>
        /// field to use in expressions when referencing a <see cref="string"/> parameter
        /// </summary>
        public new static float Single => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");

        /// <summary>
        /// field to use in expressions when referencing a <see cref="Double"/> parameter
        /// </summary>
        public new static double Double => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");

        /// <summary>
        /// field to use in expressions when referencing a <see cref="decimal"/> parameter
        /// </summary>
        public new static decimal Decimal => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");

        /// <summary>
        /// field to use in expressions when referencing a <see cref="DateTime"/> parameter
        /// </summary>
        public new static DateTime DateTime => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");

        /// <summary>
        /// field to use in expressions when referencing a <see cref="TimeSpan"/> parameter
        /// </summary>
        public new static TimeSpan TimeSpan => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");

        /// <summary>
        /// field to use in expressions when referencing a <see cref="T:byte[]"/> parameter
        /// </summary>
        public new static byte[] Blob => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");
        #endregion
    }

    /// <summary>
    /// generic parameter for specific types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DBParameter<T> : DBParameter {

        internal DBParameter() { }

        /// <summary>
        /// creates a reference to a parameter
        /// </summary>
        /// <param name="index">index of parameter</param>
        /// <returns>field to use in expressions</returns>
        public new static DBParameter<T> Index(int index)
        {
            throw new NotImplementedException("Method has no implementation since it is only used for typed expressions");
        }

        /// <summary>
        /// field to use in expressions when referencing a <see typeref="T"/> parameter
        /// </summary>
        public T Data => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");

        /// <summary>
        /// field to use in expressions when referencing a <see typeref="T"/> parameter
        /// </summary>
        public new static T Value => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");
    }
}