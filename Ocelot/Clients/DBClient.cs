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

    /// <inheritdoc />
    public override int NonQueryPrepared(Transaction transaction, string commandstring, IEnumerable<object> parameters) {
        using PreparedCommand command = PrepareCommand(transaction, commandstring, parameters);

        try {
            return command.Command.ExecuteNonQuery();
        }
        catch (Exception e) {
            throw new StatementException(commandstring, command.Command.Parameters.Cast<object>().ToArray(), e);
        }
    }

    /// <inheritdoc />
    public override DataTable QueryPrepared(Transaction transaction, string query, IEnumerable<object> parameters) {
        using PreparedCommand command = PrepareCommand(transaction, query, parameters);

        try {
            using IDataReader reader = command.Command.ExecuteReader();
            return CreateTable(reader);
        }
        catch (Exception e) {
            throw new StatementException(query, command.Command.Parameters.Cast<object>().ToArray(), e);
        }
    }

    /// <inheritdoc />
    public override object ScalarPrepared(Transaction transaction, string query, IEnumerable<object> parameters) {
        using PreparedCommand command = PrepareCommand(transaction, query, parameters);
        try {
            return command.Command.ExecuteScalar();
        }
        catch (Exception e) {
            throw new StatementException(query, command.Command.Parameters.Cast<object>().ToArray(), e);
        }
    }

    /// <inheritdoc />
    public override IEnumerable<object> SetPrepared(Transaction transaction, string query, IEnumerable<object> parameters) {
        using PreparedCommand command = PrepareCommand(transaction, query, parameters);
        IDataReader reader;
        try {
            reader = command.Command.ExecuteReader();
        }
        catch (Exception e) {
            throw new StatementException(query, command.Command.Parameters.Cast<object>().ToArray(), e);
        }

        using (reader) {
            while (reader.Read())
                yield return reader.GetValue(0);
            reader.Close();
        }
    }

    /// <inheritdoc />
    public override async Task<int> NonQueryPreparedAsync(Transaction transaction, string commandstring, IEnumerable<object> parameters, CancellationToken cancellationToken) {
        using PreparedCommand command = PrepareCommand(transaction, commandstring, parameters);
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
    public override async Task<DataTable> QueryPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters, CancellationToken cancellationToken) {
        using PreparedCommand command = PrepareCommand(transaction, query, parameters);

        try {
            using IDataReader reader = await command.Command.ExecuteReaderAsync(cancellationToken);
            return CreateTable(reader);
        }
        catch (OperationCanceledException) {
            throw;
        }
        catch (Exception e) {
            throw new StatementException(query, command.Command.Parameters.Cast<object>().ToArray(), e);
        }
    }

    /// <inheritdoc />
    public override async Task<object> ScalarPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters, CancellationToken cancellationToken) {
        using PreparedCommand command = PrepareCommand(transaction, query, parameters);
        try {
            return await command.Command.ExecuteScalarAsync(cancellationToken);
        }
        catch (OperationCanceledException) {
            throw;
        }
        catch (Exception e) {
            throw new StatementException(query, command.Command.Parameters.Cast<object>().ToArray(), e);
        }
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<object> SetPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken) {
        PreparedCommand command = PrepareCommand(transaction, query, parameters);

        DbDataReader reader;
        try {
            reader = await command.Command.ExecuteReaderAsync(cancellationToken);
        }
        catch (OperationCanceledException) {
            command.Dispose();
            throw;
        }
        catch (Exception e) {
            command.Dispose();
            throw new StatementException(query, command.Command.Parameters.Cast<object>().ToArray(), e);
        }

        Reader wrapper = new(reader, command, DBInfo);
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
        PreparedCommand prepared = PrepareCommand(transaction, command, parameters);
        try {
            return new(prepared.Command.ExecuteReader(), prepared, DBInfo);
        }
        catch (Exception) {
            prepared.Dispose();
            throw;
        }
    }

    /// <inheritdoc />
    public override async Task<Reader> ReaderAsync(Transaction transaction, string command, IEnumerable<object> parameters, CancellationToken cancellationToken) {
        PreparedCommand prepared = PrepareCommand(transaction, command, parameters);
        try {
            return new(await prepared.Command.ExecuteReaderAsync(cancellationToken), prepared, DBInfo);
        }
        catch (Exception) {
            prepared.Dispose();
            throw;
        }
    }

    /// <inheritdoc />
    public override Reader ReaderPrepared(Transaction transaction, string command, IEnumerable<object> parameters) {
        PreparedCommand prepared = PrepareCommand(transaction, command, parameters);
        try {
            return new(prepared.Command.ExecuteReader(), prepared, DBInfo);
        }
        catch (Exception) {
            prepared.Dispose();
            throw;
        }
    }

    /// <inheritdoc />
    public override async Task<Reader> ReaderPreparedAsync(Transaction transaction, string command, IEnumerable<object> parameters, CancellationToken cancellationToken) {
        PreparedCommand prepared = PrepareCommand(transaction, command, parameters);
        try {
            return new Reader(await prepared.Command.ExecuteReaderAsync(cancellationToken), prepared, DBInfo);
        }
        catch (Exception) {
            prepared.Dispose();
            throw;
        }
    }

    DataTable CreateTable(IDataReader reader) => DataTable.FromReader(reader);

    PreparedCommand PrepareCommand(Transaction transaction, string commandString, IEnumerable<object> parameters) {
        IConnection connection = Connect(transaction);
        DbCommand command = PrepareCommand(connection, commandString, parameters);
        // Transaction must be assigned before Prepare() to satisfy the ADO.NET contract
        // (Surface 6 fix: ordering was reversed in the original).
        // Manual command.Prepare() is intentionally omitted: per-call prepare-then-dispose
        // yields no server-side reuse with a pooled-connection factory (Surface 3).
        // Callers who want real plan caching should set "Maximum Auto Prepare" in their
        // Npgsql connection string; Npgsql then transparently prepares frequently-used
        // statements without the per-call overhead or the sync-over-async hazard (Surface 5).
        if (transaction != null)
            command.Transaction = transaction.DbTransaction;
        return new PreparedCommand(connection, command);
    }

    /// <inheritdoc />
    public override int NonQuery(Transaction transaction, string commandstring, IEnumerable<object> parameters) {
        using PreparedCommand command = PrepareCommand(transaction, commandstring, parameters);
        try {
            return command.Command.ExecuteNonQuery();
        }
        catch (Exception e) {
            throw new StatementException(commandstring, command.Command.Parameters.Cast<object>().ToArray(), e);
        }
    }

    /// <inheritdoc />
    public override async Task<int> NonQueryAsync(Transaction transaction, string commandstring, IEnumerable<object> parameters, CancellationToken cancellationToken) {
        using PreparedCommand command = PrepareCommand(transaction, commandstring, parameters);
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
    public override DataTable Query(Transaction transaction, string query, IEnumerable<object> parameters) {
        using PreparedCommand command = PrepareCommand(transaction, query, parameters);
        try {
            using IDataReader reader = command.Command.ExecuteReader();
            return CreateTable(reader);
        }
        catch (Exception e) {
            throw new StatementException(query, command.Command.Parameters.Cast<object>().ToArray(), e);
        }
    }

    /// <inheritdoc />
    public override async Task<DataTable> QueryAsync(Transaction transaction, string query, IEnumerable<object> parameters, CancellationToken cancellationToken) {
        using PreparedCommand command = PrepareCommand(transaction, query, parameters);
        try {
            using IDataReader reader = await command.Command.ExecuteReaderAsync(cancellationToken);
            return CreateTable(reader);
        }
        catch (OperationCanceledException) {
            throw;
        }
        catch (Exception e) {
            throw new StatementException(query, command.Command.Parameters.Cast<object>().ToArray(), e);
        }
    }

    /// <inheritdoc />
    public override object Scalar(Transaction transaction, string query, IEnumerable<object> parameters) {
        using PreparedCommand command = PrepareCommand(transaction, query, parameters);
        try {
            return command.Command.ExecuteScalar();
        }
        catch (Exception e) {
            throw new StatementException(query, command.Command.Parameters.Cast<object>().ToArray(), e);
        }
    }

    /// <inheritdoc />
    public override async Task<object> ScalarAsync(Transaction transaction, string query, IEnumerable<object> parameters, CancellationToken cancellationToken) {
        using PreparedCommand command = PrepareCommand(transaction, query, parameters);
        try {
            return await command.Command.ExecuteScalarAsync(cancellationToken);
        }
        catch (OperationCanceledException) {
            throw;
        }
        catch (Exception e) {
            throw new StatementException(query, command.Command.Parameters.Cast<object>().ToArray(), e);
        }
    }

    /// <inheritdoc />
    public override IEnumerable<object> Set(Transaction transaction, string query, IEnumerable<object> parameters) {
        using PreparedCommand command = PrepareCommand(transaction, query, parameters);
        IDataReader reader;
        try {
            reader = command.Command.ExecuteReader();
        }
        catch (Exception e) {
            throw new StatementException(query, command.Command.Parameters.Cast<object>().ToArray(), e);
        }

        using (reader) {
            while (reader.Read())
                yield return reader.GetValue(0);
            reader.Close();
        }
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<object> SetAsync(Transaction transaction, string query, IEnumerable<object> parameters, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken) {
        using PreparedCommand command = PrepareCommand(transaction, query, parameters);

        DbDataReader reader;
        try {
            reader = await command.Command.ExecuteReaderAsync(cancellationToken);
        }
        catch (OperationCanceledException) {
            throw;
        }
        catch (Exception e) {
            throw new StatementException(query, command.Command.Parameters.Cast<object>().ToArray(), e);
        }

        Reader wrapper = new(reader, command, DBInfo);
        if (DBInfo.MultipleConnectionsSupported) {
            await foreach (object item in ReadSetAsync(wrapper, cancellationToken))
                yield return item;
            yield break;
        }

        await foreach (object item in ReadSetAsync(wrapper, cancellationToken).Buffer())
            yield return item;
    }

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