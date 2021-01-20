using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Fields;
using Converter = NightlyCode.Database.Extern.Converter;

namespace NightlyCode.Database.Entities.Operations {

    /// <summary>
    /// updates values for an entity in the database
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InsertValuesOperation<T> {
        readonly IDBClient dbclient;
        readonly Func<Type, EntityDescriptor> descriptorgetter;
        IDBField[] insertcolumns;
        object[] insertvalues;
        bool returnid;

        /// <summary>
        /// creates a new insert values operation
        /// </summary>
        /// <param name="dbclient"></param>
        /// <param name="descriptorgetter"></param>
        public InsertValuesOperation(IDBClient dbclient, Func<Type, EntityDescriptor> descriptorgetter) {
            this.dbclient = dbclient;
            this.descriptorgetter = descriptorgetter;
        }

        /// <summary>
        /// indicates that the insert operation shall return the id of the inserted row
        /// </summary>
        /// <returns>this operation for fluent behavior</returns>
        public InsertValuesOperation<T> ReturnID() {
            returnid = true;
            return this;
        }

        /// <summary>
        /// sets the columns to be updated
        /// </summary>
        /// <returns></returns>
        public InsertValuesOperation<T> Columns(params IDBField[] columns) {
            insertcolumns = columns.ToArray();
            return this;
        }

        /// <summary>
        /// sets the columns to be updated
        /// </summary>
        /// <returns></returns>
        public InsertValuesOperation<T> Columns(params Expression<Func<T, object>>[] columns) {
            return Columns(columns.Select(EntityField.Create).Cast<IDBField>().ToArray());
        }

        /// <summary>
        /// sets the values to be updated
        /// </summary>
        /// <returns></returns>
        public InsertValuesOperation<T> Values(params object[] values) {
            insertvalues = values.ToArray();
            return this;
        }

        /// <summary>
        /// executes the insert operation
        /// </summary>
        /// <param name="parameters">parameters to use when executing operation</param>
        /// <returns>number of rows affected</returns>
        public long Execute(params object[] parameters) {
            return Execute(null, parameters);
        }

        /// <summary>
        /// executes the insert operation
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="parameters">parameters to use when executing operation</param>
        /// <returns>number of rows affected</returns>
        public long Execute(Transaction transaction, params object[] parameters) {
            if(parameters.Length==0 && insertcolumns?.Length != insertvalues?.Length)
                throw new InvalidOperationException("unable to execute insert operation. Number of value parameters does not match number of columns.");

            PreparedOperation operation = Prepare();

            return operation.Execute(transaction, parameters);
        }

        /// <summary>
        /// executes the insert operation
        /// </summary>
        /// <param name="parameters">parameters to use when executing operation</param>
        /// <returns>number of rows affected</returns>
        public Task<long> ExecuteAsync(params object[] parameters) {
            return ExecuteAsync(null, parameters);
        }

        /// <summary>
        /// executes the insert operation
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="parameters">parameters to use when executing operation</param>
        /// <returns>number of rows affected</returns>
        public Task<long> ExecuteAsync(Transaction transaction, params object[] parameters) {
            if(insertcolumns?.Length != insertvalues?.Length)
                throw new InvalidOperationException("unable to execute insert operation. Number of value parameters does not match number of columns.");

            PreparedOperation operation = Prepare();

            return operation.ExecuteAsync(transaction, parameters);
        }

        /// <summary>
        /// prepares the insert command for a bulk data insert
        /// </summary>
        /// <returns>operation to be used for a bulk insert</returns>
        public PreparedBulkInsertOperation PrepareBulk() {
            OperationPreparator preparator = new OperationPreparator();
            preparator.AppendText("INSERT INTO");

            EntityDescriptor descriptor = descriptorgetter(typeof(T));

            preparator.AppendText(descriptor.TableName);

            bool first = true;
            if(insertcolumns.Length > 0) {
                preparator.AppendText(" (");
                foreach(IDBField field in insertcolumns) {
                    if(!first)
                        preparator.AppendText(",");
                    else
                        first = false;
                    dbclient.DBInfo.Append(field, preparator, descriptorgetter);
                }
                preparator.AppendText(")");
            }
            preparator.AppendText("VALUES");
            
            return preparator.GetBulkInsertOperation(dbclient);
        }
        
        /// <summary>
        /// prepares operation for execution
        /// </summary>
        /// <returns>operation for execution</returns>
        public PreparedOperation Prepare() {
            OperationPreparator preparator = new OperationPreparator();
            preparator.AppendText("INSERT INTO");

            EntityDescriptor descriptor = descriptorgetter(typeof(T));

            preparator.AppendText(descriptor.TableName);

            bool first = true;
            if(insertcolumns.Length > 0) {
                preparator.AppendText(" (");
                foreach(IDBField field in insertcolumns) {
                    if(!first)
                        preparator.AppendText(",");
                    else
                        first = false;
                    dbclient.DBInfo.Append(field, preparator, descriptorgetter);
                }
                preparator.AppendText(")");
            }

            first = true;
            preparator.AppendText("VALUES(");
            if(insertvalues?.Length > 0) {
                foreach(object value in insertvalues) {
                    if(!first)
                        preparator.AppendText(",");
                    else
                        first = false;

                    object dbvalue = value;
                    if(dbvalue is Enum)
                        dbvalue = Converter.Convert(dbvalue, Enum.GetUnderlyingType(dbvalue.GetType()));
                    preparator.AppendParameter(dbvalue);
                }
            }
            else {
                foreach(IDBField column in insertcolumns) {
                    if(!first)
                        preparator.AppendText(", ");
                    else
                        first = false;
                    preparator.AppendParameter();
                }
            }
            preparator.AppendText(")");
            if(returnid) {
                dbclient.DBInfo.ReturnID(preparator, descriptor.PrimaryKeyColumn);
                return preparator.GetReturnIdOperation(dbclient);
            }

            return preparator.GetOperation(dbclient);
        }

    }
}