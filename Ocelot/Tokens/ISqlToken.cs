using System;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Tokens;

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