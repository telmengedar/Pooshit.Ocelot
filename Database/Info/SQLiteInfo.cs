using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Expressions;
using NightlyCode.Database.Entities.Operations.Fields;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Entities.Schema;
using Converter = NightlyCode.Database.Extern.Converter;

namespace NightlyCode.Database.Info {

    /// <summary>
    /// information for sqlite
    /// </summary>
    public class SQLiteInfo : DBInfo {

        /// <summary>
        /// creates a new <see cref="SQLiteInfo"/>
        /// </summary>
        public SQLiteInfo() {
            AddFieldLogic<DBFunction>(Append);
            AddFieldLogic<LimitField>(AppendLimit);
        }

        void AppendLimit(LimitField limit, OperationPreparator preparator, Func<Type, EntityDescriptor> descriptorgetter, string tablealias) {
            preparator.AppendText("LIMIT");
            if(limit.Offset.HasValue) {
                preparator.AppendText(limit.Offset.Value.ToString()).AppendText(",");
                if(limit.Limit.HasValue)
                    preparator.AppendText(limit.Limit.Value.ToString());
                else
                    preparator.AppendText("-1");
            }
            // ReSharper disable once PossibleInvalidOperationException
            else
                preparator.AppendText(limit.Limit.Value.ToString());
        }

        void Append(DBFunction function, OperationPreparator preparator, Func<Type, EntityDescriptor> descriptorgetter, string tablealias) {
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
        public override string LikeTerm => "LIKE";

        /// <summary>
        /// method used to create a replace function
        /// </summary>
        /// <param name="preparator"> </param>
        /// <param name="value"></param>
        /// <param name="src"></param>
        /// <param name="target"></param>
        /// <param name="visitor"> </param>
        /// <returns></returns>
        public override void Replace(ExpressionVisitor visitor, OperationPreparator preparator, Expression value, Expression src, Expression target) {
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
        public override void ToUpper(ExpressionVisitor visitor, OperationPreparator preparator, Expression value) {
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
        public override void ToLower(ExpressionVisitor visitor, OperationPreparator preparator, Expression value) {
            preparator.AppendText("lower(");
            visitor.Visit(value);
            preparator.AppendText(")");
        }

        /// <summary>
        /// command used to check whether a table exists
        /// </summary>
        /// <param name="db"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public override bool CheckIfTableExists(IDBClient db, string table, Transaction transaction = null) {
            return db.Query(transaction, "SELECT name FROM sqlite_master WHERE (type='table' OR type='view') AND name like @1", table).Rows.Length > 0;
        }

        /// <inheritdoc />
        public override bool IsTypeEqual(string lhs, string rhs) {
            if(lhs == "TEXT" || lhs == "VARCHAR")
                return rhs == "TEXT" || rhs == "VARCHAR";
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
                return "INTEGER";

            switch(type.Name.ToLower()) {
            case "datetime":
                return "TIMESTAMP";
            case "guid":
                return "TEXT";
            case "string":
                return "TEXT";
            case "version":
                return "INTEGER";
            case "byte":
            case "int":
            case "uint32":
            case "int32":
            case "long":
            case "uint64":
            case "int64":
            case "short":
            case "uint16":
            case "int16":
                return "INTEGER";
            case "single":
            case "float":
            case "double":
                return "FLOAT";
            case "bool":
            case "boolean":
                return "BOOLEAN";
            case "byte[]":
                return "BLOB";
            case "timespan":
                return "INTEGER";
            case "decimal":
                return "DECIMAL";
            default:
                throw new InvalidOperationException("unsupported type");
            }
        }

        /// <summary>
        /// get db representation type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public override Type GetDBRepresentation(Type type) {
            // sqlite understands datetime but not timespan
            if(type == typeof(TimeSpan))
                return typeof(long);
            if(type == typeof(Version))
                return typeof(long);
            if(type == typeof(Guid))
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
                GetDBType(DBConverterCollection.ContainsConverter(column.Property.PropertyType) ? DBConverterCollection.GetDBType(column.Property.PropertyType) : column.Property.PropertyType),
                column.PrimaryKey,
                column.AutoIncrement,
                column.IsUnique,
                column.NotNull,
                column.DefaultValue
                );
        }

        /// <inheritdoc />
        public override void CreateColumn(OperationPreparator operation, SchemaColumnDescriptor column) {
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
                if(defaultvalue is string || defaultvalue is Guid || defaultvalue is DateTime || defaultvalue is TimeSpan)
                    operation.AppendText($"'{defaultvalue}'");
                else
                    operation.AppendText(defaultvalue.ToString());
            }
        }

        /// <inheritdoc />
        public override void AddColumn(OperationPreparator operation, EntityColumnDescriptor column) {
            operation.AppendText("ADD COLUMN");
            AddColumn(operation,
                column.Name,
                GetDBType(DBConverterCollection.ContainsConverter(column.Property.PropertyType) ? DBConverterCollection.GetDBType(column.Property.PropertyType) : column.Property.PropertyType),
                column.PrimaryKey,
                column.AutoIncrement,
                column.IsUnique,
                column.NotNull,
                column.DefaultValue,
                column.Property.PropertyType
            );
        }

        void AddColumn(OperationPreparator operation, string name, string type, bool primarykey, bool autoincrement, bool unique, bool notnull, object defaultvalue, Type columntype) {
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
                if(defaultvalue == null)
                    defaultvalue = Activator.CreateInstance(columntype);
            }

            if(defaultvalue != null) {
                operation.AppendText("DEFAULT");
                if(defaultvalue is string || defaultvalue is Guid || defaultvalue is DateTime || defaultvalue is TimeSpan)
                    operation.AppendText($"'{defaultvalue}'");
                else
                    operation.AppendText(defaultvalue.ToString());
            }
        }

        /// <summary>
        /// get schema for a table in database
        /// </summary>
        /// <param name="client">database connection</param>
        /// <param name="tablename">name of table of which to get schema</param>
        /// <returns><see cref="SchemaDescriptor"/> containing all information about table</returns>
        public override SchemaDescriptor GetSchema(IDBClient client, string tablename) {
            Clients.Tables.DataTable table = client.Query("SELECT * FROM sqlite_master WHERE (name=@1)", tablename);

            if(table.Rows.Length == 0)
                throw new InvalidOperationException("Type not found in database");

            if(Converter.Convert<string>(table.Rows[0]["type"]) == "table") {
                TableDescriptor tabledescriptor = new TableDescriptor(Converter.Convert<string>(table.Rows[0]["tbl_name"]));
                AnalyseTableSql(tabledescriptor, Converter.Convert<string>(table.Rows[0]["sql"]));

                Clients.Tables.DataTable indices = client.Query("SELECT * FROM sqlite_master WHERE type='index' AND tbl_name=@1", tablename);
                tabledescriptor.Indices = AnalyseIndexDefinitions(indices).ToArray();
                return tabledescriptor;
            }
            if(Converter.Convert<string>(table.Rows[0]["type"]) == "view") {
                ViewDescriptor viewdesc = new ViewDescriptor(Converter.Convert<string>(table.Rows[0]["tbl_name"])) {
                    SQL = Converter.Convert<string>(table.Rows[0]["sql"])
                };
                return viewdesc;
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
                    yield return new IndexDescriptor(match.Groups["name"].Value, match.Groups["columns"].Value.Split(',').Select(c => c.Trim(' ', '\"')));
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
            OperationPreparator operation = new OperationPreparator();
            operation.AppendText($"ALTER TABLE {table} ADD COLUMN");
            AddColumn(operation, column);
            if(transaction != null)
                operation.GetOperation(client).Execute(transaction);
            else
                operation.GetOperation(client).Execute();
        }

        /// <inheritdoc />
        public override void DropColumn(OperationPreparator preparator, string column) {
            throw new NotSupportedException("SQLite is not able to drop columns directly");
        }

        /// <inheritdoc />
        public override void AlterColumn(OperationPreparator preparator, EntityColumnDescriptor column) {
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

        IEnumerable<UniqueDescriptor> AnalyseUniques(IEnumerable<string> definitions) {
            foreach(string definition in definitions) {
                string[] columns = definition.Trim().Split(',').Select(s => s.Trim(' ', '\'', '[', ']')).ToArray();

                yield return new UniqueDescriptor(columns);
            }
        }

        IEnumerable<string> GetDefinitions(IEnumerable<string> columns) {
            foreach(string column in columns) {
                yield return column;
            }
        }

        SchemaColumnDescriptor GetColumnDescriptor(string sql) {
            Match match = Regex.Match(sql, @"^['\[]?(?<name>[^ '\]]+)['\]]?\s+(?<type>[^ ]+)(?<pk> PRIMARY KEY)?(?<ai> AUTOINCREMENT)?(?<uq> UNIQUE)?(?<nn> NOT NULL)?( DEFAULT '?(?<default>.+)'?)?$");
            if(!match.Success)
                throw new InvalidOperationException("Error analysing column description sql");

            SchemaColumnDescriptor descriptor = new SchemaColumnDescriptor(match.Groups["name"].Value) {
                Type = match.Groups["type"].Value,
                AutoIncrement = match.Groups["ai"].Success,
                PrimaryKey = match.Groups["pk"].Success,
                NotNull = match.Groups["nn"].Success,
                DefaultValue = match.Groups["default"].Value,
                IsUnique = match.Groups["uq"].Success
            };
            return descriptor;
        }
    }
}
