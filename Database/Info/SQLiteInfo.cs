using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations;
using NightlyCode.Database.Entities.Operations.Expressions;
using NightlyCode.Database.Entities.Operations.Fields;
using NightlyCode.Database.Entities.Schema;
using Converter = NightlyCode.Database.Extern.Converter;

namespace NightlyCode.Database.Info
{

    /// <summary>
    /// information for sqlite
    /// </summary>
    public class SQLiteInfo : DBInfo
    {

        /// <summary>
        /// creates a new <see cref="SQLiteInfo"/>
        /// </summary>
        public SQLiteInfo() {
            AddFieldLogic<DBFunction>(Append);
            AddFieldLogic<LimitField>(AppendLimit);
        }

        void AppendLimit(LimitField limit, OperationPreparator preparator, Func<Type, EntityDescriptor> descriptorgetter) {
            preparator.CommandBuilder.Append("LIMIT ");
            if (limit.Offset.HasValue) {
                preparator.CommandBuilder.Append(limit.Offset.Value).Append(",");
                if (limit.Limit.HasValue)
                    preparator.CommandBuilder.Append(limit.Limit.Value);
                else preparator.CommandBuilder.Append("-1");
            }
            // ReSharper disable once PossibleInvalidOperationException
            else preparator.CommandBuilder.Append(limit.Limit.Value);
        }

        void Append(DBFunction function, OperationPreparator preparator, Func<Type, EntityDescriptor> descriptorgetter)
        {
            switch (function.Type)
            {
            case DBFunctionType.Random:
                preparator.CommandBuilder.Append("RANDOM()");
                break;
            case DBFunctionType.Count:
                preparator.CommandBuilder.Append("COUNT(*)");
                break;
            case DBFunctionType.RowID:
                preparator.CommandBuilder.Append("ROWID");
                break;
            case DBFunctionType.Length:
                preparator.CommandBuilder.Append("LENGTH(");
                CriteriaVisitor.GetCriteriaText(function.Parameter, descriptorgetter, this, preparator);
                preparator.CommandBuilder.Append(")");
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
        /// parameter used to create autoincrement columns
        /// </summary>
        public override string AutoIncrement => "AUTOINCREMENT";

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
            preparator.CommandBuilder.Append("replace(");
            visitor.Visit(value);
            preparator.CommandBuilder.Append(",");
            visitor.Visit(src);
            preparator.CommandBuilder.Append(",");
            visitor.Visit(target);
            preparator.CommandBuilder.Append(")");
        }

        /// <summary>
        /// converts an expression to uppercase using database command
        /// </summary>
        /// <param name="visitor"></param>
        /// <param name="preparator"></param>
        /// <param name="value"></param>
        public override void ToUpper(ExpressionVisitor visitor, OperationPreparator preparator, Expression value) {
            preparator.CommandBuilder.Append("upper(");
            visitor.Visit(value);
            preparator.CommandBuilder.Append(")");
        }

        /// <summary>
        /// converts an expression to lowercase using database command
        /// </summary>
        /// <param name="visitor"></param>
        /// <param name="preparator"></param>
        /// <param name="value"></param>
        public override void ToLower(ExpressionVisitor visitor, OperationPreparator preparator, Expression value)
        {
            preparator.CommandBuilder.Append("lower(");
            visitor.Visit(value);
            preparator.CommandBuilder.Append(")");
        }

        /// <summary>
        /// command used to check whether a table exists
        /// </summary>
        /// <param name="db"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public override bool CheckIfTableExists(IDBClient db, string table)
        {
            return db.Query("SELECT name FROM sqlite_master WHERE (type='table' OR type='view') AND name like @1", table).Rows.Length > 0;
        }

        /// <summary>
        /// determines whether db supports transactions
        /// </summary>
        public override bool TransactionHint => true;

        /// <summary>
        /// get db type of an application type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public override string GetDBType(Type type) {
            if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = Nullable.GetUnderlyingType(type);

            if(type.IsEnum) return "INTEGER";

            switch(type.Name.ToLower()) {
                case "datetime":
                    return "TIMESTAMP";
                case "guid":
                    return "BLOB";
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
            if (type == typeof(Guid))
                return typeof(byte[]);
            return type;
        }

        /// <summary>
        /// masks a column
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public override string MaskColumn(string column) {
            return $"'{column}'";
        }

        /// <summary>
        /// suffix to use when creating tables
        /// </summary>
        public override string CreateSuffix => null;

        /// <summary>
        /// text used to create a column
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="column"></param>
        /// <returns></returns>
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

        void CreateColumn(OperationPreparator operation, string name, string type, bool primarykey, bool autoincrement, bool unique, bool notnull, object defaultvalue) {
            operation.CommandBuilder.Append(MaskColumn(name)).Append(" ");
            operation.CommandBuilder.Append(type);

            if (primarykey)
                operation.CommandBuilder.Append(" PRIMARY KEY");
            if (autoincrement)
                operation.CommandBuilder.Append(" ").Append(AutoIncrement);
            if (unique)
                operation.CommandBuilder.Append(" UNIQUE");
            if(notnull) {
                operation.CommandBuilder.Append(" NOT NULL");
            }

            if(defaultvalue != null) {
                operation.CommandBuilder.Append(" DEFAULT ");
                if (defaultvalue is string || defaultvalue is Guid || defaultvalue is DateTime || defaultvalue is TimeSpan)
                    operation.CommandBuilder.Append($"'{defaultvalue}'");
                else operation.CommandBuilder.Append(defaultvalue);
            }
        }

        void AddColumn(OperationPreparator operation, EntityColumnDescriptor column) {
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

        void AddColumn(OperationPreparator operation, string name, string type, bool primarykey, bool autoincrement, bool unique, bool notnull, object defaultvalue, Type columntype)
        {
            operation.CommandBuilder.Append(MaskColumn(name)).Append(" ");
            operation.CommandBuilder.Append(type);

            if (primarykey)
                operation.CommandBuilder.Append(" PRIMARY KEY");
            if (autoincrement)
                operation.CommandBuilder.Append(" ").Append(AutoIncrement);
            if (unique)
                operation.CommandBuilder.Append(" UNIQUE");
            if (notnull)
            {
                operation.CommandBuilder.Append(" NOT NULL");

                // SQLite doesn't like no default values on nullable columns in add column case
                if(defaultvalue == null)
                    defaultvalue = Activator.CreateInstance(columntype);
            }

            if (defaultvalue != null)
            {
                operation.CommandBuilder.Append(" DEFAULT ");
                if (defaultvalue is string || defaultvalue is Guid || defaultvalue is DateTime || defaultvalue is TimeSpan)
                    operation.CommandBuilder.Append($"'{defaultvalue}'");
                else operation.CommandBuilder.Append(defaultvalue);
            }
        }

        /// <summary>
        /// changes creation command to creation command with return insert id statement
        /// </summary>
        /// <param name="insertcommand">insert command</param>
        /// <param name="client">db client used to execute commands</param>
        /// <param name="descriptor">descriptor of entity</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns></returns>
        public override object ReturnInsertID(IDBClient client, EntityDescriptor descriptor, string insertcommand, params object[] parameters) {
            client.NonQuery(insertcommand, parameters);
            return client.Scalar("SELECT last_insert_rowid()");
        }

        /// <summary>
        /// get schema for a table in database
        /// </summary>
        /// <param name="client">database connection</param>
        /// <param name="tablename">name of table of which to get schema</param>
        /// <returns><see cref="SchemaDescriptor"/> containing all information about table</returns>
        public override SchemaDescriptor GetSchema(IDBClient client, string tablename) {
            Clients.Tables.DataTable table = client.Query("SELECT * FROM sqlite_master WHERE (name=@1)", tablename);

            if (table.Rows.Length == 0)
                throw new InvalidOperationException("Type not found in database");

            if (Converter.Convert<string>(table.Rows[0]["type"]) == "table")
            {
                TableDescriptor tabledescriptor = new TableDescriptor(Converter.Convert<string>(table.Rows[0]["tbl_name"]));
                AnalyseTableSql(tabledescriptor, Converter.Convert<string>(table.Rows[0]["sql"]));

                Clients.Tables.DataTable indices = client.Query("SELECT * FROM sqlite_master WHERE type='index' AND tbl_name=@1", tablename);
                tabledescriptor.Indices = AnalyseIndexDefinitions(indices).ToArray();
                return tabledescriptor;
            }
            if (Converter.Convert<string>(table.Rows[0]["type"]) == "view")
            {
                ViewDescriptor viewdesc = new ViewDescriptor(Converter.Convert<string>(table.Rows[0]["tbl_name"]))
                {
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
        public override void AddColumn(IDBClient client, string table, EntityColumnDescriptor column, Transaction transaction=null) {
            OperationPreparator operation = new OperationPreparator(this);
            operation.CommandBuilder.Append($"ALTER TABLE {table} ADD COLUMN ");
            AddColumn(operation, column);
            if(transaction!=null)
                client.NonQuery(transaction, operation.CommandBuilder.ToString(), operation.Parameters.Select(p => p.Value).ToArray());
            else client.NonQuery(operation.CommandBuilder.ToString(), operation.Parameters.Select(p=>p.Value).ToArray());
        }

        /// <summary>
        /// removes a column from a table
        /// </summary>
        /// <param name="client">db access</param>
        /// <param name="table">table to modify</param>
        /// <param name="column">column to remove</param>
        /// <remarks>
        /// creates a new table and transfers the data since SQLite has no drop column command
        /// </remarks>
        public override void RemoveColumn(IDBClient client, string table, string column) {
            TableDescriptor schema = GetSchema(client, table) as TableDescriptor;
            if(schema == null)
                throw new Exception("Type not a table");

            // rename old table
            client.NonQuery($"ALTER TABLE {table} RENAME TO {table}_original");

            SchemaColumnDescriptor[] remainingcolumns = schema.Columns.Where(c => c.Name != column).ToArray();
            string columnlist = string.Join(", ", remainingcolumns.Select(c => c.Name));

            // create new table without the column
            bool flag=false;

            OperationPreparator preparator = new OperationPreparator(this);
            preparator.CommandBuilder.Append($"CREATE TABLE {table} (");
            foreach(SchemaColumnDescriptor columndesc in remainingcolumns)
            {
                if (flag)
                    preparator.CommandBuilder.Append(",");
                CreateColumn(preparator, columndesc.Name, columndesc.Type, columndesc.PrimaryKey, columndesc.AutoIncrement, columndesc.IsUnique, columndesc.NotNull, columndesc.DefaultValue);
                flag = true;
            }
            preparator.CommandBuilder.Append(")");
            client.NonQuery(preparator.CommandBuilder.ToString(), preparator.Parameters.Select(p => p.Value).ToArray());

            // transfer data to new table
            client.NonQuery($"INSERT INTO {table} ({columnlist}) SELECT {columnlist} FROM {table}_original");

            // remove old data
            client.NonQuery($"DROP TABLE {table}_original");
        }

        /// <summary>
        /// modifies a column of a table
        /// </summary>
        /// <param name="client">db access</param>
        /// <param name="table">table to modify</param>
        /// <param name="column">column to modify</param>
        /// <remarks>
        /// Removes the column and recreates it. So you will lose all your data in that column.
        /// </remarks>
        public override void AlterColumn(IDBClient client, string table, EntityColumnDescriptor column) {
            RemoveColumn(client, table, column.Name);
            AddColumn(client, table, column, null);
        }

        /// <summary>
        /// analyses the sql of a table creation and fills the table descriptor from the results
        /// </summary>
        /// <param name="descriptor">descriptor to fill</param>
        /// <param name="sql">sql to analyse</param>
        public void AnalyseTableSql(TableDescriptor descriptor, string sql) {
            Match match = Regex.Match(sql, @"^CREATE TABLE (?<name>[^ ]+) \((?<columns>.+?)(, UNIQUE \((?<unique>.+?)\))*\)$");
            if(!match.Success)
                throw new InvalidOperationException("Unable to analyse table information");

            string[] columns = match.Groups["columns"].Value.Split(',').Select(c => c.Trim()).ToArray();
            descriptor.Columns = GetDefinitions(columns).Select(GetColumnDescriptor).ToArray();


            descriptor.Uniques = AnalyseUniques(match.Groups["unique"].Captures.Cast<Capture>().Select(c=>c.Value)).ToArray();
        }

        IEnumerable<UniqueDescriptor> AnalyseUniques(IEnumerable<string> definitions) {
            foreach(string definition in definitions) {
                string[] columns = definition.Split(',').Select(s => s.Trim('\'')).ToArray();

                yield return new UniqueDescriptor(columns);
            }
        }

        IEnumerable<string> GetDefinitions(IEnumerable<string> columns) {
            foreach(string column in columns) {
                yield return column;
            }
        }

        SchemaColumnDescriptor GetColumnDescriptor(string sql) {
            Match match = Regex.Match(sql, @"^'?(?<name>[^ ']+)'? (?<type>[^ ]+)(?<pk> PRIMARY KEY)?(?<ai> AUTOINCREMENT)?(?<uq> UNIQUE)?(?<nn> NOT NULL)?( DEFAULT '?(?<default>.+)'?)?$");
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
