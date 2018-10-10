using System;

namespace Database.Entities.Attributes
{
    /// <summary>
    /// attribute describing the table the object resides in
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited=false)]
    public class TableAttribute : Attribute
    {
        readonly string table;

        /// <summary>
        /// creates a new <see cref="TableAttribute"/>
        /// </summary>
        /// <param name="table"></param>
        public TableAttribute(string table)
        {
            this.table = table;
        }

        /// <summary>
        /// table name
        /// </summary>
        public string Table => table;

        /// <summary>
        /// get the tableattribute for the specified type
        /// </summary>
        /// <param name="type">type for which to return <see cref="TableAttribute"/></param>
        /// <returns></returns>
        public static TableAttribute Get(Type type) {
            return (TableAttribute)GetCustomAttribute(type, typeof(TableAttribute));
        }
    }
}
