using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Clients {

    /// <summary>
    /// transaction of db clients
    /// </summary>
    public class Transaction : IDisposable {
        readonly IDBInfo dbinfo;
        readonly TransactionConnection connection;
        readonly SemaphoreSlim semaphore;
        bool commited;


        internal Transaction(IDBInfo dbinfo, IConnection connection, SemaphoreSlim semaphore) {
            this.dbinfo = dbinfo;
            this.connection = new TransactionConnection(connection);
            this.semaphore = semaphore;
            DbTransaction = dbinfo.BeginTransaction(connection.Connection, semaphore);
        }

        /// <summary>
        /// transaction object
        /// </summary>
        public DbTransaction DbTransaction { get; internal set; }

        /// <summary>
        /// get the associated connection and opens it if it is not open
        /// </summary>
        /// <returns>connection</returns>
        public IConnection Connect() {
            if(connection.Connection.State != ConnectionState.Open)
                connection.Connection.Open();
            return connection;
        }

        /// <summary>
        /// get the associated connection and opens it if it is not open
        /// </summary>
        /// <returns>connection</returns>
        public async Task<IConnection> ConnectAsync() {
            if(connection.Connection.State != ConnectionState.Open)
                await connection.Connection.OpenAsync();
            return connection;
        }

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
            if(DbTransaction != null) {
                if(!commited)
                    DbTransaction.Rollback();
                DbTransaction.Dispose();
                dbinfo.EndTransaction(semaphore);
                
            }
            connection.InnerConnection.Dispose();
        }

        /// <summary>
        /// disposes the transaction when it wasn't disposed before
        /// </summary>
        ~Transaction() {
            Dispose();
        }
    }
}