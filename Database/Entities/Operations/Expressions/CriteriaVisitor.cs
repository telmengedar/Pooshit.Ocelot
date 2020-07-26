﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Fields.Sql;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Fields;
using NightlyCode.Database.Info;
using Converter = NightlyCode.Database.Extern.Converter;

namespace NightlyCode.Database.Entities.Operations.Expressions {

    /// <summary>
    /// visits an expression tree to convert it to sql
    /// </summary>
    public class CriteriaVisitor : ExpressionVisitor {
        readonly Dictionary<string, string> aliases = new Dictionary<string, string>();
        readonly Func<Type, EntityDescriptor> descriptorgetter;
        readonly IOperationPreparator preparator;
        readonly IDBInfo dbinfo;

        ExpressionType remainder = ExpressionType.Default;

        /// <summary>
        /// creates a new <see cref="CriteriaVisitor"/>
        /// </summary>
        /// <param name="descriptorgetter">func used to get entity models of types</param>
        /// <param name="preparator">preparator to fill with sql</param>
        /// <param name="dbinfo">database specific implementation info</param>
        /// <param name="aliases">known table aliases</param>
        public CriteriaVisitor(Func<Type, EntityDescriptor> descriptorgetter, IOperationPreparator preparator, IDBInfo dbinfo, params Tuple<string, string>[] aliases) {
            this.descriptorgetter = descriptorgetter;
            this.dbinfo = dbinfo;
            this.preparator = preparator;
            foreach(Tuple<string, string> alias in aliases)
                this.aliases[alias.Item1] = alias.Item2;
        }

        static IEnumerable<Tuple<string, string>> GetParameterAliases(LambdaExpression expression, params string[] aliases) {
            aliases = aliases.Where(a => a != null).ToArray();
            if(expression == null || aliases.Length == 0)
                yield break;

            int index = 0;
            foreach(ParameterExpression parameter in expression.Parameters) {
                yield return new Tuple<string, string>(parameter.Name, aliases[index++]);
                if(index >= aliases.Length)
                    yield break;
            }
        }
        /// <summary>
        /// appends the criteria text of the predicate to the operation
        /// </summary>
        /// <param name="expression">expression containing predicate</param>
        /// <param name="descriptorgetter">method used to get entity models</param>
        /// <param name="dbinfo">db info</param>
        /// <param name="preparator">operation to modify</param>
        /// <param name="aliases">alias to use for properties</param>
        public static void GetCriteriaText(Expression expression, Func<Type, EntityDescriptor> descriptorgetter, IDBInfo dbinfo, IOperationPreparator preparator, params string[] aliases) {

            CriteriaVisitor visitor = new CriteriaVisitor(descriptorgetter, preparator, dbinfo, GetParameterAliases(expression as LambdaExpression, aliases).ToArray());
            visitor.Visit(expression);
        }

        string GetColumnName(ParameterExpression parameter, PropertyInfo info) {
            EntityDescriptor descriptor = descriptorgetter(parameter.Type);
            EntityColumnDescriptor column = descriptor.GetColumnByProperty(info.Name);

            if(aliases.TryGetValue(parameter.Name, out string alias))
                return string.Format("{2}.{0}{1}{0}", dbinfo.ColumnIndicator, column.Name, alias);
            return dbinfo.MaskColumn(column.Name);
            //return string.Format("{0}{1}{0}", dbinfo.ColumnIndicator, column.Name);
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
            else if (value is IDatabaseOperation dboperation) {
                AppendValueRemainder();
                dboperation.Prepare(preparator);
            }
            else {
                AppendValueRemainder();
                if(value is ISqlField sqlfield)
                    sqlfield.ToSql(dbinfo, preparator, descriptorgetter, null);
                else if(value is IDBField field)
                    dbinfo.Append(field, preparator, descriptorgetter);
                else {
                    preparator.AppendParameter(Converter.Convert(value, dbinfo.GetDBRepresentation(value.GetType())));
                }
            }

            remainder = ExpressionType.Default;
        }

        /// <inheritdoc />
        protected override Expression VisitConstant(ConstantExpression node) {
            AppendConstantValue(node.Value);
            return base.VisitConstant(node);
        }

        object GetHost(Expression expression) {
            if(expression == null)
                return null;

            if(expression is ConstantExpression constantExpression)
                return constantExpression.Value;
            if(expression is MemberExpression memberexpression) {
                object host = GetHost(memberexpression.Expression);
                if(memberexpression.Member is PropertyInfo info)
                    return info.GetValue(host, null);
                if(memberexpression.Member is FieldInfo fieldInfo)
                    return fieldInfo.GetValue(host);
                throw new NotImplementedException();
            }
            if(expression is LambdaExpression)
                return expression;
            if(expression is UnaryExpression unaryExpression)
                return unaryExpression.Operand;
            if(expression is MethodCallExpression methodcall) {
                return methodcall.Method.Invoke(GetHost(methodcall.Object), methodcall.Arguments.Select(GetHost).ToArray());
            }
            if(expression is NewArrayExpression newarray) {
                Array array = Array.CreateInstance(newarray.Type.GetElementType(), newarray.Expressions.Count);
                int index = 0;
                foreach(Expression ex in newarray.Expressions)
                    array.SetValue(GetHost(ex), index++);
                return array;
            }
            throw new NotImplementedException();
        }

        object GetValue(Expression node) {

            if(node is MemberExpression membernode) {
                if(membernode.NodeType == ExpressionType.MemberAccess) {
                    object host = GetHost(membernode.Expression);
                    if(membernode.Member is PropertyInfo info) {
                        if(host == null && !info.GetGetMethod().IsStatic)
                            throw new NullReferenceException("Null reference encountered");
                        return info.GetValue(host, null);
                    }

                    return ((FieldInfo)membernode.Member).GetValue(host);
                }

                if(membernode.NodeType == ExpressionType.Constant) {
                    object item = ((ConstantExpression)membernode.Expression).Value;
                    return ((PropertyInfo)membernode.Member).GetValue(item, null);
                }
            }
            else if(node is ConstantExpression expression) {
                return expression.Value;
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
                preparator.AppendText(GetColumnName((ParameterExpression)expression, (PropertyInfo)member));
            }
            else if(expression.NodeType == ExpressionType.MemberAccess) {
                // references a parameter to be specified later when executing the operation
                if(((PropertyInfo)member).DeclaringType == typeof(DBParameter)
                    || (((PropertyInfo)member).DeclaringType.IsGenericType && ((PropertyInfo)member).DeclaringType.GetGenericTypeDefinition() == typeof(DBParameter<>))) {
                    preparator.AppendParameter();
                }
                else {
                    object host = GetHost(expression);
                    if(host == null && !((PropertyInfo)member).GetGetMethod().IsStatic)
                        throw new NullReferenceException("Null reference encountered");

                    if(host is IDBField)
                        // always append dbfields directly
                        // since properties are only stubs for lambdas to work
                        AppendConstantValue(host);
                    else {
                        object item = ((PropertyInfo)member).GetValue(host, null);
                        AppendConstantValue(item);
                    }
                }
            }
            else
                throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override Expression VisitMember(MemberExpression node) {
            AppendValueRemainder();
            if(node.Member is PropertyInfo) {
                Expression host = node.Expression ?? node;
                if(host.NodeType == ExpressionType.Call) {
                    VisitMethodCall((MethodCallExpression)host);
                    return node;
                }
                AppendMemberValue(node.Expression ?? node, node.Member);
            }
            else if(node.Member is FieldInfo info) {
                if(node.Expression is ConstantExpression expression) {
                    object item = expression.Value;
                    object value = info.GetValue(item);
                    AppendConstantValue(value);
                }
                else {
                    object host = GetHost(node.Expression);
                    AppendConstantValue(info.GetValue(host));
                }
                //else throw new NotSupportedException($"{$"Unsupported expression type '{node.Expression.NodeType}' with member '"}{node.Member.GetType()}'");
            }
            else
                throw new Exception("Unsupported member type");
            return node;
        }

        /// <inheritdoc />
        protected override Expression VisitUnary(UnaryExpression node) {
            if(node.NodeType == ExpressionType.Convert || node.NodeType == ExpressionType.ConvertChecked || node.NodeType == ExpressionType.Quote)
                return base.VisitUnary(node);

            AppendValueRemainder();
            remainder = ExpressionType.Default;
            AddOperant(node.NodeType);
            Visit(node.Operand);
            return node;
        }

        /// <inheritdoc />
        protected override Expression VisitBinary(BinaryExpression node) {
            AppendValueRemainder();
            remainder = ExpressionType.Default;

            if(node.NodeType == ExpressionType.ArrayIndex) {
                if(!(GetValue(node.Left) is Array array))
                    throw new NullReferenceException("ArrayIndex without array");
                AppendConstantValue(array.GetValue((int)GetValue(node.Right)));
                return node;
            }

            Visit(node.Left);
            AddOperant(node.NodeType);
            Visit(node.Right);
            return node;
        }

        /// <inheritdoc />
        protected override Expression VisitNew(NewExpression node) {
            object value = node.Constructor.Invoke(node.Arguments.Select(GetHost).ToArray());
            AppendConstantValue(value);
            return node;
        }

        /// <inheritdoc />
        protected override Expression VisitNewArray(NewArrayExpression node) {
            bool first = true;
            foreach (Expression item in node.Expressions) {
                if (first) first = false;
                else preparator.AppendText(",");
                Visit(item);
            }

            return node;
        }

        /// <inheritdoc />
        protected override Expression VisitBlock(BlockExpression node) {
            preparator.AppendText("(");
            Expression result = base.VisitBlock(node);
            preparator.AppendText(")");
            return result;
        }

        /// <inheritdoc />
        protected override Expression VisitMethodCall(MethodCallExpression node) {
            AppendValueRemainder();

            if(node.Method.DeclaringType == typeof(DBOperators)) {
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

            if(node.Method.DeclaringType == typeof(Enumerable)) {
                switch(node.Method.Name) {
                case "Contains":
                    Visit(node.Arguments[1]);
                    preparator.AppendText("IN");

                    if(node.Arguments[0].NodeType == ExpressionType.MemberAccess
                        && (((MemberExpression)node.Arguments[0]).Member.DeclaringType == typeof(DBParameter)
                            || ((MemberExpression)node.Arguments[0]).Member.DeclaringType?.BaseType == typeof(DBParameter))) {
                        preparator.AppendArrayParameter();
                    }
                    else {
                        preparator.AppendText("(");
                        bool first = true;
                        foreach(object item in (IEnumerable)GetValue(node.Arguments[0])) {
                            if(first)
                                first = false;
                            else
                                preparator.AppendText(",");
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

            if(node.Method.DeclaringType == typeof(DBParameter)) {
                switch(node.Method.Name) {
                case "Index":
                    int index = (int)GetHost(node.Arguments.First());
                    preparator.AppendParameterIndex(index);
                    break;
                default:
                    throw new NotImplementedException();
                }

                return node;
            }

            if((node.Method.DeclaringType?.IsGenericType ?? false) && node.Method.DeclaringType.GetGenericTypeDefinition() == typeof(DBParameter<>)) {
                switch(node.Method.Name) {
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
                case "Total":
                    preparator.AppendText("total(");
                    break;
                case "Count":
                    preparator.AppendText("count(");
                    if (node.Arguments.Count == 0)
                        preparator.AppendText("*");
                    break;
                }

                ProcessExpressionList(node.Arguments, parameter => {
                    if (parameter is NewArrayExpression arrayparameter) {
                        ProcessExpressionList(arrayparameter.Expressions, arrayargument => {
                            Visit(arrayargument);
                        });
                    }
                    else {
                        Visit(node.Arguments[0]);
                    }
                });
                
                preparator.AppendText(")");
                return node;
            }

            if (node.Method.DeclaringType == typeof(Function)) {
                switch (node.Method.Name) {
                case nameof(Function.In):
                    if (node.Arguments.Count != 2)
                        throw new ArgumentException("Invalid method call, expected 2 arguments. First being value to check, second being collection");
                    Visit(node.Arguments[0]);
                    preparator.AppendText("IN(");
                    Visit(node.Arguments[1]);
                    preparator.AppendText(")");
                    break;
                default:
                    throw new ArgumentException("Unsupported db function call");
                }

                return node;
            }
            
            object value = node.Method.Invoke(GetHost(node.Object), node.Arguments.Select(GetHost).ToArray());
            AppendConstantValue(value);
            return node;
        }

        void ProcessExpressionList(IEnumerable<Expression> expressions, Action<Expression> actions) {
            bool first = true;
            foreach (Expression expression in expressions) {
                if (first) first = false;
                else preparator.AppendText(",");

                actions(expression);
            }
        }
    }
}