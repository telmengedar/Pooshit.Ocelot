using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using NightlyCode.DB.Clients;
using NightlyCode.DB.Entities;
using NightlyCode.DB.Entities.Descriptors;
using NightlyCode.DB.Entities.Operations;
using NightlyCode.DB.Entities.Schema;
using Converter = NightlyCode.DB.Extern.Converter;

#if UNITY
using NightlyCode.Unity.DB.Entities.Operations;
#endif

namespace NightlyCode.DB.Info
{

    /// <summary>
    /// information for sqlite
    /// </summary>
    public class SQLiteInfo : IDBInfo
    {
        /// <summary>
        /// character used for parameters
        /// </summary>
        public string Parameter => "@";

        /// <summary>
        /// parameter used when joining
        /// </summary>
        public string JoinHint => "";

        /// <summary>
        /// parameter used to create autoincrement columns
        /// </summary>
        public string AutoIncrement => "AUTOINCREMENT";

        /// <summary>
        /// character used to specify columns explicitely
        /// </summary>
        public string ColumnIndicator => "\"";

        /// <summary>
        /// term used for like expression
        /// </summary>
        public string LikeTerm => "LIKE";

        /// <summary>
        /// method used to create a replace function
        /// </summary>
        /// <param name="preparator"> </param>
        /// <param name="value"></param>
        /// <param name="src"></param>
        /// <param name="target"></param>
        /// <param name="visitor"> </param>
        /// <returns></returns>
        public void Replace(
#if UNITY
            ExpressionVisitor visitor,
#else
            ExpressionVisitor visitor,
#endif 
            OperationPreparator preparator, Expression value, Expression src, Expression target) {
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
        public void ToUpper(
#if UNITY
            ExpressionVisitor visitor,
#else
            ExpressionVisitor visitor,
#endif
            OperationPreparator preparator, Expression value) {
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
        public void ToLower(
#if UNITY
            ExpressionVisitor visitor,
#else
            ExpressionVisitor visitor,
#endif
            OperationPreparator preparator, Expression value)
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
        public bool CheckIfTableExists(IDBClient db, string table)
        {
            return db.Query("SELECT name FROM sqlite_master WHERE (type='table' OR type='view') AND name like @1", table).Rows.Count > 0;
        }

        /// <summary>
        /// determines whether db supports transactions
        /// </summary>
        public bool TransactionHint => true;

        /// <summary>
        /// get db type of an application type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetDBType(Type type) {
            if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = Nullable.GetUnderlyingType(type);

            if(type.IsEnum) return "INTEGER";

            switch(type.Name.ToLower()) {
                case "datetime":
                    return "TIMESTAMP";
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
        public Type GetDBRepresentation(Type type) {
            // sqlite understands datetime but not timespan
            if(type == typeof(TimeSpan))
                return typeof(long);
            if(type == typeof(Version))
                return typeof(long);
            return type;
        }

        /// <summary>
        /// masks a column
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public string MaskColumn(string column) {
            return $"'{column}'";
        }

        /// <summary>
        /// suffix to use when creating tables
        /// </summary>
        public string CreateSuffix => null;

        /// <summary>
        /// type of db
        /// </summary>
        public DBType Type => DBType.SQLite;

        /// <summary>
        /// text used to create a column
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public string CreateColumn(EntityColumnDescriptor column) {
            return CreateColumn(column.Name,
                GetDBType(DBConverterCollection.ContainsConverter(column.Property.PropertyType) ? DBConverterCollection.GetDBType(column.Property.PropertyType) : column.Property.PropertyType),
                column.PrimaryKey,
                column.AutoIncrement,
                column.IsUnique,
                column.NotNull,
                column.DefaultValue
                );
        }

        string CreateColumn(string name, string type, bool primarykey, bool autoincrement, bool unique, bool notnull, object defaultvalue) {
            StringBuilder commandbuilder = new StringBuilder();
            commandbuilder.Append(MaskColumn(name)).Append(" ");
            commandbuilder.Append(type);

            if (primarykey)
                commandbuilder.Append(" PRIMARY KEY");
            if (autoincrement)
                commandbuilder.Append(" ").Append(AutoIncrement);
            if (unique)
                commandbuilder.Append(" UNIQUE");
            if (notnull)
                commandbuilder.Append(" NOT NULL");

            if(defaultvalue != null) {
                if(defaultvalue is string)
                    commandbuilder.Append(" DEFAULT '").Append(defaultvalue).Append("'");
                else commandbuilder.Append(" DEFAULT ").Append(defaultvalue);
            }

            return commandbuilder.ToString();
        }

        /// <summary>
        /// changes creation command to creation command with return insert id statement
        /// </summary>
        /// <param name="insertcommand">insert command</param>
        /// <param name="client">db client used to execute commands</param>
        /// <param name="descriptor">descriptor of entity</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns></returns>
        public object ReturnInsertID(IDBClient client, EntityDescriptor descriptor, string insertcommand, params object[] parameters) {
            client.NonQuery(insertcommand, parameters);
            return client.Scalar("SELECT last_insert_rowid()");
        }

        /// <summary>
        /// get schema for a type in database
        /// </summary>
        /// <typeparam name="T">type for which to get schema</typeparam>
        /// <param name="client">database connection</param>
        /// <returns>schema of specified type</returns>
        public SchemaDescriptor GetSchema<T>(IDBClient client) {
            EntityDescriptor descriptor = EntityDescriptor.Create(typeof(T));
            return GetSchema(client, descriptor.TableName);
        }

        SchemaDescriptor GetSchema(IDBClient client, string tablename) {
            DataTable table = client.Query("SELECT * FROM sqlite_master WHERE (name=@1)", tablename);

            if (table.Rows.Count == 0)
                throw new InvalidOperationException("Type not found in database");

            if (Converter.Convert<string>(table.Rows[0]["type"]) == "table")
            {
                TableDescriptor tabledescriptor = new TableDescriptor(Converter.Convert<string>(table.Rows[0]["tbl_name"]));
                AnalyseTableSql(tabledescriptor, Converter.Convert<string>(table.Rows[0]["sql"]));

                DataTable indices = client.Query("SELECT * FROM sqlite_master WHERE type='index' AND tbl_name=@1", tablename);
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

        IEnumerable<IndexDescriptor> AnalyseIndexDefinitions(DataTable table) {
            foreach(DataRow row in table.Rows) {

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
        public void AddColumn(IDBClient client, string table, EntityColumnDescriptor column) {
            if(column.NotNull) {
                string defaultvalue = null;
                if(column.DefaultValue != null)
                    defaultvalue = Converter.Convert<string>(column.DefaultValue);
                else if(column.Property.PropertyType.IsEnum)
                    defaultvalue = "0";
                else if(column.Property.PropertyType.IsValueType)
                    defaultvalue = Converter.Convert<string>(Activator.CreateInstance(column.Property.PropertyType));

                if(defaultvalue == null)
                    client.NonQuery($"ALTER TABLE {table} ADD COLUMN {CreateColumn(column)} DEFAULT NULL");
                else client.NonQuery($"ALTER TABLE {table} ADD COLUMN {CreateColumn(column)} DEFAULT '{defaultvalue}'");
            }
            else {
                client.NonQuery($"ALTER TABLE {table} ADD COLUMN {CreateColumn(column)}");
            }
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
        public void RemoveColumn(IDBClient client, string table, string column) {
            TableDescriptor schema = GetSchema(client, table) as TableDescriptor;
            if(schema == null)
                throw new Exception("Type not a table");

            // rename old table
            client.NonQuery($"ALTER TABLE {table} RENAME TO {table}_original");

            SchemaColumnDescriptor[] remainingcolumns = schema.Columns.Where(c => c.Name != column).ToArray();
#if UNITY
            string columnlist = string.Join(", ", remainingcolumns.Select(c => c.Name).ToArray());
#else
            string columnlist = string.Join(", ", remainingcolumns.Select(c => c.Name));
#endif
            // create new table without the column
#if UNITY
            client.NonQuery($"CREATE TABLE {table} ({string.Join(", ", remainingcolumns.Select(c => CreateColumn(c.Name, c.Type, c.PrimaryKey, c.AutoIncrement, c.IsUnique, c.NotNull, c.DefaultValue)).ToArray())})");
#else
            client.NonQuery($"CREATE TABLE {table} ({string.Join(", ", remainingcolumns.Select(c => CreateColumn(c.Name, c.Type, c.PrimaryKey, c.AutoIncrement, c.IsUnique, c.NotNull, c.DefaultValue)))})");
#endif
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
        public void AlterColumn(IDBClient client, string table, EntityColumnDescriptor column) {
            RemoveColumn(client, table, column.Name);
            AddColumn(client, table, column);
        }

        /// <summary>
        /// analyses the sql of a table creation and fills the table descriptor from the results
        /// </summary>
        /// <param name="descriptor">descriptor to fill</param>
        /// <param name="sql">sql to analyse</param>
        public void AnalyseTableSql(TableDescriptor descriptor, string sql) {
            Match match = Regex.Match(sql, @"^CREATE TABLE (?<name>[^ ]+) \((?<columns>.+)\)$");
            if(!match.Success)
                throw new InvalidOperationException("Unable to analyse table information");

            string[] columns = match.Groups["columns"].Value.Split(',');
            descriptor.Columns = GetDefinitions(columns).Select(GetColumnDescriptor).ToArray();
        }

        IEnumerable<string> GetDefinitions(IEnumerable<string> columns) {
            foreach(string column in columns) {
                string definition = column.Trim();
                if(definition.StartsWith("UNIQUE"))
                    yield break;
                yield return definition;
            }
        }

        SchemaColumnDescriptor GetColumnDescriptor(string sql) {
            Match match = Regex.Match(sql, @"^'?(?<name>[^ ']+)'? (?<type>[^ ]+)(?<pk> PRIMARY KEY)?(?<ai> AUTOINCREMENT)?(?<uq> UNIQUE)?(?<nn> NOT NULL)?( DEFAULT '?(?<default>[^']+)'?)?$");
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
