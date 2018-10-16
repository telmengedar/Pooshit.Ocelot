using System.Linq.Expressions;
using System.Reflection;

namespace NightlyCode.Database.Entities.Operations.Expressions {
    public class PropertyVisitor : ExpressionVisitor
    {
        PropertyInfo property;

        /// <summary>
        /// get column name stored in expression
        /// </summary>
        /// <param name="expression">expression to analyse</param>
        /// <returns>column name if any is found in expression, null otherwise</returns>
        public PropertyInfo GetProperty(Expression expression) {
            Visit(expression);
            return property;
        }

        protected override Expression VisitMember(MemberExpression node) {
            if (node.Member is PropertyInfo info && (node.Expression ?? node).NodeType == ExpressionType.Parameter)
                property = info;
            return base.VisitMember(node);
        }
    }
}