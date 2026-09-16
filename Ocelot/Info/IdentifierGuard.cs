using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pooshit.Ocelot.Errors;
using Pooshit.Ocelot.Schemas;
using Converter = Pooshit.Ocelot.Extern.Converter;

namespace Pooshit.Ocelot.Info;

/// <summary>
/// validates caller-supplied sql identifiers at the point where they become command text
/// </summary>
public static class IdentifierGuard {
    static readonly Regex simplePattern = new("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);
    static readonly Regex qualifiedPattern = new(@"^[A-Za-z_][A-Za-z0-9_]*(\.[A-Za-z_][A-Za-z0-9_]*)*$", RegexOptions.Compiled);
    static readonly Regex bareLiteralPattern = new("^[A-Za-z0-9_.+-]+$", RegexOptions.Compiled);
    static readonly Regex typeTokenPattern = new(@"^[A-Za-z_][A-Za-z0-9_]*( [A-Za-z_][A-Za-z0-9_]*)*(\([0-9]+(, ?[0-9]+)*\))?(\[[0-9]*\])?$", RegexOptions.Compiled);

    /// <summary>
    /// validates a single unqualified identifier (column, alias, index, constraint, index type)
    /// </summary>
    /// <param name="value">identifier to validate</param>
    /// <param name="role">role of the identifier, used in the exception message</param>
    /// <param name="hint">optional hint appended to the exception message when the identifier is rejected</param>
    /// <returns><paramref name="value"/> unchanged</returns>
    public static string Simple(string value, string role, string hint = null) {
        if(value == null || !simplePattern.IsMatch(value))
            throw new InvalidIdentifierException(value, role, hint);
        return value;
    }

    /// <summary>
    /// validates an identifier which may be dot-separated (table, view, function name)
    /// </summary>
    /// <param name="value">identifier to validate</param>
    /// <param name="role">role of the identifier, used in the exception message</param>
    /// <returns><paramref name="value"/> unchanged</returns>
    public static string Qualified(string value, string role) {
        if(value == null || !qualifiedPattern.IsMatch(value))
            throw new InvalidIdentifierException(value, role);
        return value;
    }

    /// <summary>
    /// validates a sql column type token (e.g. <c>VARCHAR(255)</c>, <c>DOUBLE PRECISION</c>, <c>real[]</c>); null or empty is accepted unchanged
    /// </summary>
    /// <param name="value">type token to validate</param>
    /// <param name="role">role of the identifier, used in the exception message</param>
    /// <returns><paramref name="value"/> unchanged</returns>
    public static string TypeToken(string value, string role) {
        if(string.IsNullOrEmpty(value))
            return value;
        if(!typeTokenPattern.IsMatch(value))
            throw new InvalidIdentifierException(value, role);
        return value;
    }

    /// <summary>
    /// validates a rendered default-value literal for ddl, where sql offers no parameter binding
    /// </summary>
    /// <param name="text">rendered text of the literal</param>
    /// <param name="quoted">whether the literal is rendered inside single quotes</param>
    /// <param name="role">role of the literal, used in the exception message</param>
    /// <returns><paramref name="text"/> unchanged</returns>
    public static string DefaultLiteral(string text, bool quoted, string role) {
        if(quoted) {
            if(text == null || ContainsQuoteBreakout(text))
                throw new InvalidIdentifierException(text, role);
        }
        else if(text == null || !bareLiteralPattern.IsMatch(text))
            throw new InvalidIdentifierException(text, role);
        return text;
    }

    static bool ContainsQuoteBreakout(string text) {
        foreach(char c in text) {
            if(c == '\'' || c == '\\' || c < ' ')
                return true;
        }
        return false;
    }

    /// <summary>
    /// validates a column default value, picking the quoted/bare branch from its runtime type
    /// </summary>
    /// <param name="defaultValue">default value to validate, as stored on a column descriptor</param>
    /// <param name="role">role of the literal, used in the exception message</param>
    /// <returns>rendered text of <paramref name="defaultValue"/>, or null when <paramref name="defaultValue"/> is null</returns>
    public static string Default(object defaultValue, string role = "default value") {
        if(defaultValue == null)
            return null;
        bool quoted = defaultValue is string or Guid or DateTime or TimeSpan;
        string text = Converter.Convert<string>(defaultValue);
        return DefaultLiteral(text, quoted, role);
    }

    /// <summary>
    /// validates a column descriptor's name, type and default value
    /// </summary>
    /// <param name="column">column descriptor to validate</param>
    public static void Column(ColumnDescriptor column) {
        Simple(column.Name, "column");
        TypeToken(column.Type, "column type");
        Default(column.DefaultValue);
    }

    /// <summary>
    /// validates an index descriptor's name, type and columns
    /// </summary>
    /// <param name="index">index descriptor to validate</param>
    public static void Index(IndexDescriptor index) {
        Simple(index.Name, "index");
        if(!string.IsNullOrEmpty(index.Type))
            Simple(index.Type, "index type");
        if(index.Columns != null)
            foreach(string column in index.Columns)
                Simple(column, "column");
    }

    /// <summary>
    /// validates a unique descriptor's name and columns
    /// </summary>
    /// <param name="unique">unique descriptor to validate</param>
    public static void Unique(UniqueDescriptor unique) {
        if(!string.IsNullOrEmpty(unique.Name))
            Simple(unique.Name, "constraint");
        if(unique.Columns != null)
            foreach(string column in unique.Columns)
                Simple(column, "column");
    }

    /// <summary>
    /// validates a table name plus its column, index and unique descriptors in one pass, before any statement executes
    /// </summary>
    /// <param name="name">table name</param>
    /// <param name="columns">column descriptors of the table</param>
    /// <param name="indices">index descriptors of the table</param>
    /// <param name="uniques">unique constraint descriptors of the table</param>
    public static void TableModel(string name, IEnumerable<ColumnDescriptor> columns, IEnumerable<IndexDescriptor> indices, IEnumerable<UniqueDescriptor> uniques) {
        Qualified(name, "table");

        if(columns != null)
            foreach(ColumnDescriptor column in columns)
                Column(column);

        if(indices != null)
            foreach(IndexDescriptor index in indices)
                Index(index);

        if(uniques != null)
            foreach(UniqueDescriptor unique in uniques)
                Unique(unique);
    }
}
