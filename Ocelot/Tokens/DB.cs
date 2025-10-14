using System;
using System.Linq.Expressions;
using System.Reflection;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Fields.Sql;
using Pooshit.Ocelot.Tokens.Control;
using Pooshit.Ocelot.Tokens.Functions;
using Pooshit.Ocelot.Tokens.Operations;
using Pooshit.Ocelot.Tokens.Partitions;
using Pooshit.Ocelot.Tokens.Values;

namespace Pooshit.Ocelot.Tokens;

/// <summary>
/// class used to generate function tokens
/// </summary>
public static class DB {

    /// <summary>
    /// specifies all columns
    /// </summary>
    public static readonly ISqlToken All = new AllColumnsToken();


    /// <summary>
    /// used to create an alias for an expression
    /// </summary>
    /// <param name="token">token for which to create an alias</param>
    /// <param name="alias">alias to use</param>
    /// <returns>token to be used for alias operations</returns>
    public static AliasToken As(ISqlToken token, string alias) {
        return new(token, alias);
    }

    /// <summary>
    /// used to create an alias for an expression
    /// </summary>
    /// <param name="value">value for which to create an alias</param>
    /// <param name="alias">alias to use</param>
    /// <exception cref="NotImplementedException">thrown when this method is used outside an expression tree</exception>
    public static ISqlToken As(object value, string alias) {
        throw new NotImplementedException("Only to be used in expressions");
    }
    
    /// <summary>
    /// coalesce function used to return the first token which evaluates in non null
    /// </summary>
    /// <param name="tokens">tokens to evaluate</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken Coalesce(params ISqlToken[] tokens) {
        return new DatabaseFunction("COALESCE", tokens);
    }

    /// <summary>
    /// constant value
    /// </summary>
    /// <param name="value">value to add</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken Constant(object value) {
        return new ConstantValue(value);
    }

    /// <summary>
    /// distinct keyword
    /// </summary>
    /// <param name="value">value which to take into account</param>
    public static ISqlToken Distinct(object value) {
        throw new NotImplementedException("Only to be used in expressions");
    }
    
    /// <summary>
    /// get absolute of a value
    /// </summary>
    /// <param name="token">token of which to get absolute</param>
    /// <returns>token representing this statement</returns>
    public static ISqlToken Abs(ISqlToken token) {
        return new DatabaseFunction("ABS", token);
    }

    /// <summary>
    /// get absolute of a value
    /// </summary>
    /// <param name="value">value of which to get absolute</param>
    /// <returns>token representing this statement</returns>
    public static ISqlToken Abs(object value) {
        throw new NotImplementedException("Only to be used in expressions");
    }

    /// <summary>
    /// compute row number over a set of rows
    /// </summary>
    /// <param name="orderBy">over expression</param>
    /// <param name="ascending">whether to order in ascending order</param>
    /// <returns>token representing statement</returns>
    public static RowNumberOver RowNumber(object orderBy, bool ascending) {
        throw new NotImplementedException("Only to be used in expressions");
    }

    /// <summary>
    /// compute row number over a set of rows
    /// </summary>
    /// <param name="partitionBy">field to partition result by</param>
    /// <param name="orderBy">over expression</param>
    /// <param name="ascending">whether to order in ascending order</param>
    /// <returns>token representing statement</returns>
    public static RowNumberOver RowNumber(object partitionBy, object orderBy, bool ascending) {
        throw new NotImplementedException("Only to be used in expressions");
    }

    /// <summary>
    /// compute row number over a set of rows
    /// </summary>
    /// <param name="orderBy">over expression</param>
    /// <param name="ascending">whether to order in ascending order</param>
    /// <returns>token representing statement</returns>
    public static RowNumberOver RowNumber(IDBField orderBy, bool ascending = true) {
        return new(new(orderBy, ascending));
    }

    /// <summary>
    /// compute row number over a set of rows
    /// </summary>
    /// <param name="partitionBy">field to partition result by</param>
    /// <param name="orderBy">field to order result by</param>
    /// <param name="ascending">whether to order in ascending order</param>
    /// <returns>token representing statement</returns>
    public static RowNumberOver RowNumber(IDBField partitionBy, IDBField orderBy, bool ascending = true) {
        return new(partitionBy, new(orderBy, ascending));
    }
    
    /// <summary>
    /// sums up values
    /// </summary>
    /// <param name="token">token identifying values to sum</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken Sum(ISqlToken token) {
        return new DatabaseFunction("SUM", token);
    }

    /// <summary>
    /// sums up values
    /// </summary>
    /// <param name="value">values to sum</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken Sum(object value) {
        throw new NotImplementedException("Only to be used in expressions");
    }

    /// <summary>
    /// averages a series of values
    /// </summary>
    /// <param name="token">token identifying values to sum</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken Avg(ISqlToken token) {
        return new DatabaseFunction("AVG", token);
    }

    /// <summary>
    /// averages a series of values
    /// </summary>
    /// <param name="values">values to sum</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken Avg(object values) {
        throw new NotImplementedException("Only to be used in expressions");
    }

    /// <summary>
    /// get a minimum of a series of values
    /// </summary>
    /// <param name="token">token identifying values of which to get minimum</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken Min(ISqlToken token) {
        return new DatabaseFunction("MIN", token);
    }

    /// <summary>
    /// get a minimum of a series of values
    /// </summary>
    /// <param name="values">values of which to get minimum</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken Min(object values) {
        throw new NotImplementedException("Only to be used in expressions");
    }
    
    /// <summary>
    /// get a minimum of a series of values
    /// </summary>
    /// <param name="values">values of which to get minimum</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken Least(object values) {
        throw new NotImplementedException("Only to be used in expressions");
    }

    /// <summary>
    /// get a minimum of a series of values
    /// </summary>
    /// <param name="token">token identifying values of which to get maximum</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken Max(ISqlToken token) {
        return new DatabaseFunction("MAX", token);
    }

    /// <summary>
    /// get a minimum of a series of values
    /// </summary>
    /// <param name="values">values of which to get maximum</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken Max(object values) {
        throw new NotImplementedException("Only to be used in expressions");
    }

    /// <summary>
    /// get a minimum of a series of values
    /// </summary>
    /// <param name="token">token identifying values of which to get maximum</param>
    /// <returns>token to be used in statements</returns>
    public static IDBField Any(ISqlToken token) {
        return new Aggregate("ANY", token);
    }

    /// <summary>
    /// get a minimum of a series of values
    /// </summary>
    /// <param name="values">values of which to get maximum</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken Any(object values) {
        throw new NotImplementedException("Only to be used in expressions");
    }

    /// <summary>
    /// get a minimum of a series of values
    /// </summary>
    /// <param name="values">values of which to get maximum</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken Greatest(object values) {
        throw new NotImplementedException("Only to be used in expressions");
    }

    /// <summary>
    /// get a minimum of a series of values
    /// </summary>
    /// <param name="token">token identifying values of which to get maximum</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken Floor(ISqlToken token) {
        return new DatabaseFunction("FLOOR", token);
    }

    /// <summary>
    /// get a minimum of a series of values
    /// </summary>
    /// <param name="values">values of which to get maximum</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken Floor(object values) {
        throw new NotImplementedException("Only to be used in expressions");
    }

    /// <summary>
    /// get a minimum of a series of values
    /// </summary>
    /// <param name="token">token identifying values of which to get maximum</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken Ceiling(ISqlToken token) {
        return new DatabaseFunction("CEILING", token);
    }

    /// <summary>
    /// get a minimum of a series of values
    /// </summary>
    /// <param name="values">values of which to get maximum</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken Ceiling(object values) {
        throw new NotImplementedException("Only to be used in expressions");
    }

    /// <summary>
    /// get a minimum of a series of values
    /// </summary>
    /// <param name="token">token identifying values of which to get maximum</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken Concat(ISqlToken token) {
        return new DatabaseFunction("CONCAT", token);
    }

    /// <summary>
    /// get a minimum of a series of values
    /// </summary>
    /// <param name="values">values of which to get maximum</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken Concat(object values) {
        throw new NotImplementedException("Only to be used in expressions");
    }

    /// <summary>
    /// counts values of a column which are not null
    /// </summary>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken Count() {
        return Count(All);
    }

    /// <summary>
    /// counts values of a column which are not null
    /// </summary>
    /// <remarks>
    /// use <see cref="All"/> to count all rows
    /// </remarks>
    /// <param name="token">token specifying column to count</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken Count(ISqlToken token) {
        return new DatabaseFunction("COUNT", token);
    }

    /// <summary>
    /// counts values of a column which are not null
    /// </summary>
    /// <remarks>
    /// use <see cref="All"/> to count all rows
    /// </remarks>
    /// <param name="column">column to count</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken Count(object column) {
        throw new NotImplementedException("Only to be used in expressions");
    }

    /// <summary>
    /// creates a case statement
    /// </summary>
    /// <param name="condition">condition to evaluate</param>
    /// <param name="value">value to use when condition evaluates to true</param>
    /// <param name="elsetoken">value to use when condition evaluates to false (optional)</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken If(ISqlToken condition, ISqlToken value, ISqlToken elsetoken = null) {
        return new IfControl(condition, value, elsetoken);
    }

    /// <summary>
    /// creates a case statement
    /// </summary>
    /// <param name="condition">condition to evaluate</param>
    /// <param name="value">value to use when condition evaluates to true</param>
    /// <param name="elsetoken">value to use when condition evaluates to false (optional)</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken If(bool condition, ISqlToken value, ISqlToken elsetoken = null) {
        return new IfControl(Constant(condition), value, elsetoken);
    }

    /// <summary>
    /// creates a case statement
    /// </summary>
    /// <param name="condition">condition to evaluate</param>
    /// <param name="value">value to use when condition evaluates to true</param>
    /// <param name="elsetoken">value to use when condition evaluates to false (optional)</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken If(bool condition, object value, ISqlToken elsetoken = null) {
        return new IfControl(Constant(condition), Constant(value), elsetoken);
    }

    /// <summary>
    /// creates a case statement
    /// </summary>
    /// <param name="condition">condition to evaluate</param>
    /// <param name="value">value to use when condition evaluates to true</param>
    /// <param name="elsevalue">value to use when condition evaluates to false (optional)</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken If(bool condition, object value, object elsevalue) {
        return new IfControl(Constant(condition), Constant(value), Constant(elsevalue));
    }

    /// <summary>
    /// creates a case statement
    /// </summary>
    /// <param name="condition">condition to evaluate</param>
    /// <param name="value">value to use when condition evaluates to true</param>
    /// <param name="elsevalue">value to use when condition evaluates to false (optional)</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken If(bool condition, ISqlToken value, object elsevalue) {
        return new IfControl(Constant(condition), value, Constant(elsevalue));
    }

    /// <summary>
    /// creates a case statement
    /// </summary>
    /// <param name="condition">condition to evaluate</param>
    /// <param name="value">value to use when condition evaluates to true</param>
    /// <param name="elsetoken">value to use when condition evaluates to false (optional)</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken If(ISqlToken condition, object value, ISqlToken elsetoken = null) {
        return new IfControl(condition, Constant(value), elsetoken);
    }

    /// <summary>
    /// creates a case statement
    /// </summary>
    /// <param name="condition">condition to evaluate</param>
    /// <param name="value">value to use when condition evaluates to true</param>
    /// <param name="elsevalue">value to use when condition evaluates to false (optional)</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken If(ISqlToken condition, ISqlToken value, object elsevalue) {
        return new IfControl(condition, value, Constant(elsevalue));
    }

    /// <summary>
    /// creates a case statement with multiple cases
    /// </summary>
    /// <param name="cases">cases to match</param>
    /// <param name="elsetoken">value to use if no case matches</param>
    /// <returns>case token</returns>
    public static CaseControl Case(When[] cases, ISqlToken elsetoken = null) {
        return new(cases, elsetoken);
    }

    /// <summary>
    /// creates a case statement with multiple cases
    /// </summary>
    /// <param name="cases">cases to match</param>
    /// <param name="elsevalue">value to use if no case matches</param>
    /// <returns>case token</returns>
    public static CaseControl Case(When[] cases, object elsevalue) {
        return new(cases, Constant(elsevalue));
    }

    /// <summary>
    /// creates a when token to be used in case statements
    /// </summary>
    /// <param name="condition">condition of case</param>
    /// <param name="value">value to use if condition evaluates to true</param>
    /// <returns>when token</returns>
    public static When When(ISqlToken condition, ISqlToken value) {
        return new(condition, value);
    }

    /// <summary>
    /// creates a when token to be used in case statements
    /// </summary>
    /// <remarks>
    /// this an overload for usage in expressions
    /// </remarks>
    /// <param name="condition">condition of case</param>
    /// <param name="value">value to use if condition evaluates to true</param>
    /// <returns>when token</returns>
    public static When When(ISqlToken condition, object value) {
        return new(condition, Constant(value));
    }

    /// <summary>
    /// creates a when token to be used in case statements
    /// </summary>
    /// <remarks>
    /// this an overload for usage in expressions
    /// </remarks>
    /// <param name="condition">condition of case</param>
    /// <param name="value">value to use if condition evaluates to true</param>
    /// <returns>when token</returns>
    public static When When(bool condition, ISqlToken value) {
        return new(Constant(condition), value);
    }

    /// <summary>
    /// creates a when token to be used in case statements
    /// </summary>
    /// <remarks>
    /// this an overload for usage in expressions
    /// </remarks>
    /// <param name="condition">condition of case</param>
    /// <param name="value">value to use if condition evaluates to true</param>
    /// <returns>when token</returns>
    public static When When(bool condition, object value) {
        return new(Constant(condition), Constant(value));
    }

    /// <summary>
    /// predicate used to generate sql
    /// </summary>
    /// <param name="predicate">predicate to translate</param>
    /// <typeparam name="T">type to use as expression parameter</typeparam>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken Predicate<T>(Expression<Func<T, bool>> predicate) {
        return new ExpressionToken(predicate);
    }

    /// <summary>
    /// predicate used to generate sql
    /// </summary>
    /// <param name="valueExpression">expression specifying value to extract</param>
    /// <typeparam name="T">type to use as expression parameter</typeparam>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken Value<T>(Expression<Func<T, object>> valueExpression) {
        return new ExpressionToken(valueExpression, true);
    }

    /// <summary>
    /// predicate used to generate sql
    /// </summary>
    /// <param name="valueExpression">expression specifying value to extract</param>
    /// <param name="useBraces">determines whether to enclose expression in brackets</param>
    /// <typeparam name="T">type to use as expression parameter</typeparam>
    /// <returns>token to be used in statements</returns>
    static ISqlToken Value<T>(Expression<Func<T, object>> valueExpression, bool useBraces) {
        return new ExpressionToken(valueExpression, useBraces);
    }

    /// <summary>
    /// references a property of an entity using an expression
    /// </summary>
    /// <typeparam name="T">type of entity to reference</typeparam>
    /// <param name="expression">expression pointing to property</param>
    /// <returns>field to be used in statements</returns>
    public static ISqlToken Property<T>(Expression<Func<T, object>> expression) {
        return new PropertyToken(expression);
    }

    /// <summary>
    /// references a property of an entity using an expression
    /// </summary>
    /// <typeparam name="T">type of entity to reference</typeparam>
    /// <param name="expression">expression pointing to property</param>
    /// <param name="alias">alias to use for property reference</param>
    /// <returns>field to be used in statements</returns>
    public static ISqlToken Property<T>(Expression<Func<T, object>> expression, string alias) {
        return new PropertyToken(expression, alias);
    }

    /// <summary>
    /// references a property of an entity using a type and a name
    /// </summary>
    /// <param name="type">type where property is stored</param>
    /// <param name="property"><see cref="PropertyInfo"/> containing data for property to load</param>
    /// <returns>field to be used in statements</returns>
    public static ISqlToken Property(Type type, string property) {
        return new PropName(type, property);
    }

    /// <summary>
    /// references a property of an entity using a type and a name
    /// </summary>
    /// <param name="property"><see cref="PropertyInfo"/> containing data for property to load</param>
    /// <returns>field to be used in statements</returns>
    public static ISqlToken Property<T>(string property) {
        return new PropName<T>(property);
    }

    /// <summary>
    /// references a property of an entity using a type and a name
    /// </summary>
    /// <param name="type">type where property is stored</param>
    /// <param name="property"><see cref="PropertyInfo"/> containing data for property to load</param>
    /// <param name="ignoreCase">specifies whether to ignore character casing</param>
    /// <returns>field to be used in statements</returns>
    public static ISqlToken Property(Type type, string property, bool ignoreCase) {
        return new PropName(type, property, ignoreCase);
    }

    /// <summary>
    /// references a property of an entity using a type and a name
    /// </summary>
    /// <param name="property"><see cref="PropertyInfo"/> containing data for property to load</param>
    /// <param name="ignoreCase">specifies whether to ignore character casing</param>
    /// <returns>field to be used in statements</returns>
    public static ISqlToken Property<T>(string property, bool ignoreCase) {
        return new PropName<T>(property, ignoreCase);
    }

    /// <summary>
    /// references a property of an entity using a type and a name
    /// </summary>
    /// <param name="type">type where property is stored</param>
    /// <param name="property"><see cref="PropertyInfo"/> containing data for property to load</param>
    /// <param name="ignoreCase">specifies whether to ignore character casing</param>
    /// <param name="alias">alias to use for property reference</param>
    /// <returns>field to be used in statements</returns>
    public static ISqlToken Property(Type type, string property, bool ignoreCase, string alias) {
        return new PropName(type, property, ignoreCase, alias);
    }

    /// <summary>
    /// references a property of an entity using a type and a name
    /// </summary>
    /// <param name="property"><see cref="PropertyInfo"/> containing data for property to load</param>
    /// <param name="ignoreCase">specifies whether to ignore character casing</param>
    /// <param name="alias">alias to use for property reference</param>
    /// <returns>field to be used in statements</returns>
    public static ISqlToken Property<T>(string property, bool ignoreCase, string alias) {
        return new PropName<T>(property, ignoreCase, alias);
    }

    /// <summary>
    /// references a property of an entity using a <see cref="PropertyInfo"/>
    /// </summary>
    /// <param name="property"><see cref="PropertyInfo"/> containing data for property to load</param>
    /// <param name="alias">alias to use for property reference</param>
    /// <returns>field to be used in statements</returns>
    public static ISqlToken Property(PropertyInfo property, string alias=null) {
        return new PropertyInfoToken(property, alias);
    }

    /// <summary>
    /// specifies a column of a table
    /// </summary>
    /// <param name="name">name of column</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken Column(string name) {
        return new ColumnToken(name);
    }

    /// <summary>
    /// specifies a column of a table
    /// </summary>
    /// <param name="table">name of table</param>
    /// <param name="name">name of column</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken Column(string table, string name) {
        return new ColumnToken(table, name);
    }

    /// <summary>
    /// references a field of the statement
    /// </summary>
    /// <param name="name">name of field</param>
    /// <returns>token to be used in statements</returns>
    public static FieldToken Field(string name) {
        return new(name);
    }

    /// <summary>
    /// references a field of the statement
    /// </summary>
    /// <param name="field">name of field</param>
    /// <returns>token to be used in statements</returns>
    public static FieldToken Field(object field) {
        throw new NotImplementedException("Only to be used in expressions");
    }

    /// <summary>
    /// casts data to another type
    /// </summary>
    /// <param name="token">value to cast</param>
    /// <param name="type">type to cast value to</param>
    /// <returns>token to be used in statements</returns>
    public static IDBField Cast(ISqlToken token, CastType type) {
        return new CastToken(token, type);
    }

    /// <summary>
    /// casts data to another type
    /// </summary>
    /// <param name="value">value to cast</param>
    /// <param name="type">type to cast value to</param>
    /// <returns>token to be used in statements</returns>
    public static IDBField Cast(object value, CastType type) {
        return new CastToken(Constant(value), type);
    }

    /// <summary>
    /// allows a db operation to be used as sql token
    /// </summary>
    /// <param name="operation">operation to wrap</param>
    /// <returns>token to be used in statements</returns>
    public static ISqlToken Operation(IDatabaseOperation operation) {
        return Value<object>(o => operation, true);
    }

    /// <summary>
    /// creates a token of multiple related values
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public static TupleToken Tuple(params object[] values) {
        return new(values);
    }

    /// <summary>
    /// l1 vector distance of lhs to rhs
    /// </summary>
    /// <param name="lhs">left hand side vector</param>
    /// <param name="rhs">right hand side vector</param>
    /// <returns>vector operation</returns>
    public static OperationToken VL1(ISqlToken lhs, ISqlToken rhs) {
        return new(lhs, Operand.L1Distance, rhs);
    }

    /// <summary>
    /// l2 vector distance of lhs to rhs
    /// </summary>
    /// <param name="lhs">left hand side vector</param>
    /// <param name="rhs">right hand side vector</param>
    /// <returns>vector operation</returns>
    public static OperationToken VL2(ISqlToken lhs, ISqlToken rhs) {
        return new(lhs, Operand.L2Distance, rhs);
    }

    /// <summary>
    /// cosine vector distance of lhs to rhs
    /// </summary>
    /// <param name="lhs">left hand side vector</param>
    /// <param name="rhs">right hand side vector</param>
    /// <returns>vector operation</returns>
    public static OperationToken VCos(ISqlToken lhs, ISqlToken rhs) {
        return new(lhs, Operand.CosineDistance, rhs);
    }

    /// <summary>
    /// inner vector product of lhs to rhs
    /// </summary>
    /// <param name="lhs">left hand side vector</param>
    /// <param name="rhs">right hand side vector</param>
    /// <returns>vector operation</returns>
    public static OperationToken VProd(ISqlToken lhs, ISqlToken rhs) {
        return new(lhs, Operand.InnerProduct, rhs);
    }

    /// <summary>
    /// calls a custom function
    /// </summary>
    /// <param name="functionName">name of function to call</param>
    /// <param name="tokens">function arguments</param>
    /// <returns>function call</returns>
    public static DatabaseFunction CustomFunction(string functionName, params IDBField[] tokens) {
        return new(functionName, tokens);
    }
}