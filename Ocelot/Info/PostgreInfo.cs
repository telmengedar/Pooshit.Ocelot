using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.CustomTypes;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Entities.Operations.Expressions;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Entities.Schema;
using Pooshit.Ocelot.Errors;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Info.Postgre;
using Pooshit.Ocelot.Models;
using Pooshit.Ocelot.Schemas;
using Pooshit.Ocelot.Statements;
using Pooshit.Ocelot.Tokens;
using Pooshit.Ocelot.Tokens.Values;
using Converter = Pooshit.Ocelot.Extern.Converter;
using SchemaType = Pooshit.Ocelot.Schemas.SchemaType;

namespace Pooshit.Ocelot.Info;

/// <summary>
/// database specific logic for postgre databases
/// </summary>
public class PostgreInfo : DBInfo {
    readonly Type bigIntRangeType = Type.GetType("NpgsqlTypes.NpgsqlRange`1,Npgsql");
    readonly Type decimalRangeType = Type.GetType("NpgsqlTypes.NpgsqlRange`1,Npgsql");
    readonly Type intRangeType = Type.GetType("NpgsqlTypes.NpgsqlRange`1[System.Int32],Npgsql");
    readonly Type longRangeType = Type.GetType("NpgsqlTypes.NpgsqlRange`1[System.Int64],Npgsql");
    readonly Type dateRangeType = Type.GetType("NpgsqlTypes.NpgsqlRange`1[System.DateTime],Npgsql");

    readonly PropertyInfo lowerBigInt;
    readonly PropertyInfo upperBigInt;
    readonly PropertyInfo lowerInclusiveBigInt;
    readonly PropertyInfo upperInclusiveBigInt;

    readonly PropertyInfo lowerDecimal;
    readonly PropertyInfo upperDecimal;
    readonly PropertyInfo lowerInclusiveDecimal;
    readonly PropertyInfo upperInclusiveDecimal;

    readonly PropertyInfo lowerInt;
    readonly PropertyInfo upperInt;
    readonly PropertyInfo lowerInclusiveInt;
    readonly PropertyInfo upperInclusiveInt;
        
    readonly PropertyInfo lowerLong;
    readonly PropertyInfo upperLong;
    readonly PropertyInfo lowerInclusiveLong;
    readonly PropertyInfo upperInclusiveLong;

    readonly PropertyInfo lowerDate;
    readonly PropertyInfo upperDate;
    readonly PropertyInfo lowerInclusiveDate;
    readonly PropertyInfo upperInclusiveDate;
        
    /// <summary>
    /// creates a new <see cref="PostgreInfo"/>
    /// </summary>
    public PostgreInfo() {
        bigIntRangeType = bigIntRangeType.MakeGenericType(typeof(BigInteger));
        lowerBigInt = bigIntRangeType.GetProperty("LowerBound");
        upperBigInt = bigIntRangeType.GetProperty("UpperBound");
        lowerInclusiveBigInt = bigIntRangeType.GetProperty("LowerBoundIsInclusive");
        upperInclusiveBigInt = bigIntRangeType.GetProperty("UpperBoundIsInclusive");

        decimalRangeType = decimalRangeType.MakeGenericType(typeof(BigInteger));
        lowerDecimal = decimalRangeType.GetProperty("LowerBound");
        upperDecimal = decimalRangeType.GetProperty("UpperBound");
        lowerInclusiveDecimal = decimalRangeType.GetProperty("LowerBoundIsInclusive");
        upperInclusiveDecimal = decimalRangeType.GetProperty("UpperBoundIsInclusive");

        lowerInt = intRangeType.GetProperty("LowerBound");
        upperInt = intRangeType.GetProperty("UpperBound");
        lowerInclusiveInt = intRangeType.GetProperty("LowerBoundIsInclusive");
        upperInclusiveInt = intRangeType.GetProperty("UpperBoundIsInclusive");

        lowerLong = longRangeType.GetProperty("LowerBound");
        upperLong = longRangeType.GetProperty("UpperBound");
        lowerInclusiveLong = longRangeType.GetProperty("LowerBoundIsInclusive");
        upperInclusiveLong = longRangeType.GetProperty("UpperBoundIsInclusive");

        lowerDate = dateRangeType.GetProperty("LowerBound");
        upperDate = dateRangeType.GetProperty("UpperBound");
        lowerInclusiveDate = dateRangeType.GetProperty("LowerBoundIsInclusive");
        upperInclusiveDate = dateRangeType.GetProperty("UpperBoundIsInclusive");
            
        AddFieldLogic<DBFunction>(AppendFunction);
        AddFieldLogic<LimitField>(AppendLimit);
        AddFieldLogic<CastToken>(AppendCast);
        AddFieldLogic<Aggregate>(AppendAggregate);
    }

    void AppendAggregate(Aggregate aggregate, IOperationPreparator preparator, Func<Type, EntityDescriptor> descriptorgetter, string tablealias) {
        string method = aggregate.Method;
        if (method == "ANY")
            method = "ANY_VALUE";
        
        preparator.AppendText(method).AppendText("(");
        if(aggregate.Arguments.Length > 0) {
            Append(aggregate.Arguments[0], preparator, descriptorgetter);
            foreach(IDBField field in aggregate.Arguments.Skip(1)) {
                preparator.AppendText(", ");
                Append(field, preparator, descriptorgetter);
            }
        }

        preparator.AppendText(")");
    }

    /// <inheritdoc />
    public override bool SupportsArrayParameters => true;

    /// <inheritdoc />
    public override Expression Visit(CriteriaVisitor visitor, Expression node, IOperationPreparator operation) {
        if (node is MethodCallExpression methodCall) {
            if (methodCall.Method.DeclaringType == typeof(DB)) {
                switch (methodCall.Method.Name) {
                    case "Cast":
                        switch ((CastType)visitor.GetHost(methodCall.Arguments[1])) {
                            case CastType.Date:
                                visitor.Visit(methodCall.Arguments[0]);
                                operation.AppendText("::date");
                                break;
                            case CastType.DateTime:
                                visitor.Visit(methodCall.Arguments[0]);
                                operation.AppendText("::timestamp");
                                break;
                            case CastType.Year:
                                operation.AppendText("EXTRACT(YEAR FROM");
                                visitor.Visit(methodCall.Arguments[0]);
                                operation.AppendText(")");
                                break;
                            case CastType.Month:
                                operation.AppendText("EXTRACT(MONTH FROM");
                                visitor.Visit(methodCall.Arguments[0]);
                                operation.AppendText(")");
                                break;
                            case CastType.DayOfMonth:
                                operation.AppendText("EXTRACT(DAY FROM");
                                visitor.Visit(methodCall.Arguments[0]);
                                operation.AppendText(")");
                                break;
                            case CastType.Hour:
                                operation.AppendText("EXTRACT(HOUR FROM");
                                visitor.Visit(methodCall.Arguments[0]);
                                operation.AppendText(")");
                                break;
                            case CastType.Minute:
                                operation.AppendText("EXTRACT(MINUTE FROM");
                                visitor.Visit(methodCall.Arguments[0]);
                                operation.AppendText(")");
                                break;
                            case CastType.Second:
                                operation.AppendText("EXTRACT(SECOND FROM");
                                visitor.Visit(methodCall.Arguments[0]);
                                operation.AppendText(")");
                                break;
                            case CastType.DayOfYear:
                                operation.AppendText("EXTRACT(DOY FROM");
                                visitor.Visit(methodCall.Arguments[0]);
                                operation.AppendText(")");
                                break;
                            case CastType.DayOfWeek:
                                operation.AppendText("EXTRACT(DOW FROM");
                                visitor.Visit(methodCall.Arguments[0]);
                                operation.AppendText(")");
                                break;
                            case CastType.WeekOfYear:
                                operation.AppendText("EXTRACT(WEEK FROM ");
                                visitor.Visit(methodCall.Arguments[0]);
                                operation.AppendText(")");
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
                                operation.AppendText("EXTRACT(EPOCH FROM");
                                visitor.Visit(methodCall.Arguments[0]);
                                operation.AppendText("* 10000000)");
                            break;
                            default:
                                throw new ArgumentException("Invalid cast target type");
                        }
                        return node;
                    case "Least":
                        operation.AppendText("LEAST(");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText(")");
                        return node;
                    case "Greatest":
                        operation.AppendText("GREATEST(");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText(")");
                        return node;
                    case "Any":
                        operation.AppendText("ANY_VALUE(");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText(")");
                    break;
                }
            }
        }

        return base.Visit(visitor, node, operation);
    }

    void AppendCast(CastToken cast, IOperationPreparator preparator, Func<Type, EntityDescriptor> descriptorgetter, string tablealias) {
        switch (cast.Type) {
            case CastType.Date:
                Append(cast.Field, preparator, descriptorgetter, tablealias);
                preparator.AppendText("::date");
                break;
            case CastType.DateTime:
                Append(cast.Field, preparator, descriptorgetter, tablealias);
                preparator.AppendText("::timestamp");
                break;
            case CastType.Year:
                preparator.AppendText("EXTRACT(YEAR FROM ");
                Append(cast.Field, preparator, descriptorgetter, tablealias);
                preparator.AppendText(")");
                break;
            case CastType.Month:
                preparator.AppendText("EXTRACT(MONTH FROM ");
                Append(cast.Field, preparator, descriptorgetter, tablealias);
                preparator.AppendText(")");
                break;
            case CastType.DayOfMonth:
                preparator.AppendText("EXTRACT(DAY FROM ");
                Append(cast.Field, preparator, descriptorgetter, tablealias);
                preparator.AppendText(")");
                break;
            case CastType.Hour:
                preparator.AppendText("EXTRACT(HOUR FROM ");
                Append(cast.Field, preparator, descriptorgetter, tablealias);
                preparator.AppendText(")");
                break;
            case CastType.Minute:
                preparator.AppendText("EXTRACT(MINUTE FROM ");
                Append(cast.Field, preparator, descriptorgetter, tablealias);
                preparator.AppendText(")");
                break;
            case CastType.Second:
                preparator.AppendText("EXTRACT(SECOND FROM ");
                Append(cast.Field, preparator, descriptorgetter, tablealias);
                preparator.AppendText(")");
                break;
            case CastType.DayOfYear:
                preparator.AppendText("EXTRACT(DOY FROM ");
                Append(cast.Field, preparator, descriptorgetter, tablealias);
                preparator.AppendText(")");
                break;
            case CastType.DayOfWeek:
                preparator.AppendText("EXTRACT(DOW FROM ");
                Append(cast.Field, preparator, descriptorgetter, tablealias);
                preparator.AppendText(")");
                break;
            case CastType.WeekOfYear:
                preparator.AppendText("EXTRACT(WEEK FROM ");
                Append(cast.Field, preparator, descriptorgetter, tablealias);
                preparator.AppendText(")");
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
        if(limit.Limit!=null)
            preparator.AppendText("LIMIT").AppendField(limit.Limit, this, descriptorgetter, tablealias);
        if(limit.Offset!=null)
            preparator.AppendText("OFFSET").AppendField(limit.Offset, this, descriptorgetter, tablealias);
    }

    /// <summary>
    /// appends a database function to an <see cref="OperationPreparator"/>
    /// </summary>
    /// <param name="function">function to be executed</param>
    /// <param name="preparator">operation to append function to</param>
    /// <param name="descriptorgetter">function used to get <see cref="EntityDescriptor"/>s for types</param>
    /// <param name="tablealias">alias to use when referencing tables</param>
    public void AppendFunction(DBFunction function, IOperationPreparator preparator, Func<Type, EntityDescriptor> descriptorgetter, string tablealias) {
        switch(function.Type) {
            case DBFunctionType.Random:
                preparator.AppendText("RANDOM()");
                break;
            case DBFunctionType.Count:
                preparator.AppendText("COUNT(*)");
                break;
            case DBFunctionType.RowID:
                preparator.AppendText("OID");
                break;
            case DBFunctionType.Length:
                preparator.AppendText("char_length(");
                CriteriaVisitor.GetCriteriaText(function.Parameter, descriptorgetter, this, preparator);
                preparator.AppendText(")");
                break;
            case DBFunctionType.All:
                preparator.AppendText("*");
                break;
            default:
                throw new NotSupportedException($"Unsupported function {function.Type}");
        }
    }

    /// <summary>
    /// character used for parameters
    /// </summary>
    public override string Parameter => "@";

    /// <summary>
    /// parameter used when joining
    /// </summary>
    public override string JoinHint => "";

    /// <summary>
    /// character used to specify columns explicitely
    /// </summary>
    public override string ColumnIndicator => "\"";

    /// <summary>
    /// term used for like expression
    /// </summary>
    public override string LikeTerm => "ILIKE";

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

    /// <inheritdoc />
    public override void DropView(IDBClient client, ViewDescriptor view) {
        client.NonQuery($"DROP VIEW IF EXISTS {view.Name} CASCADE");
    }

    /// <inheritdoc />
    public override void DropTable(IDBClient client, TableDescriptor entity) {
        client.NonQuery($"DROP TABLE IF EXISTS {entity.Name} CASCADE");
    }

    /// <inheritdoc />
    public override void ToUpper(ExpressionVisitor visitor, IOperationPreparator preparator, Expression value) {
        preparator.AppendText("UPPER(");
        visitor.Visit(value);
        preparator.AppendText(")");
    }

    /// <inheritdoc />
    public override void ToLower(ExpressionVisitor visitor, IOperationPreparator preparator, Expression value) {
        preparator.AppendText("LOWER(");
        visitor.Visit(value);
        preparator.AppendText(")");
    }

    /// <inheritdoc />
    public override bool CheckIfTableExists(IDBClient db, string table, Transaction transaction = null) {
        return Converter.Convert<long>(db.Scalar(transaction, "SELECT count(*) FROM pg_class WHERE relname = @1", table)) > 0;
    }

    /// <inheritdoc />
    public override async Task<bool> CheckIfTableExistsAsync(IDBClient db, string table, Transaction transaction = null) {
        return Converter.Convert<long>(await db.ScalarAsync(transaction, "SELECT count(*) FROM pg_class WHERE relname = @1", table)) > 0;
    }

    /// <inheritdoc />
    public override bool IsTypeEqual(string lhs, string rhs) {
        lhs = GetDBType(lhs);
        rhs = GetDBType(rhs);
        return lhs == rhs;
    }

    /// <inheritdoc />
    public override string GetDBType(string type) {
        if (type.StartsWith("timestamp"))
            return "timestamp";
            
        switch(type) {
            case Types.DateTime:
                return "timestamp";
            case Types.Guid:
            case Types.String:
            case Types.Version:
            case Types.CharacterVarying:
                return "character varying";
            case Types.Int:
            case Types.Integer:
            case Types.UInt:
            case Types.UInt32:
            case Types.Int32:
            case Types.Int4:
                return "int4";
            case Types.Long:
            case Types.ULong:
            case Types.UInt64:
            case Types.Int64:
            case Types.BigInt:
            case Types.Int8:
                return "int8";
            case Types.Char:
            case Types.Byte:
            case Types.SByte:
            case Types.Short:
            case Types.UShort:
            case Types.UInt16:
            case Types.Int16:
            case Types.Int2:
                return "int2";
            case Types.Single:
            case Types.Float:
            case Types.Float4:
            case Types.SinglePrecision:
            case Types.Real:
                return "float4";
            case Types.Float8:
            case Types.Double:
            case Types.DoublePrecision:
                return "float8";
            case Types.Bool:
            case Types.Boolean:
                return "boolean";
            case Types.ByteA:
            case Types.ByteArray:
            case Types.Blob:
                return "bytea";
            case Types.Timespan:
                return "int8";
            case Types.Decimal:
            case Types.Numeric:
            case Types.BigInteger:
                return "decimal";
            case Types.NumericRange:
            case Types.IntRange:
            case Types.LongRange:
            case Types.DateRange:
                return type;
            default:
                throw new InvalidOperationException($"unsupported type '{type}'");
        }
    }

    /// <summary>
    /// get db representation type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public override Type GetDBRepresentation(Type type) {
        if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            type = Nullable.GetUnderlyingType(type);

        if(type.IsEnum)
            return typeof(int);
        if(type == typeof(TimeSpan))
            return typeof(long);
        if(type == typeof(Version))
            return typeof(string);
        if (type == typeof(Guid))
            return typeof(string);
        return type;
    }

    /// <summary>
    /// masks a column
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public override string MaskColumn(string column) {
        return $"\"{column}\"";
    }

    /// <summary>
    /// suffix to use when creating tables
    /// </summary>
    public override string CreateSuffix => null;

    /// <inheritdoc />
    public override void CreateColumn(OperationPreparator operation, EntityColumnDescriptor column) {
        operation.AppendText($"\"{column.Name}\"");
        ColumnType(operation, column);
        operation.AppendText(ColumnAttributes(column));
    }

    /// <inheritdoc />
    public override void CreateColumn(OperationPreparator operation, ColumnDescriptor column) {
        operation.AppendText($"\"{column.Name}\"");

        ColumnType(operation, column);
        operation.AppendText(ColumnAttributes(column));
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
        string command = "SELECT table_name, 'table' as type FROM information_schema.tables WHERE table_schema = ANY (current_schemas(false)) UNION SELECT table_name, 'view' as type FROM information_schema.views WHERE table_schema = ANY (current_schemas(false))";
        if (options != null) {
            if (options.Items > 0)
                command += $" LIMIT {options.Items}";
            if (options.Offset > 0)
                command += $" OFFSET {options.Offset}";
        }
            
        IDataReader reader = await client.ReaderAsync(transaction, command);
        return CreateSchemata(reader);
    }

    void ColumnType(IOperationPreparator operation, ColumnDescriptor column) {
        string pgtype = GetDBType(column.Type);
        if(column.AutoIncrement) {
            if(pgtype == "int4")
                operation.AppendText("serial4");
            else if(pgtype == "int8")
                operation.AppendText("serial8");
            else
                throw new InvalidOperationException("Autoincrement with postgre only allowed with integer types");
        }
        else {
            operation.AppendText(pgtype);
        }
    }

    void ColumnType(OperationPreparator operation, EntityColumnDescriptor column) {
        if(column.AutoIncrement) {
            if(column.Property.PropertyType == typeof(int) || column.Property.PropertyType == typeof(uint))
                operation.AppendText("serial4");
            else if(column.Property.PropertyType == typeof(long) || column.Property.PropertyType == typeof(ulong))
                operation.AppendText("serial8");
            else
                throw new InvalidOperationException("Autoincrement with postgre only allowed with integer types");
        }
        else {
            operation.AppendText(GetDBType(column.Property.PropertyType));
        }
    }

    string ColumnAttributes(ColumnDescriptor column) {
        StringBuilder builder = new();
        if(column.PrimaryKey)
            builder.Append(" PRIMARY KEY");
        if(column.IsUnique)
            builder.Append(" UNIQUE");
        if(column.NotNull)
            builder.Append(" NOT NULL");

        if(column.DefaultValue != null) {
            builder.Append(" DEFAULT ");
            builder.Append('\'').Append(Converter.Convert<string>(column.DefaultValue)).Append('\'');
        }

        return builder.ToString();
    }

    /// <inheritdoc />
    public override async Task<SchemaType> GetSchemaTypeAsync(IDBClient client, string name, Transaction transaction = null) {
        long count = await new LoadOperation<PgView>(client, EntityDescriptor.Create, DB.Count()).Where(p => p.Name == name).ExecuteScalarAsync<long>(transaction);
        if (count>0)
            return SchemaType.View;

        count = await new LoadOperation<PgColumn>(client, EntityDescriptor.Create, DB.Count()).Where(c => c.Table == name).ExecuteScalarAsync<long>(transaction);
        if (count > 0)
            return SchemaType.Table;
            

        throw new SchemaException($"Schema '{name}' not found in database");
    }

    /// <inheritdoc />
    public override void AddColumn(OperationPreparator preparator, ColumnDescriptor column) {
        preparator.AppendText("ADD COLUMN");
        CreateColumn(preparator, column);
    }

    /// <inheritdoc />
    public override void DropColumn(OperationPreparator preparator, string column) {
        preparator.AppendText("DROP COLUMN").AppendText($"\"{column}\"").AppendText("CASCADE");
    }

    /// <inheritdoc />
    public override void AlterColumn(OperationPreparator preparator, ColumnDescriptor column) {
        preparator.AppendText("ALTER COLUMN");
        preparator.AppendText($"\"{column.Name}\"");
        preparator.AppendText("TYPE");
        ColumnType(preparator, column);
    }

    /// <inheritdoc />
    public override void ReturnID(OperationPreparator preparator, ColumnDescriptor idcolumn) {
        preparator.AppendText($"RETURNING {MaskColumn(idcolumn.Name)}");
    }

    /// <inheritdoc />
    public override bool MustRecreateTable(string[] obsolete, EntityColumnDescriptor[] altered, EntityColumnDescriptor[] missing, TableDescriptor tableschema, EntityDescriptor entityschema) {
        return false;
    }

    /// <inheritdoc />
    public override bool MustRecreateTable(string[] obsolete, ColumnDescriptor[] altered, ColumnDescriptor[] missing, TableSchema tableschema, TableSchema entityschema) {
        return false;
    }

    /// <inheritdoc />
    public override async Task<string> GenerateCreateStatement(IDBClient client, string table) {
        string template;
        Stream templateStream = typeof(PostgreInfo).Assembly.GetManifestResourceStream("NightlyCode.Ocelot.Info.Postgre.createstatement.sql");
        if (templateStream == null)
            throw new InvalidOperationException("Statement template resource not found");
            
        using (StreamReader reader = new(templateStream))
            template = await reader.ReadToEndAsync();

        string statement = string.Format(template, table);

        string createStatement = Converter.Convert<string>(await client.ScalarAsync(statement));
        return createStatement.ProcessCreateStatement();
    }

    /// <inheritdoc />
    public override SchemaDescriptor GetSchema(IDBClient client, string name) {
        PgView view = new LoadOperation<PgView>(client, EntityDescriptor.Create, DB.All).Where(p => p.Name == name).ExecuteEntity();
        if(view != null)
            return new ViewDescriptor(name) {
                                                SQL = view.Definition
                                            };

        Dictionary<string, ColumnDescriptor> columns = new();
        foreach(PgColumn column in new LoadOperation<PgColumn>(client, EntityDescriptor.Create, DB.All).Where(c => c.Table == name).ExecuteEntities()) {
            columns[column.Column] = new(column.Column) {
                                                            Type = column.DataType,
                                                            NotNull = column.IsNullable == "NO",
                                                            AutoIncrement = column.Default?.StartsWith("nextval") ?? false
                                                        };
        }

        List<UniqueDescriptor> uniques = new();
        List<IndexDescriptor> indices = new();
        foreach(PgIndex index in new LoadOperation<PgIndex>(client, EntityDescriptor.Create, DB.All).Where(i => i.Table == name).ExecuteEntities()) {
            Match match = Regex.Match(index.Definition, "^CREATE (?<unique>UNIQUE )?INDEX (?<name>[^ ]+) ON (?<table>[^ ]+)( USING (?<type>[a-zA-Z]+))? \\((?<columns>.+)\\)");
            if(!match.Success)
                continue;

            string[] columnnames = match.Groups["columns"].Value.Split(',').Select(c => c.Trim()).ToArray();
            if(Regex.IsMatch(index.Name, "_pkey[0-9]*$")) {
                foreach(string column in columnnames) {
                    if(columns.TryGetValue(column, out ColumnDescriptor desc))
                        desc.PrimaryKey = true;
                }
            }
            else if(match.Groups["unique"].Success) {
                if(columnnames.Length == 1) {
                    if(columns.TryGetValue(columnnames[0], out ColumnDescriptor desc))
                        desc.IsUnique = true;
                }
                else
                    uniques.Add(new(index.Name, columnnames));
            }
            else {
                match = Regex.Match(index.Name, $"^idx_{name}_(?<indexname>.+)$");
                if(match.Success)
                    indices.Add(new(match.Groups["indexname"].Value, columnnames, match.Groups["type"].Value));
                else
                    indices.Add(new(index.Name, columnnames, match.Groups["type"].Value));
            }
        }

        TableDescriptor descriptor = new(name) {
                                                   Columns = columns.Values.ToArray(),
                                                   Uniques = uniques.ToArray(),
                                                   Indices = indices.ToArray()
                                               };

        return descriptor;
    }

    /// <inheritdoc />
    public override async Task<Schema> GetSchemaAsync(IDBClient client, string name, Transaction transaction=null) {
        PgView view = await new LoadOperation<PgView>(client, EntityDescriptor.Create, DB.All).Where(p => p.Name == name).ExecuteEntityAsync();
        if(view != null)
            return new ViewSchema {
                                      Name = name,
                                      Definition = view.Definition
                                  };

        Dictionary<string, ColumnDescriptor> columns = new();
        await foreach(PgColumn column in new LoadOperation<PgColumn>(client, EntityDescriptor.Create, DB.All).Where(c => c.Table == name).ExecuteEntitiesAsync()) {
            columns[column.Column] = new(column.Column) {
                                                            Type = column.DataType,
                                                            NotNull = column.IsNullable == "NO",
                                                            AutoIncrement = column.Default?.StartsWith("nextval") ?? false
                                                        };
        }

        List<UniqueDescriptor> uniques = [];
        List<IndexDescriptor> indices = [];
        await foreach(PgIndex index in new LoadOperation<PgIndex>(client, EntityDescriptor.Create, DB.All).Where(i => i.Table == name).ExecuteEntitiesAsync()) {
            Match match = Regex.Match(index.Definition, "^CREATE (?<unique>UNIQUE )?INDEX (?<name>[^ ]+) ON (?<table>[^ ]+)( USING (?<type>[a-zA-Z]+))? \\((?<columns>.+)\\)");
            if(!match.Success)
                continue;

            string[] columnnames = match.Groups["columns"].Value.Split(',').Select(c => c.Trim()).ToArray();
            if(Regex.IsMatch(index.Name, "_pkey[0-9]*$")) {
                foreach(string column in columnnames) {
                    if(columns.TryGetValue(column, out ColumnDescriptor desc))
                        desc.PrimaryKey = true;
                }
            }
            else if(match.Groups["unique"].Success) {
                if(columnnames.Length == 1) {
                    if(columns.TryGetValue(columnnames[0], out ColumnDescriptor desc))
                        desc.IsUnique = true;
                }
                else
                    uniques.Add(new(index.Name, columnnames));
            }
            else {
                match = Regex.Match(index.Name, $"^idx_{name}_(?<indexname>.+)$");
                if(match.Success)
                    indices.Add(new(match.Groups["indexname"].Value, columnnames, match.Groups["type"].Value));
                else
                    indices.Add(new(index.Name, columnnames, match.Groups["type"].Value));
            }
        }

        return new TableSchema {
                                   Name = name,
                                   Columns = columns.Values.ToArray(),
                                   Unique = uniques.ToArray(),
                                   Index = indices.ToArray()
                               };
    }

    /// <inheritdoc />
    public override Task Truncate(IDBClient client, string table, TruncateOptions options = null) {
        if (options?.ResetIdentity ?? false)
            return client.NonQueryAsync(options.Transaction, $"TRUNCATE {MaskColumn(table)} RESTART IDENTITY");
        return client.NonQueryAsync(options?.Transaction,$"TRUNCATE {MaskColumn(table)}");
    }
        
    /// <inheritdoc />
    public override void CreateParameter(IDbCommand command, object parameterValue) {
        IDbDataParameter parameter = command.CreateParameter();
        parameter.ParameterName = Parameter + (command.Parameters.Count + 1);
        if (parameterValue == null || parameterValue == DBNull.Value)
            parameter.Value = DBNull.Value;
        else if (parameterValue is Range<BigInteger> numericRange)
            parameter.Value = Activator.CreateInstance(bigIntRangeType, (decimal)numericRange.Lower, numericRange.LowerInclusive, (decimal)numericRange.Upper, numericRange.UpperInclusive);
        else if(parameterValue is Range<int> intRange)
            parameter.Value = Activator.CreateInstance(intRangeType, intRange.Lower, intRange.LowerInclusive, intRange.Upper, intRange.UpperInclusive);
        else if(parameterValue is Range<long> longRange)
            parameter.Value = Activator.CreateInstance(longRangeType, longRange.Lower, longRange.LowerInclusive, longRange.Upper, longRange.UpperInclusive);
        else if(parameterValue is Range<DateTime> dateRange)
            parameter.Value = Activator.CreateInstance(dateRangeType, dateRange.Lower, dateRange.LowerInclusive, dateRange.Upper, dateRange.UpperInclusive);
        else if(parameterValue is Array array) {
            Type elementType = array.GetType().GetElementType();
            Type dbType = GetDBRepresentation(elementType);
            if (dbType != elementType) {
                Array convertedArray = Array.CreateInstance(dbType, array.Length);
                for (int i = 0; i < array.Length; ++i)
                    convertedArray.SetValue(Converter.Convert(array.GetValue(i), dbType), i);
                    
                parameter.Value = convertedArray;
            }
            else parameter.Value =Converter.Convert(parameterValue, GetDBRepresentation(parameterValue.GetType()));
        }
        else parameter.Value = Converter.Convert(parameterValue, GetDBRepresentation(parameterValue.GetType())); 

        command.Parameters.Add(parameter);
    }

    /// <inheritdoc />
    public override void CreateInFragment(Expression lhs, Expression rhs, IOperationPreparator preparator, Func<Expression, Expression> visitor) {
        visitor(lhs);
        preparator.AppendText("= ANY(");
        visitor(rhs);
        preparator.AppendText(")");
    }

    /// <inheritdoc />
    public override void CreateRangeContainsFragment(Expression lhs, Expression rhs, IOperationPreparator preparator, Func<Expression, Expression> visitor) {
        visitor(lhs);
        preparator.AppendText("@>");
        visitor(rhs);
    }

    /// <inheritdoc />
    public override void CreateMatchFragment(Expression value, Expression pattern, IOperationPreparator preparator, Func<Expression, Expression> visitor) {
        visitor(value);
        preparator.AppendText("~");
        visitor(pattern);
    }

    /// <inheritdoc />
    public override object GenerateDefault(string type) {
        switch(type.ToLower()) {
            case "int8":
            case "serial8":
            case "bigint":
            case "bigserial":
                return 0L;
            case "float4":
            case "real":
                return 0.0f;
            case "float8":
            case "double":
            case "double precision":
                return 0.0;
            case "decimal":
            case "numeric":
                return 0.0m;
            case "int4":
            case "serial4":
            case "int":
            case "integer":
            case "serial":
                return 0;
            case "int2":
                return (short)0;
            case "timestamp":
            case "timestamp without time zone":
            case "timestamp with time zone":
                return new DateTime(1970, 01, 01);
            case "time":
            case "time with time zone":
            case "time without time zone":
                return new TimeSpan(0);
            case "character varying":
                return "";
            case "boolean":
                return false;
            case "bytea":
                return Array.Empty<byte>();
            default:
                return 0;
        }
    }

    // TODO: deactivated for now, leads to bugs
    /// <inheritdoc />
    public override bool PreparationSupported => false;

    /// <inheritdoc />
    public override object ValueFromReader(Reader reader, int ordinal, Type type) {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Range<>)) {
            object value;
            if (type == typeof(Range<BigInteger>)) {
                value = reader.GetValue(ordinal, decimalRangeType);
                return new Range<BigInteger>(new((decimal)lowerDecimal.GetValue(value)),
                                             new((decimal)upperDecimal.GetValue(value))) {
                                                                                             LowerInclusive = (bool)lowerInclusiveDecimal.GetValue(value),
                                                                                             UpperInclusive = (bool)upperInclusiveDecimal.GetValue(value)
                                                                                         };
            }

            if (type == typeof(Range<int>)) {
                value = reader.GetValue(ordinal);
                return new Range<int>((int)lowerInt.GetValue(value),
                                      (int)upperInt.GetValue(value)) {
                                                                         LowerInclusive = (bool)lowerInclusiveInt.GetValue(value),
                                                                         UpperInclusive = (bool)upperInclusiveInt.GetValue(value)
                                                                     };
            }

            if (type == typeof(Range<long>)) {
                value = reader.GetValue(ordinal);
                return new Range<long>((long)lowerLong.GetValue(value),
                                       (long)upperLong.GetValue(value)) {
                                                                            LowerInclusive = (bool)lowerInclusiveLong.GetValue(value),
                                                                            UpperInclusive = (bool)upperInclusiveLong.GetValue(value)
                                                                        };
            }

            if (type == typeof(Range<DateTime>)) {
                value = reader.GetValue(ordinal);
                return new Range<DateTime>((DateTime)lowerDate.GetValue(value),
                                           (DateTime)upperDate.GetValue(value)) {
                                                                                    LowerInclusive = (bool)lowerInclusiveDate.GetValue(value),
                                                                                    UpperInclusive = (bool)upperInclusiveDate.GetValue(value)
                                                                                };
            }
        }
        return base.ValueFromReader(reader, ordinal, type);
    }

    /// <inheritdoc />
    public override async Task<object> ValueFromReaderAsync(Reader reader, int ordinal, Type type) {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Range<>)) {
            object value;
            if (type == typeof(Range<BigInteger>)) {
                value = reader.GetValue(ordinal, decimalRangeType);
                return new Range<BigInteger>(new((decimal)lowerDecimal.GetValue(value)),
                                             new((decimal)upperDecimal.GetValue(value))) {
                                                                                             LowerInclusive = (bool)lowerInclusiveDecimal.GetValue(value),
                                                                                             UpperInclusive = (bool)upperInclusiveDecimal.GetValue(value)
                                                                                         };
            }

            if (type == typeof(Range<int>)) {
                value = reader.GetValue(ordinal);
                return new Range<int>((int)lowerInt.GetValue(value),
                                      (int)upperInt.GetValue(value)) {
                                                                         LowerInclusive = (bool)lowerInclusiveInt.GetValue(value),
                                                                         UpperInclusive = (bool)upperInclusiveInt.GetValue(value)
                                                                     };
            }

            if (type == typeof(Range<long>)) {
                value = reader.GetValue(ordinal);
                return new Range<long>((long)lowerLong.GetValue(value),
                                       (long)upperLong.GetValue(value)) {
                                                                            LowerInclusive = (bool)lowerInclusiveLong.GetValue(value),
                                                                            UpperInclusive = (bool)upperInclusiveLong.GetValue(value)
                                                                        };
            }

            if (type == typeof(Range<DateTime>)) {
                value = reader.GetValue(ordinal);
                return new Range<DateTime>((DateTime)lowerDate.GetValue(value),
                                           (DateTime)upperDate.GetValue(value)) {
                                                                                    LowerInclusive = (bool)lowerInclusiveDate.GetValue(value),
                                                                                    UpperInclusive = (bool)upperInclusiveDate.GetValue(value)
                                                                                };
            }
        }

        return await base.ValueFromReaderAsync(reader, ordinal, type);
    }

    /// <inheritdoc />
    public override void CreateIndexTypeFragment(StringBuilder commandBuilder, string type) {
        if (string.IsNullOrEmpty(type))
            return;
        commandBuilder.Append(" USING ").Append(type);
    }
}