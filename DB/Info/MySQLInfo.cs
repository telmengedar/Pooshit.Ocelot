using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NightlyCode.DB.Clients;
using NightlyCode.DB.Entities.Descriptors;
using NightlyCode.DB.Entities.Operations;
using NightlyCode.DB.Entities.Schema;

#if UNITY
using NightlyCode.Unity.DB.Entities.Operations;
#endif

namespace NightlyCode.DB.Info
{

    /// <summary>
    /// information for mysql db
    /// </summary>
    public class MySQLInfo : IDBInfo
    {
        public string Parameter => "?";

        public string LastValue => "LAST_INSERT_ID()";

        public string JoinHint => "STRAIGHT_JOIN";

        public string AutoIncrement => "AUTO_INCREMENT";

        public string ColumnIndicator => "`";

        public string LikeTerm => "LIKE";

        public void Replace(
#if UNITY
            ExpressionVisitor visitor,
#else
            ExpressionVisitor visitor,
#endif 
            OperationPreparator preparator, Expression value, Expression src, Expression target) {
            throw new NotImplementedException();
        }

        public void ToUpper(ExpressionVisitor visitor, OperationPreparator preparator, Expression value) {
            throw new NotImplementedException();
        }

        public bool CheckIfTableExists(IDBClient db, string table)
        {
            return db.Query("SHOW TABLES like ?1", table).Rows.Count > 0;
        }

        public bool TransactionHint => false;

        public string GetDBType(Type type) {
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

        public Type GetDBRepresentation(Type type) {
            return type;
        }

        public string MaskColumn(string column) {
            return $"`{column}`";
        }

        public string CreateSuffix => "ENGINE=InnoDB";

        public DBType Type {
            get { throw new NotImplementedException(); }
        }

        public string CreateColumn(EntityColumnDescriptor column) {
            throw new NotImplementedException();
        }

        public object ReturnInsertID(IDBClient client, EntityDescriptor descriptor, string insertcommand, params object[] parameters) {
            throw new NotImplementedException();
        }

        public SchemaDescriptor GetSchema<T>(IDBClient client) {
            throw new NotImplementedException();
        }

        public void AddColumn(IDBClient client, string table, EntityColumnDescriptor column) {
            throw new NotImplementedException();
        }

        public void RemoveColumn(IDBClient client, string table, string column) {
            throw new NotImplementedException();
        }

        public void AlterColumn(IDBClient client, string table, EntityColumnDescriptor column) {
            throw new NotImplementedException();
        }

        public TableDescriptor[] GetTables() {
            throw new NotImplementedException();
        }
    }
}
