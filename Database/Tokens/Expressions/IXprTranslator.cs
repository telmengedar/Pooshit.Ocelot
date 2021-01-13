using System.Linq.Expressions;
using NightlyCode.Database.Entities.Operations.Prepared;

namespace NightlyCode.Database.Tokens.Expressions {
    
    /// <summary>
    /// translates expressions of <see cref="Xpr"/> to Sql
    /// </summary>
    public interface IXprTranslator {
        
        /// <summary>
        /// translates an <see cref="Xpr"/> method call to sql
        /// </summary>
        /// <param name="methodcall">method call to translate</param>
        /// <param name="operation">sql operation</param>
        /// <returns>expression node</returns>
        Expression TranslateMethodCall(MethodCallExpression methodcall, IOperationPreparator operation);

        /// <summary>
        /// translates an <see cref="Xpr"/> property to sql
        /// </summary>
        /// <param name="member">member to translate</param>
        /// <param name="operation">sql operation to add text to</param>
        /// <returns>expression node</returns>
        Expression TranslateProperty(MemberExpression member, IOperationPreparator operation);
    }
}