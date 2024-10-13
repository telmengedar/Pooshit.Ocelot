using System;
using System.Reflection;
using Pooshit.Ocelot.Schemas;

namespace Pooshit.Ocelot.Entities.Descriptors
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
        /// creates a new <see cref="EntityColumnDescriptor"/>
        /// </summary>
        /// <param name="name">name of column</param>
        /// <param name="property">property source of column</param>
        public EntityColumnDescriptor(string name, PropertyInfo property)
            : base(name) {
            if (property.PropertyType.IsEnum)
                Type = Enum.GetUnderlyingType(property.PropertyType).Name.ToLower();
            else Type = !(property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) ? property.PropertyType.Name.ToLower() : property.PropertyType.GetGenericArguments()[0].Name.ToLower();
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
