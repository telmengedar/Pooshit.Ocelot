using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
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
        using PreparedCommand command = PrepareCommand(transaction, commandstring, parameters, true);

        try {
            return command.Command.ExecuteNonQuery();
        }
        catch (Exception e) {
            throw new StatementException(commandstring, command.Command.Parameters.Cast<object>().ToArray(), e);
        }
    }

    /// <inheritdoc />
    public override DataTable QueryPrepared(Transaction transaction, string query, IEnumerable<object> parameters) {
        using PreparedCommand command = PrepareCommand(transaction, query, parameters, true);

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
        using PreparedCommand command = PrepareCommand(transaction, query, parameters, true);
        try {
            return command.Command.ExecuteScalar();
        }
        catch (Exception e) {
            throw new StatementException(query, command.Command.Parameters.Cast<object>().ToArray(), e);
        }
    }

    /// <inheritdoc />
    public override IEnumerable<object> SetPrepared(Transaction transaction, string query, IEnumerable<object> parameters) {
        using PreparedCommand command = PrepareCommand(transaction, query, parameters, true);
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
    public override Task<int> NonQueryPreparedAsync(Transaction transaction, string commandstring, IEnumerable<object> parameters) {
        using PreparedCommand command = PrepareCommand(transaction, commandstring, parameters, true);
        try {
            return command.Command.ExecuteNonQueryAsync();
        }
        catch (Exception e) {
            throw new StatementException(commandstring, command.Command.Parameters.Cast<object>().ToArray(), e);
        }
    }

    /// <inheritdoc />
    public override async Task<DataTable> QueryPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters) {
        using PreparedCommand command = PrepareCommand(transaction, query, parameters, true);

        try {
            using IDataReader reader = await command.Command.ExecuteReaderAsync();
            return CreateTable(reader);
        }
        catch (Exception e) {
            throw new StatementException(query, command.Command.Parameters.Cast<object>().ToArray(), e);
        }
    }

    /// <inheritdoc />
    public override async Task<object> ScalarPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters) {
        using PreparedCommand command = PrepareCommand(transaction, query, parameters, true);
        try {
            return await command.Command.ExecuteScalarAsync();
        }
        catch (Exception e) {
            throw new StatementException(query, command.Command.Parameters.Cast<object>().ToArray(), e);
        }
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<object> SetPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters) {
        PreparedCommand command = PrepareCommand(transaction, query, parameters, true);

        DbDataReader reader;
        try {
            reader = await command.Command.ExecuteReaderAsync();
        }
        catch (Exception e) {
            throw new StatementException(query, command.Command.Parameters.Cast<object>().ToArray(), e);
        }

        Reader wrapper = new(reader, command, DBInfo);
        if (DBInfo.MultipleConnectionsSupported) {
            await foreach (object item in ReadSetAsync(wrapper))
                yield return item;
            yield break;
        }

        await foreach (object item in ReadSetAsync(wrapper).Buffer())
            yield return item;
    }

    /// <inheritdoc />
    public override Reader Reader(Transaction transaction, string command, IEnumerable<object> parameters) {
        PreparedCommand prepared = PrepareCommand(transaction, command, parameters, false);
        try {
            return new Reader(prepared.Command.ExecuteReader(), prepared, DBInfo);
        }
        catch (Exception) {
            prepared.Dispose();
            throw;
        }
    }

    /// <inheritdoc />
    public override async Task<Reader> ReaderAsync(Transaction transaction, string command, IEnumerable<object> parameters) {
        PreparedCommand prepared = PrepareCommand(transaction, command, parameters, false);
        try {
            return new(await prepared.Command.ExecuteReaderAsync(), prepared, DBInfo);
        }
        catch (Exception) {
            prepared.Dispose();
            throw;
        }
    }

    /// <inheritdoc />
    public override Reader ReaderPrepared(Transaction transaction, string command, IEnumerable<object> parameters) {
        PreparedCommand prepared = PrepareCommand(transaction, command, parameters, true);
        try {
            return new(prepared.Command.ExecuteReader(), prepared, DBInfo);
        }
        catch (Exception) {
            prepared.Dispose();
            throw;
        }
    }

    /// <inheritdoc />
    public override async Task<Reader> ReaderPreparedAsync(Transaction transaction, string command, IEnumerable<object> parameters) {
        PreparedCommand prepared = PrepareCommand(transaction, command, parameters, true);
        try {
            return new Reader(await prepared.Command.ExecuteReaderAsync(), prepared, DBInfo);
        }
        catch (Exception) {
            prepared.Dispose();
            throw;
        }
    }

    DataTable CreateTable(IDataReader reader) => DataTable.FromReader(reader);

    PreparedCommand PrepareCommand(Transaction transaction, string commandString, IEnumerable<object> parameters, bool prepare) {
        IConnection connection = Connect(transaction);
        DbCommand command = PrepareCommand(connection, commandString, parameters);
        if (prepare)
            command.Prepare();
        if (transaction != null)
            command.Transaction = transaction.DbTransaction;
        return new PreparedCommand(connection, command);
    }

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
    public override async Task<int> NonQueryAsync(Transaction transaction, string commandstring, IEnumerable<object> parameters) {
        using PreparedCommand command = PrepareCommand(transaction, commandstring, parameters, false);
        try {
            return await command.Command.ExecuteNonQueryAsync();
        }
        catch (Exception e) {
            throw new StatementException(commandstring, command.Command.Parameters.Cast<object>().ToArray(), e);
        }
    }

    /// <inheritdoc />
    public override DataTable Query(Transaction transaction, string query, IEnumerable<object> parameters) {
        using PreparedCommand command = PrepareCommand(transaction, query, parameters, false);
        try {
            using IDataReader reader = command.Command.ExecuteReader();
            return CreateTable(reader);
        }
        catch (Exception e) {
            throw new StatementException(query, command.Command.Parameters.Cast<object>().ToArray(), e);
        }
    }

    /// <inheritdoc />
    public override async Task<DataTable> QueryAsync(Transaction transaction, string query, IEnumerable<object> parameters) {
        using PreparedCommand command = PrepareCommand(transaction, query, parameters, false);
        try {
            using IDataReader reader = await command.Command.ExecuteReaderAsync();
            return CreateTable(reader);
        }
        catch (Exception e) {
            throw new StatementException(query, command.Command.Parameters.Cast<object>().ToArray(), e);
        }
    }

    /// <inheritdoc />
    public override object Scalar(Transaction transaction, string query, IEnumerable<object> parameters) {
        using PreparedCommand command = PrepareCommand(transaction, query, parameters, false);
        try {
            return command.Command.ExecuteScalar();
        }
        catch (Exception e) {
            throw new StatementException(query, command.Command.Parameters.Cast<object>().ToArray(), e);
        }
    }

    /// <inheritdoc />
    public override async Task<object> ScalarAsync(Transaction transaction, string query, IEnumerable<object> parameters) {
        using PreparedCommand command = PrepareCommand(transaction, query, parameters, false);
        try {
            return await command.Command.ExecuteScalarAsync();
        }
        catch (Exception e) {
            throw new StatementException(query, command.Command.Parameters.Cast<object>().ToArray(), e);
        }
    }

    /// <inheritdoc />
    public override IEnumerable<object> Set(Transaction transaction, string query, IEnumerable<object> parameters) {
        using PreparedCommand command = PrepareCommand(transaction, query, parameters, false);
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
    public override async IAsyncEnumerable<object> SetAsync(Transaction transaction, string query, IEnumerable<object> parameters) {
        PreparedCommand command = PrepareCommand(transaction, query, parameters, false);

        DbDataReader reader;
        try {
            reader = await command.Command.ExecuteReaderAsync();
        }
        catch (Exception e) {
            throw new StatementException(query, command.Command.Parameters.Cast<object>().ToArray(), e);
        }

        Reader wrapper = new(reader, command, DBInfo);
        if (DBInfo.MultipleConnectionsSupported) {
            await foreach (object item in ReadSetAsync(wrapper))
                yield return item;
            yield break;
        }

        await foreach (object item in ReadSetAsync(wrapper).Buffer())
            yield return item;
    }

    IEnumerable<object> ReadSet(IDataReader reader) {
        using (reader) {
            while (reader.Read())
                yield return reader.GetValue(0);
            reader.Close();
        }
    }
    
    async IAsyncEnumerable<object> ReadSetAsync(Reader reader) {
        using (reader) {
            while (await reader.ReadAsync())
                yield return await reader.FieldValueAsync<object>(0);
            await reader.CloseAsync();
        }
    }
}