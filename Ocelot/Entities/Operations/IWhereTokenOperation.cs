using Pooshit.Ocelot.Entities.Operations.Tables;
using Pooshit.Ocelot.Tokens;

namespace Pooshit.Ocelot.Entities.Operations {
    
    /// <summary>
    /// operation which allows to specify a predicate
    /// </summary>
    public interface IWhereTokenOperation {

        /// <summary>
        /// adds a predicate for the operation
        /// </summary>
        /// <param name="predicate">predicate to append</param>
        /// <param name="mergeOp">operation to use when predicate is to be merged with an existing</param>
        void Where(ISqlToken predicate, CriteriaOperator mergeOp = CriteriaOperator.AND);
    }
}