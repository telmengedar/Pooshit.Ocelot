using System;
using System.Collections.Generic;
using System.Linq;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Operations.Prepared;

namespace NightlyCode.Database.Entities.Operations.Tables {

    /// <summary>
    /// updates data of a table
    /// </summary>
    public class UpdateDataOperation {
        readonly IDBClient dbclient;
        readonly string tablename;

        string[] columns;
        object[] values;

        readonly List<LoadCriteria> criterias = new List<LoadCriteria>();

        /// <summary>
        /// creates a new <see cref="UpdateDataOperation"/>
        /// </summary>
        /// <param name="dbclient">access to database</param>
        /// <param name="tablename">name of table to update</param>
        public UpdateDataOperation(IDBClient dbclient, string tablename) {
            this.dbclient = dbclient;
            this.tablename = tablename;
        }

        /// <summary>
        /// specifies a value to update with the operation
        /// </summary>
        /// <param name="columnnames">column to update</param>
        /// <returns>this operation for fluent behavior</returns>
        public UpdateDataOperation Set(params string[] columnnames) {
            columns = columnnames;
            return this;
        }

        /// <summary>
        /// specifies values to use for update operation
        /// </summary>
        /// <param name="values">values to use when executing operation</param>
        /// <returns>this operation for fluent behavior</returns>
        public UpdateDataOperation Values(params object[] values) {
            this.values = values;
            return this;
        }

        /// <summary>
        /// specifies a critera a row has to match to be updated
        /// </summary>
        /// <param name="column">column for criteria</param>
        /// <param name="op">operator used to compare column to</param>
        /// <param name="value">value to compare against</param>
        /// <param name="type">type how criteria is linked</param>
        /// <returns>this operation for fluent behavior</returns>
        public UpdateDataOperation Where(string column, string op, string value, CriteriaOperator type = CriteriaOperator.AND)
        {
            criterias.Add(new LoadCriteria(column, op, value, type));
            return this;
        }

        /// <summary>
        /// prepares the operation for execution
        /// </summary>
        /// <returns></returns>
        public PreparedOperation Prepare()
        {
            if((columns?.Length??0)==0)
                throw new InvalidOperationException("No columns to update specified");

            OperationPreparator preparator = new OperationPreparator();
            preparator.AppendText($"UPDATE {tablename} SET ");

            if (values != null) {
                if (values.Length != columns.Length)
                    throw new InvalidOperationException("Value count does not match column count");
                preparator.AppendText($"{dbclient.DBInfo.ColumnIndicator}{columns.First()}{dbclient.DBInfo.ColumnIndicator}=");
                preparator.AppendParameter(values[0]);
                for (int i = 1; i < columns.Length; ++i) {
                    preparator.AppendText($",{dbclient.DBInfo.ColumnIndicator}{columns[i]}{dbclient.DBInfo.ColumnIndicator}=");
                    preparator.AppendParameter(values[i]);
                }
            }
            else {
                preparator.AppendText($"{dbclient.DBInfo.ColumnIndicator}{columns.First()}{dbclient.DBInfo.ColumnIndicator}=");
                preparator.AppendParameter();
                foreach (string column in columns.Skip(1)) {
                    preparator.AppendText($",{dbclient.DBInfo.ColumnIndicator}{column}{dbclient.DBInfo.ColumnIndicator}=");
                    preparator.AppendParameter();
                }
            }

            if (criterias.Any())
            {
                preparator.AppendText("WHERE");
                bool flag = true;
                foreach (LoadCriteria criteria in criterias)
                {
                    if (flag) flag = false;
                    else preparator.AppendText(criteria.Type.ToString());

                    preparator.AppendText(dbclient.DBInfo.MaskColumn(criteria.Column));
                    preparator.AppendText(criteria.Operator);
                    preparator.AppendParameter(criteria.Value);
                }
            }

            return preparator.GetOperation(dbclient);
        }

        /// <summary>
        /// executes the operation using a transaction
        /// </summary>
        /// <param name="transaction">transaction to use</param>
        /// <param name="parameters">parameters to use for operation</param>
        /// <returns>number of affected rows</returns>
        public int Execute(Transaction transaction, params object[] parameters) {
            return Prepare().Execute(transaction, parameters);
        }

        /// <summary>
        /// executes the operation using a transaction
        /// </summary>
        /// <param name="parameters">parameters to use for operation</param>
        /// <returns>number of affected rows</returns>
        public int Execute(params object[] parameters)
        {
            return Prepare().Execute(parameters);
        }

    }
}