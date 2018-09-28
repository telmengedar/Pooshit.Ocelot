using System;
using System.Collections;
using System.Linq;
using System.Text;
using NightlyCode.DB.Clients;
using NightlyCode.DB.Entities.Descriptors;

namespace NightlyCode.DB.Entities.Operations {

    /// <summary>
    /// updates existing entities in db
    /// </summary>
    public class UpdateEntitiesOperation : EntityOperation {

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="dbclient"></param>
        /// <param name="entities"></param>
        /// <param name="descriptor"></param>
        public UpdateEntitiesOperation(IDBClient dbclient, IEnumerable entities, Func<Type, EntityDescriptor> descriptor)
            : base(dbclient, entities, descriptor) {}

        public override int Execute() {
            int affected = 0;
            using(Transaction transaction = DBClient.BeginTransaction()) {
                foreach(object entity in Entities)
                    affected += UpdateEntity(entity);
                transaction.Commit();
            }
            return affected;
        }

        int UpdateEntity(object entity) {
            if(entity == null)
                throw new NullReferenceException("Unable to update null entity");


            EntityDescriptor entitydescription = GetDescriptor(entity.GetType());
            if(entitydescription.PrimaryKeyColumn == null)
                throw new InvalidOperationException("Entity to update needs a primary key");

            EntityColumnDescriptor[] interestingcolumns = entitydescription.Columns.Where(c => !c.AutoIncrement && !c.PrimaryKey).ToArray();
            StringBuilder commandbuilder = new StringBuilder("UPDATE ");
            commandbuilder.Append(entitydescription.TableName);
            int index = 1;
            commandbuilder.Append(" SET ")
#if UNITY
                          .Append(string.Join(", ", interestingcolumns.Select(c => DBClient.DBInfo.ColumnIndicator + c.Name + DBClient.DBInfo.ColumnIndicator + "=" + DBClient.DBInfo.Parameter + index++).ToArray()));
#else
                          .Append(string.Join(", ", interestingcolumns.Select(c => DBClient.DBInfo.ColumnIndicator + c.Name + DBClient.DBInfo.ColumnIndicator + "=" + DBClient.DBInfo.Parameter + index++)));
#endif
            commandbuilder.Append(" WHERE ").Append(DBClient.DBInfo.ColumnIndicator).Append(entitydescription.PrimaryKeyColumn.Name).Append(DBClient.DBInfo.ColumnIndicator).Append("=").Append(DBClient.DBInfo.Parameter).Append(index++);
            return DBClient.NonQuery(commandbuilder.ToString(), interestingcolumns.Select(c => GetValue(c, entity)).Concat(new[] {GetValue(entitydescription.PrimaryKeyColumn, entity)}).ToArray());
        }
    }
}