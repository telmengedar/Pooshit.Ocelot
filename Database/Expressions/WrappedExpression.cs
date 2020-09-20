using System.Linq;
using System.Linq.Expressions;

namespace NightlyCode.Database.Expressions
{

    /// <summary>
    /// wraps expressions and provides several logical operations for them
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WrappedExpression<T>
    {
        readonly Expression<T> expression;
        bool needsblock;

        /// <summary>
        /// creates a new wrapped expression
        /// </summary>
        /// <param name="expression"></param>
        public WrappedExpression(Expression<T> expression)
        {
            this.expression = expression;
            needsblock = expression.Body is BinaryExpression bin && bin.NodeType == ExpressionType.OrElse;
        }

        /// <summary>
        /// the wrapped expression
        /// </summary>
        public Expression<T> Content => expression;

        /// <summary>
        /// combines two expressions using the AND operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        protected static Expression<T> Conjunct(WrappedExpression<T> lhs, WrappedExpression<T> rhs)
        {
            if (lhs?.Content == null)
                return rhs.Content;
            if (rhs?.Content == null)
                return lhs.Content;

            ParameterExpression parameter = lhs.expression.Parameters.FirstOrDefault() ?? rhs.expression.Parameters.FirstOrDefault();
            Expression expression = Expression.AndAlso(lhs.needsblock ? Expression.Block(lhs.Content.Body) : lhs.Content.Body, rhs.needsblock ? Expression.Block(rhs.Content.Body) : rhs.Content.Body);
            return Expression.Lambda<T>(expression, parameter);
        }

        /// <summary>
        /// combines two expressions using the OR operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        protected static Expression<T> Disjunct(WrappedExpression<T> lhs, WrappedExpression<T> rhs)
        {
            if (lhs?.Content == null)
                return rhs.Content;
            if (rhs?.Content == null)
                return lhs.Content;

            ParameterExpression parameter = lhs.expression.Parameters.FirstOrDefault() ?? rhs.expression.Parameters.FirstOrDefault();
            Expression expression = Expression.OrElse(lhs.Content.Body, rhs.Content.Body);
            return Expression.Lambda<T>(expression, parameter);
        }

        /// <summary>
        /// combines two expressions using the AND operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static WrappedExpression<T> operator &(WrappedExpression<T> lhs, WrappedExpression<T> rhs)
        {
            return new WrappedExpression<T>(Conjunct(lhs, rhs));
        }

        /// <summary>
        /// combines two expressions using the OR operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static WrappedExpression<T> operator |(WrappedExpression<T> lhs, WrappedExpression<T> rhs)
        {
            return new WrappedExpression<T>(Disjunct(lhs, rhs));
        }
    }
}
