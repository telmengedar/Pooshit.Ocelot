using System;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Entities.Operations.Tables;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Tokens;
using Pooshit.Ocelot.Tokens.Operations;

namespace Pooshit.Ocelot.Entities.Operations;

/// <inheritdoc />
public class WhereTokenOperation : IWhereTokenOperation {
    ISqlToken rootPredicate;
        
    /// <summary>
    /// appends criteria information to an <see cref="IOperationPreparator"/>
    /// </summary>
    /// <param name="dbInfo">database specific information</param>
    /// <param name="preparator">preparator to which to add predicate</param>
    protected void AppendCriterias(IDBInfo dbInfo, IOperationPreparator preparator) {
        if (rootPredicate == null)
            return;

        preparator.AppendText("WHERE");
        rootPredicate.ToSql(dbInfo, preparator, EntityDescriptor.Create, null);
    }

    /// <inheritdoc />
    public void Where(ISqlToken predicate, CriteriaOperator mergeOp = CriteriaOperator.AND) {
        if (rootPredicate == null) {
            rootPredicate = predicate;
            return;
        }

        rootPredicate = mergeOp switch {
                            CriteriaOperator.AND => new OperationToken(rootPredicate, Operand.AndAlso, predicate),
                            CriteriaOperator.OR => new(rootPredicate, Operand.OrElse, predicate),
                            _ => throw new ArgumentOutOfRangeException(nameof(mergeOp), mergeOp, null)
                        };
    }
}