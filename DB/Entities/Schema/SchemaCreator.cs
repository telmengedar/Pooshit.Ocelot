using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NightlyCode.DB.Clients;
using NightlyCode.DB.Entities.Attributes;
using NightlyCode.DB.Entities.Descriptors;
using NightlyCode.DB.Entities.Operations;

namespace NightlyCode.DB.Entities.Schema {

    /// <summary>
    /// creates a schema in database
    /// </summary>
    public class SchemaCreator {
        readonly EntityDescriptorCache modelcache;

        /// <summary>
        /// creates a new <see cref="SchemaCreator"/>
        /// </summary>
        /// <param name="modelcache"></param>
        public SchemaCreator(EntityDescriptorCache modelcache) {
            this.modelcache = modelcache;
        }

        /// <summary>
        /// creates a new table from description
        /// </summary>
        /// <param name="client">database access</param>
        /// <param name="descriptor"><see cref="EntityDescriptor"/> which describes schema of entity</param>
        public void CreateTable(IDBClient client, EntityDescriptor descriptor) {
            OperationPreparator preparator = new OperationPreparator(client.DBInfo);
            preparator.CommandBuilder.Append("CREATE TABLE ");
            preparator.CommandBuilder.Append(descriptor.TableName).Append(" (");

            bool firstindicator = true;
            foreach (EntityColumnDescriptor column in descriptor.Columns)
            {
                if (firstindicator) firstindicator = false;
                else preparator.CommandBuilder.Append(", ");

                client.DBInfo.CreateColumn(preparator, column);
            }

            foreach (UniqueDescriptor unique in descriptor.Uniques)
            {
                preparator.CommandBuilder.Append(", UNIQUE (");
                preparator.CommandBuilder.Append(string.Join(",", unique.Columns.Select(client.DBInfo.MaskColumn).ToArray()));
                preparator.CommandBuilder.Append(")");
            }

            preparator.CommandBuilder.Append(")");

            if (!string.IsNullOrEmpty(client.DBInfo.CreateSuffix))
                preparator.CommandBuilder.Append(" ").Append(client.DBInfo.CreateSuffix);

            client.NonQuery(preparator.CommandBuilder.ToString(), preparator.Parameters.Select(p => p.Value).ToArray());

            CreateIndices(client, descriptor.TableName, descriptor.Indices);
        }

        /// <summary>
        /// creates a type in database
        /// </summary>
        /// <param name="type">type to create</param>
        /// <param name="client">client to database</param>
        public void Create(Type type, IDBClient client) {
            EntityDescriptor descriptor = modelcache.Get(type);

            if (client.DBInfo.CheckIfTableExists(client, descriptor.TableName))
                // table already exists
                return;

            // check if type points to a view
            ViewAttribute viewdef = (ViewAttribute)Attribute.GetCustomAttribute(type, typeof(ViewAttribute));
            if (viewdef != null)
            {
                using(StreamReader sr = new StreamReader(type.Assembly.GetManifestResourceStream(viewdef.Definition))) {
                    string definition = sr.ReadToEnd();
                    client.NonQuery(definition);
                }
                return;
            }

            CreateTable(client, descriptor);
        }

        /// <summary>
        /// creates indices for a table
        /// </summary>
        /// <param name="client">database access</param>
        /// <param name="table">table on which to create indices</param>
        /// <param name="indices">indices to create</param>
        public void CreateIndices(IDBClient client, string table, IEnumerable<IndexDescriptor> indices) {
            StringBuilder commandbuilder = new StringBuilder();
            foreach (IndexDescriptor indexdescriptor in indices) {
                string indexname = $"idx_{table}_{indexdescriptor.Name}";
                commandbuilder.Append("DROP INDEX IF EXISTS ").Append(indexname).AppendLine(";");
                commandbuilder.Append("CREATE INDEX ").Append(indexname).Append(" ON ").Append(table).Append(" (");
                bool firstindicator = true;
                foreach (string column in indexdescriptor.Columns)
                {
                    if (firstindicator) firstindicator = false;
                    else commandbuilder.Append(", ");
                    commandbuilder.Append(client.DBInfo.ColumnIndicator).Append(column).Append(client.DBInfo.ColumnIndicator);
                }
                commandbuilder.Append(");");
            }
            if (commandbuilder.Length > 0)
                client.NonQuery(commandbuilder.ToString());
        }
    }
}