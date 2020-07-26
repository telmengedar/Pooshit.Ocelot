using System;

// it makes no real sense to implement HashCode and Equals with DBFields
// it could be argued whether at least Equals should be supported in CriteriaVisitor
#pragma warning disable 660,661

namespace NightlyCode.Database.Fields {

    /// <summary>
    /// interface for a db field
    /// </summary>
    public interface IDBField {

        #region expression fields

        /// <summary>
        /// field used for lambda operations
        /// </summary>
        object Value { get; }

        /// <summary>
        /// field used for comparision
        /// </summary>
        int Int32 { get; }

        /// <summary>
        /// field used for comparision
        /// </summary>
        DateTime DateTime { get; }

        /// <summary>
        /// field to use in expressions when referencing a <see cref="Guid"/> parameter
        /// </summary>
        Guid Guid { get; }

        /// <summary>
        /// field to use in expressions when referencing a <see cref="long"/> parameter
        /// </summary>
        long Int64 { get; }

        /// <summary>
        /// field to use in expressions when referencing a <see cref="string"/> parameter
        /// </summary>
        string String { get; }

        /// <summary>
        /// field to use in expressions when referencing a <see cref="string"/> parameter
        /// </summary>
        float Single { get; }

        /// <summary>
        /// field to use in expressions when referencing a <see cref="string"/> parameter
        /// </summary>
        double Double { get; }

        /// <summary>
        /// field to use in expressions when referencing a <see cref="T:byte[]"/> parameter
        /// </summary>
        byte[] Blob { get; }

        #endregion
    }

    /// <summary>
    /// base implementation of <see cref="IDBField"/> for comparision properties
    /// </summary>
    public abstract class DBField : IDBField {

        #region comparision operators

        /// <summary>
        /// comparision operator used to compute with <see cref="DBField"/>s
        /// </summary>
        /// <param name="lhs">left hand side field</param>
        /// <param name="rhs">right hand side field</param>
        public static DBField operator *(DBField lhs, DBField rhs)
        {
            throw new NotImplementedException("Method has no implementation since it is only used for typed expressions");
        }

        /// <summary>
        /// comparision operator used to compute with <see cref="DBField"/>s
        /// </summary>
        /// <param name="lhs">left hand side field</param>
        /// <param name="rhs">right hand side field</param>
        public static DBField operator /(DBField lhs, DBField rhs)
        {
            throw new NotImplementedException("Method has no implementation since it is only used for typed expressions");
        }

        /// <summary>
        /// comparision operator used to compute with <see cref="DBField"/>s
        /// </summary>
        /// <param name="lhs">left hand side field</param>
        /// <param name="rhs">right hand side field</param>
        public static DBField operator +(DBField lhs, DBField rhs)
        {
            throw new NotImplementedException("Method has no implementation since it is only used for typed expressions");
        }

        /// <summary>
        /// comparision operator used to compute with <see cref="DBField"/>s
        /// </summary>
        /// <param name="lhs">left hand side field</param>
        /// <param name="rhs">right hand side field</param>
        public static DBField operator -(DBField lhs, DBField rhs)
        {
            throw new NotImplementedException("Method has no implementation since it is only used for typed expressions");
        }

        /// <summary>
        /// comparision operator used to compare <see cref="DBField"/>s
        /// </summary>
        /// <param name="lhs">left hand side field</param>
        /// <param name="rhs">right hand side field</param>
        public static bool operator<(DBField lhs, DBField rhs) {
            throw new NotImplementedException("Method has no implementation since it is only used for typed expressions");
        }

        /// <summary>
        /// comparision operator used to compare <see cref="DBField"/>s
        /// </summary>
        /// <param name="lhs">left hand side field</param>
        /// <param name="rhs">right hand side field</param>
        public static bool operator >(DBField lhs, DBField rhs) {
            throw new NotImplementedException("Method has no implementation since it is only used for typed expressions");
        }

        /// <summary>
        /// comparision operator used to compare <see cref="DBField"/>s
        /// </summary>
        /// <param name="lhs">left hand side field</param>
        /// <param name="rhs">right hand side field</param>
        public static bool operator<=(DBField lhs, DBField rhs) {
            throw new NotImplementedException("Method has no implementation since it is only used for typed expressions");
        }

        /// <summary>
        /// comparision operator used to compare <see cref="DBField"/>s
        /// </summary>
        /// <param name="lhs">left hand side field</param>
        /// <param name="rhs">right hand side field</param>
        public static bool operator >=(DBField lhs, DBField rhs) {
            throw new NotImplementedException("Method has no implementation since it is only used for typed expressions");
        }

        /// <summary>
        /// comparision operator used to compare <see cref="DBField"/>s
        /// </summary>
        /// <param name="lhs">left hand side field</param>
        /// <param name="rhs">right hand side field</param>
        public static bool operator!=(DBField lhs, DBField rhs) {
            throw new NotImplementedException("Method has no implementation since it is only used for typed expressions");
        }

        /// <summary>
        /// comparision operator used to compare <see cref="DBField"/>s
        /// </summary>
        /// <param name="lhs">left hand side field</param>
        /// <param name="rhs">right hand side field</param>
        public static bool operator %(DBField lhs, DBField rhs) {
            throw new NotImplementedException("Method has no implementation since it is only used for typed expressions");
        }

        /// <summary>
        /// comparision operator used to compare <see cref="DBField"/>s
        /// </summary>
        /// <param name="lhs">left hand side field</param>
        /// <param name="rhs">right hand side field</param>
        public static bool operator ==(DBField lhs, DBField rhs) {
            throw new NotImplementedException("Method has no implementation since it is only used for typed expressions");
        }

        /// <summary>
        /// comparision operator used to compare <see cref="DBField"/>s
        /// </summary>
        /// <param name="lhs">left hand side field</param>
        /// <param name="rhs">right hand side field</param>
        public static bool operator ==(long lhs, DBField rhs) { throw new NotImplementedException("Method has no implementation since it is only used for typed expressions"); }

        /// <summary>
        /// comparision operator used to compare <see cref="DBField"/>s
        /// </summary>
        /// <param name="lhs">left hand side field</param>
        /// <param name="rhs">right hand side field</param>
        public static bool operator !=(long lhs, DBField rhs) { throw new NotImplementedException("Method has no implementation since it is only used for typed expressions"); }

        /// <summary>
        /// comparision operator used to compare <see cref="DBField"/>s
        /// </summary>
        /// <param name="lhs">left hand side field</param>
        /// <param name="rhs">right hand side field</param>
        public static bool operator ==(DBField lhs, long rhs) { throw new NotImplementedException("Method has no implementation since it is only used for typed expressions"); }

        /// <summary>
        /// comparision operator used to compare <see cref="DBField"/>s
        /// </summary>
        /// <param name="lhs">left hand side field</param>
        /// <param name="rhs">right hand side field</param>
        public static bool operator !=(DBField lhs, long rhs) { throw new NotImplementedException("Method has no implementation since it is only used for typed expressions"); }

        /// <summary>
        /// comparision operator used to compare <see cref="DBField"/>s
        /// </summary>
        /// <param name="lhs">left hand side field</param>
        /// <param name="rhs">right hand side field</param>
        public static bool operator ==(int lhs, DBField rhs) { throw new NotImplementedException("Method has no implementation since it is only used for typed expressions"); }

        /// <summary>
        /// comparision operator used to compare <see cref="DBField"/>s
        /// </summary>
        /// <param name="lhs">left hand side field</param>
        /// <param name="rhs">right hand side field</param>
        public static bool operator !=(int lhs, DBField rhs) { throw new NotImplementedException("Method has no implementation since it is only used for typed expressions"); }

        /// <summary>
        /// comparision operator used to compare <see cref="DBField"/>s
        /// </summary>
        /// <param name="lhs">left hand side field</param>
        /// <param name="rhs">right hand side field</param>
        public static bool operator ==(DBField lhs, int rhs) { throw new NotImplementedException("Method has no implementation since it is only used for typed expressions"); }

        /// <summary>
        /// comparision operator used to compare <see cref="DBField"/>s
        /// </summary>
        /// <param name="lhs">left hand side field</param>
        /// <param name="rhs">right hand side field</param>
        public static bool operator !=(DBField lhs, int rhs) { throw new NotImplementedException("Method has no implementation since it is only used for typed expressions"); }

        #endregion

        #region expression fields
        /// <summary>
        /// field to use in expressions when referencing a <see cref="bool"/> parameter
        /// </summary>
        public bool Bool => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");

        /// <summary>
        /// field used for lambda operations
        /// </summary>
        public object Value => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");

        /// <summary>
        /// field used for comparision
        /// </summary>
        public int Int32 => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");

        /// <summary>
        /// field used for comparision
        /// </summary>
        public DateTime DateTime => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");

        /// <summary>
        /// field to use in expressions when referencing a <see cref="IDBField.Guid"/> parameter
        /// </summary>
        public Guid Guid => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");

        /// <summary>
        /// field to use in expressions when referencing a <see cref="long"/> parameter
        /// </summary>
        public long Int64 => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");

        /// <summary>
        /// field to use in expressions when referencing a <see cref="string"/> parameter
        /// </summary>
        public string String => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");

        /// <summary>
        /// field to use in expressions when referencing a <see cref="string"/> parameter
        /// </summary>
        public float Single => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");

        /// <summary>
        /// field to use in expressions when referencing a <see cref="string"/> parameter
        /// </summary>
        public double Double => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");

        /// <summary>
        /// field to use in expressions when referencing a <see cref="T:byte[]"/> parameter
        /// </summary>
        public byte[] Blob => throw new NotImplementedException("Field has no implementation since it is only used for typed expressions");
        #endregion
    }
}