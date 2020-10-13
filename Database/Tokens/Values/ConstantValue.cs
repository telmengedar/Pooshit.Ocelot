using System;
using NightlyCode.Database.Entities;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Extern;
using NightlyCode.Database.Info;

namespace NightlyCode.Database.Tokens.Values {

    /// <summary>
    /// constant value
    /// </summary>
    public class ConstantValue : SqlToken {

        /// <summary>
        /// creates a new <see cref="ConstantValue"/>
        /// </summary>
        /// <param name="value">value to add as token</param>
        internal ConstantValue(object value) {
            Value = value;
        }

        /// <summary>
        /// value
        /// </summary>
        public new object Value { get; }

        void AppendValue(object value, IOperationPreparator preparator) {
            preparator.AppendParameter(value);
        }

        /// <inheritdoc />
        public override void ToSql(IDBInfo dbinfo, IOperationPreparator preparator, Func<Type, EntityDescriptor> models, string tablealias) {
            if (Value == null)
                preparator.AppendText("NULL");
            else {
                if(DBConverterCollection.ContainsConverter(Value.GetType())) {
                    AppendValue(DBConverterCollection.ToDBValue(Value.GetType(), Value), preparator);
                }
                else if(Value is Enum) {
                    object enumvalue = Converter.Convert(Value, Enum.GetUnderlyingType(Value.GetType()));
                    AppendValue(enumvalue, preparator);
                }
                else preparator.AppendParameter(Converter.Convert(Value, dbinfo.GetDBRepresentation(Value.GetType())));
            }
        }
    }
}