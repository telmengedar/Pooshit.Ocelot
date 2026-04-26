using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Pooshit.Ocelot.Entities.Attributes;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Schemas {
    
    /// <summary>
    /// used to create a database schema
    /// </summary>
    public class SchemaCreator {

        /// <summary>
        /// creates a schema for the specified type
        /// </summary>
        /// <param name="dbInfo">database specific info</param>
        /// <typeparam name="T">type for which to create a schema</typeparam>
        /// <returns>schema for specified type</returns>
        public Schema Create<T>(IDBInfo dbInfo) {
            return Create(typeof(T), dbInfo);
        }

        /// <summary>
        /// creates a schema for the specified type
        /// </summary>
        /// <param name="type">type for which to create a schema</param>
        /// <param name="dbInfo">database specific info</param>
        /// <returns>schema for specified type</returns>
        public Schema Create(Type type, IDBInfo dbInfo) {
            TableAttribute tableattribute = TableAttribute.Get(type);
            string tablename = tableattribute == null ? type.Name.ToLower() : tableattribute.Table;

            if (Attribute.GetCustomAttribute(type, typeof(ViewAttribute)) is ViewAttribute attribute) {
                Stream viewDefinitionStream = type.Assembly.GetManifestResourceStream(attribute.Definition);
                if (viewDefinitionStream == null)
                    throw new InvalidOperationException($"Resource '{attribute.Definition}' not found");
                
                using StreamReader reader = new(viewDefinitionStream);
                return new ViewSchema {
                    Type = SchemaType.View,
                    Name = tablename,
                    Definition = reader.ReadToEnd()
                };
            }

            TableSchema schema = new() {
                Name = tablename
            };

            List<ColumnDescriptor> schemaColumns = new();
            Dictionary<string, IndexInformation> indices = new();
            Dictionary<string, List<string>> unique = new();

            PropertyInfo[] propertyinfos = type.GetProperties();
            foreach(PropertyInfo propertyinfo in propertyinfos) {
                if(IgnoreAttribute.HasIgnore(propertyinfo))
                    continue;

                ColumnDescriptor columndescriptor = CreateColumnDescriptor(propertyinfo, dbInfo);

                if(propertyinfo.GetCustomAttributes(typeof(IndexAttribute), true) is IndexAttribute[] ia) {
                    foreach(IndexAttribute indexattr in ia) {
                        string indexname = indexattr.Name ?? columndescriptor.Name;
                        if(!indices.TryGetValue(indexname, out IndexInformation columns))
                            indices[indexname] = columns = new IndexInformation(indexattr.Type);
                        columns.Columns.Add(columndescriptor.Name);
                    }
                }

                if(propertyinfo.GetCustomAttributes(typeof(UniqueAttribute), true) is UniqueAttribute[] ua) {
                    foreach(UniqueAttribute uniqueattr in ua) {
                        string uniquename = uniqueattr.Name ?? columndescriptor.Name;
                        if(uniqueattr.Name == null)
                            columndescriptor.IsUnique = true;

                        if(!unique.TryGetValue(uniquename, out List<string> columns))
                            unique[uniquename] = columns = [];
                        columns.Add(columndescriptor.Name);
                    }
                }

                schemaColumns.Add(columndescriptor);
            }

            schema.Columns = schemaColumns.ToArray();
            schema.Index = indices.Select(kvp => new IndexDescriptor(kvp.Key, kvp.Value.Columns, kvp.Value.Type)).ToArray();

            List<UniqueDescriptor> uniqueDescriptors = [];
            foreach(KeyValuePair<string, List<string>> kvp in unique) {
                uniqueDescriptors.Add(new UniqueDescriptor(kvp.Key, kvp.Value));
                if(kvp.Value.Count == 1 && schemaColumns.FirstOrDefault(c => c.Name == kvp.Value[0]) is { } col)
                    col.IsUnique = true;
            }
            schema.Unique = [..uniqueDescriptors];

            return schema;
        }

        static ColumnDescriptor CreateColumnDescriptor(PropertyInfo property, IDBInfo dbInfo) {
            ColumnAttribute column = ColumnAttribute.Get(property);
            string columnname = column == null ? property.Name.ToLower() : column.Column;

            ColumnDescriptor columndescriptor = new(columnname, dbInfo.GetDBType(property.PropertyType, -1)) {
                PrimaryKey = PrimaryKeyAttribute.IsPrimaryKey(property),
                AutoIncrement = AutoIncrementAttribute.IsAutoIncrement(property),
                NotNull = NotNullAttribute.HasNotNull(property) || (property.PropertyType.IsValueType && !(property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))),
                Length = SizeAttribute.GetLength(property)
            };

            if(!columndescriptor.PrimaryKey)
                columndescriptor.DefaultValue = DefaultValueAttribute.GetDefaultValue(property);

            return columndescriptor;
        }

    }
}