﻿using System;

namespace Pooshit.Ocelot.Entities.Attributes
{

    /// <summary>
    /// specifies that the value of the column must be unique
    /// </summary>
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Field, AllowMultiple = true)]
    public class UniqueAttribute : Attribute
    {

        /// <summary>
        /// creates a new unique attribute
        /// </summary>
        public UniqueAttribute() { }

        /// <summary>
        /// creates a new unique attribute
        /// </summary>
        /// <param name="name">name used to combine several properties to one unique statement</param>
        public UniqueAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// name used to combine several properties to one unique statement
        /// </summary>
        public string Name { get; }
    }
}
