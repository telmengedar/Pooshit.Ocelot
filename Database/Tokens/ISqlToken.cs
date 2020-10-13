using System;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Fields;
using NightlyCode.Database.Info;

namespace NightlyCode.Database.Tokens {

    /// <summary>
    /// field which is used in expressions
    /// </summary>
    public interface ISqlToken : IDBField {
        
        /// <summary>
        /// generates sql in <see cref="OperationPreparator"/>
        /// </summary>
        /// <param name="dbinfo">info of database for which to generate sql</param>
        /// <param name="preparator">preparator to fill with sql</param>
        /// <param name="models">access to entity models</param>
        /// <param name="tablealias">alias to use for table when resolving properties</param>
        void ToSql(IDBInfo dbinfo, IOperationPreparator preparator, Func<Type, EntityDescriptor> models, string tablealias);
    }
}