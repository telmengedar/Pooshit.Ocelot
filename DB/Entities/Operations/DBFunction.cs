using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NightlyCode.DB.Entities.Descriptors;
using NightlyCode.DB.Entities.Operations.Aggregates;
using NightlyCode.DB.Entities.Operations.Expressions;
using NightlyCode.DB.Info;

namespace NightlyCode.DB.Entities.Operations {

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

        public override void PrepareCommand(OperationPreparator preparator, IDBInfo dbinfo, Func<Type, EntityDescriptor> descriptorgetter) {
            switch(Type) {
            case DBFunctionType.Random:
                preparator.CommandBuilder.Append("RANDOM()");
                break;
            case DBFunctionType.Count:
                preparator.CommandBuilder.Append("COUNT(*)");
                break;
            case DBFunctionType.RowID:
                switch(dbinfo.Type) {
                case DBType.SQLite:
                    preparator.CommandBuilder.Append("ROWID");
                    break;
                case DBType.Postgre:
                    preparator.CommandBuilder.Append("OID");
                    break;
                }
                break;
            case DBFunctionType.Length:
                switch(dbinfo.Type) {
                case DBType.SQLite:
                    preparator.CommandBuilder.Append("LENGTH(");
                    CriteriaVisitor.GetCriteriaText(Parameter, descriptorgetter, dbinfo, preparator);
                    preparator.CommandBuilder.Append(")");
                    break;
                case DBType.Postgre:
                    preparator.CommandBuilder.Append("char_length(");
                    CriteriaVisitor.GetCriteriaText(Parameter, descriptorgetter, dbinfo, preparator);
                    preparator.CommandBuilder.Append(")");
                    break;
                }
                break;
            }
        }

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
            return new FieldAggregate("sum", field);
        }

        /// <summary>
        /// sums up a field in db
        /// </summary>
        public static Aggregate Sum<T>(params Expression<Func<T, object>>[] fields) {
            return new FieldAggregate("sum", ToFieldArray(fields));
        }

        /// <summary>
        /// sums up a field in db returning a floating point result
        /// </summary>
        public static Aggregate Total(params IDBField[] field)
        {
            return new FieldAggregate("total", field);
        }

        /// <summary>
        /// sums up a field in db returning a floating point result
        /// </summary>
        public static Aggregate Total<T>(params Expression<Func<T, object>>[] fields)
        {
            return new FieldAggregate("total", ToFieldArray(fields));
        }

        /// <summary>
        /// maximum value of a field or multiple values
        /// </summary>
        /// <param name="fields">fields of which to select maximum value</param>
        /// <returns>aggregate field</returns>
        public static Aggregate Max(params IDBField[] fields)
        {
            return new FieldAggregate("max", fields);
        }

        /// <summary>
        /// maximum value of a field or multiple values
        /// </summary>
        /// <param name="fields">fields of which to select maximum value</param>
        /// <returns>aggregate field</returns>
        public static Aggregate Max<T>(params Expression<Func<T, object>>[] fields)
        {
            return new FieldAggregate("max", ToFieldArray(fields));
        }

        /// <summary>
        /// minimum value of a field or multiple values
        /// </summary>
        /// <param name="fields">fields of which to select minimum value</param>
        /// <returns>aggregate field</returns>
        public static Aggregate Min(params IDBField[] fields)
        {
            return new FieldAggregate("min", fields);
        }

        /// <summary>
        /// minimum value of a field or multiple values
        /// </summary>
        /// <param name="fields">fields of which to select minimum value</param>
        /// <returns>aggregate field</returns>
        public static Aggregate Min<T>(params Expression<Func<T, object>>[] fields)
        {
            return new FieldAggregate("min", ToFieldArray(fields));
        }

        /// <summary>
        /// average value of a field or multiple values
        /// </summary>
        /// <param name="fields">fields of which to select average value</param>
        /// <returns>aggregate field</returns>
        public static Aggregate Average(params IDBField[] fields)
        {
            return new FieldAggregate("avg", fields);
        }

        /// <summary>
        /// average value of a field or multiple values
        /// </summary>
        /// <param name="fields">fields of which to select average value</param>
        /// <returns>aggregate field</returns>
        public static Aggregate Average<T>(params Expression<Func<T, object>>[] fields)
        {
            return new FieldAggregate("avg", ToFieldArray(fields));
        }
    }
}