using System;
using System.Linq.Expressions;

namespace Database.Entities.Operations {

    /// <summary>
    /// operation used to join tables
    /// </summary>
    public class JoinOperation {

        /// <summary>
        /// creates a new <see cref="JoinOperation"/>
        /// </summary>
        /// <param name="joinType">type of entity to join</param>
        /// <param name="criterias">join criterias</param>
        public JoinOperation(Type joinType, Expression criterias) {
            JoinType = joinType;
            Criterias = criterias;
        }

        /// <summary>
        /// type to join
        /// </summary>
        public Type JoinType { get; }

        /// <summary>
        /// criterias to use when joining tables
        /// </summary>
        public Expression Criterias { get; }
    }
}