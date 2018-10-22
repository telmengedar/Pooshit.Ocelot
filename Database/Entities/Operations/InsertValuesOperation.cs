using System;
using System.Linq;
using System.Linq.Expressions;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Fields;
using NightlyCode.Database.Entities.Operations.Prepared;
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
        /// <returns>number of rows affected</returns>
        public long Execute(Transaction transaction=null) {
            if (insertcolumns?.Length != insertvalues?.Length)
                throw new InvalidOperationException("unable to execute insert operation. Number of value parameters does not match number of columns.");

            PreparedOperation operation = Prepare();

            if(transaction == null)
                return Prepare().Execute();
            return Prepare().Execute(transaction);
        }

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
                    else first = false;
                    dbclient.DBInfo.Append(field, preparator, descriptorgetter);
                }
                preparator.AppendText(")");
            }

            first = true;
            preparator.AppendText("VALUES(");
            if (insertvalues?.Length > 0) {
                foreach(object value in insertvalues) {
                    if(!first)
                        preparator.AppendText(",");
                    else first = false;

                    object dbvalue = value;
                    if(dbvalue is Enum)
                        dbvalue = Converter.Convert(dbvalue, Enum.GetUnderlyingType(dbvalue.GetType()));
                    preparator.AppendParameter(dbvalue);
                }
            }
            else {
                foreach (IDBField column in insertcolumns)
                {
                    if (!first)
                        preparator.AppendText(", ");
                    else first = false;
                    preparator.AppendParameter();
                }
            }
            preparator.AppendText(")");

            return preparator.GetOperation(dbclient);
        }

    }
}