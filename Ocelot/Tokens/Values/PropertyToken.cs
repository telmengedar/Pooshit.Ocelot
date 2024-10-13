using System;
using System.Linq.Expressions;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Expressions;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Tokens.Values;

/// <summary>
/// token specifying a property
/// </summary>
public class PropertyToken : SqlToken {
        
    /// <summary>
    /// creates a new <see cref="PropertyToken"/>
    /// </summary>
    /// <param name="propertyExpression">expression pointing to property</param>
    /// <param name="alias">alias to reference</param>
    public PropertyToken(Expression propertyExpression, string alias=null) {
        PropertyExpression = propertyExpression;
        Alias = alias;
    }

    /// <summary>
    /// alias to reference
    /// </summary>
    public string Alias { get; }

    /// <summary>
    /// alias for property in result set
    /// </summary>
    public string PropertyAlias { get; set; }
        
    /// <summary>
    /// expression describing the field
    /// </summary>
    public Expression PropertyExpression { get; }


    /// <inheritdoc />
    public override void ToSql(IDBInfo dbinfo, IOperationPreparator preparator, Func<Type, EntityDescriptor> models, string tablealias) {
        string alias = Alias ?? tablealias;
        CriteriaVisitor.GetCriteriaText(PropertyExpression, models, dbinfo, preparator, alias);
    }
}