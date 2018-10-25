using System;
using System.Linq.Expressions;

namespace NightlyCode.Database.Entities.Operations.Fields {

    /// <summary>
    /// function of db
    /// </summary>
    public class DBFunction : DBField {

        DBFunction(DBFunctionType function, Expression parameter=null) {
            Type = function;
            Parameter = parameter;
        }

        /// <summary>
        /// type of function
        /// </summary>
        public DBFunctionType Type { get; }

        /// <summary>
        /// expression describing the field
        /// </summary>
        public Expression Parameter { get; }

        /// <summary>
        /// random value
        /// </summary>
        public static DBFunction Random => new DBFunction(DBFunctionType.Random);

        /// <summary>
        /// random value
        /// </summary>
        public static DBFunction All => new DBFunction(DBFunctionType.All);

        /// <summary>
        /// count the rows of the result
        /// </summary>
        public static DBFunction Count => new DBFunction(DBFunctionType.Count);

        /// <summary>
        /// table wide unique id of row
        /// </summary>
        public static DBFunction RowID => new DBFunction(DBFunctionType.RowID);

        /// <summary>
        /// used to get id of last inserted row of the session
        /// </summary>
        public static DBFunction LastInsertID => new DBFunction(DBFunctionType.LastInsertID);

        /// <summary>
        /// length of a text or blob
        /// </summary>
        /// <typeparam name="T">type of entity containing the text or blob</typeparam>
        /// <param name="fieldexpression">expression targeting the field to measure</param>
        /// <returns></returns>
        public static DBFunction Length<T>(Expression<Func<T, object>> fieldexpression) {
            return new DBFunction(DBFunctionType.Length, fieldexpression);
        }

        /// <summary>
        /// sums up a field in db returning a floating point result
        /// </summary>
        /// <typeparam name="T">type of values</typeparam>
        /// <param name="values">values of which to get total</param>
        /// <returns>total value (sum in float)</returns>
        public static T Total<T>(params T[] values)
        {
            throw new NotImplementedException("Method has no implementation since it is only used for typed expressions");
        }

        /// <summary>
        /// get the maximum of a series of values
        /// </summary>
        /// <typeparam name="T">type of values</typeparam>
        /// <param name="values">values of which to get max</param>
        /// <returns>maximum value</returns>
        public static T Max<T>(params T[] values) {
            throw new NotImplementedException("Method has no implementation since it is only used for typed expressions");
        }

        /// <summary>
        /// get the minimum of a series of values
        /// </summary>
        /// <typeparam name="T">type of values</typeparam>
        /// <param name="values">values of which to get min</param>
        /// <returns>minimum value</returns>
        public static T Min<T>(params T[] values) {
            throw new NotImplementedException("Method has no implementation since it is only used for typed expressions");
        }

        /// <summary>
        /// get the average of a series of values
        /// </summary>
        /// <typeparam name="T">type of values</typeparam>
        /// <param name="values">values of which to get average</param>
        /// <returns>average value</returns>
        public static T Average<T>(params T[] values)
        {
            throw new NotImplementedException("Method has no implementation since it is only used for typed expressions");
        }

        /// <summary>
        /// get the sum of a column
        /// </summary>
        /// <typeparam name="T">type of values</typeparam>
        /// <param name="value">column in expression of which to get sum</param>
        /// <returns>average value</returns>
        public static T Sum<T>(T value)
        {
            throw new NotImplementedException("Method has no implementation since it is only used for typed expressions");
        }
    }
}