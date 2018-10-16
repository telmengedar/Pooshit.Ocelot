using System.Linq.Expressions;
using System.Reflection;
using NightlyCode.Database.Entities.Descriptors;

namespace NightlyCode.Database.Entities.Operations.Expressions {
    public class ColumnVisitor : ExpressionVisitor
    {
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

        protected override Expression VisitMember(MemberExpression node) {
            if(node.Member is PropertyInfo && (node.Expression ?? node).NodeType == ExpressionType.Parameter)
                columnname = GetColumnName((PropertyInfo)node.Member);
            return base.VisitMember(node);
        }
    }
}