using System;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Info;

namespace NightlyCode.Database.Entities.Operations.Fields.Sql {

    /// <summary>
    /// base implementation for an sql field to make sure classes don't need to implement expression fields
    /// </summary>
    public abstract class SqlField : ISqlField {

        /// <inheritdoc />
        public object Value => throw new NotImplementedException("Only used for expressions");

        /// <inheritdoc />
        public int Int32 => throw new NotImplementedException("Only used for expressions");

        /// <inheritdoc />
        public DateTime DateTime => throw new NotImplementedException("Only used for expressions");

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
        public byte[] Blob => throw new NotImplementedException("Only used for expressions");

        /// <inheritdoc />
        public abstract void ToSql(IDBInfo dbinfo, IOperationPreparator preparator, Func<Type, EntityDescriptor> models, string tablealias);
    }
}