using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Fields;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Info;
using Converter = NightlyCode.Database.Extern.Converter;

namespace NightlyCode.Database.Entities.Operations.Expressions {

    /// <summary>
    /// visits an expression tree to convert it to sql
    /// </summary>
    public class CriteriaVisitor : ExpressionVisitor
    { 
        readonly Func<Type, EntityDescriptor> descriptorgetter;
        readonly OperationPreparator preparator;
        readonly IDBInfo dbinfo;

        ExpressionType remainder = ExpressionType.Default;

        CriteriaVisitor(Func<Type, EntityDescriptor> descriptorgetter, OperationPreparator preparator, IDBInfo dbinfo) {
            this.descriptorgetter = descriptorgetter;
            this.dbinfo = dbinfo;
            this.preparator = preparator;
        }

        public static void GetCriteriaText(Expression expression, Func<Type, EntityDescriptor> descriptorgetter, IDBInfo dbinfo, OperationPreparator preparator) {
            CriteriaVisitor visitor = new CriteriaVisitor(descriptorgetter, preparator, dbinfo);
            visitor.Visit(expression);
        }

        string GetColumnName(PropertyInfo info) {
            EntityDescriptor descriptor = descriptorgetter(info.ReflectedType);
            EntityColumnDescriptor column = descriptor.GetColumnByProperty(info.Name);
            return string.Format("{0}{1}{0}", dbinfo.ColumnIndicator, column.Name);
        }

        string GetOperant(ExpressionType type) {
            switch(type) {
            case ExpressionType.Equal:
                return "=";
            case ExpressionType.LessThan:
                return "<";
            case ExpressionType.LessThanOrEqual:
                return "<=";
            case ExpressionType.GreaterThan:
                return ">";
            case ExpressionType.GreaterThanOrEqual:
                return ">=";
            case ExpressionType.And:
                return "&";
            case ExpressionType.AndAlso:
                return "AND";
            case ExpressionType.Or:
                return "|";
            case ExpressionType.OrElse:
                return "OR";
            case ExpressionType.ExclusiveOr:
                return "^";
            case ExpressionType.Add:
                return "+";
            case ExpressionType.Negate:
            case ExpressionType.Subtract:
                return "-";
            case ExpressionType.Multiply:
                return "*";
            case ExpressionType.Divide:
                return "/";
            case ExpressionType.Not:
                return "NOT";
            default:
                throw new InvalidOperationException("Operant not supported");
            }
        }

        void AddOperant(ExpressionType type) {
            switch(type) {
            case ExpressionType.Equal:
            case ExpressionType.NotEqual:
                remainder = type;
                break;
            default:
                preparator.AppendText(GetOperant(type));
                break;
            }
        }

        void AppendValueRemainder() {
            switch(remainder) {
                case ExpressionType.Equal:
                    preparator.AppendText("=");
                    break;
                case ExpressionType.NotEqual:
                    preparator.AppendText("<>");
                    break;
            }
            remainder = ExpressionType.Default;
        }

        void AppendConstantValue(object value) {
            if(value == null) {
                switch(remainder) {
                    case ExpressionType.Equal:
                        preparator.AppendText("IS");
                        break;
                    case ExpressionType.NotEqual:
                        preparator.AppendText("IS NOT");
                        break;
                }
                preparator.AppendText("NULL");
            }
            else if(DBConverterCollection.ContainsConverter(value.GetType())) {
                AppendValueRemainder();
                AppendConstantValue(DBConverterCollection.ToDBValue(value.GetType(), value));
            }
            else if(value is Enum) {
                AppendValueRemainder();
                object enumvalue = Converter.Convert(value, Enum.GetUnderlyingType(value.GetType()));
                AppendConstantValue(enumvalue);
            }
            else {
                AppendValueRemainder();
                if (value is IDBField field)
                    dbinfo.Append(field, preparator, descriptorgetter);
                else {
                    preparator.AppendParameter(Converter.Convert(value, dbinfo.GetDBRepresentation(value.GetType())));
                }
            }

            remainder = ExpressionType.Default;
        }

        protected override Expression VisitConstant(ConstantExpression node) {
            AppendConstantValue(node.Value);
            return base.VisitConstant(node);
        }

        object GetHost(Expression expression) {
            if(expression == null)
                return null;

            if(expression is ConstantExpression)
                return ((ConstantExpression)expression).Value;
            if(expression is MemberExpression) {
                MemberExpression memberexpression = (MemberExpression)expression;
                object host = GetHost(memberexpression.Expression);
                if(memberexpression.Member is PropertyInfo)
                    return ((PropertyInfo)memberexpression.Member).GetValue(host, null);
                if(memberexpression.Member is FieldInfo)
                    return ((FieldInfo)memberexpression.Member).GetValue(host);
                throw new NotImplementedException();
            }
            if(expression is LambdaExpression)
                return expression;
            if(expression is UnaryExpression)
                return ((UnaryExpression)expression).Operand;
            if(expression is MethodCallExpression) {
                MethodCallExpression methodcall = (MethodCallExpression)expression;
                return methodcall.Method.Invoke(GetHost(methodcall.Object), methodcall.Arguments.Select(GetHost).ToArray());
            }
            if(expression is NewArrayExpression) {
                NewArrayExpression newarray = (NewArrayExpression)expression;
                Array array = Array.CreateInstance(newarray.Type.GetElementType(), newarray.Expressions.Count);
                int index = 0;
                foreach(Expression ex in newarray.Expressions)
                    array.SetValue(GetHost(ex), index++);
                return array;
            }
            throw new NotImplementedException();
        }

        object GetValue(Expression node) {

            if(node is MemberExpression) {
                MemberExpression membernode = (MemberExpression)node;
                if(node.NodeType == ExpressionType.MemberAccess) {
                    object host = GetHost(membernode.Expression);
                    if(membernode.Member is PropertyInfo) {
                        if(host == null && !((PropertyInfo)membernode.Member).GetGetMethod().IsStatic)
                            throw new NullReferenceException("Null reference encountered");
                        return ((PropertyInfo)membernode.Member).GetValue(host, null);                        
                    }

                    return ((FieldInfo)membernode.Member).GetValue(host);
                }

                if(node.NodeType == ExpressionType.Constant) {
                    object item = ((ConstantExpression)membernode.Expression).Value;
                    return ((PropertyInfo)membernode.Member).GetValue(item, null);
                }
            }
            else if(node is ConstantExpression) {
                return ((ConstantExpression)node).Value;
            }

            throw new InvalidOperationException("nodetype not supported");
        }

        void AppendMemberValue(Expression expression, MemberInfo member) {
            if(expression.NodeType == ExpressionType.Convert)
                expression = (Expression)GetHost(expression);

            if(expression.NodeType == ExpressionType.Constant) {
                object item = ((ConstantExpression)expression).Value;
                object value = ((PropertyInfo)member).GetValue(item, null);
                AppendConstantValue(value);
            }
            else if(expression.NodeType == ExpressionType.Parameter) {
                preparator.AppendText(GetColumnName((PropertyInfo)member));
            }
            else if(expression.NodeType == ExpressionType.MemberAccess) {
                // references a parameter to be specified later when executing the operation
                if (((PropertyInfo)member).DeclaringType == typeof(DBParameter) 
                    || (((PropertyInfo)member).DeclaringType.IsGenericType && ((PropertyInfo)member).DeclaringType.GetGenericTypeDefinition() == typeof(DBParameter<>)))
                {
                    preparator.AppendParameter();
                }
                else
                {
                    object host = GetHost(expression);
                    if (host == null && !((PropertyInfo)member).GetGetMethod().IsStatic)
                        throw new NullReferenceException("Null reference encountered");

                    if (host is IDBField)
                        // always append dbfields directly
                        // since properties are only stubs for lambdas to work
                        AppendConstantValue(host);
                    else
                    {
                        object item = ((PropertyInfo)member).GetValue(host, null);
                        AppendConstantValue(item);
                    }
                }
            }
            else throw new NotImplementedException();
        }

        protected override Expression VisitMember(MemberExpression node) {
            AppendValueRemainder();
            if(node.Member is PropertyInfo) {
                AppendMemberValue(node.Expression ?? node, node.Member);
            }
            else if(node.Member is FieldInfo) {
                if(node.Expression is ConstantExpression) {
                    object item = ((ConstantExpression)node.Expression).Value;
                    object value = ((FieldInfo)node.Member).GetValue(item);
                    AppendConstantValue(value);
                }
                else {
                    object host = GetHost(node.Expression);
                    AppendConstantValue(((FieldInfo)node.Member).GetValue(host));
                }
                //else throw new NotSupportedException($"{$"Unsupported expression type '{node.Expression.NodeType}' with member '"}{node.Member.GetType()}'");
            }
            else throw new Exception("Unsupported member type");
            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node) {
            if(node.NodeType == ExpressionType.Convert || node.NodeType == ExpressionType.ConvertChecked)
                return base.VisitUnary(node);

            AppendValueRemainder();
            remainder = ExpressionType.Default;
            AddOperant(node.NodeType);
            Visit(node.Operand);
            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node) {
            AppendValueRemainder();
            remainder = ExpressionType.Default;

            if (node.NodeType == ExpressionType.ArrayIndex) {
                Array array = GetValue(node.Left) as Array;
                if(array==null)
                    throw new NullReferenceException("ArrayIndex without array");
                AppendConstantValue(array.GetValue((int)GetValue(node.Right)));
                return node;
            }

            Visit(node.Left);
            AddOperant(node.NodeType);
            Visit(node.Right);
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node) {
            AppendValueRemainder();

            if (node.Method.DeclaringType==typeof(DBOperators)) {
                switch(node.Method.Name) {
                case "Like":
                    Visit(node.Arguments[0]);
                    preparator.AppendText(dbinfo.LikeTerm);
                    Visit(node.Arguments[1]);
                    break;
                case "Replace":
                    dbinfo.Replace(this, preparator, node.Arguments[0], node.Arguments[1], node.Arguments[2]);
                    break;
                default:
                    throw new NotImplementedException();
                }
                return node;
            }

            if (node.Method.DeclaringType == typeof(Enumerable)) {
                switch (node.Method.Name) {
                case "Contains":
                    Visit(node.Arguments[1]);
                    preparator.AppendText("IN");

                    if (node.Arguments[0].NodeType == ExpressionType.MemberAccess
                        && (((MemberExpression) node.Arguments[0]).Member.DeclaringType == typeof(DBParameter)
                            || ((MemberExpression) node.Arguments[0]).Member.DeclaringType?.BaseType == typeof(DBParameter))) {
                        preparator.AppendArrayParameter();
                    }
                    else {
                        preparator.AppendText("(");
                        bool first = true;
                        foreach (object item in (IEnumerable) GetValue(node.Arguments[0])) {
                            if (first)
                                first = false;
                            else preparator.AppendText(",");
                            AppendConstantValue(item);
                        }

                        preparator.AppendText(")");
                    }

                    break;
                default:
                    throw new NotImplementedException();
                }

                return node;
            }

            if(node.Method.DeclaringType == typeof(string)) {
                switch(node.Method.Name) {
                    case "ToUpper":
                        dbinfo.ToUpper(this, preparator, node.Object);
                        break;
                    case "ToLower":
                        dbinfo.ToLower(this, preparator, node.Object);
                        break;
                }
                return node;
            }

            if (node.Method.DeclaringType == typeof(DBParameter)) {
                switch (node.Method.Name) {
                    case "Index":
                        int index = (int)GetHost(node.Arguments.First());
                        preparator.AppendParameterIndex(index);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                return node;
            }

            if(node.Method.DeclaringType == typeof(DBFunction)) {
                
                switch(node.Method.Name) {
                    case "Min":
                        preparator.AppendText("min(");
                        break;
                    case "Max":
                        preparator.AppendText("max(");
                        break;
                    case "Average":
                        preparator.AppendText("avg(");
                        break;
                    case "Sum":
                        preparator.AppendText("sum(");
                        break;
                }

                if(node.Arguments[0] is NewArrayExpression arrayparameter) {
                    NewArrayExpression parameter = (NewArrayExpression)node.Arguments[0];
                    Visit(parameter.Expressions[0]);
                    foreach(Expression funcparameter in parameter.Expressions.Skip(1)) {
                        preparator.AppendText(",");
                        Visit(funcparameter);
                    }
                }
                else {
                    Visit(node.Arguments[0]);
                }
                preparator.AppendText(")");
                return node;
            }

            object value = node.Method.Invoke(GetHost(node.Object), node.Arguments.Select(GetHost).ToArray());
            AppendConstantValue(value);
            return node;
        }
    }
}