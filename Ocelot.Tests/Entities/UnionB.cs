﻿using Pooshit.Ocelot.Entities.Attributes;

namespace Pooshit.Ocelot.Tests.Entities {
    public class UnionB {

        [PrimaryKey, AutoIncrement]
        public long CrazyId { get; set; }
        public string Name { get; set; }

        public decimal NumberOfDoom { get; set; }
    }
}