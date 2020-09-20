using System;
using System.Data.Common;

namespace NightlyCode.Database.Clients {

    /// <summary>
    /// a connection which is never closed on disposal
    /// </summary>
    class TransactionConnection : IConnection {
        readonly IConnection inner;

        /// <summary>
        /// creates a new <see cref="TransactionConnection"/>
        /// </summary>
        /// <param name="inner"></param>
        public TransactionConnection(IConnection inner) {
            this.inner = inner;
        }

        /// <summary>
        /// connection wrapped by this connection
        /// </summary>
        public IConnection InnerConnection => inner;

        /// <inheritdoc />
        void IDisposable.Dispose() {
        }

        /// <inheritdoc />
        public DbConnection Connection => inner.Connection;
    }
}