using System;
using Pooshit.Ocelot.Tokens.Operations;

namespace Pooshit.Ocelot.Extensions {
    
    /// <summary>
    /// extensions for <see cref="Operand"/>s
    /// </summary>
    public static class OperandExtensions {

        /// <summary>
        /// get priority of operands
        /// </summary>
        /// <param name="operand">operand of which to get priority</param>
        /// <returns>ordinal number</returns>
        public static int GetPriority(this Operand operand) {
            switch(operand) {
                case Operand.Multiply:
                case Operand.Divide:
                    return 0;
                case Operand.Add:
                case Operand.Subtract:
                    return 1;
                case Operand.Negate:
                case Operand.Not:
                    return 2;
                case Operand.NotEqual:
                case Operand.Equal:
                case Operand.Less:
                case Operand.LessOrEqual:
                case Operand.Greater:
                case Operand.GreaterOrEqual:
                case Operand.Like:
                    return 3;
                case Operand.And:
                    return 4;
                case Operand.AndAlso:
                    return 7;
                case Operand.Or:
                    return 5;
                case Operand.OrElse:
                    return 8;
                case Operand.ExclusiveOr:
                    return 6;
                case Operand.ShiftLeft:
                case Operand.ShiftRight:
                    return 9;
                default:
                    throw new InvalidOperationException($"Operand '{operand}' not supported");
            }
        }
    }
}