using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NightlyCode.Database.Entities.Attributes;

namespace NightlyCode.Database.Entities.Descriptors {

    /// <summary>
    /// descriptor of an entity
    /// </summary>
    public class EntityDescriptor {
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
        internal void AddColumn(EntityColumnDescriptor column) {
            columndescriptors[column.Name] = column;
            properties[column.Property.Name] = column;
            if(column.PrimaryKey)
                PrimaryKeyColumn = column;
        }

        internal void RemoveColumn(EntityColumnDescriptor column) {
            columndescriptors.Remove(column.Name);
            properties.Remove(column.Property.Name);
            if(column.PrimaryKey)
                PrimaryKeyColumn = null;
        }

        /// <summary>
        /// changes the column name in model
        /// </summary>
        /// <param name="column">column to modify</param>
        /// <param name="name">new name of column</param>
        internal void ChangeColumnName(EntityColumnDescriptor column, string name) {
            columndescriptors.Remove(column.Name);
            column.Name = name;
            columndescriptors[name] = column;
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
        internal void AddUnique(UniqueDescriptor unique) {
            uniques.Add(unique);
        }

        /// <summary>
        /// removes a unique descriptor from entity model
        /// </summary>
        /// <param name="columns">columns which make up the unique</param>
        internal void RemoveUnique(string[] columns) {
            uniques.RemoveAll(u => u.Columns.SequenceEqual(columns));
        }

        /// <summary>
        /// the primary key column of the entity
        /// </summary>
        public EntityColumnDescriptor PrimaryKeyColumn { get; internal set; }

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
        public string TableName { get; internal set; }

        /// <summary>
        /// get the full column descriptor for the column
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public EntityColumnDescriptor GetColumn(string column) {
            if(!columndescriptors.TryGetValue(column, out EntityColumnDescriptor descriptor))
                throw new KeyNotFoundException($"Column '{column}' not found in entity descriptor. Known columns:\n{string.Join("\n", columndescriptors.Keys)}");
            return descriptor;
        }

        /// <summary>
        /// get the full column descriptor for the property
        /// </summary>
        /// <param name="property">name of property of which to return column model</param>
        /// <returns>column model linked to property</returns>
        public EntityColumnDescriptor GetColumnByProperty(string property) {
            return properties[property];
        }

        /// <summary>
        /// get the full column descriptor for the property
        /// </summary>
        /// <param name="property">name of property of which to return column model</param>
        /// <returns>true if property was found, false otherwise</returns>
        public bool TryGetColumnByProperty(string property, out EntityColumnDescriptor columnmodel) {
            return properties.TryGetValue(property, out columnmodel);
        }

        internal static EntityColumnDescriptor CreateColumnDescriptor(PropertyInfo property) {
            ColumnAttribute column = ColumnAttribute.Get(property);
            string columnname = column == null ? property.Name.ToLower() : column.Column;

            EntityColumnDescriptor columndescriptor = new EntityColumnDescriptor(columnname, property) {
                PrimaryKey = PrimaryKeyAttribute.IsPrimaryKey(property),
                AutoIncrement = AutoIncrementAttribute.IsAutoIncrement(property),
                NotNull = NotNullAttribute.HasNotNull(property) || (property.PropertyType.IsValueType && !(property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))),
            };

            if(!columndescriptor.PrimaryKey)
                columndescriptor.DefaultValue = DefaultValueAttribute.GetDefaultValue(property);

            return columndescriptor;
        }

        /// <summary>
        /// creates a new entity descriptor for a type
        /// </summary>
        /// <param name="type">type for which to create entity descriptor</param>
        /// <returns>entitydescriptor for specified type</returns>
        public static EntityDescriptor Create(Type type) {
            TableAttribute tableattribute = TableAttribute.Get(type);
            string tablename = tableattribute == null ? type.Name.ToLower() : tableattribute.Table;

            EntityDescriptor descriptor = new EntityDescriptor(tablename);

            Dictionary<string, List<string>> indices = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> unique = new Dictionary<string, List<string>>();

            PropertyInfo[] propertyinfos = type.GetProperties();
            foreach(PropertyInfo propertyinfo in propertyinfos) {
                if(IgnoreAttribute.HasIgnore(propertyinfo))
                    continue;

                EntityColumnDescriptor columndescriptor = CreateColumnDescriptor(propertyinfo);

                if(propertyinfo.GetCustomAttributes(typeof(IndexAttribute), true) is IndexAttribute[] ia) {
                    foreach(IndexAttribute indexattr in ia) {
                        string indexname = indexattr.Name ?? columndescriptor.Name;
                        if(!indices.TryGetValue(indexname, out List<string> columns))
                            indices[indexname] = columns = new List<string>();
                        columns.Add(columndescriptor.Name);
                    }
                }

                if(propertyinfo.GetCustomAttributes(typeof(UniqueAttribute), true) is UniqueAttribute[] ua) {
                    foreach(UniqueAttribute uniqueattr in ua) {
                        if(uniqueattr.Name == null) {
                            columndescriptor.IsUnique = true;
                            continue;
                        }

                        if(!unique.TryGetValue(uniqueattr.Name, out List<string> columns))
                            unique[uniqueattr.Name] = columns = new List<string>();
                        columns.Add(columndescriptor.Name);
                    }
                }

                descriptor.AddColumn(columndescriptor);
            }

            foreach(KeyValuePair<string, List<string>> kvp in indices)
                descriptor.AddIndex(new IndexDescriptor(kvp.Key, kvp.Value));
            foreach(KeyValuePair<string, List<string>> kvp in unique) {
                descriptor.AddUnique(new UniqueDescriptor(kvp.Value));
            }

            return descriptor;
        }
    }
}
