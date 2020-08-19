using System;
using NightlyCode.Database.Fields;

namespace NightlyCode.Database.Dto {
    public class Union {
        public Type Type { get; set; }

        public IDBField Columns { get; set; }
    }
}