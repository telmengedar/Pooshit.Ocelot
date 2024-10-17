using System;
using Pooshit.Ocelot.Extern;

namespace Pooshit.Ocelot.Fields;

/// <summary>
/// mapping for a field to load
/// </summary>
public abstract class FieldMapping<TEntity> {
    
    /// <summary>
    /// creates a new <see cref="FieldMapping{TEntity,TValue}"/>
    /// </summary>
    /// <param name="name">name of field</param>
    /// <param name="field">mapped field</param>
    protected FieldMapping(string name, IDBField field) {
        Name = name;
        Field = field;
    }

    /// <summary>
    /// name of field
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// field to load
    /// </summary>
    public IDBField Field { get; }

    /// <summary>
    /// set value to entity
    /// </summary>
    /// <param name="entity">entity to assign value to</param>
    /// <param name="value">value to assign</param>
    public abstract void SetValue(TEntity entity, object value);
}

/// <summary>
/// mapping for a field to load
/// </summary>
public class FieldMapping<TEntity, TValue> : FieldMapping<TEntity> {
    
    /// <summary>
    /// creates a new <see cref="FieldMapping{TEntity,TValue}"/>
    /// </summary>
    /// <param name="name">name of field</param>
    /// <param name="field">field to map</param>
    /// <param name="setter">setter used to set field values to entities</param>
    public FieldMapping(string name, IDBField field, Action<TEntity, TValue> setter) 
        : base(name, field) => Setter = setter;

    /// <summary>
    /// field setter
    /// </summary>
    public Action<TEntity, TValue> Setter { get; }

    /// <inheritdoc />
    public override void SetValue(TEntity entity, object value) {
        Setter(entity, Converter.Convert<TValue>(value));
    }
}