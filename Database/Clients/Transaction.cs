using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using NightlyCode.Database.Info;

namespace NightlyCode.Database.Clients {

    /// <summary>
    /// transaction of db clients
    /// </summary>
    public class Transaction : IDisposable {
        readonly IDBInfo dbinfo;
        readonly SemaphoreSlim semaphore;
        bool commited;
        

        internal Transaction(IDBInfo dbinfo, DbConnection connection, SemaphoreSlim semaphore) {
            this.dbinfo = dbinfo;
            this.semaphore = semaphore;
            DbTransaction = dbinfo.BeginTransaction(connection, semaphore);
        }

        /// <summary>
        /// transaction object
        /// </summary>
        public DbTransaction DbTransaction { get; internal set; }

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
        public void Rollback()
        {
            DbTransaction.Rollback();
            commited = true;
        }

        /// <summary>
        /// disposes the transaction, rolling back when it hasn't been commited
        /// </summary>
        public void Dispose() {
            GC.SuppressFinalize(this);
            if (DbTransaction != null) {
                if (!commited)
                    DbTransaction.Rollback();
                DbTransaction.Dispose();
                dbinfo.EndTransaction(semaphore);
            }
        }

        /// <summary>
        /// disposes the transaction when it wasn't disposed before
        /// </summary>
        ~Transaction() {
            Dispose();
        }
    }
}