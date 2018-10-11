using System;

namespace NightlyCode.Database.Entities.Operations.Fields {

    /// <summary>
    /// interface for a db field
    /// </summary>
    public interface IDBField {

        /// <summary>
        /// field used for lambda operations
        /// </summary>
        object Value { get; }

        /// <summary>
        /// field used for comparision
        /// </summary>
        int Int { get; }

        /// <summary>
        /// field used for comparision
        /// </summary>
        DateTime DateTime { get; }
    }

    public abstract class DBField : IDBField {

        public static bool operator<(DBField lhs, DBField rhs) {
            throw new NotImplementedException();
        }

        public static bool operator >(DBField lhs, DBField rhs) {
            throw new NotImplementedException();
        }

        public static bool operator<=(DBField lhs, DBField rhs) {
            throw new NotImplementedException();
        }

        public static bool operator >=(DBField lhs, DBField rhs) {
            throw new NotImplementedException();
        }

        public static bool operator!=(DBField lhs, DBField rhs) {
            throw new NotImplementedException();
        }

        public static bool operator %(DBField lhs, DBField rhs) {
            throw new NotImplementedException();
        }

        public static bool operator ==(DBField lhs, DBField rhs) {
            return !(lhs != rhs);
        }

        public object Value {
            get { throw new NotImplementedException(); }
        }

        public int Int {
            get { throw new NotImplementedException(); }
        }

        public DateTime DateTime {
            get { throw new NotImplementedException(); }
        }
    }
}