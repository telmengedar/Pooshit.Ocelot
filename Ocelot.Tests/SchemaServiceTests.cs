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
                                                                      Columns = new[] {
                                                                                          new ColumnDescriptor("clickid", "string") {
                                                                                                                                        PrimaryKey = true,
                                                                                                                                        NotNull = true
                                                                                                                                    },
                                                                                          new ColumnDescriptor("timestamp", "int64") {
                                                                                                                                         NotNull = true
                                                                                                                                     },
                                                                                          new ColumnDescriptor("dateest", "datetime") {
                                                                                                                                          NotNull = true
                                                                                                                                      },
                                                                                          new ColumnDescriptor("ip", "string"),
                                                                                          new ColumnDescriptor("jobid", "string"),
                                                                                          new ColumnDescriptor("jobtitle", "string"),
                                                                                          new ColumnDescriptor("empcode", "string"),
                                                                                          new ColumnDescriptor("company", "string"),
                                                                                          new ColumnDescriptor("cost", "decimal"),
                                                                                          new ColumnDescriptor("currency", "string"),
                                                                                          new ColumnDescriptor("redirectto", "string"),
                                                                                          new ColumnDescriptor("clientjobid", "string"),
                                                                                          new ColumnDescriptor("campaignid", "int64") {
                                                                                                                                          DefaultValue = 0,
                                                                                                                                          NotNull = true
                                                                                                                                      },
                                                                                          new ColumnDescriptor("itemid", "int64") {
                                                                                                                                      DefaultValue = 0,
                                                                                                                                      NotNull = true
                                                                                                                                  },
                                                                                          new ColumnDescriptor("targetid", "int64") {
                                                                                                                                        DefaultValue = 0,
                                                                                                                                        NotNull = true
                                                                                                                                    },
                                                                                          new ColumnDescriptor("mamgojobid", "int64") {
                                                                                                                                          DefaultValue = 0,
                                                                                                                                          NotNull = true
                                                                                                                                      },
                                                                                          new ColumnDescriptor("cpc", "decimal")
                                                                                      },
                                                                      Index = new[] {
                                                                                        new IndexDescriptor("timestamp", new[] { "timestamp" }, null),
                                                                                        new IndexDescriptor("jobid", new[] { "clientjobid" }, null),
                                                                                        new IndexDescriptor("campaign", new[] { "campaignid" }, null),
                                                                                        new IndexDescriptor("item", new[] { "itemid" }, null),
                                                                                        new IndexDescriptor("target", new[] { "targetid" }, null),
                                                                                        new IndexDescriptor("job", new[] { "mamgojobid" }, null)
                                                                                    }
                                                                  });

        client.Verify(s => s.NonQueryAsync(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<IEnumerable<object>>()), Times.Never);
    }
}