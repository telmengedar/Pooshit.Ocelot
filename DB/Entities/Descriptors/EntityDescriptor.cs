using System;
using System.Collections.Generic;
using System.Reflection;
using NightlyCode.DB.Entities.Attributes;

namespace NightlyCode.DB.Entities.Descriptors
{

    /// <summary>
    /// descriptor of an entity
    /// </summary>
    public class EntityDescriptor {
        static readonly Dictionary<Type, EntityDescriptor> descriptors = new Dictionary<Type, EntityDescriptor>();

        readonly Dictionary<string, EntityColumnDescriptor> properties = new Dictionary<string, EntityColumnDescriptor>();
        readonly Dictionary<string, EntityColumnDescriptor> columndescriptors = new Dictionary<string, EntityColumnDescriptor>();
        readonly List<IndexDescriptor> indices = new List<IndexDescriptor>();
        readonly List<UniqueDescriptor> uniques = new List<UniqueDescriptor>();

        /// <summary>
        /// creates a new <see cref="EntityDescriptor"/>
        /// </summary>
        /// <param name="tablename"></param>
        public EntityDescriptor(string tablename) {
            TableName = tablename;
        }

        /// <summary>
        /// adds a column to the descriptor
        /// </summary>
        /// <param name="column"></param>
        internal void AddColumn(EntityColumnDescriptor column)
        {
            columndescriptors[column.Name] = column;
            properties[column.Property.Name] = column;
            if(column.PrimaryKey)
                PrimaryKeyColumn = column;
        }

        /// <summary>
        /// adds an index for the entity
        /// </summary>
        /// <param name="index"></param>
        internal void AddIndex(IndexDescriptor index) {
            indices.Add(index);
        }

        /// <summary>
        /// adds an unique descriptor for the entity
        /// </summary>
        /// <param name="unique"></param>
        internal void AddUnique(UniqueDescriptor unique)
        {
            uniques.Add(unique);
        }

        /// <summary>
        /// the primary key column of the entity
        /// </summary>
        public EntityColumnDescriptor PrimaryKeyColumn { get; private set; }

        /// <summary>
        /// columns of the entity
        /// </summary>
        public IEnumerable<EntityColumnDescriptor> Columns => columndescriptors.Values;

        /// <summary>
        /// indices of the entity
        /// </summary>
        public IEnumerable<IndexDescriptor> Indices => indices;

        /// <summary>
        /// unique columns for entity
        /// </summary>
        public IEnumerable<UniqueDescriptor> Uniques => uniques;

        /// <summary>
        /// name of the table
        /// </summary>
        public string TableName { get; private set; }

        /// <summary>
        /// get the full column descriptor for the column
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public EntityColumnDescriptor GetColumn(string column) {
            return columndescriptors[column];
        }

        /// <summary>
        /// get the full column descriptor for the property
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public EntityColumnDescriptor GetColumnByProperty(string property) {
            return properties[property];
        }

        /// <summary>
        /// creates a new entity descriptor for a type
        /// </summary>
        /// <param name="type">type for which to create entity descriptor</param>
        /// <returns>entitydescriptor for specified type</returns>
        public static EntityDescriptor Create(Type type)
        {
            TableAttribute tableattribute = TableAttribute.Get(type);
            string tablename = tableattribute == null ? type.Name.ToLower() : tableattribute.Table;

            EntityDescriptor descriptor = new EntityDescriptor(tablename);

            Dictionary<string, List<string>> indices = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> unique = new Dictionary<string, List<string>>();

            PropertyInfo[] propertyinfos = type.GetProperties();
            foreach (PropertyInfo propertyinfo in propertyinfos)
            {
                if (IgnoreAttribute.HasIgnore(propertyinfo))
                    continue;

                ColumnAttribute column = ColumnAttribute.Get(propertyinfo);
                string columnname = column == null ? propertyinfo.Name.ToLower() : column.Column;

                EntityColumnDescriptor columndescriptor = new EntityColumnDescriptor(columnname, propertyinfo) {
                    PrimaryKey = PrimaryKeyAttribute.IsPrimaryKey(propertyinfo),
                    AutoIncrement = AutoIncrementAttribute.IsAutoIncrement(propertyinfo),
                    NotNull = NotNullAttribute.HasNotNull(propertyinfo) || (propertyinfo.PropertyType.IsValueType && !(propertyinfo.PropertyType.IsGenericType && propertyinfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))),
                };

                if (!columndescriptor.PrimaryKey)
                    columndescriptor.DefaultValue = DefaultValueAttribute.GetDefaultValue(propertyinfo);

                IndexAttribute[] ia = propertyinfo.GetCustomAttributes(typeof(IndexAttribute), true) as IndexAttribute[];
                if (ia != null)
                {
                    foreach (IndexAttribute indexattr in ia)
                    {
                        List<string> columns;
                        string indexname = indexattr.Name ?? columnname;
                        if (!indices.TryGetValue(indexname, out columns))
                            indices[indexname] = columns = new List<string>();
                        columns.Add(columnname);
                    }
                }

                UniqueAttribute[] ua = propertyinfo.GetCustomAttributes(typeof(UniqueAttribute), true) as UniqueAttribute[];
                if (ua != null)
                {
                    foreach (UniqueAttribute uniqueattr in ua)
                    {
                        if (uniqueattr.Name == null)
                        {
                            columndescriptor.IsUnique = true;
                            continue;
                        }

                        List<string> columns;
                        if (!unique.TryGetValue(uniqueattr.Name, out columns))
                            unique[uniqueattr.Name] = columns = new List<string>();
                        columns.Add(columnname);
                    }
                }
                descriptor.AddColumn(columndescriptor);
            }

            foreach (KeyValuePair<string, List<string>> kvp in indices)
                descriptor.AddIndex(new IndexDescriptor(kvp.Key, kvp.Value));
            foreach (KeyValuePair<string, List<string>> kvp in unique)
            {
                descriptor.AddUnique(new UniqueDescriptor(kvp.Key, kvp.Value));
            }

            return descriptor;
        }

        /// <summary>
        /// get entitydescriptor for the specified type
        /// </summary>
        /// <typeparam name="T">type of which to get entity descriptor</typeparam>
        /// <returns>entity descriptor for specified type</returns>
        public static EntityDescriptor Get<T>() {
            return Get(typeof(T));
        }

        /// <summary>
        /// get entitydescriptor for the specified type
        /// </summary>
        /// <param name="type">type of which to get entity descriptor</param>
        /// <returns>entity descriptor for specified type</returns>
        public static EntityDescriptor Get(Type type)
        {
            EntityDescriptor descriptor;
            if (!descriptors.TryGetValue(type, out descriptor))
                descriptors[type] = descriptor = Create(type);
            return descriptor;
        }

    }
}
