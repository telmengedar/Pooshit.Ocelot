using System;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Extern;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Tokens.Values {

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
                if(Value is Enum) {
                    object enumvalue = Converter.Convert(Value, Enum.GetUnderlyingType(Value.GetType()));
                    AppendValue(enumvalue, preparator);
                }
                else preparator.AppendParameter(Converter.Convert(Value, dbinfo.GetDBRepresentation(Value.GetType())));
            }
        }
    }
}