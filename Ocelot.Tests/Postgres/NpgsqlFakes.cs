using System;

namespace Npgsql {

    /// <summary>
    /// fake Npgsql-shaped exception whose <see cref="Type.FullName"/> starts with "Npgsql." so that
    /// <c>PostgreInfo.IsConnectionLost</c>'s reflection-based walk can exercise the SqlState code
    /// path without pulling in a real Npgsql dependency.
    /// Exposes a <c>SqlState</c> property that the classifier reads via reflection.
    /// </summary>
    internal class FakeNpgsqlException : Exception {
        public FakeNpgsqlException(string sqlState, string message) : base(message) {
            SqlState = sqlState;
        }

        public string SqlState { get; }
    }

    /// <summary>
    /// fake Npgsql-shaped exception without a SqlState property — exercises the message-fallback branch
    /// for NpgsqlException types that do not carry SqlState.
    /// </summary>
    internal class FakeNpgsqlNoSqlStateException : Exception {
        public FakeNpgsqlNoSqlStateException(string message) : base(message) { }
    }
}
