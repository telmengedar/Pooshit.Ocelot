using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Pooshit.Json;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Schemas;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Entities;
using Pooshit.Ocelot.Tests.Mocks;

namespace Pooshit.Ocelot.Tests;

[TestFixture, Parallelizable]
public class SchemaServiceTests {

    [Test, Parallelizable]
    public async Task CreateSchema() {
        IDBClient client=TestData.CreateDatabaseAccess();
        SchemaService service = new(client);

        await service.CreateSchema(new TableSchema {
            Name = "testtable",
            Columns = [
                new("moon") {
                    Type = "INTEGER"
                },
                new("sun") {
                    Type = "TEXT",
                    NotNull = true,
                    DefaultValue = "foom"
                }
            ]
        });

        TableSchema created = await service.GetSchema("testtable") as TableSchema;
        Assert.NotNull(created);
        Assert.AreEqual("testtable", created.Name);
        Assert.AreEqual(0, created.Index.Length);
        Assert.AreEqual(0, created.Unique.Length);
        Assert.AreEqual(2, created.Columns.Length);

        ColumnDescriptor moon = created.Columns.FirstOrDefault(c => c.Name == "moon");
        ColumnDescriptor sun = created.Columns.FirstOrDefault(c => c.Name == "sun");
        Assert.NotNull(moon);
        Assert.NotNull(sun);
        Assert.AreEqual("INTEGER", moon.Type);
        Assert.AreEqual("TEXT", sun.Type);
        Assert.True(sun.NotNull);
        Assert.AreEqual("foom", sun.DefaultValue);
    }

    [Test, Parallelizable]
    public async Task CreateSchemaWithIndex() {
        IDBClient client=TestData.CreateDatabaseAccess();
        SchemaService service = new(client);

        await service.CreateSchema(new TableSchema {
                                                       Name = "testtable",
                                                       Columns = new[] {
                                                                           new ColumnDescriptor("moon") {
                                                                                                            Type = "INTEGER"
                                                                                                        },
                                                                           new ColumnDescriptor("sun") {
                                                                                                           Type = "TEXT",
                                                                                                           NotNull = true,
                                                                                                           DefaultValue = "foom"
                                                                                                       }
                                                                       },
                                                       Index = new[] {
                                                                         new IndexDescriptor {
                                                                                                 Name = "moon",
                                                                                                 Columns = new[] { "moon" }
                                                                                             }
                                                                     }
                                                   });

        TableSchema created = await service.GetSchema("testtable") as TableSchema;
        Assert.NotNull(created);
        Assert.AreEqual("testtable", created.Name);
        Assert.AreEqual(1, created.Index.Length);
        Assert.AreEqual(0, created.Unique.Length);
        Assert.AreEqual(2, created.Columns.Length);

        ColumnDescriptor moon = created.Columns.FirstOrDefault(c => c.Name == "moon");
        ColumnDescriptor sun = created.Columns.FirstOrDefault(c => c.Name == "sun");
        IndexDescriptor index = created.Index[0];
            
        Assert.NotNull(moon);
        Assert.NotNull(sun);
        Assert.AreEqual("INTEGER", moon.Type);
        Assert.AreEqual("TEXT", sun.Type);
        Assert.True(sun.NotNull);
        Assert.AreEqual("foom", sun.DefaultValue);
        Assert.AreEqual("moon", index.Name);
        Assert.AreEqual(1, index.Columns.Length);
        Assert.AreEqual("moon", index.Columns[0]);
    }

    [Test, Parallelizable]
    public async Task ListSchemataWithIndex() {
        IDBClient client=TestData.CreateDatabaseAccess();
        SchemaService service = new(client);

        await service.CreateSchema(new TableSchema {
                                                       Name = "testtable",
                                                       Columns = new[] {
                                                                           new ColumnDescriptor("moon") {
                                                                                                            Type = "INTEGER"
                                                                                                        },
                                                                           new ColumnDescriptor("sun") {
                                                                                                           Type = "TEXT",
                                                                                                           NotNull = true,
                                                                                                           DefaultValue = "foom"
                                                                                                       }
                                                                       },
                                                       Index = new[] {
                                                                         new IndexDescriptor {
                                                                                                 Name = "moon",
                                                                                                 Columns = new[] { "moon" }
                                                                                             }
                                                                     }
                                                   });

        Schema[] schemata = (await service.ListSchemata()).ToArray();
        Assert.NotNull(schemata);
        Assert.AreEqual(1, schemata.Length);
        Assert.AreEqual("testtable", schemata[0].Name);
        Assert.AreEqual(SchemaType.Table, schemata[0].Type);
    }

    [Test, Parallelizable]
    public async Task CreateSchemaWithIndexTransaction() {
        IDBClient client=TestData.CreateDatabaseAccess();
        SchemaService service = new(client);

        using Transaction transaction = client.Transaction();
        await service.CreateSchema(new TableSchema {
                                                       Name = "testtable",
                                                       Columns = new[] {
                                                                           new ColumnDescriptor("moon") {
                                                                                                            Type = "INTEGER"
                                                                                                        },
                                                                           new ColumnDescriptor("sun") {
                                                                                                           Type = "TEXT",
                                                                                                           NotNull = true,
                                                                                                           DefaultValue = "foom"
                                                                                                       }
                                                                       },
                                                       Index = new[] {
                                                                         new IndexDescriptor {
                                                                                                 Name = "moon",
                                                                                                 Columns = new[] { "moon" }
                                                                                             }
                                                                     }
                                                   }, transaction);
        transaction.Commit();
            
        TableSchema created = await service.GetSchema("testtable") as TableSchema;
        Assert.NotNull(created);
        Assert.AreEqual("testtable", created.Name);
        Assert.AreEqual(1, created.Index.Length);
        Assert.AreEqual(0, created.Unique.Length);
        Assert.AreEqual(2, created.Columns.Length);

        ColumnDescriptor moon = created.Columns.FirstOrDefault(c => c.Name == "moon");
        ColumnDescriptor sun = created.Columns.FirstOrDefault(c => c.Name == "sun");
        IndexDescriptor index = created.Index[0];
            
        Assert.NotNull(moon);
        Assert.NotNull(sun);
        Assert.AreEqual("INTEGER", moon.Type);
        Assert.AreEqual("TEXT", sun.Type);
        Assert.True(sun.NotNull);
        Assert.AreEqual("foom", sun.DefaultValue);
        Assert.AreEqual("moon", index.Name);
        Assert.AreEqual(1, index.Columns.Length);
        Assert.AreEqual("moon", index.Columns[0]);
    }

    [Test, Parallelizable]
    public async Task UpdateSchema() {
        IDBClient client=TestData.CreateDatabaseAccess();
        SchemaService service = new(client);

        await service.CreateSchema(new TableSchema {
                                                       Name = "testtable",
                                                       Columns = new[] {
                                                                           new ColumnDescriptor("moon") {
                                                                                                            Type = "INTEGER"
                                                                                                        },
                                                                           new ColumnDescriptor("sun") {
                                                                                                           Type = "TEXT",
                                                                                                           NotNull = true,
                                                                                                           DefaultValue = "foom"
                                                                                                       }
                                                                       }
                                                   });

        await service.UpdateSchema("testtable", new TableSchema {
                                                                    Name = "testtable",
                                                                    Columns = new[] {
                                                                                        new ColumnDescriptor("sun") {
                                                                                                                        Type = "TEXT",
                                                                                                                        NotNull = true,
                                                                                                                        DefaultValue = "foom"
                                                                                                                    },
                                                                                        new ColumnDescriptor("saturn") {
                                                                                                                           Type = "FLOAT",
                                                                                                                           NotNull = true,
                                                                                                                           DefaultValue = 0.5f
                                                                                                                       }
                                                                                    },
                                                                    Index = new[] {
                                                                                      new IndexDescriptor("power", new[] { "saturn" }, null)
                                                                                  }
                                                                });
            
        TableSchema created = await service.GetSchema("testtable") as TableSchema;
        Assert.NotNull(created);
        Assert.AreEqual("testtable", created.Name);
        Assert.AreEqual(1, created.Index.Length);
        Assert.AreEqual(0, created.Unique.Length);
        Assert.AreEqual(2, created.Columns.Length);

        ColumnDescriptor saturn = created.Columns.FirstOrDefault(c => c.Name == "saturn");
        ColumnDescriptor sun = created.Columns.FirstOrDefault(c => c.Name == "sun");
        Assert.NotNull(saturn);
        Assert.NotNull(sun);
        Assert.AreEqual("FLOAT", saturn.Type);
        Assert.AreEqual("TEXT", sun.Type);
        Assert.True(sun.NotNull);
        Assert.AreEqual("0.5", saturn.DefaultValue);
        Assert.AreEqual("foom", sun.DefaultValue);
        Assert.AreEqual("power", created.Index[0].Name);
        Assert.AreEqual(1, created.Index[0].Columns.Length);
        Assert.AreEqual("saturn", created.Index[0].Columns.First());
    }

    [Test, Parallelizable]
    public async Task DontUpdateTwice() {
        PostgreInfo dbInfo = new();
        Mock<IDBClient> client = new();
        client.Setup(s => s.DBInfo).Returns(dbInfo);
        client.Setup(s => s.ReaderAsync(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<IEnumerable<object>>()))
              .Returns<Transaction, string, IEnumerable<object>>((transaction, command, parameters) => {
                                                                     object data;
                                                                     if (command.StartsWith("SELECT * FROM pg_views")) {
                                                                         return Task.FromResult(new Reader(new FakeReader(Array.Empty<string>(), Array.Empty<object[]>()), null, dbInfo));
                                                                     }

                                                                     if (command.StartsWith("SELECT * FROM information_schema.columns")) {
                                                                         data = Json.Json.Read(typeof(SchemaServiceTests).Assembly.GetManifestResourceStream("Pooshit.Ocelot.Tests.Resources.neuvooclick_pgcolumn.json"));
                                                                     }
                                                                     else if (command.StartsWith("SELECT * FROM pg_indexes")) {
                                                                         data = Json.Json.Read(typeof(SchemaServiceTests).Assembly.GetManifestResourceStream("Pooshit.Ocelot.Tests.Resources.neuvooclick_pgindexes.json"));
                                                                     }
                                                                     else throw new InvalidOperationException();

                                                                     return Task.FromResult(new Reader(new FakeReader(JPath.Select<object[]>(data, "Columns").Cast<string>().ToArray(), JPath.Select<object[]>(data, "Rows").Cast<object[]>().ToArray()), null, dbInfo));
                                                                 });

        SchemaService service = new(client.Object);
        await service.UpdateSchema("neuvooclick", new TableSchema {
            Name = "neuvooclick",
            Columns = [
                new("clickid", "string") {
                    PrimaryKey = true,
                    NotNull = true
                },
                new("timestamp", "int64") {
                    NotNull = true
                },
                new("dateest", "datetime") {
                    NotNull = true
                },
                new("ip", "string"),
                new("jobid", "string"),
                new("jobtitle", "string"),
                new("empcode", "string"),
                new("company", "string"),
                new("cost", "decimal"),
                new("currency", "string"),
                new("redirectto", "string"),
                new("clientjobid", "string"),
                new("campaignid", "int64") {
                    DefaultValue = 0,
                    NotNull = true
                },
                new("itemid", "int64") {
                    DefaultValue = 0,
                    NotNull = true
                },
                new("targetid", "int64") {
                    DefaultValue = 0,
                    NotNull = true
                },
                new("mamgojobid", "int64") {
                    DefaultValue = 0,
                    NotNull = true
                },
                new("cpc", "decimal")
            ],
            Index = [
                new("timestamp", ["timestamp"], null),
                new("jobid", ["clientjobid"], null),
                new("campaign", ["campaignid"], null),
                new("item", ["itemid"], null),
                new("target", ["targetid"], null),
                new("job", ["mamgojobid"], null)
            ]
        });

        client.Verify(s => s.NonQueryAsync(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<IEnumerable<object>>()), Times.Never);
    }

    /// <summary>
    /// Regression: before the W4 fix, SchemaCreator.Create did not set IsUnique=true for named single-column
    /// unique constraints ([Unique("name")]). This caused UpdateSchema to detect a spurious column change
    /// and emit an ALTER COLUMN statement on every call, even when the schema was already up to date.
    /// </summary>
    [Test, Parallelizable]
    public async Task NamedSingleColumnUnique_NoAlterColumnOnUpToDateSchema() {
        PostgreInfo dbInfo = new();
        Mock<IDBClient> client = new();
        client.Setup(s => s.DBInfo).Returns(dbInfo);

        // PgColumn reader columns (from information_schema.columns)
        string[] pgColumnNames = ["table_catalog", "table_schema", "table_name", "column_name", "data_type", "is_nullable", "column_default", "udt_name", "is_identity"];

        // PgIndex reader columns (from pg_indexes)
        string[] pgIndexNames = ["schemaname", "tablename", "indexname", "indexdef"];

        client.Setup(s => s.ReaderAsync(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<IEnumerable<object>>()))
              .Returns<Transaction, string, IEnumerable<object>>((transaction, command, parameters) => {
                  if (command.StartsWith("SELECT * FROM pg_views")) {
                      // no views — entity is a plain table
                      return Task.FromResult(new Reader(new FakeReader(Array.Empty<string>(), Array.Empty<object[]>()), null, dbInfo));
                  }

                  if (command.StartsWith("SELECT * FROM information_schema.columns")) {
                      // three columns: id (int8, PK via nextval), url (character varying, unique), slug (character varying, unique)
                      object[][] rows = [
                          // table_catalog, table_schema, table_name, column_name, data_type, is_nullable, column_default, udt_name, is_identity
                          ["db", "public", "uniqueconstraintentity", "id",   "int8",               "NO",  "nextval('uniqueconstraintentity_id_seq'::regclass)", "int8",             false],
                          ["db", "public", "uniqueconstraintentity", "url",  "character varying",  "YES", DBNull.Value,                                         "varchar",          false],
                          ["db", "public", "uniqueconstraintentity", "slug", "character varying",  "YES", DBNull.Value,                                         "varchar",          false]
                      ];
                      return Task.FromResult(new Reader(new FakeReader(pgColumnNames, rows), null, dbInfo));
                  }

                  if (command.StartsWith("SELECT * FROM pg_indexes")) {
                      // indexes: PK on id, named unique on url, unnamed unique on slug
                      object[][] rows = [
                          // schemaname, tablename, indexname, indexdef
                          ["public", "uniqueconstraintentity", "uniqueconstraintentity_pkey",     "CREATE UNIQUE INDEX uniqueconstraintentity_pkey ON uniqueconstraintentity USING btree (id)"],
                          ["public", "uniqueconstraintentity", "uq_uniqueconstraintentity_url",   "CREATE UNIQUE INDEX uq_uniqueconstraintentity_url ON uniqueconstraintentity (url)"],
                          ["public", "uniqueconstraintentity", "slug",                            "CREATE UNIQUE INDEX slug ON uniqueconstraintentity (slug)"]
                      ];
                      return Task.FromResult(new Reader(new FakeReader(pgIndexNames, rows), null, dbInfo));
                  }

                  throw new InvalidOperationException($"Unexpected query: {command}");
              });

        SchemaService service = new(client.Object);

        // UpdateSchema<T> calls SchemaCreator.Create<T> internally — exercises the full entity→schema path
        await service.UpdateSchema<UniqueConstraintEntity>();

        // The schema is already up to date; no ALTER COLUMN must be issued
        client.Verify(
            s => s.NonQueryAsync(
                It.IsAny<Transaction>(),
                It.Is<string>(sql => sql.IndexOf("ALTER COLUMN", StringComparison.OrdinalIgnoreCase) >= 0),
                It.IsAny<IEnumerable<object>>()),
            Times.Never,
            "ALTER COLUMN was emitted for a schema that was already up to date — " +
            "IsUnique is likely not being set for named single-column unique constraints");
    }

    /// <summary>
    /// Regression: when a Postgres database has accumulated many duplicate UNIQUE indexes on the same
    /// column (e.g. _url_key, _url_key1, … _url_key405) due to repeated schema-update runs before the
    /// deduplification fix, UpdateSchema must not emit any schema-mutating SQL for an entity whose intent
    /// already matches.
    ///
    /// UniqueDescriptor.Equals / GetHashCode are column-set based (names are ignored), so LINQ's Except
    /// collapses all 100+ duplicates to a single logical key during set comparison. This test locks in that
    /// behaviour so that adding name-based equality to UniqueDescriptor would cause it to fail loudly.
    /// </summary>
    [Test, Parallelizable]
    public async Task DuplicateUniqueIndexes_NoSqlEmittedOnUpToDateSchema() {
        PostgreInfo dbInfo = new();
        Mock<IDBClient> client = new();
        client.Setup(s => s.DBInfo).Returns(dbInfo);

        // PgColumn reader columns (from information_schema.columns)
        string[] pgColumnNames = ["table_catalog", "table_schema", "table_name", "column_name", "data_type", "is_nullable", "column_default", "udt_name", "is_identity"];

        // PgIndex reader columns (from pg_indexes)
        string[] pgIndexNames = ["schemaname", "tablename", "indexname", "indexdef"];

        client.Setup(s => s.ReaderAsync(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<IEnumerable<object>>()))
              .Returns<Transaction, string, IEnumerable<object>>((transaction, command, parameters) => {
                  if (command.StartsWith("SELECT * FROM pg_views")) {
                      return Task.FromResult(new Reader(new FakeReader(Array.Empty<string>(), Array.Empty<object[]>()), null, dbInfo));
                  }

                  if (command.StartsWith("SELECT * FROM information_schema.columns")) {
                      object[][] rows = [
                          ["db", "public", "uniqueconstraintentity", "id",   "int8",              "NO",  "nextval('uniqueconstraintentity_id_seq'::regclass)", "int8",    false],
                          ["db", "public", "uniqueconstraintentity", "url",  "character varying", "YES", DBNull.Value,                                         "varchar", false],
                          ["db", "public", "uniqueconstraintentity", "slug", "character varying", "YES", DBNull.Value,                                         "varchar", false]
                      ];
                      return Task.FromResult(new Reader(new FakeReader(pgColumnNames, rows), null, dbInfo));
                  }

                  if (command.StartsWith("SELECT * FROM pg_indexes")) {
                      // PK on id (1 row) + 100 duplicate UNIQUE indexes on url + 100 duplicate UNIQUE indexes
                      // on slug, simulating a database that accumulated them before the dedup fix.
                      object[][] pkRow = [
                          ["public", "uniqueconstraintentity", "uniqueconstraintentity_pkey",
                           "CREATE UNIQUE INDEX uniqueconstraintentity_pkey ON uniqueconstraintentity USING btree (id)"]
                      ];

                      object[][] urlRows = Enumerable.Range(0, 100).Select(i => {
                          string name = i == 0
                              ? "uniqueconstraintentity_url_key"
                              : $"uniqueconstraintentity_url_key{i}";
                          return (object[])["public", "uniqueconstraintentity", name,
                              $"CREATE UNIQUE INDEX {name} ON uniqueconstraintentity (url)"];
                      }).ToArray();

                      object[][] slugRows = Enumerable.Range(0, 100).Select(i => {
                          string name = i == 0
                              ? "uniqueconstraintentity_slug_key"
                              : $"uniqueconstraintentity_slug_key{i}";
                          return (object[])["public", "uniqueconstraintentity", name,
                              $"CREATE UNIQUE INDEX {name} ON uniqueconstraintentity (slug)"];
                      }).ToArray();

                      object[][] allRows = [..pkRow, ..urlRows, ..slugRows];
                      return Task.FromResult(new Reader(new FakeReader(pgIndexNames, allRows), null, dbInfo));
                  }

                  throw new InvalidOperationException($"Unexpected query: {command}");
              });

        SchemaService service = new(client.Object);
        await service.UpdateSchema<UniqueConstraintEntity>();

        // No schema-mutating SQL should be issued: the duplicate unique indexes all collapse to the
        // same logical key during set comparison, so the schema is considered up to date.
        string[] mutatingKeywords = ["ALTER COLUMN", "ALTER TABLE", "ADD UNIQUE", "DROP CONSTRAINT", "DROP INDEX"];
        foreach (string keyword in mutatingKeywords) {
            client.Verify(
                s => s.NonQueryAsync(
                    It.IsAny<Transaction>(),
                    It.Is<string>(sql => sql.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0),
                    It.IsAny<IEnumerable<object>>()),
                Times.Never,
                $"Schema-mutating SQL containing '{keyword}' was emitted even though the schema has 100 duplicate " +
                "unique indexes per column that should collapse to the same logical key. " +
                "Check that UniqueDescriptor.Equals / GetHashCode are still column-set based, not name-based.");
        }
    }
}