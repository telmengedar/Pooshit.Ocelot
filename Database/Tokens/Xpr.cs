using System;

namespace NightlyCode.Database.Tokens {
    
    /// <summary>
    /// provides functions and tokens to expressions
    /// </summary>
    public static class Xpr {
    
        /// <summary>
        /// coalesce function used to return the first token which evaluates in non null
        /// </summary>
        /// <param name="tokens">tokens to evaluate</param>
        /// <returns>token to be used in statements</returns>
        public static ISqlToken Coalesce(params ISqlToken[] tokens) {
            throw new NotImplementedException("Only to be used in expression trees");
        }

        /// <summary>
        /// constant value
        /// </summary>
        /// <param name="value">value to add</param>
        /// <returns>token to be used in statements</returns>
        public static ISqlToken Constant(object value) {
            throw new NotImplementedException("Only to be used in expression trees");
        }

        /// <summary>
        /// a predicate in a statement
        /// </summary>
        /// <param name="predicate">a predicate which has to evaluate to true or false</param>
        /// <returns>token to be used in statements</returns>
        public static ISqlToken Predicate(bool predicate) {
            throw new NotImplementedException("Only to be used in expression trees");
        }

        /// <summary>
        /// creates a case statement
        /// </summary>
        /// <param name="condition">condition to evaluate</param>
        /// <param name="value">value to use when condition evaluates to true</param>
        /// <param name="elsetoken">value to use when condition evaluates to false (optional)</param>
        /// <returns>token to be used in statements</returns>
        public static ISqlToken If(ISqlToken condition, ISqlToken value, ISqlToken elsetoken) {
            throw new NotImplementedException("Only to be used in expression trees");
        }

        /// <summary>
        /// creates a case statement
        /// </summary>
        /// <param name="condition">condition to evaluate</param>
        /// <param name="value">value to use when condition evaluates to true</param>
        /// <returns>token to be used in statements</returns>
        public static ISqlToken If(ISqlToken condition, ISqlToken value) {
            throw new NotImplementedException("Only to be used in expression trees");
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
            throw new NotImplementedException("Only to be used in expression trees");
        }
        
        /// <summary>
        /// sums up values
        /// </summary>
        /// <param name="token">token identifying values to sum</param>
        /// <returns>token to be used in statements</returns>
        public static ISqlToken Sum(ISqlToken token) {
            throw new NotImplementedException("Only to be used in expression trees");
        }

        /// <summary>
        /// averages a series of values
        /// </summary>
        /// <param name="token">token identifying values to sum</param>
        /// <returns>token to be used in statements</returns>
        public static ISqlToken Avg(ISqlToken token) {
            throw new NotImplementedException("Only to be used in expression trees");
        }

        /// <summary>
        /// get a minimum of a series of values
        /// </summary>
        /// <param name="token">token identifying values of which to get minimum</param>
        /// <returns>token to be used in statements</returns>
        public static ISqlToken Min(ISqlToken token) {
            throw new NotImplementedException("Only to be used in expression trees");
        }

        /// <summary>
        /// get a minimum of a series of values
        /// </summary>
        /// <param name="token">token identifying values of which to get maximum</param>
        /// <returns>token to be used in statements</returns>
        public static ISqlToken Max(ISqlToken token) {
            throw new NotImplementedException("Only to be used in expression trees");
        }
    
        /// <summary>
        /// specifies all columns
        /// </summary>
        public static ISqlToken All => throw new NotImplementedException("Only to be used in expression trees");
    }
}