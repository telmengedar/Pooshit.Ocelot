using System;
using System.Collections.Generic;
using System.Linq;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Extern;

namespace NightlyCode.Database.Entities.Schema {

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
        public void Update<T>(IDBClient client, string datasource=null) {
            SchemaDescriptor schema = datasource == null ? client.DBInfo.GetSchema(client, modelcache.Get<T>().TableName) : client.DBInfo.GetSchema(client, datasource);
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

        void UpdateTable<T>(IDBClient client, TableDescriptor currentschema)
        {
            Logger.Info(this, $"Checking schema of '{typeof(T).Name}'");

            EntityDescriptor descriptor = modelcache.Get<T>();

            List<EntityColumnDescriptor> missing = new List<EntityColumnDescriptor>();
            List<EntityColumnDescriptor> altered = new List<EntityColumnDescriptor>();
            List<string> obsolete = new List<string>();

            missing.AddRange(descriptor.Columns.Where(c => currentschema.Columns.All(sc => sc.Name != c.Name)));
            missing.ForEach(c => Logger.Info(this, $"Detected missing column '{c.Name}'"));
            foreach (SchemaColumnDescriptor column in currentschema.Columns)
            {
                EntityColumnDescriptor entitycolumn = descriptor.Columns.FirstOrDefault(c => c.Name == column.Name);
                if (entitycolumn == null)
                {
                    Logger.Info(this, $"Detected obsolete column '{column.Name}'");
                    obsolete.Add(column.Name);
                }
                else
                {
                    if (!AreTypesEqual(column.Type, client.DBInfo.GetDBType(entitycolumn.Property.PropertyType))
                        || column.PrimaryKey != entitycolumn.PrimaryKey
                        || column.AutoIncrement != entitycolumn.AutoIncrement
                        || column.IsUnique != entitycolumn.IsUnique
                        || column.NotNull != entitycolumn.NotNull
                        /* default value is not evaluated for now
                       || column.DefaultValue != entitycolumn.DefaultValue*/
                    )
                    {
                        Logger.Info(this, $"Detected altered column '{entitycolumn.Name}'", $"New -> {entitycolumn} {client.DBInfo.GetDBType(entitycolumn.Property.PropertyType)}\r\nOld -> {column} {column.Type}");
                        altered.Add(entitycolumn);
                    }
                }
            }

            bool recreatetable = obsolete.Count > 0
                                 || altered.Count > 0
                                 || missing.Any(m => m.IsUnique || m.PrimaryKey)
                                 || !currentschema.Uniques.SequenceEqual(descriptor.Uniques)
                                 || !currentschema.Indices.SequenceEqual(descriptor.Indices);

            if (recreatetable)
            {
                RecreateTable(client, currentschema, descriptor);
            }
            else
            {
                using (Transaction transaction = client.Transaction())
                {
                    if (missing.Count > 0)
                        missing.ForEach(c => client.DBInfo.AddColumn(client, descriptor.TableName, c, transaction));

                    UpdateIndices(client, currentschema, descriptor, transaction);
                    UpdateUniques(client, currentschema, descriptor, transaction);
                    transaction.Commit();
                }
            }
        }

        bool AreTypesEqual(string lhs, string rhs) {
            if(lhs == "TEXT" || lhs == "VARCHAR")
                return rhs == "TEXT" || rhs == "VARCHAR";
            return lhs == rhs;
        }

        void RecreateTable(IDBClient client, TableDescriptor olddescriptor, EntityDescriptor newdescriptor) {
            string appendix = "_original";

            using(Transaction transaction = client.Transaction()) {
                // check if an old backup table exists for whatever reason
                if(client.DBInfo.CheckIfTableExists(client, "{olddescriptor.Name}{appendix}"))
                    client.NonQuery(transaction, $"DROP TABLE {olddescriptor.Name}{appendix}");

                client.NonQuery(transaction, $"ALTER TABLE {olddescriptor.Name} RENAME TO {olddescriptor.Name}{appendix}");

                SchemaColumnDescriptor[] remainingcolumns = olddescriptor.Columns.Where(c => newdescriptor.Columns.Any(c1 => c1.Name == c.Name)).ToArray();
                EntityColumnDescriptor[] newcolumns = newdescriptor.Columns.Where(c => c.NotNull && !c.AutoIncrement && c.DefaultValue == null && olddescriptor.Columns.All(o => o.Name != c.Name)).ToArray();
#if UNITY
            string columnlist = string.Join(", ", remainingcolumns.Select(c => c.Name).ToArray());
#else
                string columnlist = string.Join(", ", remainingcolumns.Select(c => c.Name));
                string newcolumnlist = string.Join(", ", newcolumns.Select(c => c.Name));
#endif
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
                        else operation.AppendParameter(column.CreateDefaultValue());
                    }

                    operation.AppendText($"FROM {olddescriptor.Name}{appendix}");
                    operation.GetOperation(client).Execute(transaction);
                }

                // remove old data
                client.NonQuery(transaction, $"DROP TABLE {olddescriptor.Name}{appendix}");
                transaction.Commit();
            }
        }

        void UpdateUniques(IDBClient client, TableDescriptor oldschema, EntityDescriptor newschema, Transaction transaction)
        {
            //List<UniqueDescriptor> obsolete = new List<UniqueDescriptor>(oldschema.Uniques.Except(newschema.Uniques));
            List<UniqueDescriptor> missing = new List<UniqueDescriptor>(newschema.Uniques.Except(oldschema.Uniques));

            //foreach (UniqueDescriptor drop in obsolete)
                //client.NonQuery(transaction, $"ALTER TABLE {newschema.TableName} DROP UNIQUE ({string.Join(",", drop.Columns.Select(c => client.DBInfo.MaskColumn(c)))});");
            foreach (UniqueDescriptor add in missing)
                client.NonQuery(transaction, $"ALTER TABLE {newschema.TableName} ADD UNIQUE ({string.Join(",", add.Columns.Select(c => client.DBInfo.MaskColumn(c)))});");
        }

        void UpdateIndices(IDBClient client, TableDescriptor oldschema, EntityDescriptor newschema, Transaction transaction) {
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
                altered.ForEach(c => client.NonQuery(transaction, $"DROP INDEX idx_{newschema.TableName}_{c}"));
                creator.CreateIndices(client, newschema.TableName, altered, transaction);
            }

            if(missing.Count > 0) {
#if UNITY
                Logger.Info(this, $"Detected missing index '{string.Join(", ", missing.Select(c => c.Name).ToArray())}'");
#else
                Logger.Info(this, $"Detected missing index '{string.Join(", ", missing.Select(c => c.Name))}'");
#endif
                creator.CreateIndices(client, newschema.TableName, missing, transaction);
            }
        }
    }
}