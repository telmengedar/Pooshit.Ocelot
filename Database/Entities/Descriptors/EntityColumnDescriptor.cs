using System;
using System.Reflection;

namespace NightlyCode.Database.Entities.Descriptors
{
    /// <summary>
    /// descriptor for a column
    /// </summary>
    public class EntityColumnDescriptor : ColumnDescriptor {

        /// <summary>
        /// creates a new <see cref="EntityColumnDescriptor"/>
        /// </summary>
        protected EntityColumnDescriptor() { }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="property"></param>
        public EntityColumnDescriptor(string name, PropertyInfo property)
            : base(name)
        {
            Property = property;
        }

        /// <summary>
        /// get the value of an entity corresponding to the column
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public object GetValue(object entity)
        {
            return Property.GetValue(entity, null);
        }

        /// <summary>
        /// set the value to the column
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="value"></param>
        public void SetValue(object entity, object value)
        {
            Property.SetValue(entity, value, null);
        }

        /// <summary>
        /// property the column is linked to
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// creates a default value for this column
        /// </summary>
        /// <returns>value which can be used as a default</returns>
        public object CreateDefaultValue()
        {
            if (!Property.PropertyType.IsValueType)
                return null;

            return Activator.CreateInstance(Property.PropertyType);
        }
    }
}
