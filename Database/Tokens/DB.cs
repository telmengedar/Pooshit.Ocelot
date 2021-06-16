using System;
using System.Linq.Expressions;
using NightlyCode.Database.Fields;
using NightlyCode.Database.Tokens.Control;
using NightlyCode.Database.Tokens.Functions;
using NightlyCode.Database.Tokens.Values;

namespace NightlyCode.Database.Tokens {
    
    /// <summary>
    /// class used to generate function tokens
    /// </summary>
    public static class DB {

        /// <summary>
        /// specifies all columns
        /// </summary>
        public static readonly ISqlToken All = new AllColumnsToken(); 

        /// <summary>
        /// coalesce function used to return the first token which evaluates in non null
        /// </summary>
        /// <param name="tokens">tokens to evaluate</param>
        /// <returns>token to be used in statements</returns>
        public static ISqlToken Coalesce(params ISqlToken[] tokens) {
            return new AggregateFunction("COALESCE", tokens);
        }

        /// <summary>
        /// constant value
        /// </summary>
        /// <param name="value">value to add</param>
        /// <returns>token to be used in statements</returns>
        public static ISqlToken Constant(object value) {
            return new ConstantValue(value);
        }

        /// <summary>
        /// sums up values
        /// </summary>
        /// <param name="token">token identifying values to sum</param>
        /// <returns>token to be used in statements</returns>
        public static ISqlToken Sum(ISqlToken token) {
            return new AggregateFunction("SUM", token);
        }

        /// <summary>
        /// averages a series of values
        /// </summary>
        /// <param name="token">token identifying values to sum</param>
        /// <returns>token to be used in statements</returns>
        public static ISqlToken Avg(ISqlToken token) {
            return new AggregateFunction("AVG", token);
        }

        /// <summary>
        /// get a minimum of a series of values
        /// </summary>
        /// <param name="token">token identifying values of which to get minimum</param>
        /// <returns>token to be used in statements</returns>
        public static ISqlToken Min(ISqlToken token) {
            return new AggregateFunction("MIN", token);
        }

        /// <summary>
        /// get a minimum of a series of values
        /// </summary>
        /// <param name="token">token identifying values of which to get maximum</param>
        /// <returns>token to be used in statements</returns>
        public static ISqlToken Max(ISqlToken token) {
            return new AggregateFunction("MAX", token);
        }

        /// <summary>
        /// counts values of a column which are not null
        /// </summary>
        /// <remarks>
        /// use <see cref="All"/> to count all rows
        /// </remarks>
        /// <param name="token">token specifying column to count</param>
        /// <returns>token to be used in statements</returns>
        public static ISqlToken Count(ISqlToken token) {
            return new AggregateFunction("COUNT", token);
        }

        /// <summary>
        /// creates a case statement
        /// </summary>
        /// <param name="condition">condition to evaluate</param>
        /// <param name="value">value to use when condition evaluates to true</param>
        /// <param name="elsetoken">value to use when condition evaluates to false (optional)</param>
        /// <returns>token to be used in statements</returns>
        public static ISqlToken If(ISqlToken condition, ISqlToken value, ISqlToken elsetoken = null) {
            return new IfControl(condition, value, elsetoken);
        }

        /// <summary>
        /// creates a case statement with multiple cases
        /// </summary>
        /// <param name="cases">cases to match</param>
        /// <param name="elsetoken">value to use if no case matches</param>
        /// <returns>case token</returns>
        public static CaseControl Case(When[] cases, ISqlToken elsetoken = null) {
            return new CaseControl(cases, elsetoken);
        }

        /// <summary>
        /// creates a when token to be used in case statements
        /// </summary>
        /// <param name="condition">condition of case</param>
        /// <param name="value">value to use if condition evaluates to true</param>
        /// <returns>when token</returns>
        public static When When(ISqlToken condition, ISqlToken value) {
            return new When(condition, value);
        }
        
        /// <summary>
        /// predicate used to generate sql
        /// </summary>
        /// <param name="predicate">predicate to translate</param>
        /// <typeparam name="T">type to use as expression parameter</typeparam>
        /// <returns>token to be used in statements</returns>
        public static ISqlToken Predicate<T>(Expression<Func<T, bool>> predicate) {
            return new ExpressionToken(predicate);
        }

        /// <summary>
        /// references a property of an entity using an expression
        /// </summary>
        /// <typeparam name="T">type of entity to reference</typeparam>
        /// <param name="expression">expression pointing to property</param>
        /// <returns>field to be used in statements</returns>
        public static ISqlToken Property<T>(Expression<Func<T, object>> expression) {
            return new PropertyToken(expression);
        }

        /// <summary>
        /// references a property of an entity using an expression
        /// </summary>
        /// <typeparam name="T">type of entity to reference</typeparam>
        /// <param name="expression">expression pointing to property</param>
        /// <param name="alias">alias to use for property reference</param>
        /// <returns>field to be used in statements</returns>
        public static ISqlToken Property<T>(Expression<Func<T, object>> expression, string alias) {
            return new PropertyToken(expression, alias);
        }

        /// <summary>
        /// specifies a column of a table
        /// </summary>
        /// <param name="name">name of column</param>
        /// <returns>token to be used in statements</returns>
        public static ISqlToken Column(string name) {
            return new ColumnToken(name);
        }

        /// <summary>
        /// specifies a column of a table
        /// </summary>
        /// <param name="table">name of table</param>
        /// <param name="name">name of column</param>
        /// <returns>token to be used in statements</returns>
        public static ISqlToken Column(string table, string name) {
            return new ColumnToken(table, name);
        }

        /// <summary>
        /// casts data to another type
        /// </summary>
        /// <param name="token">value to cast</param>
        /// <param name="type">type to cast value to</param>
        /// <returns>token to be used in statements</returns>
        public static IDBField Cast(ISqlToken token, CastType type) {
            return new CastToken(token, type);
        }
    }
}