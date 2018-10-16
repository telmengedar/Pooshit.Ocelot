using System;
using System.Linq.Expressions;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Expressions;
using NightlyCode.Database.Entities.Operations.Prepared;

namespace NightlyCode.Database.Entities.Operations {

    /// <summary>
    /// operation used to delete entities
    /// </summary>
    public class DeleteOperation<T> {
        readonly IDBClient dbclient;
        readonly Func<Type, EntityDescriptor> descriptorgetter;
 
        /// <summary>
        /// creates a new delete operation
        /// </summary>
        /// <param name="dbclient">access to database used for execution</param>
        /// <param name="descriptorgetter">information about entities</param>
        public DeleteOperation(IDBClient dbclient, Func<Type, EntityDescriptor> descriptorgetter) {
            this.descriptorgetter = descriptorgetter;
            this.dbclient = dbclient;
        }

        /// <summary>
        /// criterias to use when loading
        /// </summary>
        protected Expression Criterias { get; set; }

        /// <summary>
        /// loads entities using the operation
        /// </summary>
        /// <returns>number of rows deleted</returns>
        public int Execute() {
            PreparedOperation operation = Prepare();
            return dbclient.NonQuery(operation.CommandText, operation.Parameters);
        }

        /// <summary>
        /// loads entities using the operation
        /// </summary>
        /// <returns>number of rows deleted</returns>
        public int Execute(Transaction transaction)
        {
            PreparedOperation operation = Prepare();
            return dbclient.NonQuery(transaction, operation.CommandText, operation.Parameters);
        }

        /// <summary>
        /// prepares the operation for execution
        /// </summary>
        /// <returns>prepared operation to be executed</returns>
        public PreparedOperation Prepare() {
            OperationPreparator preparator = new OperationPreparator(dbclient.DBInfo);
            preparator.CommandBuilder.Append("DELETE ");

            EntityDescriptor descriptor = descriptorgetter(typeof(T));
            preparator.CommandBuilder.Append(" FROM ").Append(descriptor.TableName);

            if(Criterias != null) {
                preparator.CommandBuilder.Append(" WHERE ");
                CriteriaVisitor.GetCriteriaText(Criterias, descriptorgetter, dbclient.DBInfo, preparator);
            }
            return preparator.GetOperation(dbclient);
        }

        /// <summary>
        /// specifies criterias for the operation
        /// </summary>
        /// <param name="criterias">criterias of entities to delete</param>
        public DeleteOperation<T> Where(Expression<Func<T,bool>> criterias) {
            Criterias = criterias;
            return this;
        }
    }
}