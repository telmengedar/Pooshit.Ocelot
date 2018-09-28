using System;
using System.Collections;
using System.Text;
using NightlyCode.DB.Clients;
using NightlyCode.DB.Entities.Descriptors;

namespace NightlyCode.DB.Entities.Operations {

    /// <summary>
    /// removes entities from db
    /// </summary>
    public class DeleteEntitiesOperation : EntityOperation {

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="dbclient"></param>
        /// <param name="entities"></param>
        /// <param name="descriptor"></param>
        public DeleteEntitiesOperation(IDBClient dbclient, IEnumerable entities, Func<Type, EntityDescriptor> descriptor)
            : base(dbclient, entities, descriptor) {}

        public override int Execute() {
            int affected = 0;
            using(Transaction transaction=DBClient.BeginTransaction()) {
                foreach(object entity in Entities)
                    affected += RemoveEntity(entity);
                transaction.Commit();
            }
            return affected;            
        }

        int RemoveEntity(object entity) {
            if(entity == null)
                throw new NullReferenceException("Unable to remove null entity");


            EntityDescriptor entitydescription = GetDescriptor(entity.GetType());
            if(entitydescription.PrimaryKeyColumn == null)
                throw new InvalidOperationException("Entity to remove needs a primary key");

            StringBuilder commandbuilder = new StringBuilder("DELETE FROM ");
            commandbuilder.Append(entitydescription.TableName);
            int index = 1;
            commandbuilder.Append(" WHERE ").Append(DBClient.DBInfo.ColumnIndicator).Append(entitydescription.PrimaryKeyColumn.Name).Append(DBClient.DBInfo.ColumnIndicator).Append("=").Append(DBClient.DBInfo.Parameter).Append(index++);
            return DBClient.NonQuery(commandbuilder.ToString(), entitydescription.PrimaryKeyColumn.GetValue(entity));
        }
    }
}