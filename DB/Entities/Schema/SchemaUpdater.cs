using System;
using System.Collections.Generic;
using System.Linq;
using NightlyCode.DB.Clients;
using NightlyCode.DB.Entities.Descriptors;
using NightlyCode.DB.Extern;

namespace NightlyCode.DB.Entities.Schema {

    /// <summary>
    /// updates database schemata
    /// </summary>
    public class SchemaUpdater {
        readonly SchemaCreator creator = new SchemaCreator();

        public void Update<T>(IDBClient client) {
            SchemaDescriptor schema = client.DBInfo.GetSchema<T>(client);
            if(schema is ViewDescriptor)
                UpdateView<T>(client, (ViewDescriptor)schema);
            else if(schema is TableDescriptor)
                UpdateTable<T>(client, (TableDescriptor)schema);
            else throw new Exception("Invalid descriptor type");
        }

        void UpdateView<T>(IDBClient client, ViewDescriptor view) {
            client.NonQuery("DROP VIEW " + view.Name);
            creator.Create(typeof(T), client);
        }

        void UpdateTable<T>(IDBClient client, TableDescriptor currentschema) {
            Logger.Info(this, $"Checking schema of '{typeof(T).Name}'");

            EntityDescriptor descriptor = EntityDescriptor.Get<T>();

            List<EntityColumnDescriptor> missing = new List<EntityColumnDescriptor>();
            List<EntityColumnDescriptor> altered = new List<EntityColumnDescriptor>();
            List<string> obsolete = new List<string>();

            missing.AddRange(descriptor.Columns.Where(c => currentschema.Columns.All(sc => sc.Name != c.Name)));
            missing.ForEach(c => Logger.Info(this, $"Detected missing column '{c.Name}'"));
            foreach(SchemaColumnDescriptor column in currentschema.Columns) {
                EntityColumnDescriptor entitycolumn = descriptor.Columns.FirstOrDefault(c => c.Name == column.Name);
                if(entitycolumn == null) {
                    Logger.Info(this, $"Detected obsolete column '{column.Name}'");
                    obsolete.Add(column.Name);
                }
                else {
                    if(!AreTypesEqual(column.Type, client.DBInfo.GetDBType(entitycolumn.Property.PropertyType)) 
                       || column.PrimaryKey != entitycolumn.PrimaryKey
                       || column.AutoIncrement != entitycolumn.AutoIncrement
                       || column.IsUnique != entitycolumn.IsUnique
                       || column.NotNull != entitycolumn.NotNull
                        /* default value is not evaluated for now
                       || column.DefaultValue != entitycolumn.DefaultValue*/
                        ) {
                        Logger.Info(this, $"Detected altered column '{entitycolumn.Name}'", $"New -> {entitycolumn} {client.DBInfo.GetDBType(entitycolumn.Property.PropertyType)}\r\nOld -> {column} {column.Type}");
                        altered.Add(entitycolumn);
                    }
                }
            }

            bool recreatetable = obsolete.Count > 0 || altered.Count > 0 || missing.Any(m => m.IsUnique || m.PrimaryKey);
            if(recreatetable) {
                RecreateTable(client, currentschema, descriptor);
            }
            else {
                if(missing.Count > 0) {
                    missing.ForEach(c => client.DBInfo.AddColumn(client, descriptor.TableName, c));
                    UpdateIndices(client, currentschema, descriptor);
                }
            }
        }

        bool AreTypesEqual(string lhs, string rhs) {
            if(lhs == "TEXT" || lhs == "VARCHAR")
                return rhs == "TEXT" || rhs == "VARCHAR";
            return lhs == rhs;
        }

        void RecreateTable(IDBClient client, TableDescriptor olddescriptor, EntityDescriptor newdescriptor) {
            // rename old table
            client.NonQuery($"ALTER TABLE {olddescriptor.Name} RENAME TO {olddescriptor.Name}_original");

            SchemaColumnDescriptor[] remainingcolumns = olddescriptor.Columns.Where(c => newdescriptor.Columns.Any(c1 => c1.Name == c.Name)).ToArray();
#if UNITY
            string columnlist = string.Join(", ", remainingcolumns.Select(c => c.Name).ToArray());
#else
            string columnlist = string.Join(", ", remainingcolumns.Select(c => c.Name));
#endif
            creator.CreateTable(client, newdescriptor);

            // transfer data to new table
            client.NonQuery($"INSERT INTO {newdescriptor.TableName} ({columnlist}) SELECT {columnlist} FROM {olddescriptor.Name}_original");

            // remove old data
            client.NonQuery($"DROP TABLE {olddescriptor.Name}_original");
        }

        void UpdateIndices(IDBClient client, TableDescriptor oldschema, EntityDescriptor newschema) {
            List<IndexDescriptor> missing = new List<IndexDescriptor>(newschema.Indices.Where(i => oldschema.Indices.All(i2 => i2.Name != i.Name)));
            List<IndexDescriptor> altered = new List<IndexDescriptor>();
            List<string> obsolete = new List<string>();

            foreach(IndexDescriptor index in oldschema.Indices) {
                IndexDescriptor newdefinition = newschema.Indices.FirstOrDefault(i => i.Name == index.Name);
                if(newdefinition == null)
                    obsolete.Add(index.Name);
                else {
                    if(!newdefinition.Columns.SequenceEqual(index.Columns))
                        altered.Add(newdefinition);
                }
            }

            if(altered.Count > 0) {
#if UNITY
                Logger.Info(this, $"Detected altered index definition for '{string.Join(", ", altered.Select(i => i.Name).ToArray())}'");
#else
                Logger.Info(this, $"Detected altered index definition for '{string.Join(", ", altered.Select(i => i.Name))}'");
#endif
                altered.ForEach(c => client.NonQuery($"DROP INDEX idx_{newschema.TableName}_{c}"));
                creator.CreateIndices(client, newschema.TableName, altered);
            }

            if(missing.Count > 0) {
#if UNITY
                Logger.Info(this, $"Detected missing index '{string.Join(", ", missing.Select(c => c.Name).ToArray())}'");
#else
                Logger.Info(this, $"Detected missing index '{string.Join(", ", missing.Select(c => c.Name))}'");
#endif
                creator.CreateIndices(client, newschema.TableName, missing);
            }
        }
    }
}