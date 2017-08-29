using System;
using System.Collections;
using System.Linq;
using System.Text;
using NightlyCode.DB.Clients;
using NightlyCode.DB.Entities.Descriptors;
using Converter = NightlyCode.DB.Extern.Converter;

namespace NightlyCode.DB.Entities.Operations {

    /// <summary>
    /// inserts new entities to the db
    /// </summary>
    public class InsertEntitiesOperation : EntityOperation {

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="dbclient"> </param>
        /// <param name="entities"></param>
        /// <param name="descriptor"> </param>
        public InsertEntitiesOperation(IDBClient dbclient, IEnumerable entities, Func<Type, EntityDescriptor> descriptor)
            : base(dbclient, entities, descriptor) {}

        public override int Execute() {
            int affected = 0;
            using(Transaction transaction=DBClient.BeginTransaction()) {
                foreach(object entity in Entities) {
                    InsertEntity(entity);
                    affected++;
                }
                transaction.Commit();
            }
            return affected;
        }

        void InsertEntity(object entity) {
            if(entity == null)
                throw new NullReferenceException("Unable to insert null entity");


            EntityDescriptor entitydescription = GetDescriptor(entity.GetType());

            EntityColumnDescriptor[] interestingcolumns = entitydescription.Columns.Where(c => !c.AutoIncrement).ToArray();
            StringBuilder commandbuilder = new StringBuilder("INSERT INTO ");
            commandbuilder.Append(entitydescription.TableName);
            commandbuilder.Append(" (")
                            .Append(DBClient.DBInfo.ColumnIndicator)
#if UNITY
                            .Append(string.Join(string.Format("{0}, {0}", DBClient.DBInfo.ColumnIndicator), interestingcolumns.Select(c => c.Name).ToArray()))
#else
                            .Append(string.Join(string.Format("{0}, {0}", DBClient.DBInfo.ColumnIndicator), interestingcolumns.Select(c => c.Name)))
#endif
                            .Append(DBClient.DBInfo.ColumnIndicator)
                            .Append(")");
            int index = 1;
            commandbuilder.Append(" VALUES(")
#if UNITY
                          .Append(string.Join(", ", interestingcolumns.Select(c => DBClient.DBInfo.Parameter + index++).ToArray()))
#else
                          .Append(string.Join(", ", interestingcolumns.Select(c => DBClient.DBInfo.Parameter + index++)))
#endif
                          .Append(")");
            if(entitydescription.PrimaryKeyColumn != null && entitydescription.PrimaryKeyColumn.AutoIncrement) {
                object id = Converter.Convert(DBClient.DBInfo.ReturnInsertID(DBClient, entitydescription, commandbuilder.ToString(), interestingcolumns.Select(c => GetValue(c, entity)).ToArray()), entitydescription.PrimaryKeyColumn.Property.PropertyType);
                entitydescription.PrimaryKeyColumn.SetValue(entity, id);
            }
            else {
                DBClient.NonQuery(commandbuilder.ToString(), interestingcolumns.Select(c => GetValue(c, entity)).ToArray());
            }
        }
    }
}