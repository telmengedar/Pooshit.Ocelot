using System;
using System.Data;
using System.Threading;

namespace NightlyCode.DB.Clients {

    /// <summary>
    /// transaction of db clients
    /// </summary>
    public class Transaction : IDisposable {
        readonly object lockstate;
        bool commited = false;

        internal Transaction(object lockstate) {
            this.lockstate = lockstate;
            Monitor.Enter(lockstate);
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

        public void Dispose() {
            GC.SuppressFinalize(this);
            if(!commited)
                DbTransaction.Rollback();
            Monitor.Exit(lockstate);
        }

        ~Transaction() {
            Dispose();
        }
    }
}