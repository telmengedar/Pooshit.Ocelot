using System;
using System.Linq.Expressions;
using System.Text;
using NightlyCode.DB.Clients;
using NightlyCode.DB.Entities;
using NightlyCode.DB.Entities.Descriptors;
using NightlyCode.DB.Entities.Operations;
using NightlyCode.DB.Entities.Schema;
using Converter = NightlyCode.DB.Extern.Converter;

#if UNITY
using NightlyCode.Unity.DB.Entities.Operations;
#endif

namespace NightlyCode.DB.Info {
    public class PostgreInfo : IDBInfo {

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
        public string AutoIncrement { get; }

        /// <summary>
        /// character used to specify columns explicitely
        /// </summary>
        public string ColumnIndicator => "\"";

        /// <summary>
        /// term used for like expression
        /// </summary>
        public string LikeTerm => "ILIKE";

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

        public void ToUpper(ExpressionVisitor visitor, OperationPreparator preparator, Expression value) {
            throw new NotImplementedException();
        }

        public void ToLower(ExpressionVisitor visitor, OperationPreparator preparator, Expression value) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// command used to check whether a table exists
        /// </summary>
        /// <param name="db"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public bool CheckIfTableExists(IDBClient db, string table) {
            return Converter.Convert<long>(db.Scalar("SELECT count(*) FROM pg_class WHERE relname = @1", table)) > 0;
        }

        /// <summary>
        /// determines whether db supports transactions
        /// </summary>
        public bool TransactionHint => false;

        /// <summary>
        /// get db type of an application type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetDBType(Type type) {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = Nullable.GetUnderlyingType(type);

            if (type.IsEnum) return "integer";

            switch (type.Name.ToLower())
            {
                case "datetime":
                    return "timestamp";
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
                    return "int4";
                case "decimal":
                    return "decimal";
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
            if(type.IsEnum)
                return typeof(int);
            if(type == typeof(TimeSpan))
                return typeof(int);
            if (type == typeof(Version))
                return typeof(string);
            return type;
        }

        /// <summary>
        /// masks a column
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public string MaskColumn(string column) {
            return $"\"{column}\"";
        }

        /// <summary>
        /// suffix to use when creating tables
        /// </summary>
        public string CreateSuffix => null;

        /// <summary>
        /// type of db
        /// </summary>
        public DBType Type => DBType.Postgre;

        /// <summary>
        /// text used to create a column
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public void CreateColumn(OperationPreparator operation, EntityColumnDescriptor column) {
            operation.CommandBuilder.Append($"\"{column.Name}\" ");
            if (column.AutoIncrement)
            {
                if (column.Property.PropertyType == typeof(int))
                    operation.CommandBuilder.Append("serial4");
                else if (column.Property.PropertyType == typeof(long))
                    operation.CommandBuilder.Append("serial8");
                else throw new InvalidOperationException("Autoincrement with postgre only allowed with integer types");
            }
            else {
                if (DBConverterCollection.ContainsConverter(column.Property.PropertyType))
                    operation.CommandBuilder.Append(GetDBType(DBConverterCollection.GetDBType(column.Property.PropertyType)));
                else operation.CommandBuilder.Append(GetDBType(column.Property.PropertyType));
            }

            if (column.PrimaryKey)
                operation.CommandBuilder.Append(" PRIMARY KEY");
            if (column.IsUnique)
                operation.CommandBuilder.Append(" UNIQUE");
            if (column.NotNull)
                operation.CommandBuilder.Append(" NOT NULL");

            if (column.DefaultValue != null)
            {
                operation.CommandBuilder.Append(" DEFAULT ");
                operation.AppendParameter(column.DefaultValue);
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
        public object ReturnInsertID(IDBClient client, EntityDescriptor descriptor, string insertcommand, params object[] parameters) {
            return client.Scalar(insertcommand + " RETURNING " + MaskColumn(descriptor.PrimaryKeyColumn.Name), parameters);
        }

        public SchemaDescriptor GetSchema<T>(IDBClient client) {
            throw new NotImplementedException();
        }

        public void AddColumn(IDBClient client, string table, EntityColumnDescriptor column, Transaction transaction) {
            throw new NotImplementedException();
        }

        public void RemoveColumn(IDBClient client, string table, string column) {
            throw new NotImplementedException();
        }

        public void AlterColumn(IDBClient client, string table, EntityColumnDescriptor column) {
            throw new NotImplementedException();
        }

        public SchemaDescriptor GetSchema(IDBClient client, string name)
        {
            throw new NotImplementedException();
        }
    }
}