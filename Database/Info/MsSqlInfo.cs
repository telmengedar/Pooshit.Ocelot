using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Expressions;
using NightlyCode.Database.Entities.Operations.Fields;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Entities.Schema;
using NightlyCode.Database.Fields;
using Converter = NightlyCode.Database.Extern.Converter;

namespace NightlyCode.Database.Info {

    /// <summary>
    /// database specific logic for sqlserver databases
    /// </summary>
    public class MsSqlInfo : DBInfo {

        /// <summary>
        /// creates a new <see cref="PostgreInfo"/>
        /// </summary>
        public MsSqlInfo() {
            AddFieldLogic<DBFunction>(AppendFunction);
            AddFieldLogic<LimitField>(AppendLimit);
        }

        void AppendLimit(LimitField limit, IOperationPreparator preparator, Func<Type, EntityDescriptor> descriptorgetter, string tablealias) {
            if (!preparator.Tokens.Any(t => t is CommandTextToken ctt && ctt.Text == "ORDER BY"))
                preparator.AppendText("ORDER BY(SELECT NULL)");
            
            if (limit.Offset!=null)
                preparator.AppendText("OFFSET").AppendField(limit.Offset, this, descriptorgetter, tablealias).AppendText("ROWS");
            else preparator.AppendText("OFFSET").AppendParameter(0).AppendText("ROWS");

            if (limit.Limit!=null)
                preparator.AppendText("FETCH NEXT").AppendField(limit.Limit, this, descriptorgetter, tablealias).AppendText("ROWS ONLY");
        }

        /// <summary>
        /// appends a database function to an <see cref="OperationPreparator"/>
        /// </summary>
        /// <param name="function">function to be executed</param>
        /// <param name="preparator">operation to append function to</param>
        /// <param name="descriptorgetter">function used to get <see cref="EntityDescriptor"/>s for types</param>
        /// <param name="tablealias">alias for table to use</param>
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
        public override string LikeTerm => "LIKE";

        /// <inheritdoc />
        public override void DropView(IDBClient client, ViewDescriptor view) {
        }

        /// <inheritdoc />
        public override void DropTable(IDBClient client, TableDescriptor entity) {
        }

        /// <inheritdoc />
        public override void CreateColumn(OperationPreparator operation, EntityColumnDescriptor column) {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void CreateColumn(OperationPreparator operation, SchemaColumnDescriptor column) {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override SchemaDescriptor GetSchema(IDBClient client, string name) {
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
        public override void AddColumn(OperationPreparator preparator, EntityColumnDescriptor column) {
            throw new NotImplementedException();
        }

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
        public override string GetDBType(Type type) {
            throw new NotImplementedException();
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
        public override void ReturnID(OperationPreparator preparator, ColumnDescriptor idcolumn) {
            preparator.AppendText($"; SELECT SCOPE_IDENTITY();");
        }

        /// <inheritdoc />
        public override bool MustRecreateTable(string[] obsolete, EntityColumnDescriptor[] altered, EntityColumnDescriptor[] missing, TableDescriptor tableschema, EntityDescriptor entityschema) {
            return false;
        }

        /// <inheritdoc />
        public override Task<string> GenerateCreateStatement(IDBClient client, string table) {
            throw new NotImplementedException();
        }
    }
}