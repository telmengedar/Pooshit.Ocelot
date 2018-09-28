using System.Data;
using NightlyCode.DB.Clients;
using NightlyCode.DB.Info;
using Npgsql;

namespace NightlyCode.DB.Providers {

    public class PostgreProvider : IDBProvider {
        readonly PostgreInfo info = new PostgreInfo();

        public IDbConnection CreateConnection(string connectionstring) {
            return new NpgsqlConnection(connectionstring);
        }

        public IDBInfo DatabaseInfo => info;

        public static IDBClient CreatePostgre(string host, int port, string database, string user, string password)
        {
            return new DBClient(new PostgreProvider(), $"Server={host}; Port={port}; Database={database}; User Id={user}; Password={password}");
        }
    }
}