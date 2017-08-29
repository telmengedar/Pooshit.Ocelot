using System;
using System.Linq.Expressions;

namespace NightlyCode.DB.Entities.Operations {

    /// <summary>
    /// operation used to join tables
    /// </summary>
    public class JoinOperation {

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="joinType"></param>
        /// <param name="criterias"></param>
        public JoinOperation(Type joinType, Expression criterias) {
            JoinType = joinType;
            Criterias = criterias;
        }

        /// <summary>
        /// type to join
        /// </summary>
        public Type JoinType { get; private set; }

        /// <summary>
        /// criterias to use when joining tables
        /// </summary>
        public Expression Criterias { get; private set; }
    }
}