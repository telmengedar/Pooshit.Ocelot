﻿using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Entities.Operations.Prepared {

    /// <summary>
    /// token used to separate statement sections
    /// </summary>
    public class SeparatorToken : IOperationToken {

        /// <inheritdoc />
        public string GetText(IDBInfo dbinfo) {
            return ", ";
        }
    }
}