using System;
using System.Data;
using System.Linq.Expressions;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations;
using NightlyCode.Database.Entities.Operations.Fields;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Entities.Schema;

namespace NightlyCode.Database.Info
{

    /// <summary>
    /// db specific information
    /// </summary>
    public interface IDBInfo
    {
        /// <summary>
        /// character used for parameters
        /// </summary>
        string Parameter { get; }

        /// <summary>
        /// parameter used when joining
        /// </summary>
        string JoinHint { get; }

        /// <summary>
        /// parameter used to create autoincrement columns
        /// </summary>
        string AutoIncrement { get; }

        /// <summary>
        /// character used to specify columns explicitely
        /// </summary>
        string ColumnIndicator { get; }

        /// <summary>
        /// term used for like expression
        /// </summary>
        string LikeTerm { get; }

        /// <summary>
        /// method used to create a replace function
        /// </summary>
        /// <param name="preparator"> </param>
        /// <param name="value"></param>
        /// <param name="src"></param>
        /// <param name="target"></param>
        /// <param name="visitor"> </param>
        /// <returns></returns>
        void Replace(ExpressionVisitor visitor, OperationPreparator preparator, Expression value, Expression src, Expression target);

        /// <summary>
        /// converts an expression to uppercase using database command
        /// </summary>
        /// <param name="visitor"></param>
        /// <param name="preparator"></param>
        /// <param name="value"></param>
        void ToUpper(ExpressionVisitor visitor, OperationPreparator preparator, Expression value);

        /// <summary>
        /// converts an expression to lowercase using database command
        /// </summary>
        /// <param name="visitor"></param>
        /// <param name="preparator"></param>
        /// <param name="value"></param>
        void ToLower(ExpressionVisitor visitor, OperationPreparator preparator, Expression value);

        /// <summary>
        /// command used to check whether a table exists
        /// </summary>
        /// <param name="db"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        bool CheckIfTableExists(IDBClient db, string table);

        /// <summary>
        /// get db type of an application type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        string GetDBType(Type type);

        /// <summary>
        /// get db representation type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        Type GetDBRepresentation(Type type);

        /// <summary>
        /// masks a column
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        string MaskColumn(string column);

        /// <summary>
        /// suffix to use when creating tables
        /// </summary>
        string CreateSuffix { get; }

        /// <summary>
        /// text used to create a column
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        void CreateColumn(OperationPreparator operation, EntityColumnDescriptor column);

        /// <summary>
        /// get schema for a table in database
        /// </summary>
        /// <param name="client">database connection</param>
        /// <param name="name">name of table of which to get schema</param>
        /// <returns><see cref="SchemaDescriptor"/> containing all information about table</returns>
        SchemaDescriptor GetSchema(IDBClient client, string name);

        /// <summary>
        /// adds a column to a table
        /// </summary>
        /// <param name="client">db access</param>
        /// <param name="table">table to modify</param>
        /// <param name="column">column to add</param>
        /// <param name="transaction">transaction to use (optional)</param>
        void AddColumn(IDBClient client, string table, EntityColumnDescriptor column, Transaction transaction=null);

        /// <summary>
        /// removes a column from a table
        /// </summary>
        /// <param name="client">db access</param>
        /// <param name="table">table to modify</param>
        /// <param name="column">column to remove</param>
        void RemoveColumn(IDBClient client, string table, string column);

        /// <summary>
        /// modifies a column of a table
        /// </summary>
        /// <param name="client">db access</param>
        /// <param name="table">table to modify</param>
        /// <param name="column">column to modify</param>
        void AlterColumn(IDBClient client, string table, EntityColumnDescriptor column);

        /// <summary>
        /// appends a database field to an <see cref="OperationPreparator"/>
        /// </summary>
        /// <param name="field">field to append</param>
        /// <param name="preparator">operation to append function to</param>
        /// <param name="descriptorgetter">function used to get <see cref="EntityDescriptor"/>s for types</param>
        void Append(IDBField field, OperationPreparator preparator, Func<Type, EntityDescriptor> descriptorgetter);

        /// <summary>
        /// begins a new transaction
        /// </summary>
        /// <returns></returns>
        IDbTransaction BeginTransaction(IDbConnection connection);

        /// <summary>
        /// ends a transaction
        /// </summary>
        void EndTransaction();
    }
}
