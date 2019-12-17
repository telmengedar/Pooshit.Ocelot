using System;
using System.Linq.Expressions;

namespace NightlyCode.Database.Expressions
{

    /// <summary>
    /// a wrapped expression for database operations
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PredicateExpression<T> : WrappedExpression<Func<T, bool>>
    {

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="expression"></param>
        public PredicateExpression(Expression<Func<T, bool>> expression)
            : base(expression)
        {
        }

        /// <summary>
        /// combines two expressions using the AND operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static PredicateExpression<T> operator &(PredicateExpression<T> lhs, PredicateExpression<T> rhs)
        {
            return new PredicateExpression<T>(Conjunct(lhs, rhs));
        }

        /// <summary>
        /// combines two expressions using the OR operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static PredicateExpression<T> operator |(PredicateExpression<T> lhs, PredicateExpression<T> rhs)
        {
            return new PredicateExpression<T>(Disjunct(lhs, rhs));
        }

        /// <summary>
        /// combines two expressions using the AND operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static PredicateExpression<T> operator &(PredicateExpression<T> lhs, Expression<Func<T, bool>> rhs)
        {
            return new PredicateExpression<T>(Conjunct(lhs, new PredicateExpression<T>(rhs)));
        }

        /// <summary>
        /// combines two expressions using the OR operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static PredicateExpression<T> operator |(PredicateExpression<T> lhs, Expression<Func<T, bool>> rhs)
        {
            return new PredicateExpression<T>(Disjunct(lhs, new PredicateExpression<T>(rhs)));
        }

        /// <summary>
        /// implicit cast operator
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static implicit operator PredicateExpression<T>(Expression<Func<T, bool>> expression)
        {
            return new PredicateExpression<T>(expression);
        }

        /// <summary>
        /// implicit cast operator
        /// </summary>
        /// <param name="expression">expression to get content from</param>
        /// <returns>expression contained in wrapped predicate expression</returns>
        public static implicit operator Expression<Func<T, bool>>(PredicateExpression<T> expression) {
            return expression.Content;
        }

    }
}