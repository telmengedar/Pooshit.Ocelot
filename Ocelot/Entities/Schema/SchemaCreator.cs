using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities.Attributes;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Schemas;

namespace Pooshit.Ocelot.Entities.Schema; 

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
    /// <param name="transaction"></param>
    public void CreateTable(IDBClient client, EntityDescriptor descriptor, Transaction transaction = null) {
        OperationPreparator preparator = new();
        preparator.AppendText($"CREATE TABLE {descriptor.TableName} (");

        bool firstindicator = true;
        foreach(EntityColumnDescriptor column in descriptor.Columns) {
            if(firstindicator)
                firstindicator = false;
            else
                preparator.AppendText(",");

            client.DBInfo.CreateColumn(preparator, column);
        }

        foreach(UniqueDescriptor unique in descriptor.Uniques.Where(u => u.Columns.Count() > 1)) {
            preparator.AppendText(", UNIQUE (");
            preparator.AppendText(string.Join(",", unique.Columns.Select(client.DBInfo.MaskColumn).ToArray()));
            preparator.AppendText(")");
        }

        preparator.AppendText(")");

        if(!string.IsNullOrEmpty(client.DBInfo.CreateSuffix))
            preparator.AppendText(client.DBInfo.CreateSuffix);


        if(transaction != null)
            preparator.GetOperation(client, false).Execute(transaction);
        else
            preparator.GetOperation(client, false).Execute();

        CreateIndices(client, descriptor.TableName, descriptor.Indices, transaction);
    }

    /// <summary>
    /// creates a type in database
    /// </summary>
    /// <param name="type">type to create</param>
    /// <param name="client">client to database</param>
    public void Create(Type type, IDBClient client) {
        EntityDescriptor descriptor = modelcache.Get(type);

        if(client.DBInfo.CheckIfTableExists(client, descriptor.TableName))
            // table already exists
            return;

        string viewdefinition = ViewAttribute.GetViewDefinition(type);
        if(!string.IsNullOrEmpty(viewdefinition)) {
            client.NonQuery(viewdefinition);
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
    /// <param name="transaction">transaction to use (optional)</param>
    public void CreateIndices(IDBClient client, string table, IEnumerable<IndexDescriptor> indices, Transaction transaction = null) {
        StringBuilder commandbuilder = new();
        foreach(IndexDescriptor indexdescriptor in indices) {
            string indexname = $"idx_{table}_{indexdescriptor.Name}";
            commandbuilder.Append("DROP INDEX IF EXISTS ").Append(indexname).AppendLine(";");
            commandbuilder.Append("CREATE INDEX ").Append(indexname).Append(" ON ").Append(table).Append(" (");
            bool firstindicator = true;
            foreach(string column in indexdescriptor.Columns) {
                if(firstindicator)
                    firstindicator = false;
                else
                    commandbuilder.Append(", ");
                commandbuilder.Append(client.DBInfo.ColumnIndicator).Append(column).Append(client.DBInfo.ColumnIndicator);
            }
            commandbuilder.Append(");");
        }

        if(commandbuilder.Length > 0) {
            if(transaction == null)
                client.NonQuery(commandbuilder.ToString());
            else
                client.NonQuery(transaction, commandbuilder.ToString());
        }
    }
}