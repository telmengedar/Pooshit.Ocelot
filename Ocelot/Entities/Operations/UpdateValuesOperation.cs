using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Expressions;
using Pooshit.Ocelot.Entities.Operations.Prepared;

namespace Pooshit.Ocelot.Entities.Operations {

    /// <summary>
    /// updates values for an entity in the database
    /// </summary>
    /// <typeparam name="T">type of data to update</typeparam>
    public class UpdateValuesOperation<T> {
        readonly IDBClient dbclient;
        readonly Func<Type, EntityDescriptor> descriptorgetter;
        readonly List<Expression> updatesetters = new();

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
            updatesetters.AddRange(setters);
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
        public long Execute(Transaction transaction = null) {
            return Prepare(false).Execute(transaction);
        }

        /// <summary>
        /// executes the operation
        /// </summary>
        /// <param name="transaction">transaction to use (optional)</param>
        /// <returns>number of affected rows</returns>
        public Task<long> ExecuteAsync(Transaction transaction = null) {
            return Prepare(false).ExecuteAsync(transaction);
        }

        /// <summary>
        /// prepares the operation for execution
        /// </summary>
        /// <returns>prepared operation</returns>
        public PreparedOperation Prepare() {
            return Prepare(true);
        }
        
        PreparedOperation Prepare(bool dbPrepare) {
            OperationPreparator preparator = new OperationPreparator();
            preparator.AppendText("UPDATE");

            EntityDescriptor descriptor = descriptorgetter(typeof(T));

            preparator.AppendText(descriptor.TableName);
            preparator.AppendText("SET");

            if(updatesetters == null)
                throw new InvalidOperationException("No update operations specified");

            bool first = true;
            foreach(Expression setter in updatesetters) {
                if(!first)
                    preparator.AppendText(",");
                else
                    first = false;

                CriteriaVisitor.GetAssignmentText(setter, descriptorgetter, dbclient.DBInfo, preparator);
            }

            if(Criterias != null) {
                preparator.AppendText("WHERE");
                CriteriaVisitor.GetCriteriaText(Criterias, descriptorgetter, dbclient.DBInfo, preparator);
            }
            return preparator.GetOperation(dbclient, dbPrepare);
        }

    }
}