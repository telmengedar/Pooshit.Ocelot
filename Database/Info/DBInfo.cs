using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations;
using NightlyCode.Database.Entities.Operations.Expressions;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Entities.Schema;
using NightlyCode.Database.Fields;
using NightlyCode.Database.Tokens;
using NightlyCode.Database.Tokens.Values;

namespace NightlyCode.Database.Info {

    /// <summary>
    /// base implementation for db specific logic
    /// </summary>
    public abstract class DBInfo : IDBInfo {
        readonly Dictionary<Type, Action<IDBField, IOperationPreparator, Func<Type, EntityDescriptor>, string>> fieldlogic = new Dictionary<Type, Action<IDBField, IOperationPreparator, Func<Type, EntityDescriptor>, string>>();

        /// <summary>
        /// creates a new <see cref="DBInfo"/>
        /// </summary>
        protected DBInfo() {
            AddFieldLogic<ConstantValue>(AppendConstant);
            AddFieldLogic<EntityField>(AppendEntityField);
            AddFieldLogic<Aggregate>(AppendAggregate);
        }

        /// <summary>
        /// adds a logic to use when generating code for a database field
        /// </summary>
        /// <typeparam name="T">type of field</typeparam>
        /// <param name="logic">logic to use when generating code</param>
        protected void AddFieldLogic<T>(Action<T, IOperationPreparator, Func<Type, EntityDescriptor>, string> logic) {
            fieldlogic[typeof(T)] = (field, preparator, getter, alias) => logic((T)field, preparator, getter, alias);
        }

        void AppendAggregate(Aggregate aggregate, IOperationPreparator preparator, Func<Type, EntityDescriptor> descriptorgetter, string tablealias) {
            preparator.AppendText(aggregate.Method).AppendText("(");
            if(aggregate.Arguments.Length > 0) {
                Append(aggregate.Arguments[0], preparator, descriptorgetter);
                foreach(IDBField field in aggregate.Arguments.Skip(1)) {
                    preparator.AppendText(", ");
                    Append(field, preparator, descriptorgetter);
                }
            }

            preparator.AppendText(")");
        }

        void AppendConstant(ConstantValue constantValue, IOperationPreparator preparator, Func<Type, EntityDescriptor> descriptorgetter, string tablealias) {
            if(constantValue.Value == null)
                preparator.AppendText("NULL");
            else {
                if(constantValue.Value is Expression expression)
                    CriteriaVisitor.GetCriteriaText(expression, descriptorgetter, this, preparator, tablealias);
                else
                    preparator.AppendParameter(constantValue.Value);
            }
        }

        void AppendEntityField(EntityField field, IOperationPreparator preparator, Func<Type, EntityDescriptor> descriptorgetter, string tablealias) {
            CriteriaVisitor.GetCriteriaText(field.FieldExpression, descriptorgetter, this, preparator, tablealias);
        }

        /// <summary>
        /// character used for parameters
        /// </summary>
        public abstract string Parameter { get; }

        /// <summary>
        /// parameter used when joining
        /// </summary>
        public abstract string JoinHint { get; }

        /// <summary>
        /// character used to specify columns explicitely
        /// </summary>
        public abstract string ColumnIndicator { get; }

        /// <summary>
        /// term used for like expression
        /// </summary>
        public abstract string LikeTerm { get; }

        /// <summary>
        /// method used to create a replace function
        /// </summary>
        /// <param name="preparator"> </param>
        /// <param name="value"></param>
        /// <param name="src"></param>
        /// <param name="target"></param>
        /// <param name="visitor"> </param>
        /// <returns></returns>
        public abstract void Replace(ExpressionVisitor visitor, IOperationPreparator preparator, Expression value, Expression src, Expression target);

        /// <summary>
        /// converts an expression to uppercase using database command
        /// </summary>
        /// <param name="visitor"></param>
        /// <param name="preparator"></param>
        /// <param name="value"></param>
        public abstract void ToUpper(ExpressionVisitor visitor, IOperationPreparator preparator, Expression value);

        /// <summary>
        /// converts an expression to lowercase using database command
        /// </summary>
        /// <param name="visitor"></param>
        /// <param name="preparator"></param>
        /// <param name="value"></param>
        public abstract void ToLower(ExpressionVisitor visitor, IOperationPreparator preparator, Expression value);

        /// <summary>
        /// command used to check whether a table exists
        /// </summary>
        /// <param name="db"></param>
        /// <param name="table"></param>
        /// <param name="transaction">transaction to use</param>
        /// <returns></returns>
        public abstract bool CheckIfTableExists(IDBClient db, string table, Transaction transaction = null);

        /// <summary>
        /// get db type of an application type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public abstract string GetDBType(Type type);

        /// <inheritdoc />
        public virtual bool IsTypeEqual(string lhs, string rhs) {
            return lhs == rhs;
        }

        /// <summary>
        /// get db representation type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public abstract Type GetDBRepresentation(Type type);

        /// <summary>
        /// masks a column
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public abstract string MaskColumn(string column);

        /// <summary>
        /// suffix to use when creating tables
        /// </summary>
        public abstract string CreateSuffix { get; }

        /// <inheritdoc />
        public virtual void DropView(IDBClient client, ViewDescriptor view) {
            client.NonQuery($"DROP VIEW {view.Name}");
        }

        /// <inheritdoc />
        public virtual void DropTable(IDBClient client, TableDescriptor entity) {
            client.NonQuery($"DROP TABLE {entity.Name}");
        }

        /// <inheritdoc />
        public abstract void CreateColumn(OperationPreparator operation, EntityColumnDescriptor column);

        /// <inheritdoc />
        public abstract void CreateColumn(OperationPreparator operation, SchemaColumnDescriptor column);

        /// <summary>
        /// get schema for a table in database
        /// </summary>
        /// <param name="client">database connection</param>
        /// <param name="name">name of table of which to get schema</param>
        /// <returns><see cref="SchemaDescriptor"/> containing all information about table</returns>
        public abstract SchemaDescriptor GetSchema(IDBClient client, string name);

        /// <summary>
        /// adds a column to a table
        /// </summary>
        /// <param name="preparator">preparator to which to add sql</param>
        /// <param name="column">column to add</param>
        public abstract void AddColumn(OperationPreparator preparator, EntityColumnDescriptor column);

        /// <summary>
        /// removes a column from a table
        /// </summary>
        /// <param name="preparator">preparator to which to add sql</param>
        /// <param name="column">column to remove</param>
        public abstract void DropColumn(OperationPreparator preparator, string column);

        /// <summary>
        /// modifies a column of a table
        /// </summary>
        /// <param name="preparator">preparator to which to add sql</param>
        /// <param name="column">column to modify</param>
        public abstract void AlterColumn(OperationPreparator preparator, EntityColumnDescriptor column);

        /// <inheritdoc />
        public void Append(IDBField field, IOperationPreparator preparator, Func<Type, EntityDescriptor> descriptorgetter, string tablealias = null) {
            if (field is ISqlToken sqltoken) {
                sqltoken.ToSql(this, preparator, descriptorgetter, tablealias);
                return;
            }
            
            if(!fieldlogic.TryGetValue(field.GetType(), out Action<IDBField, IOperationPreparator, Func<Type, EntityDescriptor>, string> logic))
                throw new NotSupportedException($"{field.GetType()} not supported");
            logic(field, preparator, descriptorgetter, tablealias);
        }

        /// <inheritdoc />
        public DbTransaction BeginTransaction(DbConnection connection, SemaphoreSlim semaphore) {
            semaphore?.Wait();
            try {
                return connection.BeginTransaction();
            }
            catch(Exception) {
                semaphore?.Release();
                throw;
            }
        }

        /// <inheritdoc />
        public void EndTransaction(SemaphoreSlim semaphore) {
            semaphore?.Release();
        }

        /// <inheritdoc />
        public abstract void ReturnID(OperationPreparator preparator, ColumnDescriptor idcolumn);

        /// <inheritdoc />
        public abstract bool MustRecreateTable(string[] obsolete, EntityColumnDescriptor[] altered, EntityColumnDescriptor[] missing, TableDescriptor tableschema, EntityDescriptor entityschema);
    }
}