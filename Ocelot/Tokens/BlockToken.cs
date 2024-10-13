using System;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Tokens {
    
    /// <summary>
    /// token which wraps another token in a block
    /// </summary>
    public class BlockToken : SqlToken {
        readonly ISqlToken block;

        /// <summary>
        /// creates a new <see cref="BlockToken"/>
        /// </summary>
        /// <param name="block">block to wrap</param>
        public BlockToken(ISqlToken block) {
            this.block = block;
        }

        /// <inheritdoc />
        public override void ToSql(IDBInfo dbinfo, IOperationPreparator preparator, Func<Type, EntityDescriptor> models, string tablealias) {
            preparator.AppendText("(");
            block.ToSql(dbinfo, preparator, models, tablealias);
            preparator.AppendText(")");
        }
    }
}