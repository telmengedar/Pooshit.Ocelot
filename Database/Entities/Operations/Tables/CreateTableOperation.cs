﻿using System;
using System.Collections.Generic;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Entities.Schema;

namespace NightlyCode.Database.Entities.Operations.Tables {

    /// <summary>
    /// operation used to create tables in a database
    /// </summary>
    public class CreateTableOperation {
        readonly IDBClient dbclient;
        readonly string tablename;
        readonly List<SchemaColumnDescriptor> columns=new List<SchemaColumnDescriptor>();

        /// <summary>
        /// creates a new <see cref="CreateTableOperation"/>
        /// </summary>
        /// <param name="dbclient">access to database</param>
        /// <param name="tablename">name of table to create</param>
        public CreateTableOperation(IDBClient dbclient, string tablename) {
            this.dbclient = dbclient;
            this.tablename = tablename;
        }

        /// <summary>
        /// specifies a column to create
        /// </summary>
        /// <param name="descriptor">descriptor for column</param>
        /// <returns>this operation for fluent behavior</returns>
        public CreateTableOperation Column(SchemaColumnDescriptor descriptor) {
            columns.Add(descriptor);
            return this;
        }

        /// <summary>
        /// specifies a column to create
        /// </summary>
        /// <param name="name">name of column</param>
        /// <param name="type">type of data column contains</param>
        /// <param name="primarykey">true if column is a primary key</param>
        /// <param name="autoincrement">true if column is auto incremented</param>
        /// <param name="defaultvalue">default value for column</param>
        /// <returns>this operation for fluent behavior</returns>
        public CreateTableOperation Column(string name, Type type = null, bool primarykey = false, bool autoincrement = false, object defaultvalue=null) {
            SchemaColumnDescriptor descriptor = new SchemaColumnDescriptor(name) {
                Type = dbclient.DBInfo.GetDBType(type ?? typeof(string)),
                PrimaryKey = primarykey,
                AutoIncrement = autoincrement,
                DefaultValue = defaultvalue
            };
            return Column(descriptor);
        }

        /// <summary>
        /// executes the operation
        /// </summary>
        /// <param name="transaction">transaction to use (optional)</param>
        /// <returns>number of affected rows</returns>
        public int Execute(Transaction transaction=null) {
            if (transaction == null)
                return Prepare().Execute();
            return Prepare().Execute(transaction);
        }

        /// <summary>
        /// prepares the operation for execution
        /// </summary>
        /// <returns>operation to execute</returns>
        public PreparedOperation Prepare() {
            OperationPreparator preparator=new OperationPreparator();
            preparator.AppendText("CREATE TABLE").AppendText(tablename).AppendText("(");

            bool firstindicator = true;
            foreach (SchemaColumnDescriptor column in columns)
            {
                if (firstindicator) firstindicator = false;
                else preparator.AppendText(",");

                dbclient.DBInfo.CreateColumn(preparator, column);
            }

            preparator.AppendText(")");

            if (!string.IsNullOrEmpty(dbclient.DBInfo.CreateSuffix))
                preparator.AppendText(dbclient.DBInfo.CreateSuffix);

            return preparator.GetOperation(dbclient);
        }
    }
}