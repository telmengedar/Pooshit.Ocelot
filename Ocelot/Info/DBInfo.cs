using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.CustomTypes;
using Pooshit.Ocelot.Entities.Attributes;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Entities.Operations.Expressions;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Entities.Schema;
using Pooshit.Ocelot.Extern;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Models;
using Pooshit.Ocelot.Schemas;
using Pooshit.Ocelot.Statements;
using Pooshit.Ocelot.Tokens;
using Pooshit.Ocelot.Tokens.Values;
using SchemaType = Pooshit.Ocelot.Schemas.SchemaType;

namespace Pooshit.Ocelot.Info;

/// <summary>
/// base implementation for db specific logic
/// </summary>
public abstract class DBInfo : IDBInfo {
    readonly Dictionary<Type, Action<IDBField, IOperationPreparator, Func<Type, EntityDescriptor>, string>> fieldlogic = new();

    /// <summary>
    /// creates a new <see cref="DBInfo"/>
    /// </summary>
    protected DBInfo() {
        AddFieldLogic<ConstantValue>(AppendConstant);
        AddFieldLogic<EntityField>(AppendEntityField);
        AddFieldLogic<Aggregate>(AppendAggregate);
        AddFieldLogic<FieldToken>(AppendFieldToken);
        AddFieldLogic<TupleToken>(AppendTupleToken);
    }
    
    /// <summary>
    /// adds a logic to use when generating code for a database field
    /// </summary>
    /// <typeparam name="T">type of field</typeparam>
    /// <param name="logic">logic to use when generating code</param>
    protected void AddFieldLogic<T>(Action<T, IOperationPreparator, Func<Type, EntityDescriptor>, string> logic) {
        fieldlogic[typeof(T)] = (field, preparator, getter, alias) => logic((T)field, preparator, getter, alias);
    }

    void AppendFieldToken(FieldToken field, IOperationPreparator preparator, Func<Type, EntityDescriptor> descriptorGetter, string tableAlias) {
        preparator.AppendText(field.Field);
    }

    void AppendTupleToken(TupleToken tuple, IOperationPreparator preparator, Func<Type, EntityDescriptor> descriptorGetter, string tableAlias) {
        preparator.AppendText("(");
        foreach (object value in tuple.Values) {
            
        }
        preparator.AppendText(")");
    }

    void AppendAggregate(Aggregate aggregate, IOperationPreparator preparator, Func<Type, EntityDescriptor> descriptorgetter, string tablealias) {
        string method = aggregate.Method;
        if (method == "ANY")
            method = "MIN";
        
        preparator.AppendText(method).AppendText("(");
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

    /// <inheritdoc />
    public virtual bool PreparationSupported => false;

    /// <inheritdoc />
    public virtual bool MultipleConnectionsSupported => true;

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

    /// <inheritdoc />
    public virtual void Length(ExpressionVisitor visitor, IOperationPreparator preparator, Expression value) {
        preparator.AppendText("LENGTH(");
        visitor.Visit(value);
        preparator.AppendText(")");
    }

    /// <summary>
    /// converts an expression to lowercase using database command
    /// </summary>
    /// <param name="visitor"></param>
    /// <param name="preparator"></param>
    /// <param name="value"></param>
    public abstract void ToLower(ExpressionVisitor visitor, IOperationPreparator preparator, Expression value);

    /// <inheritdoc />
    public abstract bool CheckIfTableExists(IDBClient db, string table, Transaction transaction = null);

    /// <inheritdoc />
    public abstract Task<bool> CheckIfTableExistsAsync(IDBClient db, string table, Transaction transaction = null);

    /// <summary>
    /// get db type of an application type
    /// </summary>
    /// <param name="type">type info</param>
    /// <param name="length">length of array type (optional)</param>
    /// <returns>type used in database</returns>
    public string GetDBType(Type type, int length=0) {
        if (type.IsGenericType) {
            if(type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = Nullable.GetUnderlyingType(type);
            else if (type.GetGenericTypeDefinition() == typeof(Range<>)) {
                Type[] arguments = type.GetGenericArguments();
                if (arguments[0] == typeof(BigInteger))
                    return "numrange";
                if (arguments[0] == typeof(int) || arguments[0] == typeof(uint))
                    return "int4range";
                if (arguments[0] == typeof(long) || arguments[0] == typeof(ulong))
                    return "int8range";
                if (arguments[0] == typeof(DateTime))
                    return "daterange";
            }
        }

        if(type.IsEnum)
            return GetDBType(Types.Int);
        
        return GetDBType(type.Name.ToLower(), length);
    }

    /// <inheritdoc />
    public abstract string GetDBType(string type, int length=0);

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
    public virtual bool SupportsArrayParameters => false;

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
    public abstract void CreateColumn(OperationPreparator operation, ColumnDescriptor column);

    /// <inheritdoc />
    public abstract Task<IEnumerable<Schema>> ListSchemataAsync(IDBClient client, PageOptions options = null, Transaction transaction = null);

    /// <inheritdoc />
    public abstract SchemaDescriptor GetSchema(IDBClient client, string name);

    /// <inheritdoc />
    public abstract Task<Schema> GetSchemaAsync(IDBClient client, string name, Transaction transaction = null);

    /// <inheritdoc />
    public abstract Task<SchemaType> GetSchemaTypeAsync(IDBClient client, string name, Transaction transaction = null);

    /// <summary>
    /// adds a column to a table
    /// </summary>
    /// <param name="preparator">preparator to which to add sql</param>
    /// <param name="column">column to add</param>
    public abstract void AddColumn(OperationPreparator preparator, ColumnDescriptor column);

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
    public abstract void AlterColumn(OperationPreparator preparator, ColumnDescriptor column);

    /// <inheritdoc />
    public void Append(IDBField field, IOperationPreparator preparator, Func<Type, EntityDescriptor> descriptorgetter, string tablealias = null) {
        if (!fieldlogic.TryGetValue(field.GetType(), out Action<IDBField, IOperationPreparator, Func<Type, EntityDescriptor>, string> logic)) {
            if (field is ISqlToken sqltoken) {
                sqltoken.ToSql(this, preparator, descriptorgetter, tablealias);
                return;
            }
            throw new NotSupportedException($"{field.GetType()} not supported");
        }
        logic(field, preparator, descriptorgetter, tablealias);
    }

    /// <inheritdoc />
    public virtual Expression Visit(CriteriaVisitor visitor, Expression node, IOperationPreparator operation) {
        if (node is MethodCallExpression methodCall) {
            if (methodCall.Method.DeclaringType == typeof(DB)) {
                switch (methodCall.Method.Name) {
                    case "Abs":
                        operation.AppendText("ABS(");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText(")");
                        break;
                    case "As":
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText("AS");
                        string alias = visitor.GetHost(methodCall.Arguments[1]) as string;
                        operation.AppendText(alias);
                        break;
                    case "Sum":
                        operation.AppendText("SUM(");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText(")");
                        break;
                    case "Avg":
                        operation.AppendText("AVG(");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText(")");
                        break;
                    case "Least":
                    case "Min":
                        operation.AppendText("MIN(");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText(")");
                        break;
                    case "Greatest":
                    case "Max":
                        operation.AppendText("MAX(");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText(")");
                        break;
                    case "Any":
                        operation.AppendText("MIN(");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText(")");
                    break;
                    case "Field":
                        operation.AppendText(visitor.GetHost(methodCall.Arguments[0]) as string);
                    break;
                    case "Floor":
                        operation.AppendText("FLOOR(");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText(")");
                        break;
                    case "Ceiling":
                        operation.AppendText("CEILING(");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText(")");
                        break;
                    case "Concat":
                        operation.AppendText("CONCAT(");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText(")");
                    break;
                    case "Count":
                        if (methodCall.Arguments.Count > 0) {
                            operation.AppendText("COUNT(");
                            visitor.Visit(methodCall.Arguments[0]);
                            operation.AppendText(")");
                        }
                        else operation.AppendText("COUNT(*)");
                        break;
                    case "Coalesce":
                        operation.AppendText("COALESCE(");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText(")");
                        break;
                    case "Case":
                        operation.AppendText("CASE");
                        if (methodCall.Arguments[0] is NewArrayExpression arrayparameter) {
                            foreach (Expression when in arrayparameter.Expressions)
                                visitor.Visit(when);
                        }
                        else {
                            if (methodCall.Arguments[0] is not MethodCallExpression ce || ce.Method.DeclaringType != typeof(DB) || ce.Method.Name != "When")
                                operation.AppendText("WHEN");
                            visitor.Visit(methodCall.Arguments[0]);
                        }

                        if (methodCall.Arguments[1] is not ConstantExpression { Value: null }) {
                            operation.AppendText("ELSE");
                            visitor.Visit(methodCall.Arguments[1]);
                        }

                        operation.AppendText("END");
                        break;
                    case "Distinct":
                        operation.AppendText("DISTINCT");
                        visitor.Visit(methodCall.Arguments[0]);
                        break;
                    case "When":
                        operation.AppendText("WHEN");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText("THEN");
                        visitor.Visit(methodCall.Arguments[1]);
                        break;
                    case "If":
                        operation.AppendText("CASE WHEN");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText("THEN");
                        visitor.Visit(methodCall.Arguments[1]);
                        if (methodCall.Arguments[2] is not ConstantExpression || methodCall.Arguments[2] is ConstantExpression { Value: not null }) {
                            operation.AppendText("ELSE");
                            visitor.Visit(methodCall.Arguments[2]);
                        }
                        operation.AppendText("END");
                        break;
                    case "RowNumber":
                        operation.AppendText("ROW_NUMBER() OVER(");
                        int argumentIndex = 0;
                        if (methodCall.Arguments.Count == 3) {
                            if (methodCall.Arguments[0] is not ConstantExpression { Value: null }) {
                                operation.AppendText("PARTITION BY");
                                visitor.Visit(methodCall.Arguments[0]);
                            }

                            ++argumentIndex;
                        }

                        if (methodCall.Arguments[argumentIndex] is not ConstantExpression { Value: null }) {
                            operation.AppendText("ORDER BY");
                            visitor.Visit(methodCall.Arguments[argumentIndex++]);
                            if (methodCall.Arguments[argumentIndex] is not ConstantExpression { Value: true })
                                operation.AppendText("DESC");
                        }

                        operation.AppendText(")");
                    break;
                    default:
                        return null;
                }

                /*ProcessExpressionList(node.Arguments, parameter => {
                                                          if (parameter is NewArrayExpression arrayparameter) {
                                                              ProcessExpressionList(arrayparameter.Expressions, arrayargument => { Visit(arrayargument); });
                                                          }
                                                          else {
                                                              visitor.Visit(node.Arguments[0]);
                                                          }
                                                      });*/

                return node;
            }

            if (methodCall.Method.DeclaringType == typeof(Math)) {
                switch (methodCall.Method.Name) {
                    case nameof(Math.Acos):
                        operation.AppendText("ACOS(");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText(")");
                        break;
                    case nameof(Math.Sin):
                        operation.AppendText("SIN(");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText(")");
                        break;
                    case nameof(Math.Cos):
                        operation.AppendText("COS(");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText(")");
                        break;
                    case nameof(Math.Tan):
                        operation.AppendText("TAN(");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText(")");
                        break;
                    case nameof(Math.Asin):
                        operation.AppendText("ASIN(");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText(")");
                        break;
                    case nameof(Math.Atan):
                        operation.AppendText("ATAN(");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText(")");
                        break;
                    case nameof(Math.Atan2):
                        operation.AppendText("ATAN2(");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText(")");
                        break;
                    case nameof(Math.Abs):
                        operation.AppendText("ABS(");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText(")");
                        break;
                    case nameof(Math.Ceiling):
                        operation.AppendText("CEIL(");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText(")");
                        break;
                    case nameof(Math.Floor):
                        operation.AppendText("FLOOR(");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText(")");
                        break;
                    case nameof(Math.Log):
                        if (methodCall.Arguments.Count > 1) {
                            operation.AppendText("LOG(");
                            visitor.Visit(methodCall.Arguments[0]);
                            operation.AppendText(",");
                            visitor.Visit(methodCall.Arguments[1]);
                            operation.AppendText(")");
                        }
                        else {
                            operation.AppendText("LN(");
                            visitor.Visit(methodCall.Arguments[0]);
                            operation.AppendText(")");
                        }
                        break;
                    case nameof(Math.Log10):
                        operation.AppendText("LOG(");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText(")");
                        break;
                    case nameof(Math.Pow):
                        operation.AppendText("POWER(");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText(",");
                        visitor.Visit(methodCall.Arguments[1]);
                        operation.AppendText(")");
                        break;
                    case nameof(Math.Round):
                        operation.AppendText("ROUND(");
                        visitor.Visit(methodCall.Arguments[0]);
                        if (methodCall.Arguments.Count > 1) {
                            operation.AppendText(",");
                            visitor.Visit(methodCall.Arguments[1]);
                        }
                        operation.AppendText(")");
                        break;
                    case nameof(Math.Sqrt):
                        operation.AppendText("SQRT(");
                        visitor.Visit(methodCall.Arguments[0]);
                        operation.AppendText(")");
                        break;
                    default:
                        throw new ArgumentException("Unsupported math function");
                }
                return node;
            }

            throw new ArgumentException("Unsupported method");
        }

        return null;
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

    /// <inheritdoc />
    public abstract bool MustRecreateTable(string[] obsolete, ColumnDescriptor[] altered, ColumnDescriptor[] missing, TableSchema currentSchema, TableSchema targetSchema);

    /// <inheritdoc />
    public abstract Task<string> GenerateCreateStatement(IDBClient client, string table);

    /// <inheritdoc />
    public virtual Task Truncate(IDBClient client, string table, TruncateOptions options = null) {
        if (options?.ResetIdentity ?? false)
            throw new InvalidOperationException("Unsupported truncate option");
        return client.NonQueryAsync(options?.Transaction,$"TRUNCATE {table}");
    }

    /// <inheritdoc />
    public virtual void CreateParameter(IDbCommand command, object parameterValue) {
        IDbDataParameter parameter = command.CreateParameter();
        parameter.ParameterName = Parameter + (command.Parameters.Count + 1);
        parameter.Value = parameterValue == null || parameterValue == DBNull.Value ?
                              DBNull.Value :
                              Converter.Convert(parameterValue, GetDBRepresentation(parameterValue.GetType()));

        command.Parameters.Add(parameter);
    }

    /// <inheritdoc />
    public virtual void CreateInFragment(Expression lhs, Expression rhs, IOperationPreparator preparator, Func<Expression, Expression> visitor) {
        visitor(lhs);
        preparator.AppendText("IN(");
        visitor(rhs);
        preparator.AppendText(")");
    }

    /// <inheritdoc />
    public virtual void CreateRangeContainsFragment(Expression lhs, Expression rhs, IOperationPreparator preparator, Func<Expression, Expression> visitor) {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public virtual void CreateMatchFragment(Expression value, Expression pattern, IOperationPreparator preparator, Func<Expression, Expression> visitor) {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public abstract object GenerateDefault(string type);

    /// <inheritdoc />
    public virtual object ValueFromReader(Reader reader, int ordinal, Type type) {
        if (type == typeof(BigInteger))
            return reader.FieldValue<BigInteger>(ordinal);
        return reader.GetValue(ordinal);
    }

    /// <inheritdoc />
    public virtual async Task<object> ValueFromReaderAsync(Reader reader, int ordinal, Type type) {
        if (type == typeof(BigInteger))
            return await reader.FieldValueAsync<BigInteger>(ordinal);
        return reader.GetValue(ordinal);
    }

    /// <inheritdoc />
    public virtual void CreateIndexTypeFragment(StringBuilder commandBuilder, string type) {
    }
}