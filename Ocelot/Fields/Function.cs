﻿using System;
using System.Collections;
using Pooshit.Ocelot.CustomTypes;
using Pooshit.Ocelot.Entities.Operations;

namespace Pooshit.Ocelot.Fields {
    
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
        
        /// <summary>
        /// determines whether a value is in a collection
        /// </summary>
        /// <param name="range">range to check</param>
        /// <param name="value">value which has to appear in a collection</param>
        /// <returns>field used when building statement</returns>
        public static bool Contains<T>(this Range<T> range, object value) {
            throw new NotImplementedException("Only used for database lambdas");
        }
    }
}