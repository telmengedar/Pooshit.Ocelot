using System;

namespace Pooshit.Ocelot.Errors {
    
    /// <summary>
    /// exception thrown when a statement could not be executed
    /// </summary>
    public class StatementException : Exception {
        
        /// <summary>
        /// creates a new <see cref="StatementException"/>
        /// </summary>
        /// <param name="statement">executed statement</param>
        /// <param name="parameters">provided parameters</param>
        /// <param name="innerException">exception which triggered this exception</param>
        public StatementException(string statement, object[] parameters, Exception innerException=null) 
            : base($"Error executing '{statement}'", innerException) {
            Statement = statement;
            Parameters = parameters;
        }

        /// <summary>
        /// executed statement
        /// </summary>
        public string Statement { get; }

        /// <summary>
        /// provided parameters
        /// </summary>
        public object[] Parameters { get; }
    }
}