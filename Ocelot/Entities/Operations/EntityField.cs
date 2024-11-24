using System;
using System.Linq.Expressions;
using Pooshit.Ocelot.Fields;

namespace Pooshit.Ocelot.Entities.Operations;

/// <summary>
/// field describing a property of an entity
/// </summary>
public class EntityField : DBField {

    /// <summary>
    /// creates a new <see cref="EntityField"/>
    /// </summary>
    internal EntityField(Expression fieldexpression) {
        FieldExpression = fieldexpression;
    }

    /// <summary>
    /// expression describing the field
    /// </summary>
    public Expression FieldExpression { get; }

    /// <summary>
    /// creates an entity field
    /// </summary>
    /// <typeparam name="T">type of entity argument</typeparam>
    /// <param name="fieldexpression">expression to wrap</param>
    /// <returns></returns>
    public static EntityField Create<T>(Expression<Func<T, object>> fieldexpression) {
        return new(fieldexpression);
    }
    
    /// <summary>
    /// creates an entity field
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fieldexpression"></param>
    /// <returns></returns>
    public static EntityField Create(Expression<Func<object>> fieldexpression) {
        return new(fieldexpression);
    }
}