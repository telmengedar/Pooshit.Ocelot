using System;
using System.Collections.Generic;
using System.Linq;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Prepared;

namespace NightlyCode.Database.Entities.Operations.Entities {

    /// <summary>
    /// updates existing entities in db
    /// </summary>
    public class UpdateEntitiesOperation<T> : IEntityOperation<T> {
        readonly IDBClient dbclient;
        readonly EntityDescriptor entitydescription;
        readonly EntityColumnDescriptor[] interestingcolumns;
        readonly PreparedOperation preparedoperation;

        /// <summary>
        /// creates a new <see cref="UpdateEntitiesOperation{T}"/>
        /// </summary>
        /// <param name="dbclient">access to database</param>
        /// <param name="descriptor">access to entity description</param>
        public UpdateEntitiesOperation(IDBClient dbclient, Func<Type, EntityDescriptor> descriptor) {
            entitydescription = descriptor(typeof(T));
            if (entitydescription.PrimaryKeyColumn == null)
                throw new InvalidOperationException("Entity to update needs a primary key");

            this.dbclient = dbclient;
            interestingcolumns = entitydescription.Columns.Where(c => !c.AutoIncrement && !c.PrimaryKey).ToArray();
            preparedoperation = Prepare();
        }

        PreparedOperation Prepare() {
            OperationPreparator preparator = new OperationPreparator();
            preparator.AppendText($"UPDATE {entitydescription.TableName} SET ");
            preparator.AppendText($"{dbclient.DBInfo.ColumnIndicator}{interestingcolumns.First().Name}{dbclient.DBInfo.ColumnIndicator}=");
            preparator.AppendParameter();
            foreach (EntityColumnDescriptor column in interestingcolumns.Skip(1)) {
                preparator.AppendText($",{dbclient.DBInfo.ColumnIndicator}{column.Name}{dbclient.DBInfo.ColumnIndicator}=");
                preparator.AppendParameter();
            }

            preparator.AppendText($"WHERE {dbclient.DBInfo.ColumnIndicator}{entitydescription.PrimaryKeyColumn.Name}{dbclient.DBInfo.ColumnIndicator}=");
            preparator.AppendParameter();
            return preparator.GetOperation(dbclient);
        }

        /// <summary>
        /// executes the operation
        /// </summary>
        /// <param name="entities">entities on which to operate</param>
        /// <returns>number of affected rows</returns>
        public int Execute(params T[] entities) {
            foreach (T entity in entities)
                preparedoperation.Execute(interestingcolumns.Select(c => c.GetValue(entity)).Concat(new[] { entitydescription.PrimaryKeyColumn.GetValue(entity) }).ToArray());
            return entities.Length;
        }

        /// <summary>
        /// executes the operation using a transaction
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="entities">entities on which to operate</param>
        /// <returns>number of affected rows</returns>
        public int Execute(Transaction transaction, params T[] entities) {
            foreach (T entity in entities)
                preparedoperation.Execute(transaction, interestingcolumns.Select(c => c.GetValue(entity)).Concat(new[] {entitydescription.PrimaryKeyColumn.GetValue(entity)}).ToArray());
            return entities.Length;
        }

        /// <summary>
        /// executes the operation
        /// </summary>
        /// <param name="entities">entities on which to operate</param>
        /// <returns>number of affected rows</returns>
        public int Execute(IEnumerable<T> entities) {
            int count = 0;
            foreach (T entity in entities) {
                preparedoperation.Execute(interestingcolumns.Select(c => c.GetValue(entity)).Concat(new[] {entitydescription.PrimaryKeyColumn.GetValue(entity)}).ToArray());
                ++count;
            }

            return count;
        }

        /// <summary>
        /// executes the operation using a transaction
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="entities">entities on which to operate</param>
        /// <returns>number of affected rows</returns>
        public int Execute(Transaction transaction, IEnumerable<T> entities) {
            int count = 0;
            foreach (T entity in entities)
            {
                preparedoperation.Execute(transaction, interestingcolumns.Select(c => c.GetValue(entity)).Concat(new[] { entitydescription.PrimaryKeyColumn.GetValue(entity) }).ToArray());
                ++count;
            }

            return count;
        }
    }
}