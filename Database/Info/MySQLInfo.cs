using System;
using System.Data;
using System.Linq.Expressions;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Entities.Schema;

namespace NightlyCode.Database.Info
{

    /// <summary>
    /// information for mysql db
    /// </summary>
    public class MySQLInfo : DBInfo
    {
        public override string Parameter => "?";

        public string LastValue => "LAST_INSERT_ID()";

        public override string JoinHint => "STRAIGHT_JOIN";

        public override string AutoIncrement => "AUTO_INCREMENT";

        public override string ColumnIndicator => "`";

        public override string LikeTerm => "LIKE";

        public override void Replace(ExpressionVisitor visitor, OperationPreparator preparator, Expression value, Expression src, Expression target) {
            throw new NotImplementedException();
        }

        public override void ToUpper(ExpressionVisitor visitor, OperationPreparator preparator, Expression value) {
            throw new NotImplementedException();
        }

        public override void ToLower(ExpressionVisitor visitor, OperationPreparator preparator, Expression value) {
            throw new NotImplementedException();
        }

        public override bool CheckIfTableExists(IDBClient db, string table)
        {
            return db.Query("SHOW TABLES like ?1", table).Rows.Length > 0;
        }
        public override string GetDBType(Type type) {
            if (type.IsEnum) return "INT";

            switch (type.Name)
            {
                case "DateTime":
                    return "DATETIME";
                case "string":
                case "String":
                    return "VARCHAR(200)";
                case "int":
                case "Int32":
                    return "INT";
                case "long":
                case "Int64":
                    return "BIGINT";
                case "short":
                case "Int16":
                    return "SMALLINT";
                case "float":
                case "Float":
                    return "FLOAT";
                case "double":
                case "Double":
                    return "DOUBLE";
                case "bool":
                case "Boolean":
                    return "BOOLEAN";
                case "byte[]":
                case "Byte[]":
                    return "BLOB";
                default:
                    return "???";
            }
        }

        public override Type GetDBRepresentation(Type type) {
            return type;
        }

        public override string MaskColumn(string column) {
            return $"`{column}`";
        }

        public override string CreateSuffix => "ENGINE=InnoDB";

        public override void CreateColumn(OperationPreparator operation, EntityColumnDescriptor column) {
            throw new NotImplementedException();
        }

        public override void AddColumn(IDBClient client, string table, EntityColumnDescriptor column, Transaction transaction) {
            throw new NotImplementedException();
        }

        public override void RemoveColumn(IDBClient client, string table, string column) {
            throw new NotImplementedException();
        }

        public override void AlterColumn(IDBClient client, string table, EntityColumnDescriptor column) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// begins a new transaction
        /// </summary>
        /// <returns></returns>
        public override IDbTransaction BeginTransaction(IDbConnection connection, object connectionlock) {
            return connection.BeginTransaction();
        }

        /// <summary>
        /// ends a transaction
        /// </summary>
        public override void EndTransaction(object connectionlock) {
        }

        public override SchemaDescriptor GetSchema(IDBClient client, string name)
        {
            throw new NotImplementedException();
        }
    }
}
