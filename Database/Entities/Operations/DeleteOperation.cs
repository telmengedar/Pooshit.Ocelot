using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
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
        public long Execute(Transaction transaction = null) {
            return Prepare().Execute(transaction);
        }

        /// <summary>
        /// loads entities using the operation
        /// </summary>
        /// <returns>number of rows deleted</returns>
        public Task<long> ExecuteAsync(Transaction transaction = null) {
            return Prepare().ExecuteAsync(transaction);
        }

        /// <summary>
        /// prepares the operation for execution
        /// </summary>
        /// <returns>prepared operation to be executed</returns>
        public PreparedOperation Prepare() {
            OperationPreparator preparator = new OperationPreparator();
            preparator.AppendText("DELETE");

            EntityDescriptor descriptor = descriptorgetter(typeof(T));
            preparator.AppendText("FROM").AppendText(descriptor.TableName);

            if(Criterias != null) {
                preparator.AppendText("WHERE");
                CriteriaVisitor.GetCriteriaText(Criterias, descriptorgetter, dbclient.DBInfo, preparator);
            }
            return preparator.GetOperation(dbclient);
        }

        /// <summary>
        /// specifies criterias for the operation
        /// </summary>
        /// <param name="criterias">criterias of entities to delete</param>
        public DeleteOperation<T> Where(Expression<Func<T, bool>> criterias) {
            Criterias = criterias;
            return this;
        }
    }
}