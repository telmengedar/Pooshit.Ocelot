using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Extern;
using Pooshit.Ocelot.Models;

namespace Pooshit.Ocelot.Schemas; 

/// <inheritdoc />
public class SchemaService : ISchemaService {
        
    /// <summary>
    /// creates a new <see cref="SchemaService"/>
    /// </summary>
    /// <param name="database">access to database</param>
    public SchemaService(IDBClient database) {
        Database = database;
    }

    /// <inheritdoc />
    public IDBClient Database { get; }

    /// <inheritdoc />
    public Task CreateSchema(Schema schema, Transaction transaction = null) {
        if (Database.DBInfo.CheckIfTableExists(Database, schema.Name, transaction))
            throw new ArgumentException($"'{schema.Name}' already exists in database");

        if (schema is ViewSchema viewSchema) {
            return Database.NonQueryAsync(transaction, viewSchema.Definition);
        }

        return CreateTable(Database, schema as TableSchema, transaction);
    }

    /// <inheritdoc />
    public Task CreateSchema<T>(Transaction transaction = null) {
        Schema schema = new SchemaCreator().Create<T>(Database.DBInfo);
        return CreateSchema(schema, transaction);
    }

    /// <inheritdoc />
    public async Task CreateOrUpdateSchema<T>(Transaction transaction = null) {
        Schema schema = new SchemaCreator().Create<T>(Database.DBInfo);
        if (await Database.DBInfo.CheckIfTableExistsAsync(Database, schema.Name, transaction))
            await UpdateSchema(schema.Name, schema, transaction);
        else await CreateSchema(schema, transaction);
    }

    /// <inheritdoc />
    public Task<bool> ExistsSchema(string name, Transaction transaction = null) {
        return Database.DBInfo.CheckIfTableExistsAsync(Database, name, transaction);
    }

    /// <inheritdoc />
    public Task<bool> ExistsSchema<T>(Transaction transaction = null) {
        Schema schema = new SchemaCreator().Create<T>(Database.DBInfo);
        return ExistsSchema(schema.Name, transaction);
    }

    async Task RecreateTable(TableSchema existingSchema, TableSchema targetSchema, Transaction transaction) {
        string appendix = "_original";

        // check if an old backup table exists for whatever reason
        if (await Database.DBInfo.CheckIfTableExistsAsync(Database, $"{existingSchema.Name}{appendix}", transaction))
            await Database.NonQueryAsync(transaction, $"DROP TABLE {existingSchema.Name}{appendix}");

        await Database.NonQueryAsync(transaction, $"ALTER TABLE {existingSchema.Name} RENAME TO {existingSchema.Name}{appendix}");

        ColumnDescriptor[] remainingcolumns = existingSchema.Columns.Where(c => targetSchema.Columns.Any(c1 => c1.Name == c.Name)).ToArray();
        ColumnDescriptor[] newcolumns = targetSchema.Columns.Where(c => c.NotNull && !c.AutoIncrement && c.DefaultValue == null && existingSchema.Columns.All(o => o.Name != c.Name)).ToArray();
        string columnlist = string.Join(", ", remainingcolumns.Select(c => Database.DBInfo.MaskColumn(c.Name)));
        string newcolumnlist = string.Join(", ", newcolumns.Select(c => Database.DBInfo.MaskColumn(c.Name)));
        await CreateSchema(targetSchema, transaction);

        // transfer data to new table
        if (newcolumns.Length == 0)
            await Database.NonQueryAsync(transaction, $"INSERT INTO {targetSchema.Name} ({columnlist}) SELECT {columnlist} FROM {existingSchema.Name}{appendix}");
        else {
            // new schema has columns which mustn't be null
            OperationPreparator operation = new();
            operation.AppendText($"INSERT INTO {targetSchema.Name} ({columnlist},{newcolumnlist}) SELECT {columnlist}");
            foreach (ColumnDescriptor column in newcolumns) {
                operation.AppendText(",");
                if (column.NotNull)
                    operation.AppendParameter(column.DefaultValue ?? Database.DBInfo.GenerateDefault(column.Type));
                else operation.AppendParameter(column.DefaultValue);
            }

            operation.AppendText($"FROM {existingSchema.Name}{appendix}");
            await operation.GetOperation(Database, false).ExecuteAsync(transaction);
        }

        // remove old data
        await Database.NonQueryAsync(transaction, $"DROP TABLE {existingSchema.Name}{appendix}");
    }

    /// <inheritdoc />
    public async Task UpdateSchema(string name, Schema schema, Transaction transaction = null) {
        Logger.Info(this, $"Checking schema of '{name}'");

        if(schema is not TableSchema targetSchema)
            throw new ArgumentException("Views are not supported for now");

        targetSchema.Index ??= Array.Empty<IndexDescriptor>();
        targetSchema.Unique ??= Array.Empty<UniqueDescriptor>();

        if (await GetSchema(name, transaction) is not TableSchema existingSchema)
            throw new ArgumentException("Can not update view to a table");
            
        List<ColumnDescriptor> missing = new();
        List<ColumnDescriptor> altered = new();
        List<string> obsolete = new();

        missing.AddRange(targetSchema.Columns.Where(c => existingSchema.Columns.All(sc => sc.Name != c.Name)));
        missing.ForEach(c => Logger.Info(this, $"Detected missing column '{c.Name}'"));
            
        foreach(ColumnDescriptor column in existingSchema.Columns) {
            ColumnDescriptor entitycolumn = targetSchema.Columns.FirstOrDefault(c => c.Name == column.Name);
            if(entitycolumn == null) {
                Logger.Info(this, $"Detected obsolete column '{column.Name}'");
                obsolete.Add(column.Name);
            }
            else {
                if(!Database.DBInfo.IsTypeEqual(column.Type, entitycolumn.Type)
                   || column.PrimaryKey != entitycolumn.PrimaryKey
                   || column.AutoIncrement != entitycolumn.AutoIncrement
                   || column.IsUnique != entitycolumn.IsUnique
                   || column.NotNull != entitycolumn.NotNull
                   /* default value is not evaluated for now
                  || column.DefaultValue != entitycolumn.DefaultValue*/
                  ) {
                    Logger.Info(this, $"Detected altered column '{entitycolumn.Name}'", $"New -> {entitycolumn} {entitycolumn.Type}\r\nOld -> {column} {column.Type}");
                    altered.Add(entitycolumn);
                }
            }
        }

        // check if anything changed at all
        if (obsolete.Count == 0 && missing.Count == 0 && altered.Count == 0 && existingSchema.Unique.Equals(targetSchema.Unique) && existingSchema.Index.Equals(targetSchema.Index)) {
            Logger.Info(this,$"Table '{name}' already up to date");
            return;
        }

        bool recreatetable = Database.DBInfo.MustRecreateTable(obsolete.ToArray(), altered.ToArray(), missing.ToArray(), existingSchema, targetSchema);

        if(recreatetable) {
            await RecreateTable(existingSchema, targetSchema,transaction);
        }
        else {
            if(obsolete.Count > 0 || missing.Count > 0 || altered.Count > 0) {
                AlterTableOperation alteroperation = new(Database, name);
                alteroperation.Drop(obsolete.ToArray());
                alteroperation.Add(missing.ToArray());
                alteroperation.Modify(altered.ToArray());
                await alteroperation.Prepare().ExecuteAsync(transaction);
            }

            if(!existingSchema.Index.Equals(targetSchema.Index))
                await UpdateIndices(Database, existingSchema, targetSchema, transaction);
            if(!existingSchema.Unique.Equals(targetSchema.Unique))
                await UpdateUniques(Database, existingSchema, targetSchema, transaction);
        }
    }

    /// <inheritdoc />
    public Task UpdateSchema<T>(Transaction transaction = null) {
        Schema schema = new SchemaCreator().Create<T>(Database.DBInfo);
        return UpdateSchema(schema.Name, schema, transaction);
    }

    /// <inheritdoc />
    public Task<IEnumerable<Schema>> ListSchemata(PageOptions options = null, Transaction transaction = null) {
        return Database.DBInfo.ListSchemataAsync(Database, options, transaction);
    }

    async Task UpdateUniques(IDBClient client, TableSchema oldschema, TableSchema newschema, Transaction transaction) {
        List<UniqueDescriptor> obsolete = new(oldschema.Unique.Except(newschema.Unique));
        List<UniqueDescriptor> missing = new(newschema.Unique.Except(oldschema.Unique));

        foreach (UniqueDescriptor drop in obsolete)
            await client.NonQueryAsync(transaction, $"ALTER TABLE {newschema.Name} DROP CONSTRAINT IF EXISTS {drop.Name};");

        foreach(UniqueDescriptor add in missing)
            await client.NonQueryAsync(transaction, $"ALTER TABLE {newschema.Name} ADD UNIQUE ({string.Join(",", add.Columns.Select(c => client.DBInfo.MaskColumn(c)))});");
    }

    async Task UpdateIndices(IDBClient client, TableSchema oldschema, TableSchema newschema, Transaction transaction) {
        List<IndexDescriptor> missing = new(newschema.Index.Where(i => oldschema.Index.All(i2 => i2.Name != i.Name)));
        List<IndexDescriptor> altered = new();
        List<string> obsolete = new();

        foreach(IndexDescriptor index in oldschema.Index) {
            IndexDescriptor newdefinition = newschema.Index.FirstOrDefault(i => i.Name == index.Name);
            if (newdefinition == null) {
                if (index.Columns.All(c => newschema.Columns.Any(col => col.Name == c)))
                    // if any of the columns was removed from the current schema the index was removed automatically
                    obsolete.Add(index.Name);
            }
            else {
                if (!newdefinition.Columns.SequenceEqual(index.Columns) || (!string.IsNullOrEmpty(index.Type) && newdefinition.Type?.ToLower() != index.Type?.ToLower()))
                    altered.Add(newdefinition);
            }
        }

        if (obsolete.Count > 0) {
            Logger.Info(this, $"Detected obsolete index definition for '{string.Join(", ", obsolete)}'");
            foreach (string index in obsolete)
                await client.NonQueryAsync(transaction, $"DROP INDEX idx_{newschema.Name}_{index}");
        }
            
        if(altered.Count > 0) {
            Logger.Info(this, $"Detected altered index definition for '{string.Join(", ", altered.Select(i => i.Name))}'");
            foreach (IndexDescriptor index in altered)
                await client.NonQueryAsync(transaction, $"DROP INDEX idx_{newschema.Name}_{index.Name}");

            await CreateIndices(client, newschema.Name, altered, transaction);
        }

        if(missing.Count > 0) {
            Logger.Info(this, $"Detected missing index '{string.Join(", ", missing.Select(c => c.Name))}'");
            await CreateIndices(client, newschema.Name, missing, transaction);
        }
    }

    /// <inheritdoc />
    public Task<Schema> GetSchema(string name, Transaction transaction = null) {
        return Database.DBInfo.GetSchemaAsync(Database, name, transaction);
    }

    /// <inheritdoc />
    public async Task RemoveSchema(string name, Transaction transaction = null) {
        if (name.Contains('"') || name.Contains('[') || name.Contains("']"))
            throw new ArgumentException("Illegal schema name", nameof(name));
            
        SchemaType type = await GetSchemaType(name, transaction);
        switch (type) {
            case SchemaType.Table:
                await Database.NonQueryAsync($"DROP TABLE {Database.DBInfo.MaskColumn(name)}");
                break;
            case SchemaType.View:
                await Database.NonQueryAsync($"DROP VIEW {Database.DBInfo.MaskColumn(name)}");
                break;
            default:
                throw new InvalidOperationException($"Unknown schema type '{type}'");
        }
    }

    /// <inheritdoc />
    public Task RemoveSchema<T>(Transaction transaction = null) {
        Schema schema = new SchemaCreator().Create<T>(Database.DBInfo);
        return RemoveSchema(schema.Name, transaction);
    }

    /// <inheritdoc />
    public Task<SchemaType> GetSchemaType(string name, Transaction transaction = null) {
        return Database.DBInfo.GetSchemaTypeAsync(Database, name, transaction);
    }

    async Task CreateTable(IDBClient client, TableSchema schema, Transaction transaction = null) {
        OperationPreparator preparator = new();
        preparator.AppendText($"CREATE TABLE {schema.Name} (");

        bool firstindicator = true;
        foreach(ColumnDescriptor column in schema.Columns) {
            if(firstindicator)
                firstindicator = false;
            else
                preparator.AppendText(",");

            client.DBInfo.CreateColumn(preparator, column);
        }

        if (schema.Unique != null) {
            foreach (UniqueDescriptor unique in schema.Unique.Where(u => u.Columns.Count() > 1)) {
                preparator.AppendText(", UNIQUE (");
                preparator.AppendText(string.Join(",", unique.Columns.Select(client.DBInfo.MaskColumn).ToArray()));
                preparator.AppendText(")");
            }
        }

        preparator.AppendText(")");

        if(!string.IsNullOrEmpty(client.DBInfo.CreateSuffix))
            preparator.AppendText(client.DBInfo.CreateSuffix);

        await preparator.GetOperation(client, false).ExecuteAsync(transaction);

        if (schema.Index?.Length > 0)
            await CreateIndices(client, schema.Name, schema.Index, transaction);
    }
        
    Task CreateIndices(IDBClient client, string table, IEnumerable<IndexDescriptor> indices, Transaction transaction = null) {
        StringBuilder commandBuilder = new();
        foreach(IndexDescriptor indexDescriptor in indices) {
            string indexname = $"idx_{table}_{indexDescriptor.Name}";
            commandBuilder.Append("DROP INDEX IF EXISTS ").Append(indexname).AppendLine(";");
            commandBuilder.Append("CREATE INDEX ").Append(indexname).Append(" ON ").Append(table);
            client.DBInfo.CreateIndexTypeFragment(commandBuilder, indexDescriptor.Type);
            commandBuilder.Append(" (");
            bool firstindicator = true;
            foreach(string column in indexDescriptor.Columns) {
                if(firstindicator)
                    firstindicator = false;
                else
                    commandBuilder.Append(", ");
                commandBuilder.Append(client.DBInfo.ColumnIndicator).Append(column).Append(client.DBInfo.ColumnIndicator);
            }
            commandBuilder.Append(");");
        }

        return client.NonQueryAsync(transaction, commandBuilder.ToString());
    }
}