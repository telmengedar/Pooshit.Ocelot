using System;
using System.Linq.Expressions;

namespace Pooshit.Ocelot.Entities.Operations;

/// <summary>
/// operation used to join tables
/// </summary>
public class JoinOperation {
    
    /// <summary>
    /// creates a new <see cref="JoinOperation"/>
    /// </summary>
    /// <param name="type">type of entity to join</param>
    /// <param name="criterias">join criterias</param>
    /// <param name="additionalcriterias">additional criterias for join operation (optional)</param>
    /// <param name="alias">name of alias to use</param>
    /// <param name="joinop">join operation type</param>
    public JoinOperation(Type type, Expression criterias, JoinOp joinop = JoinOp.Inner, Expression additionalcriterias = null, string alias = null) {
        Type = type;
        Criterias = criterias;
        JoinType = joinop;
        AdditionalCriterias = additionalcriterias;
        Alias = alias;
    }

    /// <summary>
    /// creates a new <see cref="JoinOperation"/>
    /// </summary>
    /// <param name="operation">operation to join</param>
    /// <param name="criterias">join criterias</param>
    /// <param name="additionalcriterias">additional criterias for join operation (optional)</param>
    /// <param name="alias">name of alias to use</param>
    /// <param name="joinop">join operation type</param>
    public JoinOperation(IDatabaseOperation operation, Expression criterias, JoinOp joinop = JoinOp.Inner, Expression additionalcriterias = null, string alias = null) {
        Operation = operation;
        Criterias = criterias;
        JoinType = joinop;
        AdditionalCriterias = additionalcriterias;
        Alias = alias;
    }

    /// <summary>
    /// type to join
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// operation to join
    /// </summary>
    public IDatabaseOperation Operation { get; set; }
    
    /// <summary>
    /// criterias to use when joining tables
    /// </summary>
    public Expression Criterias { get; }

    /// <summary>
    /// criterias to use when joining tables
    /// </summary>
    public Expression AdditionalCriterias { get; }

    /// <summary>
    /// name of alias to use
    /// </summary>
    public string Alias { get; }

    /// <summary>
    /// join operation type
    /// </summary>
    public JoinOp JoinType { get; }
}