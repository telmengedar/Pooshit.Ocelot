using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Expressions;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Entities.Schema;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Models;
using Pooshit.Ocelot.Schemas;
using Pooshit.Ocelot.Statements;
using SchemaType = Pooshit.Ocelot.Schemas.SchemaType;

namespace Pooshit.Ocelot.Info;

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
    /// determines whether the provider supports prepared statements
    /// </summary>
    bool PreparationSupported { get; }

    /// <summary>
    /// determines whether multiple parallel connections to the database are supported
    /// </summary>
    bool MultipleConnectionsSupported { get; }
        
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
    /// determines the length of a string using a database command
    /// </summary>
    /// <param name="visitor">visitor of expression tree</param>
    /// <param name="preparator">used to build operation text</param>
    /// <param name="value">expression of string value</param>
    void Length(ExpressionVisitor visitor, IOperationPreparator preparator, Expression value);

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
    /// <param name="db">client to database</param>
    /// <param name="table">name of table to check</param>
    /// <param name="transaction">transaction to use (optional)</param>
    /// <returns>true if table exists, false otherwise</returns>
    bool CheckIfTableExists(IDBClient db, string table, Transaction transaction = null);

    /// <summary>
    /// command used to check whether a table exists
    /// </summary>
    /// <param name="db">client to database</param>
    /// <param name="table">name of table to check</param>
    /// <param name="transaction">transaction to use (optional)</param>
    /// <returns>true if table exists, false otherwise</returns>
    Task<bool> CheckIfTableExistsAsync(IDBClient db, string table, Transaction transaction = null);

    /// <summary>
    /// get db type of an application type
    /// </summary>
    /// <param name="type">type for which to get db type</param>
    /// <returns>text representation of type</returns>
    string GetDBType(Type type);

    /// <summary>
    /// get db type of an application type
    /// </summary>
    /// <param name="type">type for which to get db type</param>
    /// <returns>text representation of type</returns>
    string GetDBType(string type);
        
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
    /// determines whether array parameters are supported
    /// </summary>
    bool SupportsArrayParameters { get; }

    /// <summary>
    /// drops a view from database
    /// </summary>
    /// <param name="client">client to use to execute command</param>
    /// <param name="view">view to drop</param>
    void DropView(IDBClient client, ViewDescriptor view);

    /// <summary>
    /// drops a table from database
    /// </summary>
    /// <param name="client">client to use to execute command</param>
    /// <param name="entity">entity to drop</param>
    void DropTable(IDBClient client, TableDescriptor entity);

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
    void CreateColumn(OperationPreparator operation, ColumnDescriptor column);

    /// <summary>
    /// lists schemata in database
    /// </summary>
    /// <param name="client">database connection</param>
    /// <param name="transaction">transaction to use (optional)</param>
    /// <param name="options">options to apply</param>
    /// <returns>schemata in database</returns>
    Task<IEnumerable<Schema>> ListSchemataAsync(IDBClient client, PageOptions options=null, Transaction transaction = null);
            
    /// <summary>
    /// get schema for a table in database
    /// </summary>
    /// <param name="client">database connection</param>
    /// <param name="name">name of table of which to get schema</param>
    /// <returns><see cref="SchemaDescriptor"/> containing all information about table</returns>
    SchemaDescriptor GetSchema(IDBClient client, string name);

    /// <summary>
    /// get schema for a table in database
    /// </summary>
    /// <param name="client">database connection</param>
    /// <param name="name">name of table of which to get schema</param>
    /// <param name="transaction">transaction to use</param>
    /// <returns><see cref="SchemaDescriptor"/> containing all information about table</returns>
    Task<Schema> GetSchemaAsync(IDBClient client, string name, Transaction transaction=null);

    /// <summary>
    /// get type of a schema
    /// </summary>
    /// <param name="client">database connection</param>
    /// <param name="name">name of schema of which to get type</param>
    /// <param name="transaction">transaction to use (optional)</param>
    /// <returns>type of schema</returns>
    Task<SchemaType> GetSchemaTypeAsync(IDBClient client, string name, Transaction transaction = null);
        
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
    void AddColumn(OperationPreparator preparator, ColumnDescriptor column);

    /// <summary>
    /// adds alter column statement to the preparator
    /// </summary>
    /// <param name="preparator">preparator to which to add sql</param>
    /// <param name="column">column info for which to generate sql</param>
    void AlterColumn(OperationPreparator preparator, ColumnDescriptor column);

    /// <summary>
    /// appends a database field to an <see cref="OperationPreparator"/>
    /// </summary>
    /// <param name="field">field to append</param>
    /// <param name="preparator">operation to append function to</param>
    /// <param name="descriptorgetter">function used to get <see cref="EntityDescriptor"/>s for types</param>
    /// <param name="tablealias">alias to use when resolving properties</param>
    void Append(IDBField field, IOperationPreparator preparator, Func<Type, EntityDescriptor> descriptorgetter, string tablealias = null);

    /// <summary>
    /// visits an expression in an expression tree
    /// </summary>
    /// <param name="visitor">visitor used to process child nodes</param>
    /// <param name="node">node to visit</param>
    /// <param name="operation">database operation to fill</param>
    Expression Visit(CriteriaVisitor visitor, Expression node, IOperationPreparator operation);
        
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

    /// <summary>
    /// determines whether table has to get recreated
    /// </summary>
    /// <param name="obsolete">list of columns which need to get dropped</param>
    /// <param name="altered">list of columns which were altered in definition</param>
    /// <param name="missing">list of columns which need to get added</param>
    /// <param name="currentSchema">schema of table currently stored in database</param>
    /// <param name="targetSchema">schema of entity which needs to get mapped to database</param>
    /// <returns>true when table has to get recreated, false otherwise</returns>
    bool MustRecreateTable(string[] obsolete, ColumnDescriptor[] altered, ColumnDescriptor[] missing, TableSchema currentSchema, TableSchema targetSchema);

    /// <summary>
    /// generates a create statement which can be used to create the specified table in another database (of the same type)
    /// </summary>
    /// <param name="client">database connection</param>
    /// <param name="table">table for which to generate statement</param>
    /// <returns>statement data</returns>
    Task<string> GenerateCreateStatement(IDBClient client, string table);

    /// <summary>
    /// truncates a table
    /// </summary>
    /// <param name="client">client used to send command to</param>
    /// <param name="table">table to truncate</param>
    /// <param name="options">options to apply</param>
    Task Truncate(IDBClient client, string table, TruncateOptions options = null);

    /// <summary>
    /// creates a parameter 
    /// </summary>
    /// <param name="command">command for which to create parameter</param>
    /// <param name="parameterValue">parameter value to add</param>
    void CreateParameter(IDbCommand command, object parameterValue);

    /// <summary>
    /// creates fragment in statement used for IN function
    /// </summary>
    /// <param name="lhs">value to look for</param>
    /// <param name="rhs">collection of values to check against</param>
    /// <param name="preparator">statement text preparator</param>
    /// <param name="visitor">visitor used for expressions</param>
    void CreateInFragment(Expression lhs, Expression rhs, IOperationPreparator preparator, Func<Expression, Expression> visitor);

    /// <summary>
    /// creates fragment in statement used for IN function
    /// </summary>
    /// <param name="lhs">value to look for</param>
    /// <param name="rhs">collection of values to check against</param>
    /// <param name="preparator">statement text preparator</param>
    /// <param name="visitor">visitor used for expressions</param>
    void CreateRangeContainsFragment(Expression lhs, Expression rhs, IOperationPreparator preparator, Func<Expression, Expression> visitor);

    /// <summary>
    /// generates a default value for the specified type
    /// </summary>
    /// <param name="type">type for which to generate default value</param>
    /// <returns>default value for specified type</returns>
    object GenerateDefault(string type);

    /// <summary>
    /// reads a typed value from a reader
    /// </summary>
    /// <param name="reader">reader used to retrieve value</param>
    /// <param name="ordinal">ordinal index of column to read</param>
    /// <param name="type">type to read</param>
    /// <returns>read value</returns>
    object ValueFromReader(Reader reader, int ordinal, Type type);

    /// <summary>
    /// create fragment to be used for index types
    /// </summary>
    /// <param name="commandBuilder">builder used to build command</param>
    /// <param name="type">index type</param>
    /// <returns>text fragment to be used for index type</returns>
    void CreateIndexTypeFragment(StringBuilder commandBuilder, string type);
}