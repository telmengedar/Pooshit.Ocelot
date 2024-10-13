using System;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Extensions;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Tokens.Operations {
    
    /// <summary>
    /// token used to represent an operation
    /// </summary>
    public class OperationToken : SqlToken {
        readonly ISqlToken lhs;
        readonly Operand op;
        readonly ISqlToken rhs;

        /// <summary>
        /// creates a new <see cref="OperationToken"/>
        /// </summary>
        /// <param name="lhs">left hand side operand</param>
        /// <param name="op">operator</param>
        /// <param name="rhs">right hand side operand</param>
        public OperationToken(ISqlToken lhs, Operand op, ISqlToken rhs) {
            this.lhs = lhs;
            this.op = op;
            this.rhs = rhs;
        }

        void AppendOperand(IOperationPreparator preparator) {
            switch (op) {
                case Operand.Not:
                    preparator.AppendText("!");
                    break;
                case Operand.Negate:
                    preparator.AppendText("~");
                    break;
                case Operand.Multiply:
                    preparator.AppendText("*");
                    break;
                case Operand.Divide:
                    preparator.AppendText("/");
                    break;
                case Operand.Add:
                    preparator.AppendText("+");
                    break;
                case Operand.Subtract:
                    preparator.AppendText("-");
                    break;
                case Operand.Like:
                    preparator.AppendText("LIKE");
                    break;
                case Operand.NotLike:
                    preparator.AppendText("NOT LIKE");
                    break;
                case Operand.NotEqual:
                    preparator.AppendText("<>");
                    break;
                case Operand.Equal:
                    preparator.AppendText("=");
                    break;
                case Operand.Less:
                    preparator.AppendText("<");
                    break;
                case Operand.LessOrEqual:
                    preparator.AppendText("<=");
                    break;
                case Operand.Greater:
                    preparator.AppendText(">");
                    break;
                case Operand.GreaterOrEqual:
                    preparator.AppendText(">=");
                    break;
                case Operand.And:
                    preparator.AppendText("&");
                    break;
                case Operand.Or:
                    preparator.AppendText("|");
                    break;
                case Operand.ExclusiveOr:
                    preparator.AppendText("^");
                    break;
                case Operand.AndAlso:
                    preparator.AppendText("AND");
                    break;
                case Operand.OrElse:
                    preparator.AppendText("OR");
                    break;
                case Operand.ShiftLeft:
                    preparator.AppendText("<<");
                    break;
                case Operand.ShiftRight:
                    preparator.AppendText(">>");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <inheritdoc />
        public override void ToSql(IDBInfo dbinfo, IOperationPreparator preparator, Func<Type, EntityDescriptor> models, string tablealias) {
            if (lhs != null) {
                if (lhs is OperationToken lhsOperation && lhsOperation.op.GetPriority() < op.GetPriority()) {
                    preparator.AppendText("(");
                    lhs.ToSql(dbinfo, preparator, models, tablealias);
                    preparator.AppendText(")");
                }
                else lhs.ToSql(dbinfo, preparator, models, tablealias);
            }

            AppendOperand(preparator);

            if (rhs != null) {
                if (rhs is OperationToken rhsOperation && rhsOperation.op.GetPriority() < op.GetPriority()) {
                    preparator.AppendText("(");
                    rhs.ToSql(dbinfo, preparator, models, tablealias);
                    preparator.AppendText(")");
                }
                else rhs.ToSql(dbinfo, preparator, models, tablealias);
            }
        }
    }
}