using System;
using System.Linq.Expressions;
using System.Reflection;
using NightlyCode.DB.Entities.Descriptors;

#if UNITY
using NightlyCode.Unity.DB.Entities.Operations;
#endif

namespace NightlyCode.DB.Entities.Operations.Expressions {
    public class ColumnVisitor
#if UNITY
        : ExpressionVisitor {
#else
        : ExpressionVisitor
    {
#endif 
        readonly EntityDescriptor descriptor;
        string columnname;

        /// <summary>
        /// creates a new <see cref="ColumnVisitor"/>
        /// </summary>
        /// <param name="descriptor">schema info for entity to be analysed</param>
        public ColumnVisitor(EntityDescriptor descriptor) {
            this.descriptor = descriptor;
        }

        /// <summary>
        /// get column name stored in expression
        /// </summary>
        /// <param name="expression">expression to analyse</param>
        /// <returns>column name if any is found in expression, null otherwise</returns>
        public string GetColumnName(Expression expression) {
            Visit(expression);
            return columnname;
        }

        string GetColumnName(PropertyInfo info) {
            EntityColumnDescriptor column = descriptor.GetColumnByProperty(info.Name);
            return column.Name;
        }

#if UNITY
        protected override Expression VisitMemberAccess(MemberExpression node) {
#else
        protected override Expression VisitMember(MemberExpression node) {
#endif
            if(node.Member is PropertyInfo && (node.Expression ?? node).NodeType == ExpressionType.Parameter)
                columnname = GetColumnName((PropertyInfo)node.Member);
            return base.VisitMember(node);
        }
    }
}