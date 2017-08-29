using System.Data;
using NightlyCode.DB.Info;
using Npgsql;

namespace NightlyCode.DB.Providers {

    public class PostgreProvider : IDBProvider {
        readonly PostgreInfo info = new PostgreInfo();

        public IDbConnection CreateConnection(string connectionstring) {
            return new NpgsqlConnection(connectionstring);
        }

        public IDBInfo DatabaseInfo => info;
    }
}