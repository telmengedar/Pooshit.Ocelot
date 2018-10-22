using System;
using System.Linq.Expressions;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations;
using NightlyCode.Database.Entities.Operations.Expressions;
using NightlyCode.Database.Entities.Operations.Fields;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Entities.Schema;
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
        }

        /// <summary>
        /// appends a database function to an <see cref="OperationPreparator"/>
        /// </summary>
        /// <param name="function">function to be executed</param>
        /// <param name="preparator">operation to append function to</param>
        /// <param name="descriptorgetter">function used to get <see cref="EntityDescriptor"/>s for types</param>
        public void AppendFunction(DBFunction function, OperationPreparator preparator, Func<Type, EntityDescriptor> descriptorgetter)
        {
            switch (function.Type)
            {
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
        public override string AutoIncrement { get; }

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
        public override void Replace(ExpressionVisitor visitor, OperationPreparator preparator, Expression value, Expression src, Expression target) {
            preparator.AppendText("replace(");
            visitor.Visit(value);
            preparator.AppendText(",");
            visitor.Visit(src);
            preparator.AppendText(",");
            visitor.Visit(target);
            preparator.AppendText(")");
        }

        public override void ToUpper(ExpressionVisitor visitor, OperationPreparator preparator, Expression value) {
            throw new NotImplementedException();
        }

        public override void ToLower(ExpressionVisitor visitor, OperationPreparator preparator, Expression value) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// command used to check whether a table exists
        /// </summary>
        /// <param name="db"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public override bool CheckIfTableExists(IDBClient db, string table) {
            return Converter.Convert<long>(db.Scalar("SELECT count(*) FROM pg_class WHERE relname = @1", table)) > 0;
        }

        /// <summary>
        /// determines whether db supports transactions
        /// </summary>
        public override bool TransactionHint => false;

        /// <summary>
        /// get db type of an application type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public override string GetDBType(Type type) {
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
        public override Type GetDBRepresentation(Type type) {
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
        public override string MaskColumn(string column) {
            return $"\"{column}\"";
        }

        /// <summary>
        /// suffix to use when creating tables
        /// </summary>
        public override string CreateSuffix => null;

        /// <summary>
        /// text used to create a column
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public override void CreateColumn(OperationPreparator operation, EntityColumnDescriptor column) {
            operation.AppendText($"\"{column.Name}\" ");
            if (column.AutoIncrement)
            {
                if (column.Property.PropertyType == typeof(int))
                    operation.AppendText("serial4");
                else if (column.Property.PropertyType == typeof(long))
                    operation.AppendText("serial8");
                else throw new InvalidOperationException("Autoincrement with postgre only allowed with integer types");
            }
            else {
                if (DBConverterCollection.ContainsConverter(column.Property.PropertyType))
                    operation.AppendText(GetDBType(DBConverterCollection.GetDBType(column.Property.PropertyType)));
                else operation.AppendText(GetDBType(column.Property.PropertyType));
            }

            if (column.PrimaryKey)
                operation.AppendText("PRIMARY KEY");
            if (column.IsUnique)
                operation.AppendText("UNIQUE");
            if (column.NotNull)
                operation.AppendText("NOT NULL");

            if (column.DefaultValue != null)
            {
                operation.AppendText("DEFAULT");
                operation.AppendParameter(column.DefaultValue);
            }

        }

        public override void AddColumn(IDBClient client, string table, EntityColumnDescriptor column, Transaction transaction) {
            throw new NotImplementedException();
        }

        public override void RemoveColumn(IDBClient client, string table, string column) {
            throw new NotImplementedException();
        }

        public override void AlterColumn(IDBClient client, string table, EntityColumnDescriptor column) {
            throw new NotImplementedException();
        }

        public override SchemaDescriptor GetSchema(IDBClient client, string name)
        {
            throw new NotImplementedException();
        }
    }
}