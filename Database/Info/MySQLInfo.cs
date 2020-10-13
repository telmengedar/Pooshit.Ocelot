using System;
using System.Linq.Expressions;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Entities.Schema;

namespace NightlyCode.Database.Info {

    /// <summary>
    /// information for mysql db
    /// </summary>
    public class MySQLInfo : DBInfo {
        /// <inheritdoc />
        public override string Parameter => "?";

        /// <inheritdoc />
        public override string JoinHint => "STRAIGHT_JOIN";

        /// <inheritdoc />
        public override string ColumnIndicator => "`";

        /// <inheritdoc />
        public override string LikeTerm => "LIKE";

        /// <inheritdoc />
        public override void Replace(ExpressionVisitor visitor, IOperationPreparator preparator, Expression value, Expression src, Expression target) {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void ToUpper(ExpressionVisitor visitor, IOperationPreparator preparator, Expression value) {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void ToLower(ExpressionVisitor visitor, IOperationPreparator preparator, Expression value) {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override bool CheckIfTableExists(IDBClient db, string table, Transaction transaction = null) {
            return db.Query(transaction, "SHOW TABLES like ?1", table).Rows.Length > 0;
        }

        /// <inheritdoc />
        public override string GetDBType(Type type) {
            if(type.IsEnum)
                return "INT";

            switch(type.Name) {
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

        /// <inheritdoc />
        public override Type GetDBRepresentation(Type type) {
            return type;
        }

        /// <inheritdoc />
        public override string MaskColumn(string column) {
            return $"`{column}`";
        }

        /// <inheritdoc />
        public override string CreateSuffix => "ENGINE=InnoDB";

        /// <inheritdoc />
        public override void CreateColumn(OperationPreparator operation, EntityColumnDescriptor column) {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void CreateColumn(OperationPreparator operation, SchemaColumnDescriptor column) {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void AddColumn(OperationPreparator preparator, EntityColumnDescriptor column) {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void DropColumn(OperationPreparator preparator, string column) {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void AlterColumn(OperationPreparator preparator, EntityColumnDescriptor column) {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void ReturnID(OperationPreparator preparator, ColumnDescriptor idcolumn) {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override bool MustRecreateTable(string[] obsolete, EntityColumnDescriptor[] altered, EntityColumnDescriptor[] missing, TableDescriptor tableschema, EntityDescriptor entityschema) {
            return true;
        }

        /// <inheritdoc />
        public override SchemaDescriptor GetSchema(IDBClient client, string name) {
            throw new NotImplementedException();
        }
    }
}
