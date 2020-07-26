using System;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Fields;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Entities.Schema;
using NightlyCode.Database.Fields;

namespace NightlyCode.Database.Info {

    /// <summary>
    /// db specific information
    /// </summary>
    public interface IDBInfo {

        /// <summary>
        /// character used for parameters
        /// </summary>
        string Parameter { get; }

        /// <summary>
        /// parameter used when joining
        /// </summary>
        string JoinHint { get; }

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
        void Replace(ExpressionVisitor visitor, IOperationPreparator preparator, Expression value, Expression src, Expression target);

        /// <summary>
        /// converts an expression to uppercase using database command
        /// </summary>
        /// <param name="visitor"></param>
        /// <param name="preparator"></param>
        /// <param name="value"></param>
        void ToUpper(ExpressionVisitor visitor, IOperationPreparator preparator, Expression value);

        /// <summary>
        /// converts an expression to lowercase using database command
        /// </summary>
        /// <param name="visitor"></param>
        /// <param name="preparator"></param>
        /// <param name="value"></param>
        void ToLower(ExpressionVisitor visitor, IOperationPreparator preparator, Expression value);

        /// <summary>
        /// command used to check whether a table exists
        /// </summary>
        /// <param name="db"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        bool CheckIfTableExists(IDBClient db, string table, Transaction transaction = null);

        /// <summary>
        /// get db type of an application type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        string GetDBType(Type type);

        /// <summary>
        /// determines whether two types are equal
        /// </summary>
        /// <param name="lhs">first type to check</param>
        /// <param name="rhs">second type to check whether it is equal to the first type</param>
        /// <returns>true when types are equal, false otherwise</returns>
        bool IsTypeEqual(string lhs, string rhs);

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
        /// drops a view from database
        /// </summary>
        /// <param name="client">client to use to execute command</param>
        /// <param name="view">view to drop</param>
        void DropView(IDBClient client, ViewDescriptor view);

        /// <summary>
        /// text used to create a column
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="column"></param>
        void CreateColumn(OperationPreparator operation, EntityColumnDescriptor column);

        /// <summary>
        /// text used to create a column
        /// </summary>
        /// <param name="operation">operation where to add column creation text</param>
        /// <param name="column">column description</param>
        void CreateColumn(OperationPreparator operation, SchemaColumnDescriptor column);

        /// <summary>
        /// get schema for a table in database
        /// </summary>
        /// <param name="client">database connection</param>
        /// <param name="name">name of table of which to get schema</param>
        /// <returns><see cref="SchemaDescriptor"/> containing all information about table</returns>
        SchemaDescriptor GetSchema(IDBClient client, string name);

        /// <summary>
        /// adds drop column statement to the preparator
        /// </summary>
        /// <param name="preparator">preparator to which to add sql</param>
        /// <param name="column">name of column to drop</param>
        void DropColumn(OperationPreparator preparator, string column);

        /// <summary>
        /// adds add column sql text to the preparator
        /// </summary>
        /// <param name="preparator">preparator to which to add sql</param>
        /// <param name="column">column info for which to generate sql</param>
        void AddColumn(OperationPreparator preparator, EntityColumnDescriptor column);

        /// <summary>
        /// adds alter column statement to the preparator
        /// </summary>
        /// <param name="preparator">preparator to which to add sql</param>
        /// <param name="column">column info for which to generate sql</param>
        void AlterColumn(OperationPreparator preparator, EntityColumnDescriptor column);

        /// <summary>
        /// appends a database field to an <see cref="OperationPreparator"/>
        /// </summary>
        /// <param name="field">field to append</param>
        /// <param name="preparator">operation to append function to</param>
        /// <param name="descriptorgetter">function used to get <see cref="EntityDescriptor"/>s for types</param>
        /// <param name="tablealias">alias to use when resolving properties</param>
        void Append(IDBField field, IOperationPreparator preparator, Func<Type, EntityDescriptor> descriptorgetter, string tablealias = null);

        /// <summary>
        /// begins a new transaction
        /// </summary>
        /// <returns>transaction object to assign to command</returns>
        DbTransaction BeginTransaction(DbConnection connection, SemaphoreSlim semaphore);

        /// <summary>
        /// ends a transaction
        /// </summary>
        void EndTransaction(SemaphoreSlim semaphore);

        /// <summary>
        /// adds statement used to return id of insert operation
        /// </summary>
        /// <param name="preparator">command to modify</param>
        /// <param name="idcolumn">id column of which to return value</param>
        void ReturnID(OperationPreparator preparator, ColumnDescriptor idcolumn);

        /// <summary>
        /// determines whether table has to get recreated
        /// </summary>
        /// <param name="obsolete">list of columns which need to get dropped</param>
        /// <param name="altered">list of columns which were altered in definition</param>
        /// <param name="missing">list of columns which need to get added</param>
        /// <param name="tableschema">schema of table currently stored in database</param>
        /// <param name="entityschema">schema of entity which needs to get mapped to database</param>
        /// <returns>true when table has to get recreated, false otherwise</returns>
        bool MustRecreateTable(string[] obsolete, EntityColumnDescriptor[] altered, EntityColumnDescriptor[] missing, TableDescriptor tableschema, EntityDescriptor entityschema);
    }
}
