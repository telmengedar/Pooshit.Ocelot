using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Entities.Schema;
using Pooshit.Ocelot.Models;
using Pooshit.Ocelot.Schemas;
using SchemaType = Pooshit.Ocelot.Schemas.SchemaType;

namespace Pooshit.Ocelot.Info;

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
    public override async Task<bool> CheckIfTableExistsAsync(IDBClient db, string table, Transaction transaction = null) {
        return (await db.QueryAsync(transaction, "SHOW TABLES like ?1", table)).Rows.Length > 0;
    }

    /// <inheritdoc />
    public override string GetDBType(string type, int length=0) {
        switch(type) {
            case Types.DateTime:
                return "DATETIME";
            case Types.String:
                return "VARCHAR(200)";
            case Types.UInt:
            case Types.Int:
            case Types.Int32:
            case Types.UInt32:
                return "INT";
            case Types.Long:
            case Types.ULong:
            case Types.Int64:
            case Types.UInt64:
                return "BIGINT";
            case Types.Short:
            case Types.UShort:
            case Types.Int16:
            case Types.UInt16:
                return "SMALLINT";
            case Types.Float:
                return "FLOAT";
            case Types.Double:
                return "DOUBLE";
            case Types.Bool:
            case Types.Boolean:
                return "BOOLEAN";
            case Types.ByteArray:
            case Types.Blob:
                return "BLOB";
            default:
                throw new InvalidOperationException("unsupported type");
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
    public override void CreateColumn(OperationPreparator operation, ColumnDescriptor column) {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override Task<IEnumerable<Schema>> ListSchemataAsync(IDBClient client, PageOptions options = null, Transaction transaction = null) {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override Task<Schema> GetSchemaAsync(IDBClient client, string name, Transaction transaction = null) {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override Task<SchemaType> GetSchemaTypeAsync(IDBClient client, string name, Transaction transaction = null) {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override void AddColumn(OperationPreparator preparator, ColumnDescriptor column) {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override void DropColumn(OperationPreparator preparator, string column) {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override void AlterColumn(OperationPreparator preparator, ColumnDescriptor column) {
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
    public override bool MustRecreateTable(string[] obsolete, ColumnDescriptor[] altered, ColumnDescriptor[] missing, TableSchema currentSchema, TableSchema targetSchema) {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override Task<string> GenerateCreateStatement(IDBClient client, string table) {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override object GenerateDefault(string type) {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override SchemaDescriptor GetSchema(IDBClient client, string name) {
        throw new NotImplementedException();
    }
}