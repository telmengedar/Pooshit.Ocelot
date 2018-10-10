﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Database.Clients;
using Database.Entities.Descriptors;
using Database.Entities.Operations;
using Database.Entities.Operations.Aggregates;
using Database.Entities.Operations.Expressions;
using Database.Entities.Schema;

namespace Database.Info {

    /// <summary>
    /// base implementation for db specific logic
    /// </summary>
    public abstract class DBInfo : IDBInfo {
        readonly Dictionary<Type, Action<IDBField, OperationPreparator, Func<Type, EntityDescriptor>>> fieldlogic = new Dictionary<Type, Action<IDBField, OperationPreparator, Func<Type, EntityDescriptor>>>();

        /// <summary>
        /// creates a new <see cref="DBInfo"/>
        /// </summary>
        protected DBInfo() {
            AddFieldLogic<Constant>(AppendConstant);
            AddFieldLogic<DBParameter>(AppendParameter);
            AddFieldLogic<EntityField>(AppendEntityField);
            AddFieldLogic<Aggregate>(AppendAggregate);
        }

        /// <summary>
        /// adds a logic to use when generating code for a database field
        /// </summary>
        /// <typeparam name="T">type of field</typeparam>
        /// <param name="logic">logic to use when generating code</param>
        protected void AddFieldLogic<T>(Action<T, OperationPreparator, Func<Type, EntityDescriptor>> logic) {
            fieldlogic[typeof(T)] = (field, preparator, getter) => logic((T) field, preparator, getter);
        }

        void AppendAggregate(Aggregate aggregate, OperationPreparator preparator, Func<Type, EntityDescriptor> descriptorgetter) {
            preparator.CommandBuilder.Append(aggregate.Method).Append("(");
            if (aggregate.Arguments.Length > 0) {
                Append(aggregate.Arguments[0], preparator, descriptorgetter);
                foreach (IDBField field in aggregate.Arguments.Skip(1)) {
                    preparator.CommandBuilder.Append(", ");
                    Append(field, preparator, descriptorgetter);
                }
            }

            preparator.CommandBuilder.Append(")");
        }

        void AppendConstant(Constant constant, OperationPreparator preparator, Func<Type, EntityDescriptor> descriptorgetter) {
            if (constant.Value == null)
                preparator.CommandBuilder.Append("NULL");
            else
            {
                if (constant.Value is Expression)
                    CriteriaVisitor.GetCriteriaText((Expression)constant.Value, descriptorgetter, this, preparator);
                else preparator.AppendParameter(constant.Value);
            }
        }

        void AppendParameter(DBParameter parameter, OperationPreparator preparator, Func<Type, EntityDescriptor> descriptorgetter) {
            preparator.CommandBuilder.Append(Parameter + parameter.Index);
        }

        void AppendEntityField(EntityField field, OperationPreparator preparator, Func<Type, EntityDescriptor> descriptorgetter) {
            CriteriaVisitor.GetCriteriaText(field.FieldExpression, descriptorgetter, this, preparator);
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
        /// parameter used to create autoincrement columns
        /// </summary>
        public abstract string AutoIncrement { get; }

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
        public abstract void Replace(ExpressionVisitor visitor, OperationPreparator preparator, Expression value, Expression src, Expression target);

        /// <summary>
        /// converts an expression to uppercase using database command
        /// </summary>
        /// <param name="visitor"></param>
        /// <param name="preparator"></param>
        /// <param name="value"></param>
        public abstract void ToUpper(ExpressionVisitor visitor, OperationPreparator preparator, Expression value);

        /// <summary>
        /// converts an expression to lowercase using database command
        /// </summary>
        /// <param name="visitor"></param>
        /// <param name="preparator"></param>
        /// <param name="value"></param>
        public abstract void ToLower(ExpressionVisitor visitor, OperationPreparator preparator, Expression value);

        /// <summary>
        /// command used to check whether a table exists
        /// </summary>
        /// <param name="db"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public abstract bool CheckIfTableExists(IDBClient db, string table);

        /// <summary>
        /// determines whether db supports transactions
        /// </summary>
        public abstract bool TransactionHint { get; }

        /// <summary>
        /// get db type of an application type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public abstract string GetDBType(Type type);

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

        /// <summary>
        /// text used to create a column
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public abstract void CreateColumn(OperationPreparator operation, EntityColumnDescriptor column);

        /// <summary>
        /// changes creation command to creation command with return insert id statement
        /// </summary>
        /// <param name="insertcommand">insert command</param>
        /// <param name="client">db client used to execute commands</param>
        /// <param name="descriptor">descriptor of entity</param>
        /// <param name="parameters">parameters for command</param>
        /// <returns></returns>
        public abstract object ReturnInsertID(IDBClient client, EntityDescriptor descriptor, string insertcommand, params object[] parameters);

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
        /// <param name="client">db access</param>
        /// <param name="table">table to modify</param>
        /// <param name="column">column to add</param>
        /// <param name="transaction">transaction to use (optional)</param>
        public abstract void AddColumn(IDBClient client, string table, EntityColumnDescriptor column, Transaction transaction = null);

        /// <summary>
        /// removes a column from a table
        /// </summary>
        /// <param name="client">db access</param>
        /// <param name="table">table to modify</param>
        /// <param name="column">column to remove</param>
        public abstract void RemoveColumn(IDBClient client, string table, string column);

        /// <summary>
        /// modifies a column of a table
        /// </summary>
        /// <param name="client">db access</param>
        /// <param name="table">table to modify</param>
        /// <param name="column">column to modify</param>
        public abstract void AlterColumn(IDBClient client, string table, EntityColumnDescriptor column);

        /// <summary>
        /// appends a database field to an <see cref="OperationPreparator"/>
        /// </summary>
        /// <param name="field">field to append</param>
        /// <param name="preparator">operation to append function to</param>
        /// <param name="descriptorgetter">function used to get <see cref="EntityDescriptor"/>s for types</param>
        public void Append(IDBField field, OperationPreparator preparator, Func<Type, EntityDescriptor> descriptorgetter) {
            if (!fieldlogic.TryGetValue(field.GetType(), out Action<IDBField, OperationPreparator, Func<Type, EntityDescriptor>> logic))
                throw new NotSupportedException($"{field.GetType()} not supported");
            logic(field, preparator, descriptorgetter);
        }

    }
}