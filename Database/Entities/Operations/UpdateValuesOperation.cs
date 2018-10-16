using System;
using System.Linq;
using System.Linq.Expressions;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Expressions;

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

        public int Execute(Transaction transaction)
        {
            PreparedOperation operation = Prepare();
            return dbclient.NonQuery(transaction, operation.CommandText, operation.Parameters);
        }

        public int Execute() {
            PreparedOperation operation = Prepare();
            return dbclient.NonQuery(operation.CommandText, operation.Parameters);
        }

        public PreparedOperation Prepare() {
            OperationPreparator preparator = new OperationPreparator(dbclient.DBInfo);
            preparator.CommandBuilder.Append("UPDATE ");

            EntityDescriptor descriptor = descriptorgetter(typeof(T));

            preparator.CommandBuilder.Append(descriptor.TableName);
            preparator.CommandBuilder.Append(" SET ");

            if(updatesetters == null)
                throw new InvalidOperationException("No update operations specified");

            bool first=true;
            foreach(Expression setter in updatesetters) {
                if(!first)
                    preparator.CommandBuilder.Append(", ");
                else first = false;

                CriteriaVisitor.GetCriteriaText(setter, descriptorgetter, dbclient.DBInfo, preparator);
            }

            if(Criterias != null) {
                preparator.CommandBuilder.Append(" WHERE ");
                CriteriaVisitor.GetCriteriaText(Criterias, descriptorgetter, dbclient.DBInfo, preparator);
            }
            return preparator.GetOperation(dbclient);
        }

    }
}