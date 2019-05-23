using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Expressions;
using NightlyCode.Database.Entities.Operations.Prepared;

namespace NightlyCode.Database.Entities.Operations {

    /// <summary>
    /// updates values for an entity in the database
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UpdateValuesOperation<T> {
        readonly IDBClient dbclient;
        readonly Func<Type, EntityDescriptor> descriptorgetter;
        Expression[] updatesetters;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="dbclient"></param>
        /// <param name="descriptorgetter"></param>
        public UpdateValuesOperation(IDBClient dbclient, Func<Type, EntityDescriptor> descriptorgetter) {
            this.dbclient = dbclient;
            this.descriptorgetter = descriptorgetter;
        }

        /// <summary>
        /// sets the values to be updated
        /// </summary>
        /// <param name="setters"></param>
        /// <returns></returns>
        public UpdateValuesOperation<T> Set(params Expression<Func<T, bool>>[] setters) {
            updatesetters = setters;
            return this;
        }

        /// <summary>
        /// sets the criterias for the update
        /// </summary>
        /// <param name="criterias"></param>
        /// <returns></returns>
        public UpdateValuesOperation<T> Where(Expression<Func<T, bool>> criterias) {
            Criterias = criterias;
            return this;
        }

        /// <summary>
        /// criterias to use
        /// </summary>
        protected Expression Criterias { get; set; }

        /// <summary>
        /// executes the operation
        /// </summary>
        /// <param name="transaction">transaction to use (optional)</param>
        /// <returns>number of affected rows</returns>
        public int Execute(Transaction transaction=null)
        {
            return Prepare().Execute(transaction);
        }

        /// <summary>
        /// executes the operation
        /// </summary>
        /// <param name="transaction">transaction to use (optional)</param>
        /// <returns>number of affected rows</returns>
        public Task<int> ExecuteAsync(Transaction transaction = null)
        {
            return Prepare().ExecuteAsync(transaction);
        }

        /// <summary>
        /// prepares the operation for execution
        /// </summary>
        /// <returns>prepared operation</returns>
        public PreparedOperation Prepare() {
            OperationPreparator preparator = new OperationPreparator();
            preparator.AppendText("UPDATE");

            EntityDescriptor descriptor = descriptorgetter(typeof(T));

            preparator.AppendText(descriptor.TableName);
            preparator.AppendText("SET");

            if(updatesetters == null)
                throw new InvalidOperationException("No update operations specified");

            bool first=true;
            foreach(Expression setter in updatesetters) {
                if(!first)
                    preparator.AppendText(",");
                else first = false;

                CriteriaVisitor.GetCriteriaText(setter, descriptorgetter, dbclient.DBInfo, preparator);
            }

            if(Criterias != null) {
                preparator.AppendText("WHERE");
                CriteriaVisitor.GetCriteriaText(Criterias, descriptorgetter, dbclient.DBInfo, preparator);
            }
            return preparator.GetOperation(dbclient);
        }

    }
}