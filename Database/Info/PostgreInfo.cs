using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations;
using NightlyCode.Database.Entities.Operations.Expressions;
using NightlyCode.Database.Entities.Operations.Fields;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Entities.Schema;
using NightlyCode.Database.Fields;
using NightlyCode.Database.Info.Postgre;
using NightlyCode.Database.Tokens;
using Converter = NightlyCode.Database.Extern.Converter;

namespace NightlyCode.Database.Info {

    /// <summary>
    /// database specific logic for postgre databases
    /// </summary>
    public class PostgreInfo : DBInfo {

        /// <summary>
        /// creates a new <see cref="PostgreInfo"/>
        /// </summary>
        public PostgreInfo() {
            AddFieldLogic<DBFunction>(AppendFunction);
            AddFieldLogic<LimitField>(AppendLimit);
        }

        void AppendLimit(LimitField limit, IOperationPreparator preparator, Func<Type, EntityDescriptor> descriptorgetter, string tablealias) {

            if(limit.Limit.HasValue)
                preparator.AppendText("LIMIT").AppendText(limit.Limit.Value.ToString());
            if(limit.Offset.HasValue)
                preparator.AppendText("OFFSET").AppendText(limit.Offset.Value.ToString());
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
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void ToLower(ExpressionVisitor visitor, IOperationPreparator preparator, Expression value) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// command used to check whether a table exists
        /// </summary>
        /// <param name="db">dbclient to use to check for table existence</param>
        /// <param name="table">table name to check for</param>
        /// <param name="transaction">transaction to use</param>
        /// <returns></returns>
        public override bool CheckIfTableExists(IDBClient db, string table, Transaction transaction = null) {
            return Converter.Convert<long>(db.Scalar(transaction, "SELECT count(*) FROM pg_class WHERE relname = @1", table)) > 0;
        }

        /// <inheritdoc />
        public override bool IsTypeEqual(string lhs, string rhs) {
            switch(lhs) {
            case "int8":
            case "serial8":
            case "bigint":
            case "bigserial":
                return rhs == "int8" || rhs == "serial8" || rhs == "bigint" || rhs == "bigserial";
            case "float4":
            case "real":
                return rhs == "float4" || rhs == "real";
            case "float8":
            case "double":
            case "double precision":
                return rhs == "float8" || rhs == "double" || rhs == "double precision";
            case "decimal":
            case "numeric":
                return rhs == "decimal" || rhs == "numeric";
            case "int4":
            case "serial4":
            case "int":
            case "integer":
            case "serial":
                return rhs == "int4" || rhs == "serial4" || rhs == "int" || rhs == "integer" || rhs == "serial";
            case "timestamp":
            case "timestamp without time zone":
            case "timestamp with time zone":
                return rhs.StartsWith("timestamp");
            case "time":
            case "time with time zone":
            case "time without time zone":
                return rhs == "time" || rhs == "time with time zone" || rhs == "time without time zone";
            }

            return base.IsTypeEqual(lhs, rhs);
        }

        /// <summary>
        /// get db type of an application type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public override string GetDBType(Type type) {
            if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = Nullable.GetUnderlyingType(type);

            if(type.IsEnum)
                return "integer";

            switch(type.Name.ToLower()) {
            case "datetime":
                return "timestamp";
            case "guid":
            case "string":
            case "version":
                return "character varying";
            case "int":
            case "uint32":
            case "int32":
                return "int4";
            case "long":
            case "uint64":
            case "int64":
                return "int8";
            case "byte":
            case "short":
            case "uint16":
            case "int16":
                return "int2";
            case "single":
            case "float":
                return "float4";
            case "double":
                return "float8";
            case "bool":
            case "boolean":
                return "boolean";
            case "byte[]":
                return "bytea";
            case "timespan":
                return "int8";
            case "decimal":
                return "decimal";
            default:
                throw new InvalidOperationException($"unsupported type '{type.Name}'");
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
        public override void CreateColumn(OperationPreparator operation, SchemaColumnDescriptor column) {
            operation.AppendText($"\"{column.Name}\" ");

            ColumnType(operation, column);
            operation.AppendText(ColumnAttributes(column));
        }

        void ColumnType(OperationPreparator operation, SchemaColumnDescriptor column) {
            if(column.AutoIncrement) {
                if(column.Type == "int4")
                    operation.AppendText("serial4");
                else if(column.Type == "int8")
                    operation.AppendText("serial8");
                else
                    throw new InvalidOperationException("Autoincrement with postgre only allowed with integer types");
            }
            else {
                operation.AppendText(column.Type);
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
                if(DBConverterCollection.ContainsConverter(column.Property.PropertyType))
                    operation.AppendText(GetDBType(DBConverterCollection.GetDBType(column.Property.PropertyType)));
                else
                    operation.AppendText(GetDBType(column.Property.PropertyType));
            }
        }

        string ColumnAttributes(ColumnDescriptor column) {
            StringBuilder builder = new StringBuilder();
            if(column.PrimaryKey)
                builder.Append(" PRIMARY KEY");
            if(column.IsUnique)
                builder.Append(" UNIQUE");
            if(column.NotNull)
                builder.Append(" NOT NULL");

            if(column.DefaultValue != null) {
                builder.Append(" DEFAULT ");
                builder.Append('\'').Append(column.DefaultValue).Append('\'');
            }

            return builder.ToString();
        }

        /// <inheritdoc />
        public override void AddColumn(OperationPreparator preparator, EntityColumnDescriptor column) {
            preparator.AppendText("ADD COLUMN");
            CreateColumn(preparator, column);
        }

        /// <inheritdoc />
        public override void DropColumn(OperationPreparator preparator, string column) {
            preparator.AppendText("DROP COLUMN").AppendText($"\"{column}\"").AppendText("CASCADE");
        }

        /// <inheritdoc />
        public override void AlterColumn(OperationPreparator preparator, EntityColumnDescriptor column) {
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
        public override SchemaDescriptor GetSchema(IDBClient client, string name) {
            PgView view = new LoadOperation<PgView>(client, EntityDescriptor.Create, DB.All).Where(p => p.Name == name).ExecuteEntity();
            if(view != null)
                return new ViewDescriptor(name) {
                    SQL = view.Definition
                };

            Dictionary<string, SchemaColumnDescriptor> columns = new Dictionary<string, SchemaColumnDescriptor>();
            foreach(PgColumn column in new LoadOperation<PgColumn>(client, EntityDescriptor.Create, DB.All).Where(c => c.Table == name).ExecuteEntities()) {
                columns[column.Column] = new SchemaColumnDescriptor(column.Column) {
                    Type = column.DataType,
                    NotNull = column.IsNullable == "NO",
                    AutoIncrement = column.Default?.StartsWith("nextval") ?? false
                };
            }

            List<UniqueDescriptor> uniques = new List<UniqueDescriptor>();
            List<IndexDescriptor> indices = new List<IndexDescriptor>();
            foreach(PgIndex index in new LoadOperation<PgIndex>(client, EntityDescriptor.Create, DB.All).Where(i => i.Table == name).ExecuteEntities()) {
                Match match = Regex.Match(index.Definition, "^CREATE (?<unique>UNIQUE )?INDEX (?<name>[^ ]+) ON (?<table>[^ ]+)( USING [a-zA-Z]+)? \\((?<columns>.+)\\)");
                if(!match.Success)
                    continue;

                string[] columnnames = match.Groups["columns"].Value.Split(',').Select(c => c.Trim()).ToArray();
                if(Regex.IsMatch(index.Name, "_pkey[0-9]*$")) {
                    foreach(string column in columnnames) {
                        if(columns.TryGetValue(column, out SchemaColumnDescriptor desc))
                            desc.PrimaryKey = true;
                    }
                }
                else if(match.Groups["unique"].Success) {
                    if(columnnames.Length == 1) {
                        if(columns.TryGetValue(columnnames[0], out SchemaColumnDescriptor desc))
                            desc.IsUnique = true;
                    }
                    else
                        uniques.Add(new UniqueDescriptor(columnnames));
                }
                else {
                    match = Regex.Match(index.Name, $"^idx_{name}_(?<indexname>.+)$");
                    if(match.Success)
                        indices.Add(new IndexDescriptor(match.Groups["indexname"].Value, columnnames));
                    else
                        indices.Add(new IndexDescriptor(index.Name, columnnames));
                }
            }

            TableDescriptor descriptor = new TableDescriptor(name) {
                Columns = columns.Values.ToArray(),
                Uniques = uniques.ToArray(),
                Indices = indices.ToArray()
            };

            return descriptor;
        }
    }
}