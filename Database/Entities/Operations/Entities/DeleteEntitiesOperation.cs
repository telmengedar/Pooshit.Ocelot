using System;
using System.Collections.Generic;
using System.Linq;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Prepared;

namespace NightlyCode.Database.Entities.Operations.Entities {

    /// <summary>
    /// removes entities from db
    /// </summary>
    public class DeleteEntitiesOperation<T> : IEntityOperation<T> {
        readonly EntityDescriptor entitydescriptor;
        readonly PreparedOperation preparedoperation;

        /// <summary>
        /// creates a new <see cref="DeleteEntitiesOperation{T}"/>
        /// </summary>
        /// <param name="dbclient">access to database</param>
        /// <param name="descriptor">access to entity descriptor</param>
        public DeleteEntitiesOperation(IDBClient dbclient, Func<Type, EntityDescriptor> descriptor) {
            entitydescriptor = descriptor(typeof(T));
            if (entitydescriptor.PrimaryKeyColumn == null)
                throw new InvalidOperationException("Entity to remove needs a primary key");


            OperationPreparator preparator = new OperationPreparator();
            preparator.AppendText($"DELETE FROM {entitydescriptor.TableName} WHERE ");
            preparator.AppendText($"{dbclient.DBInfo.ColumnIndicator}{entitydescriptor.PrimaryKeyColumn.Name}{dbclient.DBInfo.ColumnIndicator} IN ");
            preparator.AppendArrayParameter();
            preparedoperation = preparator.GetOperation(dbclient);
        }

        /// <summary>
        /// executes the operation
        /// </summary>
        /// <param name="entities">entities on which to operate</param>
        /// <returns>number of affected rows</returns>
        public int Execute(params T[] entities) {
            return preparedoperation.Execute(entities.Select(e => entitydescriptor.PrimaryKeyColumn.GetValue(e)).ToArray());
        }

        /// <summary>
        /// executes the operation using a transaction
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="entities">entities on which to operate</param>
        /// <returns>number of affected rows</returns>
        public int Execute(Transaction transaction, params T[] entities) {
            return preparedoperation.Execute(transaction, entities.Select(e => entitydescriptor.PrimaryKeyColumn.GetValue(e)).ToArray());
        }

        /// <summary>
        /// executes the operation
        /// </summary>
        /// <param name="entities">entities on which to operate</param>
        /// <returns>number of affected rows</returns>
        public int Execute(IEnumerable<T> entities) {
            return preparedoperation.Execute(entities.Select(e => entitydescriptor.PrimaryKeyColumn.GetValue(e)).ToArray());
        }

        /// <summary>
        /// executes the operation using a transaction
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="entities">entities on which to operate</param>
        /// <returns>number of affected rows</returns>
        public int Execute(Transaction transaction, IEnumerable<T> entities) {
            return preparedoperation.Execute(transaction, entities.Select(e => entitydescriptor.PrimaryKeyColumn.GetValue(e)).ToArray());
        }
    }
}