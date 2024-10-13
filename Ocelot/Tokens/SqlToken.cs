using System;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Tokens;

/// <summary>
/// base implementation for an sql field to make sure classes don't need to implement expression fields
/// </summary>
public abstract class SqlToken : ISqlToken {

    /// <inheritdoc />
    public object Value => throw new NotImplementedException("Only used for expressions");

    /// <inheritdoc />
    public int Int32 => throw new NotImplementedException("Only used for expressions");

    /// <inheritdoc />
    public DateTime DateTime => throw new NotImplementedException("Only used for expressions");

    /// <inheritdoc />
    public TimeSpan TimeSpan => throw new NotImplementedException("Only used for expressions");

    /// <inheritdoc />
    public Guid Guid => throw new NotImplementedException("Only used for expressions");

    /// <inheritdoc />
    public long Int64 => throw new NotImplementedException("Only used for expressions");

    /// <inheritdoc />
    public string String => throw new NotImplementedException("Only used for expressions");

    /// <inheritdoc />
    public float Single => throw new NotImplementedException("Only used for expressions");

    /// <inheritdoc />
    public double Double => throw new NotImplementedException("Only used for expressions");

    /// <inheritdoc />
    public decimal Decimal => throw new NotImplementedException("Only used for expressions");

    /// <inheritdoc />
    public byte[] Blob => throw new NotImplementedException("Only used for expressions");

    /// <inheritdoc />
    public bool Bool => throw new NotImplementedException("Only used for expressions");

    /// <inheritdoc />
    public T Type<T>() => throw new NotImplementedException();

    /// <inheritdoc />
    public abstract void ToSql(IDBInfo dbinfo, IOperationPreparator preparator, Func<Type, EntityDescriptor> models, string tablealias);
}