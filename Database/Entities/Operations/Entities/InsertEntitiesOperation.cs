﻿using System;
using System.Collections.Generic;
using System.Linq;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Fields;
using NightlyCode.Database.Entities.Operations.Prepared;
using Converter = NightlyCode.Database.Extern.Converter;

namespace NightlyCode.Database.Entities.Operations.Entities {

    /// <summary>
    /// inserts new entities to the db
    /// </summary>
    public class InsertEntitiesOperation<T> : IEntityOperation<T> {
        readonly IDBClient dbclient;
        readonly EntityDescriptor entitydescriptor;
        readonly EntityColumnDescriptor[] interestingcolumns;
        readonly PreparedOperation insertoperation;
        readonly PreparedLoadValuesOperation loadreturnid;

        /// <summary>
        /// creates a new <see cref="InsertEntitiesOperation{T}"/>
        /// </summary>
        /// <param name="entitymanager">access to entity layer of database</param>
        /// <param name="descriptor">access to entity description</param>
        public InsertEntitiesOperation(IEntityManager entitymanager, Func<Type, EntityDescriptor> descriptor) {
            dbclient = entitymanager.DBClient;
            entitydescriptor = descriptor(typeof(T));
            interestingcolumns = entitydescriptor.Columns.Where(c => !c.AutoIncrement).ToArray();
            loadreturnid = entitymanager.Load<object>(DBFunction.LastInsertID).Prepare();
            insertoperation = PrepareOperation();
        }

        PreparedOperation PrepareOperation() {
            OperationPreparator preparator = new OperationPreparator(dbclient.DBInfo);
            preparator.CommandBuilder.Append($"INSERT INTO {entitydescriptor.TableName} ({dbclient.DBInfo.ColumnIndicator}{string.Join(string.Format("{0}, {0}", dbclient.DBInfo.ColumnIndicator), interestingcolumns.Select(c => c.Name))}{dbclient.DBInfo.ColumnIndicator}) VALUES(");
            foreach (EntityColumnDescriptor unused in interestingcolumns) {
                if (preparator.Parameters.Count > 0)
                    preparator.CommandBuilder.Append(", ");
                preparator.AppendParameter();
            }
            preparator.CommandBuilder.Append(")");

            return preparator.GetOperation(dbclient);
        }

        /// <summary>
        /// executes the operation
        /// </summary>
        /// <param name="entities">entities on which to operate</param>
        /// <returns>number of affected rows</returns>
        public int Execute(params T[] entities) {
            foreach (T entity in entities)
            {
                insertoperation.Execute(interestingcolumns.Select(c => c.GetValue(entity)).ToArray());
                if (entitydescriptor.PrimaryKeyColumn != null && entitydescriptor.PrimaryKeyColumn.AutoIncrement)
                    entitydescriptor.PrimaryKeyColumn.SetValue(entity, Converter.Convert(loadreturnid.ExecuteScalar<object>(), entitydescriptor.PrimaryKeyColumn.Property.PropertyType));
            }
            return entities.Length;
        }

        /// <summary>
        /// executes the operation using a transaction
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="entities">entities on which to operate</param>
        /// <returns>number of affected rows</returns>
        public int Execute(Transaction transaction, params T[] entities) {
            foreach (T entity in entities) {
                insertoperation.Execute(transaction, interestingcolumns.Select(c => c.GetValue(entity)).ToArray());
                if (entitydescriptor.PrimaryKeyColumn != null && entitydescriptor.PrimaryKeyColumn.AutoIncrement)
                    entitydescriptor.PrimaryKeyColumn.SetValue(entity, Converter.Convert(loadreturnid.ExecuteScalar<object>(transaction), entitydescriptor.PrimaryKeyColumn.Property.PropertyType));
            }
            return entities.Length;
        }

        /// <summary>
        /// executes the operation
        /// </summary>
        /// <param name="entities">entities on which to operate</param>
        /// <returns>number of affected rows</returns>
        public int Execute(IEnumerable<T> entities) {
            int count = 0;
            foreach (T entity in entities)
            {
                insertoperation.Execute(interestingcolumns.Select(c => c.GetValue(entity)).ToArray());
                if (entitydescriptor.PrimaryKeyColumn != null && entitydescriptor.PrimaryKeyColumn.AutoIncrement)
                    entitydescriptor.PrimaryKeyColumn.SetValue(entity, Converter.Convert(loadreturnid.ExecuteScalar<object>(), entitydescriptor.PrimaryKeyColumn.Property.PropertyType));
                ++count;
            }
            return count;
        }

        public int Execute(Transaction transaction, IEnumerable<T> entities) {
            int count = 0;
            foreach (T entity in entities)
            {
                insertoperation.Execute(transaction, interestingcolumns.Select(c => c.GetValue(entity)).ToArray());
                if (entitydescriptor.PrimaryKeyColumn != null && entitydescriptor.PrimaryKeyColumn.AutoIncrement)
                    entitydescriptor.PrimaryKeyColumn.SetValue(entity, Converter.Convert(loadreturnid.ExecuteScalar<object>(transaction), entitydescriptor.PrimaryKeyColumn.Property.PropertyType));
                ++count;
            }

            return count;
        }
    }
}