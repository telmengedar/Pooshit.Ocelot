using System;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Extern;

namespace NightlyCode.Database.Entities.Operations.Tables {

    /// <summary>
    /// operation used to insert data into a table
    /// </summary>
    public class InsertDataOperation {
        readonly IDBClient dbclient;
        readonly string tablename;
        string[] columns;
        object[] values;

        /// <summary>
        /// creates a new <see cref="InsertDataOperation"/>
        /// </summary>
        /// <param name="dbclient">access to database</param>
        /// <param name="tablename">name of table to insert data into</param>
        public InsertDataOperation(IDBClient dbclient, string tablename) {
            this.dbclient = dbclient;
            this.tablename = tablename;
        }

        /// <summary>
        /// specifies columns to fill data into
        /// </summary>
        /// <param name="columnnames">names of columns in table</param>
        /// <returns>this operation for fluent behavior</returns>
        public InsertDataOperation Columns(params string[] columnnames) {
            columns = columnnames;
            return this;
        }

        /// <summary>
        /// values to fill into the table
        /// </summary>
        /// <param name="data">column values</param>
        /// <returns>this operation for fluent behavior</returns>
        public InsertDataOperation Values(params object[] data) {
            values = data;
            return this;
        }

        /// <summary>
        /// executes the operation
        /// </summary>
        /// <param name="transaction">transaction to use (optional)</param>
        /// <returns>number of rows inserted (should be 1 in all non error cases)</returns>
        public int Execute(Transaction transaction = null) {
            PreparedOperation operation=Prepare();

            if (transaction == null)
                return operation.Execute();
            return operation.Execute(transaction);
        }

        /// <summary>
        /// prepares this operation for execution
        /// </summary>
        /// <returns>operation prepared for execution</returns>
        public PreparedOperation Prepare() {
            OperationPreparator preparator = new OperationPreparator();
            preparator.AppendText("INSERT INTO");
            preparator.AppendText(tablename);

            bool first = true;
            if (columns.Length > 0)
            {
                preparator.AppendText("(");
                foreach (string field in columns)
                {
                    if (!first)
                        preparator.AppendText(",");
                    else first = false;
                    preparator.AppendText(dbclient.DBInfo.MaskColumn(field));
                }
                preparator.AppendText(")");
            }

            first = true;
            preparator.AppendText("VALUES(");
            if (values?.Length > 0)
            {
                foreach (object value in values)
                {
                    if (!first)
                        preparator.AppendText(",");
                    else first = false;

                    object dbvalue = value;
                    if (dbvalue is Enum)
                        dbvalue = Converter.Convert(dbvalue, Enum.GetUnderlyingType(dbvalue.GetType()));
                    preparator.AppendParameter(dbvalue);
                }
            }
            else
            {
                foreach (string column in columns)
                {
                    if (!first)
                        preparator.AppendText(",");
                    else first = false;
                    preparator.AppendParameter();
                }
            }
            preparator.AppendText(")");

            return preparator.GetOperation(dbclient);
        }
    }
}