using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities.Attributes;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Extern;
using Pooshit.Ocelot.Schemas;

namespace Pooshit.Ocelot.Entities.Schema {

    /// <summary>
    /// updates database schemata
    /// </summary>
    public class SchemaUpdater {
        readonly EntityDescriptorCache modelcache;
        readonly SchemaCreator creator;

        /// <summary>
        /// creates a new <see cref="SchemaUpdater"/>
        /// </summary>
        /// <param name="modelcache">access to entity models</param>
        public SchemaUpdater(EntityDescriptorCache modelcache) {
            this.modelcache = modelcache;
            creator = new SchemaCreator(modelcache);
        }

        /// <summary>
        /// updates the schema of the specified type
        /// </summary>
        /// <typeparam name="T">schema type to update</typeparam>
        /// <param name="client">database connection</param>
        /// <param name="datasource">table from which to update schema (optional)</param>
        public void Update<T>(IDBClient client, string datasource = null) {
            SchemaDescriptor schema = datasource == null ? client.DBInfo.GetSchema(client, modelcache.Get<T>().TableName) : client.DBInfo.GetSchema(client, datasource);
            Update<T>(client, schema);
        }

        /// <summary>
        /// updates the schema of the specified type
        /// </summary>
        /// <typeparam name="T">schema type to update</typeparam>
        /// <param name="client">database connection</param>
        /// <param name="schema">schema to use to update</param>
        public void Update<T>(IDBClient client, SchemaDescriptor schema) {
            if(schema is ViewDescriptor descriptor)
                UpdateView<T>(client, descriptor);
            else if(schema is TableDescriptor tableDescriptor)
                UpdateTable<T>(client, tableDescriptor);
            else
                throw new Exception("Invalid descriptor type");
        }

        string NormalizedSqlHash(string text) {
            return new string(text.Where(char.IsLetterOrDigit).ToArray());
        }

        /// <summary>
        /// extract view creation sql from CREATE VIEW content
        /// </summary>
        /// <param name="text">text from which to extract sql</param>
        /// <returns>extracted text</returns>
        public string GetViewCreationSql(string text) {
            Match match = Regex.Match(text, "^CREATE[\\s]+VIEW[\\s]+[a-zA-Z0-9_-]+[\\s]+AS[\\s]+(?<sql>.*)", RegexOptions.Singleline);
            if(match.Success)
                return match.Groups["sql"].Value;
            return null;
        }

        void UpdateView<T>(IDBClient client, ViewDescriptor view) {
            string existing = NormalizedSqlHash(view.SQL);
            string required = NormalizedSqlHash(GetViewCreationSql(ViewAttribute.GetViewDefinition(typeof(T))));
            if(existing == required)
                return;

            client.DBInfo.DropView(client, view);
            creator.Create(typeof(T), client);
        }

        void UpdateTable<T>(IDBClient client, TableDescriptor currentschema) {
            Logger.Info(this, $"Checking schema of '{typeof(T).Name}'");

            EntityDescriptor descriptor = modelcache.Get<T>();

            List<EntityColumnDescriptor> missing = [];
            List<EntityColumnDescriptor> altered = [];
            List<string> obsolete = [];

            missing.AddRange(descriptor.Columns.Where(c => currentschema.Columns.All(sc => sc.Name != c.Name)));
            missing.ForEach(c => Logger.Info(this, $"Detected missing column '{c.Name}'"));
            foreach(ColumnDescriptor column in currentschema.Columns) {
                EntityColumnDescriptor entitycolumn = descriptor.Columns.FirstOrDefault(c => c.Name == column.Name);
                if(entitycolumn == null) {
                    Logger.Info(this, $"Detected obsolete column '{column.Name}'");
                    obsolete.Add(column.Name);
                }
                else {
                    if(!client.DBInfo.IsTypeEqual(column.Type, client.DBInfo.GetDBType(entitycolumn.Property.PropertyType, SizeAttribute.GetLength(entitycolumn.Property)))
                        || column.PrimaryKey != entitycolumn.PrimaryKey
                        || column.AutoIncrement != entitycolumn.AutoIncrement
                        || column.IsUnique != entitycolumn.IsUnique
                        || column.NotNull != entitycolumn.NotNull
                    /* default value is not evaluated for now
                   || column.DefaultValue != entitycolumn.DefaultValue*/
                    ) {
                        Logger.Info(this, $"Detected altered column '{entitycolumn.Name}'", $"New -> {entitycolumn} {client.DBInfo.GetDBType(entitycolumn.Property.PropertyType, SizeAttribute.GetLength(entitycolumn.Property))}\r\nOld -> {column} {column.Type}");
                        altered.Add(entitycolumn);
                    }
                }
            }

            // check if anything changed at all
            if(obsolete.Count == 0 && missing.Count == 0 && altered.Count == 0 && currentschema.Uniques.Equals(descriptor.Uniques) && currentschema.Indices.Equals(descriptor.Indices))
                return;

            bool recreatetable = client.DBInfo.MustRecreateTable(obsolete.ToArray(), altered.ToArray(), missing.ToArray(), currentschema, descriptor);

            if(recreatetable) {
                RecreateTable(client, currentschema, descriptor);
            }
            else {
                using Transaction transaction = client.Transaction();
                if(obsolete.Count > 0 || missing.Count > 0 || altered.Count > 0) {
                    AlterTableOperation alteroperation = new(client, descriptor.TableName);
                    alteroperation.Drop(obsolete.ToArray());
                    alteroperation.Add(missing.ToArray());
                    alteroperation.Modify(altered.ToArray());
                    alteroperation.Prepare().Execute(transaction);
                }

                if(!currentschema.Indices.Equals(descriptor.Indices))
                    UpdateIndices(client, currentschema, descriptor, transaction);
                if(!currentschema.Uniques.Equals(descriptor.Uniques))
                    UpdateUniques(client, currentschema, descriptor, transaction);

                transaction?.Commit();
            }
        }

        void RecreateTable(IDBClient client, TableDescriptor olddescriptor, EntityDescriptor newdescriptor) {
            string appendix = "_original";

            using Transaction transaction = client.Transaction();

            // check if an old backup table exists for whatever reason
            if(client.DBInfo.CheckIfTableExists(client, "{olddescriptor.Name}{appendix}", transaction))
                client.NonQuery(transaction, $"DROP TABLE {olddescriptor.Name}{appendix}");

            client.NonQuery(transaction, $"ALTER TABLE {olddescriptor.Name} RENAME TO {olddescriptor.Name}{appendix}");

            ColumnDescriptor[] remainingcolumns = olddescriptor.Columns.Where(c => newdescriptor.Columns.Any(c1 => c1.Name == c.Name)).ToArray();
            EntityColumnDescriptor[] newcolumns = newdescriptor.Columns.Where(c => c.NotNull && !c.AutoIncrement && c.DefaultValue == null && olddescriptor.Columns.All(o => o.Name != c.Name)).ToArray();
            string columnlist = string.Join(", ", remainingcolumns.Select(c => client.DBInfo.MaskColumn(c.Name)));
            string newcolumnlist = string.Join(", ", newcolumns.Select(c => client.DBInfo.MaskColumn(c.Name)));
            creator.CreateTable(client, newdescriptor, transaction);

            // transfer data to new table
            if(newcolumns.Length == 0)
                client.NonQuery(transaction, $"INSERT INTO {newdescriptor.TableName} ({columnlist}) SELECT {columnlist} FROM {olddescriptor.Name}{appendix}");
            else {
                // new schema has columns which mustn't be null
                OperationPreparator operation = new OperationPreparator();
                operation.AppendText($"INSERT INTO {newdescriptor.TableName} ({columnlist},{newcolumnlist}) SELECT {columnlist}");
                foreach(EntityColumnDescriptor column in newcolumns) {
                    operation.AppendText(",");
                    if(column.DefaultValue != null)
                        operation.AppendParameter(column.DefaultValue);
                    else
                        operation.AppendParameter(column.CreateDefaultValue());
                }

                operation.AppendText($"FROM {olddescriptor.Name}{appendix}");
                operation.GetOperation(client, false).Execute(transaction);
            }

            // remove old data
            client.NonQuery(transaction, $"DROP TABLE {olddescriptor.Name}{appendix}");
            transaction?.Commit();
        }

        void UpdateUniques(IDBClient client, TableDescriptor oldschema, EntityDescriptor newschema, Transaction transaction) {
            //List<UniqueDescriptor> obsolete = new List<UniqueDescriptor>(oldschema.Uniques.Except(newschema.Uniques));
            List<UniqueDescriptor> missing = new(newschema.Uniques.Except(oldschema.Uniques));

            //foreach (UniqueDescriptor drop in obsolete)
            //client.NonQuery(transaction, $"ALTER TABLE {newschema.TableName} DROP UNIQUE ({string.Join(",", drop.Columns.Select(c => client.DBInfo.MaskColumn(c)))});");
            foreach(UniqueDescriptor add in missing)
                client.NonQuery(transaction, $"ALTER TABLE {newschema.TableName} ADD UNIQUE ({string.Join(",", add.Columns.Select(c => client.DBInfo.MaskColumn(c)))});");
        }

        void UpdateIndices(IDBClient client, TableDescriptor oldschema, EntityDescriptor newschema, Transaction transaction) {
            List<IndexDescriptor> missing = new(newschema.Indices.Where(i => oldschema.Indices.All(i2 => i2.Name != i.Name)));
            List<IndexDescriptor> altered = new();
            List<string> obsolete = new();

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
                Logger.Info(this, $"Detected altered index definition for '{string.Join(", ", altered.Select(i => i.Name))}'");
                altered.ForEach(c => client.NonQuery(transaction, $"DROP INDEX idx_{newschema.TableName}_{c}"));
                creator.CreateIndices(client, newschema.TableName, altered, transaction);
            }

            if(missing.Count > 0) {
                Logger.Info(this, $"Detected missing index '{string.Join(", ", missing.Select(c => c.Name))}'");
                creator.CreateIndices(client, newschema.TableName, missing, transaction);
            }
        }
    }
}