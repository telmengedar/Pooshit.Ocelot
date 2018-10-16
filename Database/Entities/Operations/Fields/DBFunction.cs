using System;
using System.Collections.Generic;
using System.Linq;
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

        static IDBField ToField<T>(Expression<Func<T, object>> expression) {
            return EntityField.Create(expression);
        }

        static IEnumerable<IDBField> ToFields<T>(IEnumerable<Expression<Func<T, object>>> expressions)
        {
            return expressions.Select(ToField);
        }

        static IDBField[] ToFieldArray<T>(IEnumerable<Expression<Func<T, object>>> expressions)
        {
            return ToFields(expressions).ToArray();
        }

        /// <summary>
        /// random value
        /// </summary>
        public static DBFunction Random => new DBFunction(DBFunctionType.Random);

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
        /// sums up a field in db
        /// </summary>
        public static Aggregate Sum(params IDBField[] field)
        {
            return new Aggregate("sum", field);
        }

        /// <summary>
        /// sums up a field in db
        /// </summary>
        public static Aggregate Sum<T>(params Expression<Func<T, object>>[] fields) {
            return new Aggregate("sum", ToFieldArray(fields));
        }

        /// <summary>
        /// sums up a field in db returning a floating point result
        /// </summary>
        public static Aggregate Total(params IDBField[] field)
        {
            return new Aggregate("total", field);
        }

        /// <summary>
        /// sums up a field in db returning a floating point result
        /// </summary>
        public static Aggregate Total<T>(params Expression<Func<T, object>>[] fields)
        {
            return new Aggregate("total", ToFieldArray(fields));
        }

        /// <summary>
        /// maximum value of a field or multiple values
        /// </summary>
        /// <param name="fields">fields of which to select maximum value</param>
        /// <returns>aggregate field</returns>
        public static Aggregate Max(params IDBField[] fields)
        {
            return new Aggregate("max", fields);
        }

        /// <summary>
        /// maximum value of a field or multiple values
        /// </summary>
        /// <param name="fields">fields of which to select maximum value</param>
        /// <returns>aggregate field</returns>
        public static Aggregate Max<T>(params Expression<Func<T, object>>[] fields)
        {
            return new Aggregate("max", ToFieldArray(fields));
        }

        /// <summary>
        /// minimum value of a field or multiple values
        /// </summary>
        /// <param name="fields">fields of which to select minimum value</param>
        /// <returns>aggregate field</returns>
        public static Aggregate Min(params IDBField[] fields)
        {
            return new Aggregate("min", fields);
        }

        /// <summary>
        /// minimum value of a field or multiple values
        /// </summary>
        /// <param name="fields">fields of which to select minimum value</param>
        /// <returns>aggregate field</returns>
        public static Aggregate Min<T>(params Expression<Func<T, object>>[] fields)
        {
            return new Aggregate("min", ToFieldArray(fields));
        }

        /// <summary>
        /// average value of a field or multiple values
        /// </summary>
        /// <param name="fields">fields of which to select average value</param>
        /// <returns>aggregate field</returns>
        public static Aggregate Average(params IDBField[] fields)
        {
            return new Aggregate("avg", fields);
        }

        /// <summary>
        /// average value of a field or multiple values
        /// </summary>
        /// <param name="fields">fields of which to select average value</param>
        /// <returns>aggregate field</returns>
        public static Aggregate Average<T>(params Expression<Func<T, object>>[] fields)
        {
            return new Aggregate("avg", ToFieldArray(fields));
        }
    }
}