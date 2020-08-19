using System;
using System.Collections;
using NightlyCode.Database.Entities.Operations;

namespace NightlyCode.Database.Fields {
    
    /// <summary>
    /// provides functions to lambda expressions
    /// </summary>
    public static class Function {

        /// <summary>
        /// determines whether a value is in a collection
        /// </summary>
        /// <param name="value">value which has to appear in a collection</param>
        /// <param name="collection">collection against which to check value</param>
        /// <returns>field used when building statement</returns>
        public static bool In(this object value, IEnumerable collection) {
            throw new NotImplementedException("Only used for database lambdas");
        }

        /// <summary>
        /// determines whether a value is in a collection
        /// </summary>
        /// <param name="value">value which has to appear in a collection</param>
        /// <param name="collection">collection against which to check value</param>
        /// <returns>field used when building statement</returns>
        public static bool In(this object value, Array collection) {
            throw new NotImplementedException("Only used for database lambdas");
        }
        
        /// <summary>
        /// determines whether a value is in a collection
        /// </summary>
        /// <param name="value">value which has to appear in a collection</param>
        /// <param name="statement">collection statement against which to check value</param>
        /// <returns>field used when building statement</returns>
        public static bool In(this object value, IDatabaseOperation statement) {
            throw new NotImplementedException("Only used for database lambdas");
        }
    }
}