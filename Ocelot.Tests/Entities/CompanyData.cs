﻿using Pooshit.Ocelot.Entities.Attributes;

namespace Pooshit.Ocelot.Tests.Entities {
    public class CompanyData {
        /// <summary>
        /// name of company
        /// </summary>
        [Unique]
        [Index("name")]
        public string Name { get; set; }

        /// <summary>
        /// company website
        /// </summary>
        [Unique]
        [Index("url")]
        public string Url { get; set; }

    }
}