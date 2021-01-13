using System;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Fields;
using NightlyCode.Database.Info;

namespace NightlyCode.Database.Tokens.Values {
    
    /// <summary>
    /// token used to cast values to another type
    /// </summary>
    public class CastToken : DBField {

        /// <summary>
        /// creates a new <see cref="CastToken"/>
        /// </summary>
        /// <param name="field">field to cast</param>
        /// <param name="type">type to cast field to</param>
        public CastToken(IDBField field, CastType type) {
            Field = field;
            Type = type;
        }
        
        /// <summary>
        /// field to cast
        /// </summary>
        public IDBField Field { get; }
        
        /// <summary>
        /// type to cast field to
        /// </summary>
        public CastType Type { get; }
    }
}