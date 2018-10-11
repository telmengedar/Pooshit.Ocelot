using System;
using System.Reflection;

namespace NightlyCode.Database.Entities.Attributes
{
    /// <summary>
    /// attribute describing the column of a value
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple=false)]
    public class ColumnAttribute : Attribute
    {
        readonly string column;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="column"></param>
        public ColumnAttribute(string column)
        {
            this.column = column;
        }

        /// <summary>
        /// name of the column
        /// </summary>
        public string Column
        {
            get { return column; }
        }

        /// <summary>
        /// get the columnattribute for the property
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static ColumnAttribute Get(PropertyInfo property) {
            return (ColumnAttribute)GetCustomAttribute(property, typeof(ColumnAttribute));
        }
    }
}
