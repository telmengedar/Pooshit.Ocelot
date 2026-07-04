using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pooshit.Ocelot.Errors;
using Pooshit.Ocelot.Extensions;
using Pooshit.Ocelot.Info;
using DataTable = Pooshit.Ocelot.Clients.Tables.DataTable;

namespace Pooshit.Ocelot.Clients;

/// <summary>
/// client to execute database commands
/// </summary>
public class DBClient : ADbClient {
    /// <summary>
    /// creates a new <see cref="DBClient" />
    /// </summary>
    /// <param name="connectionprovider">provides connection to database</param>
    /// <param name="dbinfo">information for database statements</param>
    internal DBClient(IConnectionProvider connectionprovider, IDBInfo dbinfo) {
        Connection = connectionprovider;
        DBInfo = dbinfo;
    }

    /// <inheritdoc />
    public override IDBInfo DBInfo { get; }

    /// <inheritdoc />
    public override IConnectionProvider Connection { get; }

    DbCommand PrepareCommand(IConnection connection, string commandtext, IEnumerable<object> parameters) {
        DbCommand command = connection.Connection.CreateCommand();
        command.CommandText = commandtext;
        command.CommandTimeout = 0;

        foreach (object value in parameters)
            DBInfo.CreateParameter(command, value);

        return command;
    }

    IConnection Connect(Transaction transaction) => transaction?.Connect() ?? Connection.Connect();

    Task<IConnection> ConnectAsync(Transaction transaction) => transaction?.ConnectAsync() ?? Connection.ConnectAsync();

    /// <inheritdoc />
    public override Transaction Transaction() => new(DBInfo, Connection.Connect(), null);

    PreparedCommand PrepareCommand(Transaction transaction, string commandString, IEnumerable<object> parameters, bool prepare) {
        IConnection connection = Connect(transaction);
        DbCommand command = PrepareCommand(connection, commandString, parameters);
        if (prepare)
            command.Prepare();
        if (transaction != null)
            command.Transaction = transaction.DbTransaction;
        return new PreparedCommand(connection, command);
    }

    // ── Parameter materialization ─────────────────────────────────────────────────

    /// <summary>
    /// materializes an <see cref="IEnumerable{T}"/> of parameters to an array so it can be
    /// iterated more than once — required for the connection-loss retry path
    /// </summary>
    static object[] MaterializeParameters(IEnumerable<object> parameters) =>
        parameters as object[] ?? parameters.ToArray();

    // ── Sync read helpers (with connection-loss retry) ────────────────────────────

    /// <summary>
    /// opens a <see cref="DbDataReader"/> with a single connection-loss retry.
    /// The caller owns the returned (<see cref="PreparedCommand"/>, <see cref="DbDataReader"/>)
    /// and must dispose both.
    /// </summary>
    (PreparedCommand, DbDataReader) OpenReaderWithRetry(Transaction transaction, string commandText, object[] paramArray, bool prepare) {
        PreparedCommand command = null;
        try {
            command = PrepareCommand(transaction, commandText, paramArray, prepare);
            DbDataReader reader = command.Command.ExecuteReader();
            return (command, reader);
        }
        catch (Exception e) when (transaction == null && DBInfo.IsConnectionLost(e)) {
            command?.Dispose();
            command = null;
            // Retry once: the dialect detected a dead connection; reconnect and re-prepare
            PreparedCommand retryCmd = null;
            try {
                retryCmd = PrepareCommand(null, commandText, paramArray, prepare);
                DbDataReader retryReader = retryCmd.Command.ExecuteReader();
                return (retryCmd, retryReader);
            }
            catch (Exception retryEx) {
                retryCmd?.Dispose();
                throw new StatementException(commandText, paramArray, retryEx);
            }
        }
        catch (Exception e) {
            command?.Dispose();
            throw new StatementException(commandText, paramArray, e);
        }
    }

    /// <summary>
    /// executes a materializing read delegate with a single connection-loss retry.
    /// Handles the <see cref="PreparedCommand"/> lifecycle; the caller receives the result directly.
    /// </summary>
    T ExecuteMaterializedReadWithRetry<T>(Transaction transaction, string commandText, object[] paramArray, bool prepare, Func<DbCommand, T> execute) {
        PreparedCommand command = null;
        try {
            command = PrepareCommand(transaction, commandText, paramArray, prepare);
            return execute(command.Command);
        }
        catch (Exception e) when (transaction == null && DBInfo.IsConnectionLost(e)) {
            command?.Dispose();
            command = null;
            PreparedCommand retryCmd = null;
            try {
                retryCmd = PrepareCommand(null, commandText, paramArray, prepare);
                return execute(retryCmd.Command);
            }
            catch (Exception retryEx) {
                throw new StatementException(commandText, paramArray, retryEx);
            }
            finally {
                retryCmd?.Dispose();
            }
        }
        catch (Exception e) {
            throw new StatementException(commandText, paramArray, e);
        }
        finally {
            command?.Dispose();
        }
    }

    // ── Async read helpers (with connection-loss retry) ───────────────────────────

    /// <summary>
    /// opens a <see cref="DbDataReader"/> asynchronously with a single connection-loss retry.
    /// The caller owns the returned (<see cref="PreparedCommand"/>, <see cref="DbDataReader"/>)
    /// and must dispose both.
    /// </summary>
    async Task<(PreparedCommand, DbDataReader)> OpenReaderWithRetryAsync(Transaction transaction, string commandText, object[] paramArray, bool prepare, CancellationToken cancellationToken) {
        PreparedCommand command = null;
        try {
            command = PrepareCommand(transaction, commandText, paramArray, prepare);
            DbDataReader reader = await command.Command.ExecuteReaderAsync(cancellationToken);
            return (command, reader);
        }
        catch (OperationCanceledException) {
            command?.Dispose();
            throw;
        }
        catch (Exception e) when (transaction == null && DBInfo.IsConnectionLost(e)) {
            command?.Dispose();
            command = null;
            PreparedCommand retryCmd = null;
            try {
                retryCmd = PrepareCommand(null, commandText, paramArray, prepare);
                DbDataReader retryReader = await retryCmd.Command.ExecuteReaderAsync(cancellationToken);
                return (retryCmd, retryReader);
            }
            catch (OperationCanceledException) {
                retryCmd?.Dispose();
                throw;
            }
            catch (Exception retryEx) {
                retryCmd?.Dispose();
                throw new StatementException(commandText, paramArray, retryEx);
            }
        }
        catch (Exception e) {
            command?.Dispose();
            throw new StatementException(commandText, paramArray, e);
        }
    }

    /// <summary>
    /// executes an async materializing read delegate with a single connection-loss retry.
    /// </summary>
    async Task<T> ExecuteMaterializedReadWithRetryAsync<T>(Transaction transaction, string commandText, object[] paramArray, bool prepare, Func<DbCommand, CancellationToken, Task<T>> executeAsync, CancellationToken cancellationToken) {
        PreparedCommand command = null;
        try {
            command = PrepareCommand(transaction, commandText, paramArray, prepare);
            return await executeAsync(command.Command, cancellationToken);
        }
        catch (OperationCanceledException) {
            throw;
        }
        catch (Exception e) when (transaction == null && DBInfo.IsConnectionLost(e)) {
            command?.Dispose();
            command = null;
            PreparedCommand retryCmd = null;
            try {
                retryCmd = PrepareCommand(null, commandText, paramArray, prepare);
                return await executeAsync(retryCmd.Command, cancellationToken);
            }
            catch (OperationCanceledException) {
                throw;
            }
            catch (Exception retryEx) {
                throw new StatementException(commandText, paramArray, retryEx);
            }
            finally {
                retryCmd?.Dispose();
            }
        }
        catch (Exception e) {
            throw new StatementException(commandText, paramArray, e);
        }
        finally {
            command?.Dispose();
        }
    }

    // ── Write paths — no retry (blind write retry could double-apply) ─────────────

    /// <inheritdoc />
    public override int NonQuery(Transaction transaction, string commandstring, IEnumerable<object> parameters) {
        using PreparedCommand command = PrepareCommand(transaction, commandstring, parameters, false);
        try {
            return command.Command.ExecuteNonQuery();
        }
        catch (Exception e) {
            throw new StatementException(commandstring, command.Command.Parameters.Cast<object>().ToArray(), e);
        }
    }

    /// <inheritdoc />
    public override async Task<int> NonQueryAsync(Transaction transaction, string commandstring, IEnumerable<object> parameters, CancellationToken cancellationToken) {
        using PreparedCommand command = PrepareCommand(transaction, commandstring, parameters, false);
        try {
            return await command.Command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (OperationCanceledException) {
            throw;
        }
        catch (Exception e) {
            throw new StatementException(commandstring, command.Command.Parameters.Cast<object>().ToArray(), e);
        }
    }

    /// <inheritdoc />
    public override int NonQueryPrepared(Transaction transaction, string commandstring, IEnumerable<object> parameters) {
        using PreparedCommand command = PrepareCommand(transaction, commandstring, parameters, true);
        try {
            return command.Command.ExecuteNonQuery();
        }
        catch (Exception e) {
            throw new StatementException(commandstring, command.Command.Parameters.Cast<object>().ToArray(), e);
        }
    }

    /// <inheritdoc />
    public override async Task<int> NonQueryPreparedAsync(Transaction transaction, string commandstring, IEnumerable<object> parameters, CancellationToken cancellationToken) {
        using PreparedCommand command = PrepareCommand(transaction, commandstring, parameters, true);
        try {
            return await command.Command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (OperationCanceledException) {
            throw;
        }
        catch (Exception e) {
            throw new StatementException(commandstring, command.Command.Parameters.Cast<object>().ToArray(), e);
        }
    }

    // ── Non-prepared read paths (with retry) ──────────────────────────────────────

    /// <inheritdoc />
    public override DataTable Query(Transaction transaction, string query, IEnumerable<object> parameters) {
        object[] paramArray = MaterializeParameters(parameters);
        return ExecuteMaterializedReadWithRetry(transaction, query, paramArray, false, cmd => {
            using IDataReader reader = cmd.ExecuteReader();
            return CreateTable(reader);
        });
    }

    /// <inheritdoc />
    public override async Task<DataTable> QueryAsync(Transaction transaction, string query, IEnumerable<object> parameters, CancellationToken cancellationToken) {
        object[] paramArray = MaterializeParameters(parameters);
        return await ExecuteMaterializedReadWithRetryAsync(transaction, query, paramArray, false, async (cmd, ct) => {
            using IDataReader reader = await cmd.ExecuteReaderAsync(ct);
            return CreateTable(reader);
        }, cancellationToken);
    }

    /// <inheritdoc />
    public override object Scalar(Transaction transaction, string query, IEnumerable<object> parameters) {
        object[] paramArray = MaterializeParameters(parameters);
        return ExecuteMaterializedReadWithRetry(transaction, query, paramArray, false, cmd => cmd.ExecuteScalar());
    }

    /// <inheritdoc />
    public override async Task<object> ScalarAsync(Transaction transaction, string query, IEnumerable<object> parameters, CancellationToken cancellationToken) {
        object[] paramArray = MaterializeParameters(parameters);
        return await ExecuteMaterializedReadWithRetryAsync(transaction, query, paramArray, false,
            (cmd, ct) => cmd.ExecuteScalarAsync(ct), cancellationToken);
    }

    /// <inheritdoc />
    public override IEnumerable<object> Set(Transaction transaction, string query, IEnumerable<object> parameters) {
        object[] paramArray = MaterializeParameters(parameters);
        (PreparedCommand cmd, DbDataReader reader) = OpenReaderWithRetry(transaction, query, paramArray, false);
        using (cmd) {
            using (reader) {
                while (reader.Read())
                    yield return reader.GetValue(0);
                reader.Close();
            }
        }
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<object> SetAsync(Transaction transaction, string query, IEnumerable<object> parameters, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken) {
        object[] paramArray = MaterializeParameters(parameters);
        (PreparedCommand command, DbDataReader dbReader) = await OpenReaderWithRetryAsync(transaction, query, paramArray, false, cancellationToken);

        Reader wrapper = new(dbReader, command, DBInfo);
        if (DBInfo.MultipleConnectionsSupported) {
            await foreach (object item in ReadSetAsync(wrapper, cancellationToken))
                yield return item;
            yield break;
        }

        await foreach (object item in ReadSetAsync(wrapper, cancellationToken).Buffer())
            yield return item;
    }

    /// <inheritdoc />
    public override Reader Reader(Transaction transaction, string command, IEnumerable<object> parameters) {
        object[] paramArray = MaterializeParameters(parameters);
        (PreparedCommand cmd, DbDataReader reader) = OpenReaderWithRetry(transaction, command, paramArray, false);
        return new Reader(reader, cmd, DBInfo);
    }

    /// <inheritdoc />
    public override async Task<Reader> ReaderAsync(Transaction transaction, string command, IEnumerable<object> parameters, CancellationToken cancellationToken) {
        object[] paramArray = MaterializeParameters(parameters);
        (PreparedCommand cmd, DbDataReader reader) = await OpenReaderWithRetryAsync(transaction, command, paramArray, false, cancellationToken);
        return new Reader(reader, cmd, DBInfo);
    }

    // ── Prepared read paths (with retry) ─────────────────────────────────────────

    /// <inheritdoc />
    public override DataTable QueryPrepared(Transaction transaction, string query, IEnumerable<object> parameters) {
        object[] paramArray = MaterializeParameters(parameters);
        return ExecuteMaterializedReadWithRetry(transaction, query, paramArray, true, cmd => {
            using IDataReader reader = cmd.ExecuteReader();
            return CreateTable(reader);
        });
    }

    /// <inheritdoc />
    public override async Task<DataTable> QueryPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters, CancellationToken cancellationToken) {
        object[] paramArray = MaterializeParameters(parameters);
        return await ExecuteMaterializedReadWithRetryAsync(transaction, query, paramArray, true, async (cmd, ct) => {
            using IDataReader reader = await cmd.ExecuteReaderAsync(ct);
            return CreateTable(reader);
        }, cancellationToken);
    }

    /// <inheritdoc />
    public override object ScalarPrepared(Transaction transaction, string query, IEnumerable<object> parameters) {
        object[] paramArray = MaterializeParameters(parameters);
        return ExecuteMaterializedReadWithRetry(transaction, query, paramArray, true, cmd => cmd.ExecuteScalar());
    }

    /// <inheritdoc />
    public override async Task<object> ScalarPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters, CancellationToken cancellationToken) {
        object[] paramArray = MaterializeParameters(parameters);
        return await ExecuteMaterializedReadWithRetryAsync(transaction, query, paramArray, true,
            (cmd, ct) => cmd.ExecuteScalarAsync(ct), cancellationToken);
    }

    /// <inheritdoc />
    public override IEnumerable<object> SetPrepared(Transaction transaction, string query, IEnumerable<object> parameters) {
        object[] paramArray = MaterializeParameters(parameters);
        (PreparedCommand cmd, DbDataReader reader) = OpenReaderWithRetry(transaction, query, paramArray, true);
        using (cmd) {
            using (reader) {
                while (reader.Read())
                    yield return reader.GetValue(0);
                reader.Close();
            }
        }
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<object> SetPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken) {
        object[] paramArray = MaterializeParameters(parameters);
        (PreparedCommand command, DbDataReader dbReader) = await OpenReaderWithRetryAsync(transaction, query, paramArray, true, cancellationToken);

        Reader wrapper = new(dbReader, command, DBInfo);
        if (DBInfo.MultipleConnectionsSupported) {
            await foreach (object item in ReadSetAsync(wrapper, cancellationToken))
                yield return item;
            yield break;
        }

        await foreach (object item in ReadSetAsync(wrapper, cancellationToken).Buffer())
            yield return item;
    }

    /// <inheritdoc />
    public override Reader ReaderPrepared(Transaction transaction, string command, IEnumerable<object> parameters) {
        object[] paramArray = MaterializeParameters(parameters);
        (PreparedCommand cmd, DbDataReader reader) = OpenReaderWithRetry(transaction, command, paramArray, true);
        return new Reader(reader, cmd, DBInfo);
    }

    /// <inheritdoc />
    public override async Task<Reader> ReaderPreparedAsync(Transaction transaction, string command, IEnumerable<object> parameters, CancellationToken cancellationToken) {
        object[] paramArray = MaterializeParameters(parameters);
        (PreparedCommand cmd, DbDataReader reader) = await OpenReaderWithRetryAsync(transaction, command, paramArray, true, cancellationToken);
        return new Reader(reader, cmd, DBInfo);
    }

    DataTable CreateTable(IDataReader reader) => DataTable.FromReader(reader);

    IEnumerable<object> ReadSet(IDataReader reader) {
        using (reader) {
            while (reader.Read())
                yield return reader.GetValue(0);
            reader.Close();
        }
    }

    async IAsyncEnumerable<object> ReadSetAsync(Reader reader, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default) {
        using (reader) {
            while (await reader.ReadAsync(cancellationToken)) {
                cancellationToken.ThrowIfCancellationRequested();
                yield return await reader.FieldValueAsync<object>(0);
            }
            await reader.CloseAsync();
        }
    }
}
