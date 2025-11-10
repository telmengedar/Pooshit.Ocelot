using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities.Attributes;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Expressions;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Entities.Schema;
using Pooshit.Ocelot.Errors;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Models;
using Pooshit.Ocelot.Schemas;
using Pooshit.Ocelot.Statements;
using Pooshit.Ocelot.Tokens;
using Pooshit.Ocelot.Tokens.Values;
using Converter = Pooshit.Ocelot.Extern.Converter;
using SchemaType = Pooshit.Ocelot.Schemas.SchemaType;

namespace Pooshit.Ocelot.Info;

/// <summary>
/// information for sqlite
/// </summary>
public class SQLiteInfo : DBInfo {

    /// <summary>
    /// creates a new <see cref="SQLiteInfo"/>
    /// </summary>
    public SQLiteInfo() {
        AddFieldLogic<DBFunction>(AppendFunction);
        AddFieldLogic<LimitField>(AppendLimit);
        AddFieldLogic<CastToken>(AppendCast);
        AddFieldLogic<NowToken>(AppendNowToken);
    }

    public override Expression Visit(CriteriaVisitor visitor, Expression node, IOperationPreparator operation) {
        Expression expression = base.Visit(visitor, node, operation);
        if (expression != null)
            return expression;

        if (node is MethodCallExpression methodCall) {
            if (methodCall.Method.DeclaringType == typeof(DB)) {
                switch (methodCall.Method.Name) {
                    case "Cast":
                        switch ((CastType)visitor.GetHost(methodCall.Arguments[1])) {
                            case CastType.Date:
                                operation.AppendText("date(strftime('%Y-%m-%d',");
                                visitor.Visit(methodCall.Arguments[0]);
                                operation.AppendText("/10000000 - 62135596800, 'unixepoch'))");
                            break;
                            case CastType.DateTime:
                                operation.AppendText("datetime(strftime('%Y-%m-%d %H:%M:%S',");
                                visitor.Visit(methodCall.Arguments[0]);
                                operation.AppendText("/10000000 - 62135596800, 'unixepoch'))");
                            break;
                            case CastType.Year:
                                operation.AppendText("strftime('%Y',");
                                visitor.Visit(methodCall.Arguments[0]);
                                operation.AppendText("/10000000 - 62135596800, 'unixepoch')");
                            break;
                            case CastType.Month:
                                operation.AppendText("strftime('%m',");
                                visitor.Visit(methodCall.Arguments[0]);
                                operation.AppendText("/10000000 - 62135596800, 'unixepoch')");
                            break;
                            case CastType.DayOfMonth:
                                operation.AppendText("strftime('%d',");
                                visitor.Visit(methodCall.Arguments[0]);
                                operation.AppendText("/10000000 - 62135596800, 'unixepoch')");
                            break;
                            case CastType.Hour:
                                operation.AppendText("strftime('%H',");
                                visitor.Visit(methodCall.Arguments[0]);
                                operation.AppendText("/10000000 - 62135596800, 'unixepoch')");
                            break;
                            case CastType.Minute:
                                operation.AppendText("strftime('%M',");
                                visitor.Visit(methodCall.Arguments[0]);
                                operation.AppendText("/10000000 - 62135596800, 'unixepoch')");
                            break;
                            case CastType.Second:
                                operation.AppendText("strftime('%S',");
                                visitor.Visit(methodCall.Arguments[0]);
                                operation.AppendText("/10000000 - 62135596800, 'unixepoch')");
                            break;
                            case CastType.DayOfYear:
                                operation.AppendText("strftime('%j',");
                                visitor.Visit(methodCall.Arguments[0]);
                                operation.AppendText("/10000000 - 62135596800, 'unixepoch')");
                            break;
                            case CastType.DayOfWeek:
                                operation.AppendText("strftime('%w',");
                                visitor.Visit(methodCall.Arguments[0]);
                                operation.AppendText("/10000000 - 62135596800, 'unixepoch')");
                            break;
                            case CastType.WeekOfYear:
                                operation.AppendText("strftime('%W',");
                                visitor.Visit(methodCall.Arguments[0]);
                                operation.AppendText("/10000000 - 62135596800, 'unixepoch')");
                            break;
                            case CastType.Integer:
                                operation.AppendText("CAST(");
                                visitor.Visit(methodCall.Arguments[0]);
                                operation.AppendText("AS INTEGER)");
                            break;
                            case CastType.Float:
                                operation.AppendText("CAST(");
                                visitor.Visit(methodCall.Arguments[0]);
                                operation.AppendText("AS FLOAT)");
                            break;
                            case CastType.Text:
                                operation.AppendText("CAST(");
                                visitor.Visit(methodCall.Arguments[0]);
                                operation.AppendText("AS TEXT)");
                            break;
                            case CastType.Ticks:
                                visitor.Visit(methodCall.Arguments[0]);
                            break;
                            default:
                                throw new ArgumentException("Invalid cast target type");
                        }

                        return node;
                }
            }
        }

        return null;
    }

    void AppendCast(CastToken cast, IOperationPreparator preparator, Func<Type, EntityDescriptor> descriptorgetter, string tablealias) {
        switch (cast.Type) {
            case CastType.Date:
                preparator.AppendText("date(strftime('%Y-%m-%d',");
                Append(cast.Field, preparator, descriptorgetter, tablealias);
                preparator.AppendText("/10000000 - 62135596800, 'unixepoch'))");
                break;
            case CastType.DateTime:
                preparator.AppendText("datetime(strftime('%Y-%m-%d %H:%M:%S',");
                Append(cast.Field, preparator, descriptorgetter, tablealias);
                preparator.AppendText("/10000000 - 62135596800, 'unixepoch'))");
                break;
            case CastType.Year:
                preparator.AppendText("strftime('%Y',");
                Append(cast.Field, preparator, descriptorgetter, tablealias);
                preparator.AppendText("/10000000 - 62135596800, 'unixepoch')");
                break;
            case CastType.Month:
                preparator.AppendText("strftime('%m',");
                Append(cast.Field, preparator, descriptorgetter, tablealias);
                preparator.AppendText("/10000000 - 62135596800, 'unixepoch')");
                break;
            case CastType.DayOfMonth:
                preparator.AppendText("strftime('%d',");
                Append(cast.Field, preparator, descriptorgetter, tablealias);
                preparator.AppendText("/10000000 - 62135596800, 'unixepoch')");
                break;
            case CastType.Hour:
                preparator.AppendText("strftime('%H',");
                Append(cast.Field, preparator, descriptorgetter, tablealias);
                preparator.AppendText("/10000000 - 62135596800, 'unixepoch')");
                break;
            case CastType.Minute:
                preparator.AppendText("strftime('%M',");
                Append(cast.Field, preparator, descriptorgetter, tablealias);
                preparator.AppendText("/10000000 - 62135596800, 'unixepoch')");
                break;
            case CastType.Second:
                preparator.AppendText("strftime('%S',");
                Append(cast.Field, preparator, descriptorgetter, tablealias);
                preparator.AppendText("/10000000 - 62135596800, 'unixepoch')");
                break;
            case CastType.DayOfYear:
                preparator.AppendText("strftime('%j',");
                Append(cast.Field, preparator, descriptorgetter, tablealias);
                preparator.AppendText("/10000000 - 62135596800, 'unixepoch')");
                break;
            case CastType.DayOfWeek:
                preparator.AppendText("strftime('%w',");
                Append(cast.Field, preparator, descriptorgetter, tablealias);
                preparator.AppendText("/10000000 - 62135596800, 'unixepoch')");
                break;
            case CastType.WeekOfYear:
                preparator.AppendText("strftime('%W',");
                Append(cast.Field, preparator, descriptorgetter, tablealias);
                preparator.AppendText("/10000000 - 62135596800, 'unixepoch')");
                break;
            case CastType.Integer:
                preparator.AppendText("CAST(");
                Append(cast.Field, preparator, descriptorgetter, tablealias);
                preparator.AppendText("AS INTEGER)");
                break;
            case CastType.Float:
                preparator.AppendText("CAST(");
                Append(cast.Field, preparator, descriptorgetter, tablealias);
                preparator.AppendText("AS FLOAT)");
                break;
            case CastType.Text:
                preparator.AppendText("CAST(");
                Append(cast.Field, preparator, descriptorgetter, tablealias);
                preparator.AppendText("AS TEXT)");
                break;
            default:
                throw new ArgumentException("Invalid cast target type");
        }
    }

    void AppendLimit(LimitField limit, IOperationPreparator preparator, Func<Type, EntityDescriptor> descriptorgetter, string tablealias) {
        preparator.AppendText("LIMIT");
        if(limit.Offset!=null) {
            preparator.AppendField(limit.Offset, this, descriptorgetter, tablealias).AppendText(",");
            if(limit.Limit!=null)
                preparator.AppendField(limit.Limit, this, descriptorgetter, tablealias);
            else
                preparator.AppendText("-1");
        }
        // ReSharper disable once PossibleInvalidOperationException
        else
            preparator.AppendField(limit.Limit, this, descriptorgetter, tablealias);
    }

    void AppendNowToken(NowToken field, IOperationPreparator preparator, Func<Type, EntityDescriptor> descriptor, string alias) {
        preparator.AppendText("now");
    }

    void AppendFunction(DBFunction function, IOperationPreparator preparator, Func<Type, EntityDescriptor> descriptorgetter, string tablealias) {
        switch(function.Type) {
            case DBFunctionType.Random:
                preparator.AppendText("RANDOM()");
                break;
            case DBFunctionType.Count:
                preparator.AppendText("COUNT(*)");
                break;
            case DBFunctionType.RowID:
                preparator.AppendText("ROWID");
                break;
            case DBFunctionType.Length:
                preparator.AppendText("LENGTH(");
                CriteriaVisitor.GetCriteriaText(function.Parameter, descriptorgetter, this, preparator);
                preparator.AppendText(")");
                break;
            case DBFunctionType.LastInsertID:
                preparator.AppendText("last_insert_rowid()");
                break;
            case DBFunctionType.All:
                preparator.AppendText("*");
                break;
            default:
                throw new NotSupportedException($"Unsupported function {function.Type}");
        }
    }

    /// <inheritdoc />
    public override string Parameter => "@";

    /// <inheritdoc />
    public override string JoinHint => "";

    /// <inheritdoc />
    public override string ColumnIndicator => "\"";

    /// <inheritdoc />
    public override string LikeTerm => "LIKE";

    /// <inheritdoc />
    public override bool MultipleConnectionsSupported => false;

    /// <summary>
    /// method used to create a replace function
    /// </summary>
    /// <param name="preparator"> </param>
    /// <param name="value"></param>
    /// <param name="src"></param>
    /// <param name="target"></param>
    /// <param name="visitor"> </param>
    /// <returns></returns>
    public override void Replace(ExpressionVisitor visitor, IOperationPreparator preparator, Expression value, Expression src, Expression target) {
        preparator.AppendText("replace(");
        visitor.Visit(value);
        preparator.AppendText(",");
        visitor.Visit(src);
        preparator.AppendText(",");
        visitor.Visit(target);
        preparator.AppendText(")");
    }

    /// <summary>
    /// converts an expression to uppercase using database command
    /// </summary>
    /// <param name="visitor"></param>
    /// <param name="preparator"></param>
    /// <param name="value"></param>
    public override void ToUpper(ExpressionVisitor visitor, IOperationPreparator preparator, Expression value) {
        preparator.AppendText("upper(");
        visitor.Visit(value);
        preparator.AppendText(")");
    }

    /// <summary>
    /// converts an expression to lowercase using database command
    /// </summary>
    /// <param name="visitor"></param>
    /// <param name="preparator"></param>
    /// <param name="value"></param>
    public override void ToLower(ExpressionVisitor visitor, IOperationPreparator preparator, Expression value) {
        preparator.AppendText("lower(");
        visitor.Visit(value);
        preparator.AppendText(")");
    }

    /// <inheritdoc />
    public override bool CheckIfTableExists(IDBClient db, string table, Transaction transaction = null) {
        return db.Query(transaction, "SELECT name FROM sqlite_master WHERE (type='table' OR type='view') AND name like @1", table).Rows.Length > 0;
    }

    /// <inheritdoc />
    public override async Task<bool> CheckIfTableExistsAsync(IDBClient db, string table, Transaction transaction = null) {
        return (await db.QueryAsync(transaction, "SELECT name FROM sqlite_master WHERE (type='table' OR type='view') AND name like @1", table)).Rows.Length > 0;
    }

    /// <inheritdoc />
    public override bool IsTypeEqual(string lhs, string rhs) {
        if(lhs is "TEXT" or "VARCHAR")
            return rhs is "TEXT" or "VARCHAR";
        return base.IsTypeEqual(lhs, rhs);
    }

    /// <inheritdoc />
    public override void CreateParameter(IDbCommand command, object parameterValue) {
        // currently sqlite doesn't have a mapping for biginteger
        if (parameterValue is BigInteger)
            parameterValue = parameterValue.ToString();
        base.CreateParameter(command, parameterValue);
    }

    /// <inheritdoc />
    public override string GetDBType(string type, int length=0) {
        switch(type) {
            case Types.DateTime:
                //return "TIMESTAMP";
                // still issues when comparing datetime in db if using timestamp format
                return "INTEGER";
            case Types.Guid:
            case Types.String:
                return "TEXT";
            case Types.Version:
                return "INTEGER";
            case Types.Byte:
            case Types.SByte:
            case Types.Int:
            case Types.UInt32:
            case Types.Int32:
            case Types.Long:
            case Types.ULong:
            case Types.UInt64:
            case Types.Int64:
            case Types.Short:
            case Types.UShort:
            case Types.UInt16:
            case Types.Int16:
                return "INTEGER";
            case Types.Single:
            case Types.Float:
            case Types.Double:
                return "FLOAT";
            case Types.Bool:
            case Types.Boolean:
                return "BOOLEAN";
            case Types.ByteArray:
            case Types.Blob:
                return "BLOB";
            case Types.Timespan:
                return "INTEGER";
            case Types.Decimal:
            case Types.BigInteger:
                return "DECIMAL";
            case Types.SingleArray:
                return "TEXT";
            default:
                throw new InvalidOperationException($"unsupported type '{type}");
        }
    }

    /// <summary>
    /// get db representation type
    /// </summary>
    /// <param name="type">type of which to get db representation</param>
    /// <returns>type to use for database</returns>
    public override Type GetDBRepresentation(Type type) {
        if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            type = Nullable.GetUnderlyingType(type);

        // sqlite understands datetime but not timespan
        if (type == typeof(DateTime))
            return typeof(long);
        if(type == typeof(TimeSpan))
            return typeof(long);
        if(type == typeof(Version))
            return typeof(long);
        if(type == typeof(Guid))
            return typeof(string);
        if (type == typeof(float[]))
            return typeof(string);
        return type;
    }

    /// <summary>
    /// masks a column
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public override string MaskColumn(string column) {
        return $"[{column}]";
    }

    /// <summary>
    /// suffix to use when creating tables
    /// </summary>
    public override string CreateSuffix => null;

    /// <inheritdoc />
    public override void CreateColumn(OperationPreparator operation, EntityColumnDescriptor column) {
        CreateColumn(operation,
                     column.Name,
                     GetDBType(column.Property.PropertyType, SizeAttribute.GetLength(column.Property)),
                     column.PrimaryKey,
                     column.AutoIncrement,
                     column.IsUnique,
                     column.NotNull,
                     column.DefaultValue
                    );
    }

    /// <inheritdoc />
    public override void CreateColumn(OperationPreparator operation, ColumnDescriptor column) {
        CreateColumn(operation,
                     column.Name,
                     column.Type,
                     column.PrimaryKey,
                     column.AutoIncrement,
                     column.IsUnique,
                     column.NotNull,
                     column.DefaultValue
                    );
    }

    IEnumerable<Schema> CreateSchemata(IDataReader reader) {
        using (reader) {
            while (reader.Read()) {
                yield return new() {
                    Name = reader.GetString(0),
                    Type = Converter.Convert<SchemaType>(reader.GetString(1))
                };
            }
        }
    }

    /// <inheritdoc />
    public override async Task<IEnumerable<Schema>> ListSchemataAsync(IDBClient client, PageOptions options = null, Transaction transaction = null) {
        string command = "SELECT [name],[type] FROM sqlite_master WHERE [type]='table' OR [type]='view'";
        if (options != null) {
            if (options.Offset > 0) {
                command += $" LIMIT {options.Offset},{(options.Items > 0 ? options.Items : -1)}";
            }
            else if (options.Items > 0)
                command += $" LIMIT {options.Items}";
        }
        IDataReader reader = await client.ReaderAsync(command);
        return CreateSchemata(reader);
    }

    void CreateColumn(OperationPreparator operation, string name, string type, bool primarykey, bool autoincrement, bool unique, bool notnull, object defaultvalue) {
        operation.AppendText(MaskColumn(name));
        operation.AppendText(type);

        if(primarykey)
            operation.AppendText("PRIMARY KEY");
        if(autoincrement)
            operation.AppendText("AUTOINCREMENT");
        if(unique)
            operation.AppendText("UNIQUE");
        if(notnull) {
            operation.AppendText("NOT NULL");
        }

        if(defaultvalue != null) {
            operation.AppendText("DEFAULT");
            if(defaultvalue is string or Guid or DateTime or TimeSpan)
                operation.AppendText($"'{Converter.Convert<string>(defaultvalue)}'");
            else
                operation.AppendText(Converter.Convert<string>(defaultvalue));
        }
    }

    /// <inheritdoc />
    public override async Task<SchemaType> GetSchemaTypeAsync(IDBClient client, string name, Transaction transaction = null) {
        Clients.Tables.DataTable table = await client.QueryAsync("SELECT * FROM sqlite_master WHERE (name=@1)", name);

        if(table.Rows.Length == 0)
            throw new InvalidOperationException("Type not found in database");

        string schemaType = Converter.Convert<string>(table.Rows[0]["type"]);
        switch (schemaType) {
            case "table":
                return SchemaType.Table;
            case "view":
                return SchemaType.View;
            default:
                throw new SchemaException($"'{name}' is of unknown type '{schemaType}'");
        }
    }

    /// <inheritdoc />
    public override void AddColumn(OperationPreparator operation, ColumnDescriptor column) {
        operation.AppendText("ADD COLUMN");
        AddColumn(operation,
                  column.Name,
                  column.Type,
                  column.PrimaryKey,
                  column.AutoIncrement,
                  column.IsUnique,
                  column.NotNull,
                  column.DefaultValue
                 );
    }

    void AddColumn(OperationPreparator operation, string name, string type, bool primarykey, bool autoincrement, bool unique, bool notnull, object defaultvalue) {
        operation.AppendText(MaskColumn(name));
        operation.AppendText(type);

        if(primarykey)
            operation.AppendText("PRIMARY KEY");
        if(autoincrement)
            operation.AppendText("AUTOINCREMENT");
        if(unique)
            operation.AppendText("UNIQUE");
        if(notnull) {
            operation.AppendText("NOT NULL");

            // SQLite doesn't like no default values on nullable columns in add column case
            defaultvalue ??= GenerateDefault(type);
        }

        if(defaultvalue != null) {
            operation.AppendText("DEFAULT");
            if(defaultvalue is string or Guid or DateTime or TimeSpan)
                operation.AppendText($"'{defaultvalue}'");
            else
                operation.AppendText(defaultvalue.ToString());
        }
    }

    /// <inheritdoc />
    public override SchemaDescriptor GetSchema(IDBClient client, string tablename) {
        Clients.Tables.DataTable table = client.Query("SELECT * FROM sqlite_master WHERE (name=@1)", tablename);

        if(table.Rows.Length == 0)
            throw new InvalidOperationException("Type not found in database");

        if(Converter.Convert<string>(table.Rows[0]["type"]) == "table") {
            TableDescriptor tabledescriptor = new(Converter.Convert<string>(table.Rows[0]["tbl_name"]));
            AnalyseTableSql(tabledescriptor, Converter.Convert<string>(table.Rows[0]["sql"]));

            Clients.Tables.DataTable indices = client.Query("SELECT * FROM sqlite_master WHERE type='index' AND tbl_name=@1", tablename);
            tabledescriptor.Indices = AnalyseIndexDefinitions(indices).ToArray();
            return tabledescriptor;
        }
        if(Converter.Convert<string>(table.Rows[0]["type"]) == "view") {
            ViewDescriptor viewdesc = new(Converter.Convert<string>(table.Rows[0]["tbl_name"])) {
                                                                                                    SQL = Converter.Convert<string>(table.Rows[0]["sql"])
                                                                                                };
            return viewdesc;
        }

        throw new InvalidOperationException("Invalid entity type in database");
    }

    /// <inheritdoc />
    public override async Task<Schema> GetSchemaAsync(IDBClient client, string tablename, Transaction transaction=null) {
        Clients.Tables.DataTable table = await client.QueryAsync(transaction, "SELECT * FROM sqlite_master WHERE (name=@1)", tablename);

        if(table.Rows.Length == 0)
            throw new InvalidOperationException("Type not found in database");

        if(Converter.Convert<string>(table.Rows[0]["type"]) == "table") {
            TableSchema tabledescriptor = new() {
                                                    Name = Converter.Convert<string>(table.Rows[0]["tbl_name"])
                                                };
                
            AnalyseTableSql(tabledescriptor, Converter.Convert<string>(table.Rows[0]["sql"]));

            Clients.Tables.DataTable indices = await client.QueryAsync(transaction, "SELECT * FROM sqlite_master WHERE type='index' AND tbl_name=@1", tablename);
            tabledescriptor.Index = AnalyseIndexDefinitions(indices).ToArray();
            return tabledescriptor;
        }
        if(Converter.Convert<string>(table.Rows[0]["type"]) == "view") {
            return new ViewSchema {
                                      Name = Converter.Convert<string>(table.Rows[0]["tbl_name"]),
                                      Definition = Converter.Convert<string>(table.Rows[0]["sql"])
                                  };
        }

        throw new InvalidOperationException("Invalid entity type in database");
    }

    IEnumerable<IndexDescriptor> AnalyseIndexDefinitions(Clients.Tables.DataTable table) {
        foreach(Clients.Tables.DataRow row in table.Rows) {

            // sqlite has some internal index definitions which are not important here 
            // and can't be analyzed anyways because they have no sql definition
            string sql = Converter.Convert<string>(row["sql"]);
            if(string.IsNullOrEmpty(sql))
                continue;

            Match match = Regex.Match(sql, @"^CREATE INDEX idx_(?<tablename>.+)_(?<name>.+) ON (?<table>.+) \((?<columns>.+)\)$");
            if(match.Success)
                yield return new IndexDescriptor(match.Groups["name"].Value, match.Groups["columns"].Value.Split(',').Select(c => c.Trim(' ', '\"')), null);
        }
    }

    /// <summary>
    /// adds a column to a table
    /// </summary>
    /// <param name="client">db access</param>
    /// <param name="table">table to modify</param>
    /// <param name="column">column to add</param>
    /// <param name="transaction">transaction to use (optional)</param>
    public void AddColumn(IDBClient client, string table, EntityColumnDescriptor column, Transaction transaction = null) {
        OperationPreparator operation = new();
        operation.AppendText($"ALTER TABLE {table} ADD COLUMN");
        AddColumn(operation, column);
        if(transaction != null)
            operation.GetOperation(client, false).Execute(transaction);
        else
            operation.GetOperation(client, false).Execute();
    }

    /// <inheritdoc />
    public override void DropColumn(OperationPreparator preparator, string column) {
        throw new NotSupportedException("SQLite is not able to drop columns directly");
    }

    /// <inheritdoc />
    public override void AlterColumn(OperationPreparator preparator, ColumnDescriptor column) {
        throw new NotSupportedException("SQLite is not able to alter columns directly");
    }

    /// <inheritdoc />
    public override void ReturnID(OperationPreparator preparator, ColumnDescriptor idcolumn) {
        preparator.AppendText(";SELECT last_insert_rowid()");
    }

    /// <inheritdoc />
    public override bool MustRecreateTable(string[] obsolete, EntityColumnDescriptor[] altered, EntityColumnDescriptor[] missing, TableDescriptor tableschema, EntityDescriptor entityschema) {
        return obsolete.Length > 0
               || altered.Length > 0
               || missing.Any(m => m.IsUnique || m.PrimaryKey)
               || !tableschema.Uniques.Equals(entityschema.Uniques)
               || !tableschema.Indices.Equals(entityschema.Indices);
    }

    /// <inheritdoc />
    public override bool MustRecreateTable(string[] obsolete, ColumnDescriptor[] altered, ColumnDescriptor[] missing, TableSchema tableschema, TableSchema entityschema) {
        return obsolete.Length > 0
               || altered.Length > 0
               || missing.Any(m => m.IsUnique || m.PrimaryKey)
               || !tableschema.Unique.Equals(entityschema.Unique)
               || !tableschema.Index.Equals(entityschema.Index);
    }

    /// <inheritdoc />
    public override async Task<string> GenerateCreateStatement(IDBClient client, string table) {
        return Converter.Convert<string>(await client.ScalarAsync($"SELECT sql FROM sqlite_master WHERE tbl_name = '{table}'"));
    }

    /// <summary>
    /// analyses the sql of a table creation and fills the table descriptor from the results
    /// </summary>
    /// <param name="descriptor">descriptor to fill</param>
    /// <param name="sql">sql to analyse</param>
    public void AnalyseTableSql(TableDescriptor descriptor, string sql) {
        Match match = Regex.Match(sql, @"^CREATE TABLE\s+(?<name>[^ ]+)\s+\((?<columns>.+?)(\s*,\s*UNIQUE\s*\((?<unique>.+?)\))*\s*\)$");
        if(!match.Success)
            throw new InvalidOperationException("Unable to analyse table information");

        string[] columns = match.Groups["columns"].Value.Split(',').Select(c => c.Trim()).ToArray();
        descriptor.Columns = GetDefinitions(columns).Select(GetColumnDescriptor).ToArray();


        descriptor.Uniques = AnalyseUniques(match.Groups["unique"].Captures.Cast<Capture>().Select(c => c.Value)).ToArray();
    }

    void AnalyseTableSql(TableSchema descriptor, string sql) {
        Match match = Regex.Match(sql, @"^CREATE TABLE\s+(?<name>[^ ]+)\s+\((?<columns>.+?)(\s*,\s*UNIQUE\s*\((?<unique>.+?)\))*\s*\)$");
        if(!match.Success)
            throw new InvalidOperationException("Unable to analyse table information");

        string[] columns = match.Groups["columns"].Value.Split(',').Select(c => c.Trim()).ToArray();
        descriptor.Columns = GetDefinitions(columns).Select(GetColumnDescriptor).ToArray();


        descriptor.Unique = AnalyseUniques(match.Groups["unique"].Captures.Cast<Capture>().Select(c => c.Value)).ToArray();
    }
        
    IEnumerable<UniqueDescriptor> AnalyseUniques(IEnumerable<string> definitions) {
        foreach(string definition in definitions) {
            string[] columns = definition.Trim().Split(',').Select(s => s.Trim(' ', '\'', '[', ']')).ToArray();

            yield return new UniqueDescriptor(null, columns);
        }
    }

    IEnumerable<string> GetDefinitions(IEnumerable<string> columns) {
        foreach(string column in columns) {
            yield return column;
        }
    }

    ColumnDescriptor GetColumnDescriptor(string sql) {
        Match match = Regex.Match(sql, @"^['\[]?(?<name>[^ '\]]+)['\]]?\s+(?<type>[^ ]+)(?<pk> PRIMARY KEY)?(?<ai> AUTOINCREMENT)?(?<uq> UNIQUE)?(?<nn> NOT NULL)?( DEFAULT '?(?<default>[^']+)'?)?$");
        if(!match.Success)
            throw new InvalidOperationException("Error analysing column description sql");

        ColumnDescriptor descriptor = new(match.Groups["name"].Value) {
                                                                          Type = match.Groups["type"].Value,
                                                                          AutoIncrement = match.Groups["ai"].Success,
                                                                          PrimaryKey = match.Groups["pk"].Success,
                                                                          NotNull = match.Groups["nn"].Success,
                                                                          DefaultValue = match.Groups["default"].Value,
                                                                          IsUnique = match.Groups["uq"].Success
                                                                      };
        return descriptor;
    }

    /// <inheritdoc />
    public override async Task Truncate(IDBClient client, string table, TruncateOptions options = null) {
        if (options?.Transaction == null) {
            using Transaction transaction = client.Transaction();
            await client.NonQueryAsync(transaction, $"DELETE FROM {MaskColumn(table)}");
            if (options?.ResetIdentity ?? false)
                await client.NonQueryAsync(transaction, $"UPDATE {MaskColumn("sqlite_sequence")} SET {MaskColumn("seq")} = 0 WHERE {MaskColumn("name")} = '{table}'");
            transaction.Commit();
        }
        else {
            await client.NonQueryAsync(options.Transaction, $"DELETE FROM {MaskColumn(table)}");
            if (options.ResetIdentity)
                await client.NonQueryAsync(options.Transaction, $"UPDATE {MaskColumn("sqlite_sequence")} SET {MaskColumn("seq")} = 0 WHERE {MaskColumn("name")} = '{table}'");
        }
    }

    /// <inheritdoc />
    public override object GenerateDefault(string type) {
        switch (type.ToUpper()) {
            case "TEXT":
                return "";
            case "FLOAT":
                return 0.0f;
            case "DECIMAL":
                return 0.0;
            case "BLOB":
                return Array.Empty<byte>();
            default:
                return 0;
        }
    }

    /// <inheritdoc />
    public override object ValueFromReader(Reader reader, int ordinal, Type type) {
        return reader.GetValue(ordinal);
    }
    
    /// <inheritdoc />
    public override Task<object> ValueFromReaderAsync(Reader reader, int ordinal, Type type) {
        return Task.FromResult(reader.GetValue(ordinal));
    }

}