using System;
using System.Linq;
using System.Linq.Expressions;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Expressions;

namespace NightlyCode.Database.Entities.Operations {

    /// <summary>
    /// operation used to load entities
    /// </summary>
    public class DeleteOperation<T> {
        readonly IDBClient dbclient;
        readonly Func<Type, EntityDescriptor> descriptorgetter;
 
        /// <summary>
        /// creates a new delete operation
        /// </summary>
        /// <param name="origin"></param>
        internal DeleteOperation(DeleteOperation<T> origin)
            : this(origin.dbclient, origin.descriptorgetter) {
        }

        /// <summary>
        /// creates a new delete operation
        /// </summary>
        /// <param name="dbclient"> </param>
        /// <param name="descriptorgetter"></param>
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
            return dbclient.NonQuery(operation.CommandText, operation.Parameters.Select(p => p.Value).ToArray());
        }

        /// <summary>
        /// loads entities using the operation
        /// </summary>
        /// <returns>number of rows deleted</returns>
        public int Execute(Transaction transaction)
        {
            PreparedOperation operation = Prepare();
            return dbclient.NonQuery(transaction, operation.CommandText, operation.Parameters.Select(p => p.Value).ToArray());
        }

        /// <summary>
        /// prepares the operation for execution
        /// </summary>
        /// <returns></returns>
        public PreparedOperation Prepare() {
            OperationPreparator preparator = new OperationPreparator(dbclient.DBInfo);
            preparator.CommandBuilder.Append("DELETE ");

            EntityDescriptor descriptor = descriptorgetter(typeof(T));
            preparator.CommandBuilder.Append(" FROM ").Append(descriptor.TableName);

            if(Criterias != null) {
                preparator.CommandBuilder.Append(" WHERE ");
                CriteriaVisitor.GetCriteriaText(Criterias, descriptorgetter, dbclient.DBInfo, preparator);
            }
            return preparator.GetOperation();
        }

        /// <summary>
        /// specifies criterias for the operation
        /// </summary>
        /// <param name="criterias"></param>
        /// <returns></returns>
        public DeleteOperation<T> Where(Expression<Func<T,bool>> criterias) {
            Criterias = criterias;
            return this;
        }
    }
}