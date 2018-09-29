using System;
using System.Linq.Expressions;
using NightlyCode.DB.Entities.Descriptors;
using NightlyCode.DB.Entities.Operations.Expressions;
using NightlyCode.DB.Info;

namespace NightlyCode.DB.Entities.Operations {

    /// <summary>
    /// constant value
    /// </summary>
    public class Constant : DBField {

        /// <summary>
        /// creates a new <see cref="Constant"/>
        /// </summary>
        /// <param name="value"></param>
        Constant(object value) {
            Value = value;
        }

        /// <summary>
        /// value
        /// </summary>
        public new object Value { get; }

        /// <summary>
        /// text used to represent the field
        /// </summary>
        public override void PrepareCommand(OperationPreparator preparator, IDBInfo dbinfo, Func<Type, EntityDescriptor> descriptorgetter) {
            if(Value==null)
                preparator.CommandBuilder.Append("NULL");
            else {
                if(Value is Expression)
                    CriteriaVisitor.GetCriteriaText((Expression)Value, descriptorgetter, dbinfo, preparator);
                else preparator.AppendParameter(Value);
            }
        }

        /// <summary>
        /// creates a new constant
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Constant Create(object value) {
            return new Constant(value);
        }

        public static explicit operator Constant(byte value)
        {
            return new Constant(value);
        }

        public static explicit operator Constant(short value)
        {
            return new Constant(value);
        }

        public static explicit operator Constant(int value) {
            return new Constant(value);
        }

        public static explicit operator Constant(long value)
        {
            return new Constant(value);
        }

        public static explicit operator Constant(sbyte value)
        {
            return new Constant(value);
        }

        public static explicit operator Constant(ushort value)
        {
            return new Constant(value);
        }

        public static explicit operator Constant(uint value)
        {
            return new Constant(value);
        }

        public static explicit operator Constant(ulong value)
        {
            return new Constant(value);
        }

        public static explicit operator Constant(float value)
        {
            return new Constant(value);
        }

        public static explicit operator Constant(double value)
        {
            return new Constant(value);
        }

        public static explicit operator Constant(char value)
        {
            return new Constant(value);
        }

        public static explicit operator Constant(string value)
        {
            return new Constant(value);
        }
    }
}