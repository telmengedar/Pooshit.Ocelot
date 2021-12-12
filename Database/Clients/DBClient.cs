using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using NightlyCode.Database.Errors;
using NightlyCode.Database.Info;
using DataTable = NightlyCode.Database.Clients.Tables.DataTable;

namespace NightlyCode.Database.Clients {

    /// <summary>
    /// client to execute database commands
    /// </summary>
    public class DBClient : ADbClient {

        /// <summary>
        /// creates a new <see cref="DBClient"/>
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

        IConnection Connect(Transaction transaction) {
            return transaction?.Connect() ?? Connection.Connect();
        }

        Task<IConnection> ConnectAsync(Transaction transaction) {
            return transaction?.ConnectAsync() ?? Connection.ConnectAsync();
        }

        /// <inheritdoc />
        public override Transaction Transaction() {
            return new Transaction(DBInfo, Connection.Connect(), null);
        }
        
        /// <inheritdoc />
        public override int NonQueryPrepared(Transaction transaction, string commandstring, IEnumerable<object> parameters) {
            using PreparedCommand command = PrepareCommand(transaction, commandstring, parameters, true);

            try {
                return command.Command.ExecuteNonQuery();
            }
            catch (Exception e) {
                throw new StatementException(commandstring, parameters.ToArray(), e);
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
                throw new StatementException(query, parameters.ToArray(), e);
            }
        }

        /// <inheritdoc />
        public override object ScalarPrepared(Transaction transaction, string query, IEnumerable<object> parameters) {
            using PreparedCommand command = PrepareCommand(transaction, query, parameters, true);
            try {
                return command.Command.ExecuteScalar();
            }
            catch (Exception e) {
                throw new StatementException(query, parameters.ToArray(), e);
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
                throw new StatementException(query, parameters.ToArray(), e);
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
                throw new StatementException(commandstring, parameters.ToArray(), e);
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
                throw new StatementException(query, parameters.ToArray(), e);
            }
        }

        /// <inheritdoc />
        public override async Task<object> ScalarPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters) {
            using PreparedCommand command = PrepareCommand(transaction, query, parameters, true);
            try {
                return await command.Command.ExecuteScalarAsync();
            }
            catch (Exception e) {
                throw new StatementException(query, parameters.ToArray(), e);
            }
        }

        /// <inheritdoc />
        public override async Task<object[]> SetPreparedAsync(Transaction transaction, string query, IEnumerable<object> parameters) {
            using PreparedCommand command = PrepareCommand(transaction, query, parameters, true);

            try {
                // needs to be converted to array to allow accessing the reader while command is still opened
                return ReadSet(await command.Command.ExecuteReaderAsync()).ToArray();
            }
            catch (Exception e) {
                throw new StatementException(query, parameters.ToArray(), e);
            }
        }

        DataTable CreateTable(IDataReader reader) {
            return DataTable.FromReader(reader);
        }
        
        PreparedCommand PrepareCommand(Transaction transaction, string commandString, IEnumerable<object> parameters, bool prepare) {
            IConnection connection = Connect(transaction);
            DbCommand command = PrepareCommand(connection, commandString, parameters);
            if (prepare)
                command.Prepare();
            if(transaction != null)
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
                throw new StatementException(commandstring, parameters.ToArray(), e);
            }
        }

        /// <inheritdoc />
        public override async Task<int> NonQueryAsync(Transaction transaction, string commandstring, IEnumerable<object> parameters) {
            using PreparedCommand command = PrepareCommand(transaction, commandstring, parameters, false);
            try {
                return await command.Command.ExecuteNonQueryAsync();
            }
            catch (Exception e) {
                throw new StatementException(commandstring, parameters.ToArray(), e);
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
                throw new StatementException(query, parameters.ToArray(), e);
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
                throw new StatementException(query, parameters.ToArray(), e);
            }
        }

        /// <inheritdoc />
        public override object Scalar(Transaction transaction, string query, IEnumerable<object> parameters) {
            using PreparedCommand command = PrepareCommand(transaction, query, parameters, false);
            try {
                return command.Command.ExecuteScalar();
            }
            catch (Exception e) {
                throw new StatementException(query, parameters.ToArray(), e);
            }
        }

        /// <inheritdoc />
        public override async Task<object> ScalarAsync(Transaction transaction, string query, IEnumerable<object> parameters) {
            using PreparedCommand command = PrepareCommand(transaction, query, parameters, false);
            try {
                return await command.Command.ExecuteScalarAsync();
            }
            catch (Exception e) {
                throw new StatementException(query, parameters.ToArray(), e);
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
                throw new StatementException(query, parameters.ToArray(), e);
            }

            using (reader)
                while (reader.Read())
                    yield return reader.GetValue(0);
            reader.Close();
        }

        /// <inheritdoc />
        public override async Task<object[]> SetAsync(Transaction transaction, string query, IEnumerable<object> parameters) {
            using PreparedCommand command = PrepareCommand(transaction, query, parameters, false);

            try {
                // needs to be converted to array to allow accessing the reader while command is still opened
                return ReadSet(await command.Command.ExecuteReaderAsync()).ToArray();
            }
            catch (Exception e) {
                throw new StatementException(query, parameters.ToArray(), e);
            }
        }

        IEnumerable<object> ReadSet(IDataReader reader) {
            using(reader) {
                while(reader.Read())
                    yield return reader.GetValue(0);
                reader.Close();
            }
        }
    }
}
