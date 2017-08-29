using System;
using System.Linq.Expressions;
using NightlyCode.DB.Clients;
using NightlyCode.DB.Entities.Descriptors;

namespace NightlyCode.DB.Entities.Operations {

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
        /// <returns></returns>
        public int Execute() {
            return Prepare().Execute();
        }

        /// <summary>
        /// prepares the operation for execution
        /// </summary>
        /// <returns></returns>
        public PreparedDeleteOperation<T> Prepare() {
            OperationPreparator preparator = new OperationPreparator(dbclient.DBInfo);
            preparator.CommandBuilder.Append("DELETE ");

            EntityDescriptor descriptor = descriptorgetter(typeof(T));
            preparator.CommandBuilder.Append(" FROM ").Append(descriptor.TableName);

            if(Criterias != null) {
                preparator.CommandBuilder.Append(" WHERE ");
                CriteriaVisitor.GetCriteriaText(Criterias, descriptorgetter, dbclient.DBInfo, preparator);
            }
            return new PreparedDeleteOperation<T>(dbclient, preparator.GetOperation());
        }

        /// <summary>
        /// specifies criterias for the operation
        /// </summary>
        /// <param name="criterias"></param>
        /// <returns></returns>
        public DeleteOperation<T> Where(Expression<Predicate<T>> criterias) {
            Criterias = criterias;
            return this;
        }
    }
}