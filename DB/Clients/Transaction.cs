using NightlyCode.DB.Entities.Operations;
using System;
using System.Data;

namespace NightlyCode.DB.Clients {

    /// <summary>
    /// transaction of db clients
    /// </summary>
    public class Transaction : IDisposable {
        readonly IDBClient client;
        bool commited;
        
        internal Transaction(IDBClient client, IDbTransaction transaction) {
            this.client = client;
            DbTransaction = transaction;
        }

        /// <summary>
        /// executes a prepared operation without result
        /// </summary>
        /// <param name="operation">operation to execute</param>
        /// <param name="values">operation parameters</param>
        public int Execute(PreparedOperation operation, params object[] values)
        {
            return client.NonQuery(this, operation.CommandText, values);
        }

        /// <summary>
        /// transaction object
        /// </summary>
        public IDbTransaction DbTransaction { get; internal set; }

        /// <summary>
        /// commits the transaction
        /// </summary>
        public void Commit() {
            DbTransaction.Commit();
            commited = true;
        }

        /// <summary>
        /// rolls back all changes made in transaction
        /// </summary>
        public void Rollback() {
            DbTransaction.Rollback();
            commited = true;
        }

        /// <summary>
        /// disposes the transaction, rolling back when it hasn't been commited
        /// </summary>
        public void Dispose() {
            GC.SuppressFinalize(this);
            if(!commited)
                DbTransaction.Rollback();
        }

        /// <summary>
        /// disposes the transaction when it wasn't disposed before
        /// </summary>
        ~Transaction() {
            Dispose();
        }
    }
}