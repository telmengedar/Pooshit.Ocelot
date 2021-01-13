using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Info;

namespace NightlyCode.Database.Tokens.Expressions {
    /// <inheritdoc />
    public class XprTranslator : IXprTranslator {
        readonly Func<Expression, Expression> basevisitor;

        /// <summary>
        /// creates a new <see cref="XprTranslator"/>
        /// </summary>
        /// <param name="basevisitor">visitor used to translate other expressions</param>
        public XprTranslator(Func<Expression, Expression> basevisitor) {
            this.basevisitor = basevisitor;
        }

        void AppendAggregate(IOperationPreparator operation, string method, IEnumerable<Expression> parameters) {
            operation.AppendText(method);
            operation.AppendText("(");
            bool first = true;
            foreach (Expression argument in parameters) {
                if (first)
                    first = false;
                else operation.AppendText(",");
                basevisitor(argument);
            }
            operation.AppendText(")");
        }
        
        /// <inheritdoc />
        public Expression TranslateMethodCall(MethodCallExpression methodcall, IOperationPreparator operation) {
            switch (methodcall.Method.Name) {
            case nameof(Xpr.Avg):
                AppendAggregate(operation, "AVG", methodcall.Arguments);
                break;
            case nameof(Xpr.Coalesce):
                AppendAggregate(operation, "COALESCE", methodcall.Arguments);
                break;
            case nameof(Xpr.Count):
                AppendAggregate(operation, "COUNT", methodcall.Arguments);
                break;
            case nameof(Xpr.Min):
                AppendAggregate(operation, "MIN", methodcall.Arguments);
                break;
            case nameof(Xpr.Max):
                AppendAggregate(operation, "MAX", methodcall.Arguments);
                break;
            case nameof(Xpr.Sum):
                AppendAggregate(operation, "SUM", methodcall.Arguments);
                break;
            case nameof(Xpr.If):
                operation.AppendText("CASE WHEN");
                basevisitor(methodcall.Arguments[0]);
                operation.AppendText("THEN");
                basevisitor(methodcall.Arguments[1]);
                if (methodcall.Arguments.Count>2) {
                    operation.AppendText("ELSE");
                    basevisitor(methodcall.Arguments[2]);
                }
                operation.AppendText("END");
                break;
            case nameof(Xpr.Predicate):
            case nameof(Xpr.Constant):
                basevisitor(methodcall.Arguments[0]);
                break;
            default:
                throw new ArgumentException($"Unsupported method '{methodcall.Method.Name}'");
            }
            return methodcall;
        }

        /// <inheritdoc />
        public Expression TranslateProperty(MemberExpression member, IOperationPreparator operation) {
            switch (member.Member.Name) {
            case nameof(Xpr.All):
                operation.AppendText("*");
                break;
            default:
                throw new ArgumentException($"Unsupported member '{member.Member.Name}'");
            }
            return member;
        }
    }
}