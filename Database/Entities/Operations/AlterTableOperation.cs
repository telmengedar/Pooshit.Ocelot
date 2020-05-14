using System.Collections.Generic;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Prepared;

namespace NightlyCode.Database.Entities.Operations {

    /// <summary>
    /// operation used to alter a table
    /// </summary>
    public class AlterTableOperation : IOperation {
        readonly IDBClient dbclient;
        readonly string tablename;

        readonly List<string> todrop = new List<string>();
        readonly List<EntityColumnDescriptor> toadd = new List<EntityColumnDescriptor>();
        readonly List<EntityColumnDescriptor> tomodify = new List<EntityColumnDescriptor>();

        /// <summary>
        /// creates a new <see cref="AlterTableOperation"/>
        /// </summary>
        /// <param name="dbclient">client to use to execute statement</param>
        /// <param name="tablename">name of table to alter</param>
        public AlterTableOperation(IDBClient dbclient, string tablename) {
            this.dbclient = dbclient;
            this.tablename = tablename;
        }

        /// <summary>
        /// adds columns to drop
        /// </summary>
        /// <param name="columns">name of columns to drop</param>
        /// <returns>this operation for fluent behavior</returns>
        public AlterTableOperation Drop(params string[] columns) {
            todrop.AddRange(columns);
            return this;
        }

        /// <summary>
        /// adds columns to add
        /// </summary>
        /// <param name="columns">column info of columns to add</param>
        /// <returns>this operation for fluent behavior</returns>
        public AlterTableOperation Add(params EntityColumnDescriptor[] columns) {
            toadd.AddRange(columns);
            return this;
        }

        /// <summary>
        /// adds columns to modify
        /// </summary>
        /// <param name="columns">column info of columns to modify</param>
        /// <returns>this operation for fluent behavior</returns>
        public AlterTableOperation Modify(params EntityColumnDescriptor[] columns) {
            tomodify.AddRange(columns);
            return this;
        }

        /// <inheritdoc />
        public PreparedOperation Prepare() {
            OperationPreparator preparator = new OperationPreparator();
            preparator.AppendText("ALTER TABLE").AppendText(tablename);

            bool first = true;
            foreach(string column in todrop) {
                if(!first)
                    preparator.AppendText(",");
                first = false;
                dbclient.DBInfo.DropColumn(preparator, column);
            }

            foreach(EntityColumnDescriptor column in toadd) {
                if(!first)
                    preparator.AppendText(",");
                first = false;
                dbclient.DBInfo.AddColumn(preparator, column);
            }

            foreach(EntityColumnDescriptor column in tomodify) {
                if(!first)
                    preparator.AppendText(",");
                first = false;
                dbclient.DBInfo.AlterColumn(preparator, column);
            }

            return preparator.GetOperation(dbclient);
        }
    }
}