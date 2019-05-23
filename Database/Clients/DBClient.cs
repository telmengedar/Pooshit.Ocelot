using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using NightlyCode.Database.Info;

namespace NightlyCode.Database.Clients
{

    /// <summary>
    /// client to execute database commands
    /// </summary>
    public class DBClient : IDBClient {
        readonly DbConnection connection;
        readonly IDBInfo dbinfo;

        /// <summary>
        /// creates a new <see cref="DBClient"/>
        /// </summary>
        /// <param name="connection">connection to database</param>
        /// <param name="dbinfo">information for database statements</param>
        internal DBClient(DbConnection connection, IDBInfo dbinfo)
        {
            this.connection = connection;
            this.dbinfo = dbinfo;
        }

        /// <inheritdoc />
        public IDBInfo DBInfo => dbinfo;

        /// <inheritdoc />
        public DbConnection Connection => connection;

        void OpenConnection() {
            if (connection.State == ConnectionState.Open)
                return;
            connection.Open();
        }

        Task OpenConnectionAsync() {
            if (connection.State == ConnectionState.Open)
                return Task.FromResult(0);
            return connection.OpenAsync();
        }

        DbCommand PrepareCommand(string commandtext, IEnumerable<object> parameters)
        {
            DbCommand command = connection.CreateCommand();
            command.CommandText = commandtext;
            command.CommandTimeout = 0;

            foreach (object value in parameters)
            {
                DbParameter parameter = command.CreateParameter();
                parameter.ParameterName = dbinfo.Parameter + (command.Parameters.Count + 1);
                parameter.Value = value ?? DBNull.Value;
                command.Parameters.Add(parameter);
            }

            return command;
        }

        /// <summary>
        /// begins a transaction
        /// </summary>
        /// <returns>Transaction object to use</returns>
        public Transaction Transaction() {
            return new Transaction(dbinfo, connection, null);
        }

        Tables.DataTable CreateTable(IDataReader reader) {
            return Tables.DataTable.FromReader(reader);
        }

        /// <inheritdoc />
        public Tables.DataTable Query(string query, params object[] parameters) {
            return Query(query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public Task<Tables.DataTable> QueryAsync(string query, params object[] parameters)
        {
            return QueryAsync(query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public Tables.DataTable Query(string query, IEnumerable<object> parameters) {
            return Query(null, query, parameters);
        }

        /// <inheritdoc />
        public Task<Tables.DataTable> QueryAsync(string query, IEnumerable<object> parameters) {
            return QueryAsync(null, query, parameters);
        }

        /// <inheritdoc />
        public int NonQuery(string commandstring, params object[] parameters) {
            return NonQuery(commandstring, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public Task<int> NonQueryAsync(string commandstring, params object[] parameters)
        {
            return NonQueryAsync(commandstring, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public int NonQuery(string commandstring, IEnumerable<object> parameters) {
            return NonQuery(null, commandstring, parameters);
        }

        /// <inheritdoc />
        public Task<int> NonQueryAsync(string commandstring, IEnumerable<object> parameters) {
            return NonQueryAsync(null, commandstring, parameters);
        }

        /// <inheritdoc />
        public object Scalar(string query, params object[] parameters) {
            return Scalar(query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public Task<object> ScalarAsync(string query, params object[] parameters)
        {
            return ScalarAsync(query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public object Scalar(string query, IEnumerable<object> parameters) {
            return Scalar(null, query, parameters);
        }

        /// <inheritdoc />
        public Task<object> ScalarAsync(string query, IEnumerable<object> parameters) {
            return ScalarAsync(null, query, parameters);
        }

        /// <inheritdoc />
        public IEnumerable<object> Set(string query, params object[] parameters) {
            return Set(query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public Task<IEnumerable<object>> SetAsync(string query, params object[] parameters)
        {
            return SetAsync(query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public IEnumerable<object> Set(string query, IEnumerable<object> parameters) {
            return Set(null, query, parameters);
        }

        /// <inheritdoc />
        public Task<IEnumerable<object>> SetAsync(string query, IEnumerable<object> parameters) {
            return SetAsync(null, query, parameters);
        }

        /// <inheritdoc />
        public int NonQuery(Transaction transaction, string commandstring, params object[] parameters) {
            return NonQuery(transaction, commandstring, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public Task<int> NonQueryAsync(Transaction transaction, string commandstring, params object[] parameters)
        {
            return NonQueryAsync(transaction, commandstring, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public int NonQuery(Transaction transaction, string commandstring, IEnumerable<object> parameters) {
            OpenConnection();
            using (DbCommand command = PrepareCommand(commandstring, parameters)) {
                if(transaction!=null)
                    command.Transaction = transaction.DbTransaction;
                return command.ExecuteNonQuery();
            }
        }

        /// <inheritdoc />
        public async Task<int> NonQueryAsync(Transaction transaction, string commandstring, IEnumerable<object> parameters) {
            await OpenConnectionAsync();
            using (DbCommand command = PrepareCommand(commandstring, parameters)) {
                if (transaction != null)
                    command.Transaction = transaction.DbTransaction;
                return await command.ExecuteNonQueryAsync();
            }
        }

        /// <inheritdoc />
        public Tables.DataTable Query(Transaction transaction, string query, params object[] parameters) {
            return Query(transaction, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public Task<Tables.DataTable> QueryAsync(Transaction transaction, string query, params object[] parameters)
        {
            return QueryAsync(transaction, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public Tables.DataTable Query(Transaction transaction, string query, IEnumerable<object> parameters) {
            OpenConnection();
            using (IDbCommand command = PrepareCommand(query, parameters)) {
                if (transaction != null)
                    command.Transaction = transaction.DbTransaction;
                using (IDataReader reader = command.ExecuteReader())
                    return CreateTable(reader);
            }
        }

        /// <inheritdoc />
        public async Task<Tables.DataTable> QueryAsync(Transaction transaction, string query, IEnumerable<object> parameters) {
            await OpenConnectionAsync();
            using (DbCommand command = PrepareCommand(query, parameters)) {
                if (transaction != null)
                    command.Transaction = transaction.DbTransaction;
                using (IDataReader reader = await command.ExecuteReaderAsync())
                    return CreateTable(reader);
            }
        }

        /// <inheritdoc />
        public object Scalar(Transaction transaction, string query, params object[] parameters) {
            return Scalar(transaction, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public Task<object> ScalarAsync(Transaction transaction, string query, params object[] parameters)
        {
            return ScalarAsync(transaction, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public object Scalar(Transaction transaction, string query, IEnumerable<object> parameters) {
            OpenConnection();
            using (IDbCommand command = PrepareCommand(query, parameters)) {
                if (transaction != null)
                    command.Transaction = transaction.DbTransaction;
                return command.ExecuteScalar();
            }
        }

        /// <inheritdoc />
        public async Task<object> ScalarAsync(Transaction transaction, string query, IEnumerable<object> parameters) {
            await OpenConnectionAsync();
            using (DbCommand command = PrepareCommand(query, parameters)) {
                if (transaction != null)
                    command.Transaction = transaction.DbTransaction;
                return await command.ExecuteScalarAsync();
            }
        }

        /// <inheritdoc />
        public IEnumerable<object> Set(Transaction transaction, string query, params object[] parameters) {
            return Set(transaction, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public Task<IEnumerable<object>> SetAsync(Transaction transaction, string query, params object[] parameters)
        {
            return SetAsync(transaction, query, (IEnumerable<object>)parameters);
        }

        /// <inheritdoc />
        public IEnumerable<object> Set(Transaction transaction, string query, IEnumerable<object> parameters) {
            OpenConnection();
            using (IDbCommand command = PrepareCommand(query, parameters)) {
                if (transaction != null)
                    command.Transaction = transaction.DbTransaction;
                using (IDataReader reader = command.ExecuteReader()) {
                    while (reader.Read())
                        yield return reader.GetValue(0);
                    reader.Close();
                }
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<object>> SetAsync(Transaction transaction, string query, IEnumerable<object> parameters) {
            await OpenConnectionAsync();
            using (DbCommand command = PrepareCommand(query, parameters)) {
                if (transaction != null)
                    command.Transaction = transaction.DbTransaction;

                // needs to be converted to array to allow accessing the reader while command is still opened
                return ReadSet(await command.ExecuteReaderAsync()).ToArray();
            }
        }

        IEnumerable<object> ReadSet(IDataReader reader)
        {
            using (reader)
            {
                while (reader.Read())
                    yield return reader.GetValue(0);
                reader.Close();
            }
        }
    }
}
