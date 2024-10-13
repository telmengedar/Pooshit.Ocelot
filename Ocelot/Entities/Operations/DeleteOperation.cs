using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Expressions;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Entities.Operations.Tables;
using Pooshit.Ocelot.Tokens;

namespace Pooshit.Ocelot.Entities.Operations {

    /// <summary>
    /// operation used to delete entities
    /// </summary>
    public class DeleteOperation : WhereTokenOperation {
        readonly IDBClient dbclient;
        readonly string table;
        
        /// <summary>
        /// creates a new delete operation
        /// </summary>
        /// <param name="dbclient">access to database used for execution</param>
        /// <param name="table">name of table to drop</param>
        public DeleteOperation(IDBClient dbclient, string table) {
            this.dbclient = dbclient;
            this.table = table;
        }

        /// <summary>
        /// adds a predicate for the operation
        /// </summary>
        /// <param name="predicate">predicate to append</param>
        /// <param name="mergeOp">operation to use when predicate is to be merged with an existing</param>
        public new DeleteOperation Where(ISqlToken predicate, CriteriaOperator mergeOp = CriteriaOperator.AND) {
            base.Where(predicate, mergeOp);
            return this;
        }
        
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
            OperationPreparator preparator = new();
            preparator.AppendText("DELETE");
            preparator.AppendText("FROM").AppendText(table);

            AppendCriterias(dbclient.DBInfo, preparator);
            
            return preparator.GetOperation(dbclient, false);
        }
    }
        
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
            OperationPreparator preparator = new();
            preparator.AppendText("DELETE");

            EntityDescriptor descriptor = descriptorgetter(typeof(T));
            preparator.AppendText("FROM").AppendText(descriptor.TableName);

            if(Criterias != null) {
                preparator.AppendText("WHERE");
                CriteriaVisitor.GetCriteriaText(Criterias, descriptorgetter, dbclient.DBInfo, preparator);
            }
            return preparator.GetOperation(dbclient, false);
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