using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Moq;
using NightlyCode.Database.Tests.Entities;
using NightlyCode.Database.Tests.Mocks;
using NightlyCode.Database.Tests.Models;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Clients.Tables;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Entities.Operations.Expressions;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Entities.Schema;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Info.Postgre;
using Pooshit.Ocelot.Schemas;
using Pooshit.Ocelot.Tokens;

namespace NightlyCode.Database.Tests.Postgres;

[TestFixture, Parallelizable]
public class PostgreInfoTests {

    [Test, Parallelizable]
    public void GetView() {
        PostgreInfo info = new();
        Mock<IDBClient> client = new();
        client.SetupGet(c => c.DBInfo).Returns(info);
        client.Setup(c => c.Reader(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<IEnumerable<object>>())).Returns(
                                                                                                                          new Reader(new FakeReader(
                                                                                                                                                    new[] { "schemaname", "viewname", "viewowner", "definition" },
                                                                                                                                                    new[] {
                                                                                                                                                              new object[] { "public", "testview", "postgres", "SELECT whatever FROM something" }
                                                                                                                                                          }), null, info)
                                                                                                                         );

        ViewDescriptor descriptor = info.GetSchema(client.Object, "testview") as ViewDescriptor;

        Assert.NotNull(descriptor);
        Assert.AreEqual("testview", descriptor.Name);
        Assert.AreEqual("SELECT whatever FROM something", descriptor.SQL);
    }

    [Test, Parallelizable]
    public void GetSchema() {
        PostgreInfo info = new();

        Mock<IDBClient> client = new();
        client.SetupGet(c => c.DBInfo).Returns(info);
            
        client.Setup(c=>c.Reader(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<IEnumerable<object>>())).Returns<Transaction, string, IEnumerable<object>>((tr, text, pr) => {
                                                                                                                                                                      if(text.Contains(" information_schema.columns ")) {
                                                                                                                                                                          return new Reader(new FakeReader(new[] { "table_catalog", "table_schema", "table_name", "column_name", "data_type", "is_nullable", "column_default" },
                                                                                                                                                                                                           new[] {
                                                                                                                                                                                                                     new object[] { "xx.io", "public", "companyaddress", "id", "bigint", false, "nextval('company_id_seq'::regclass)" },
                                                                                                                                                                                                                     new object[] { "xx.io", "public", "companyaddress", "companyid", "bigint", false, DBNull.Value },
                                                                                                                                                                                                                     new object[] { "xx.io", "public", "companyaddress", "country", "character varying", true, DBNull.Value },
                                                                                                                                                                                                                     new object[] { "xx.io", "public", "companyaddress", "state", "character varying", true, DBNull.Value },
                                                                                                                                                                                                                     new object[] { "xx.io", "public", "companyaddress", "city", "character varying", true, DBNull.Value },
                                                                                                                                                                                                                     new object[] { "xx.io", "public", "companyaddress", "postalcode", "character varying", true, DBNull.Value },
                                                                                                                                                                                                                     new object[] { "xx.io", "public", "companyaddress", "line", "character varying", true, DBNull.Value }
                                                                                                                                                                                                                 }), null, info);
                                                                                                                                                                      }

                                                                                                                                                                      if(text.Contains(" pg_indexes ")) {
                                                                                                                                                                          return new Reader(new FakeReader(new[] { "schemaname", "tablename", "indexname", "indexdef" },
                                                                                                                                                                                                           new[] {
                                                                                                                                                                                                                     new object[] { "public", "companyaddress", "companyaddress_pkey", "CREATE UNIQUE INDEX companyaddress_pkey ON public.companyaddress USING btree (id)" },
                                                                                                                                                                                                                     new object[] { "public", "companyaddress", "companyaddress_country_state_city_postalcode_line_key", "CREATE UNIQUE INDEX companyaddress_country_state_city_postalcode_line_key ON public.companyaddress USING btree (country, state, city, postalcode, line)" },
                                                                                                                                                                                                                     new object[] { "public", "companyaddress", "idx_companyaddress_company", "CREATE INDEX idx_companyaddress_company ON public.companyaddress USING btree (companyid)" }
                                                                                                                                                                                                                 }), null, info);
                                                                                                                                                                      }

                                                                                                                                                                      return new Reader(new FakeReader(Array.Empty<string>(), Array.Empty<object[]>()), null, info);
                                                                                                                                                                  });

        TableDescriptor descriptor = info.GetSchema(client.Object, "companyaddress") as TableDescriptor;

        Assert.NotNull(descriptor);
        Assert.AreEqual("companyaddress", descriptor.Name);
        Assert.AreEqual(1, descriptor.Uniques.Length);
        Assert.AreEqual(1, descriptor.Indices.Length);
        Assert.True(new[] { "country", "state", "city", "postalcode", "line" }.SequenceEqual(descriptor.Uniques.First().Columns));
        Assert.True(new[] { "companyid" }.SequenceEqual(descriptor.Indices.First().Columns));
    }

    [Test, Parallelizable]
    public void LimitStatement() {
        PostgreInfo dbinfo = new PostgreInfo();
        Mock<IDBClient> client = new Mock<IDBClient>();
        client.SetupGet(c => c.DBInfo).Returns(dbinfo);


        PreparedLoadOperation loadop = new LoadOperation<PgView>(client.Object, type => new EntityDescriptor("test"), v=>DBFunction.Count()).Limit(7).Prepare();

        Assert.AreEqual("SELECT count( * ) FROM test LIMIT @1", loadop.CommandText);
    }

    [Test, Parallelizable]
    public void OffsetStatement() {
        PostgreInfo dbinfo = new PostgreInfo();
        Mock<IDBClient> client = new Mock<IDBClient>();
        client.SetupGet(c => c.DBInfo).Returns(dbinfo);


        PreparedLoadOperation loadop = new LoadOperation<PgView>(client.Object, type => new EntityDescriptor("test"), v=>DBFunction.Count()).Offset(3).Prepare();

        Assert.AreEqual("SELECT count( * ) FROM test OFFSET @1", loadop.CommandText);
    }

    [Test, Parallelizable]
    public void LimitAndOffsetStatement() {
        PostgreInfo dbinfo = new PostgreInfo();
        Mock<IDBClient> client = new Mock<IDBClient>();
        client.SetupGet(c => c.DBInfo).Returns(dbinfo);


        PreparedLoadOperation loadop = new LoadOperation<PgView>(client.Object, type => new EntityDescriptor("test"), v=>DBFunction.Count()).Limit(7).Offset(3).Prepare();

        Assert.AreEqual("SELECT count( * ) FROM test LIMIT @1 OFFSET @2", loadop.CommandText);
    }

    [Test, Parallelizable]
    public void NoUpdateForSameTypes() {
        PostgreInfo info = new PostgreInfo();

        Mock<IDBClient> client = new Mock<IDBClient>();
        client.SetupGet(c => c.DBInfo).Returns(info);
        client.Setup(c=>c.Reader(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<IEnumerable<object>>())).Returns<Transaction, string, IEnumerable<object>>((tr, text, pr) => {
                                                                                                                                                                      if(text.Contains(" information_schema.columns ")) {
                                                                                                                                                                          return new Reader(new FakeReader(new[] { "table_catalog", "table_schema", "table_name", "column_name", "data_type", "is_nullable", "column_default" },
                                                                                                                                                                                                           new[] {
                                                                                                                                                                                                                     new object[] { "xx.io", "public", "companyaddress", "id", "bigint", false, "nextval('company_id_seq'::regclass)" },
                                                                                                                                                                                                                     new object[] { "xx.io", "public", "companyaddress", "companyid", "bigint", false, DBNull.Value },
                                                                                                                                                                                                                     new object[] { "xx.io", "public", "companyaddress", "country", "character varying", true, DBNull.Value },
                                                                                                                                                                                                                     new object[] { "xx.io", "public", "companyaddress", "state", "character varying", true, DBNull.Value },
                                                                                                                                                                                                                     new object[] { "xx.io", "public", "companyaddress", "city", "character varying", true, DBNull.Value },
                                                                                                                                                                                                                     new object[] { "xx.io", "public", "companyaddress", "postalcode", "character varying", true, DBNull.Value },
                                                                                                                                                                                                                     new object[] { "xx.io", "public", "companyaddress", "line", "character varying", true, DBNull.Value }
                                                                                                                                                                                                                 }), null, info);
                                                                                                                                                                      }

                                                                                                                                                                      if(text.Contains(" pg_indexes ")) {
                                                                                                                                                                          return new Reader(new FakeReader(new[] { "schemaname", "tablename", "indexname", "indexdef" },
                                                                                                                                                                                                           new[] {
                                                                                                                                                                                                                     new object[] { "public", "companyaddress", "companyaddress_pkey", "CREATE UNIQUE INDEX companyaddress_pkey ON public.companyaddress USING btree (id)" },
                                                                                                                                                                                                                     new object[] { "public", "companyaddress", "companyaddress_country_state_city_postalcode_line_key", "CREATE UNIQUE INDEX companyaddress_country_state_city_postalcode_line_key ON public.companyaddress USING btree (country, state, city, postalcode, line)" },
                                                                                                                                                                                                                     new object[] { "public", "companyaddress", "idx_companyaddress_company", "CREATE INDEX idx_companyaddress_company ON public.companyaddress USING btree (companyid)" }
                                                                                                                                                                                                                 }), null, info);
                                                                                                                                                                      }

                                                                                                                                                                      return new Reader(new FakeReader(Array.Empty<string>(), Array.Empty<object[]>()), null, info);
                                                                                                                                                                  });
        client.Setup(c => c.NonQuery(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<object[]>())).Returns<Transaction, string, object[]>((tr, text, pr) => {
                                                                                                                                                    if(text.StartsWith("ALTER") || text.StartsWith("DROP") || text.StartsWith("CREATE") || text.StartsWith("INSERT"))
                                                                                                                                                        Assert.Fail();
                                                                                                                                                    return 0;
                                                                                                                                                });
        client.Setup(c => c.Scalar(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<object[]>())).Returns(1L);
        SchemaUpdater updater = new(new EntityDescriptorCache());
        updater.Update<CompanyAddress>(client.Object);
    }

    [Test, Parallelizable]
    public async Task NoUpdateForCampaign() {
        PostgreInfo info = new();

        Mock<IDBClient> client = new();
        client.SetupGet(c => c.DBInfo).Returns(info);
        client.Setup(c => c.ScalarAsync(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<object[]>())).Returns(Task.FromResult((object)1));
        client.Setup(c => c.ReaderAsync(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<IEnumerable<object>>)).Returns<Transaction, string, IEnumerable<object>>((transaction, command, parameters) => {
                                                                                                                                                                           return Task.FromResult(new Reader(new FakeReader(Array.Empty<string>(), Array.Empty<object[]>()), null, info));
                                                                                                                                                                       });
        client.Setup(c=>c.Reader(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<IEnumerable<object>>())).Returns<Transaction, string, IEnumerable<object>>((tr, text, pr) => {
                                                                                                                                                                      if(text.Contains(" information_schema.columns ")) {
                                                                                                                                                                          return new Reader(new FakeReader(new[] { "table_catalog", "table_schema", "table_name", "column_name", "data_type", "is_nullable", "column_default" },
                                                                                                                                                                                                           new[] {
                                                                                                                                                                                                                     new object[] { "mamgo-dev", "public", "campaign", "id", "bigint", false, "nextval('company_id_seq'::regclass)" },
                                                                                                                                                                                                                     new object[] { "mamgo-dev", "public", "campaign", "name", "character varying", true, DBNull.Value },
                                                                                                                                                                                                                     new object[] { "mamgo-dev", "public", "campaign", "budget", "numeric", false, DBNull.Value },
                                                                                                                                                                                                                     new object[] { "mamgo-dev", "public", "campaign", "cpa", "numeric", false, DBNull.Value },
                                                                                                                                                                                                                     new object[] { "mamgo-dev", "public", "campaign", "status", "integer", false, DBNull.Value },
                                                                                                                                                                                                                     new object[] { "mamgo-dev", "public", "campaign", "description", "character varying", true, DBNull.Value },
                                                                                                                                                                                                                     new object[] { "mamgo-dev", "public", "campaign", "predicate", "character varying", true, DBNull.Value },
                                                                                                                                                                                                                     new object[] { "mamgo-dev", "public", "campaign", "ccr", "numeric", false, 0 },
                                                                                                                                                                                                                     new object[] { "mamgo-dev", "public", "campaign", "usemamgolandingpages", "boolean", false, false },
                                                                                                                                                                                                                     new object[] { "mamgo-dev", "public", "campaign", "origin", "character varying", true, DBNull.Value },
                                                                                                                                                                                                                     new object[] { "mamgo-dev", "public", "campaign", "usejobufo", "boolean", false, false },
                                                                                                                                                                                                                     new object[] { "mamgo-dev", "public", "campaign", "applyurlpattern", "character varying", true, DBNull.Value },
                                                                                                                                                                                                                     new object[] { "mamgo-dev", "public", "campaign", "routingscript", "character varying", true, DBNull.Value },
                                                                                                                                                                                                                     new object[] { "mamgo-dev", "public", "campaign", "usepublisher", "boolean", false, false },
                                                                                                                                                                                                                 }), null, info);
                                                                                                                                                                      }

                                                                                                                                                                      if(text.Contains(" pg_indexes ")) {
                                                                                                                                                                          return new Reader(new FakeReader(new[] { "schemaname", "tablename", "indexname", "indexdef" },
                                                                                                                                                                                                           Array.Empty<object[]>()), null, info);
                                                                                                                                                                      }

                                                                                                                                                                      return new Reader(new FakeReader(Array.Empty<string>(), Array.Empty<object[]>()), null, info);
                                                                                                                                                                  });
        client.Setup(c => c.NonQuery(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<object[]>())).Returns<Transaction, string, object[]>((tr, text, pr) => {
                                                                                                                                                    if(text.StartsWith("ALTER") || text.StartsWith("DROP") || text.StartsWith("CREATE") || text.StartsWith("INSERT"))
                                                                                                                                                        Assert.Fail();
                                                                                                                                                    return 0;
                                                                                                                                                });
        client.Setup(c => c.Scalar(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<object[]>())).Returns(1L);
        SchemaService schemaService = new(client.Object);
        await schemaService.CreateOrUpdateSchema<Campaign>();
    }

    [Test, Parallelizable]
    public void NoUpdateForSameTypes2() {
        DataTableColumns indexcolumns = new DataTableColumns() {
                                                                   ["schemaname"] = 0,
                                                                   ["tablename"] = 1,
                                                                   ["indexname"] = 2,
                                                                   ["indexdef"] = 3
                                                               };

        PostgreInfo info = new PostgreInfo();

        Mock<IDBClient> client = new Mock<IDBClient>();
        client.SetupGet(c => c.DBInfo).Returns(info);
        client.Setup(c => c.Reader(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<IEnumerable<object>>())).Returns<Transaction, string, IEnumerable<object>>((tr, text, pr) => {
                                                                                                                                                                        if(text.Contains(" information_schema.columns ")) {
                                                                                                                                                                            return new Reader(new FakeReader(
                                                                                                                                                                                                             new[] { "table_catalog", "table_schema", "table_name", "column_name", "data_type", "is_nullable", "column_default" },
                                                                                                                                                                                                             new[] {
                                                                                                                                                                                                                       new object[] { "xx.io", "public", "company", "id", "bigint", "NO", "nextval('company_id_seq1'::regclass)" },
                                                                                                                                                                                                                       new object[] { "xx.io", "public", "company", "name", "character varying", "YES", DBNull.Value },
                                                                                                                                                                                                                       new object[] { "xx.io", "public", "company", "url", "character varying", "YES", DBNull.Value }
                                                                                                                                                                                                                   }), null, info);
                                                                                                                                                                        }

                                                                                                                                                                        if(text.Contains(" pg_indexes ")) {
                                                                                                                                                                            return new Reader(new FakeReader(
                                                                                                                                                                                                             new[] { "schemaname", "tablename", "indexname", "indexdef" },
                                                                                                                                                                                                             new[] {
                                                                                                                                                                                                                       new object[] { "public", "company", "company_pkey1", "CREATE UNIQUE INDEX company_pkey1 ON public.company USING btree (id)" },
                                                                                                                                                                                                                       new object[] { "public", "company", "company_name_key1", "CREATE UNIQUE INDEX company_name_key1 ON public.company USING btree (name)" },
                                                                                                                                                                                                                       new object[] { "public", "company", "company_url_key1", "CREATE UNIQUE INDEX company_url_key1 ON public.company USING btree (url)" },
                                                                                                                                                                                                                       new object[] { "public", "company", "idx_company_name", "CREATE INDEX idx_company_name ON public.company USING btree (name)" },
                                                                                                                                                                                                                       new object[] { "public", "company", "idx_company_url", "CREATE INDEX idx_company_url ON public.company USING btree (url)" },
                                                                                                                                                                                                                   }), null, info);
                                                                                                                                                                        }

                                                                                                                                                                        return new Reader(new FakeReader(Array.Empty<string>(), Array.Empty<object[]>()), null, info);
                                                                                                                                                                    });
        client.Setup(c => c.NonQuery(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<object[]>())).Returns<Transaction, string, object[]>((tr, text, pr) => {
                                                                                                                                                    if(text.StartsWith("ALTER") || text.StartsWith("DROP") || text.StartsWith("CREATE") || text.StartsWith("INSERT"))
                                                                                                                                                        Assert.Fail();
                                                                                                                                                    return 0;
                                                                                                                                                });
        client.Setup(c => c.Scalar(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<object[]>())).Returns(1L);
        SchemaUpdater updater = new(new EntityDescriptorCache());
        updater.Update<Company>(client.Object);
    }

    [Test, Parallelizable]
    public void ExtractViewSql() {

        string sql;
        using(StreamReader reader = new(typeof(PostgreInfoTests).Assembly.GetManifestResourceStream("NightlyCode.Ocelot.Tests.Resources.postgresview.sql")))
            sql = reader.ReadToEnd();

        string viewsql = new SchemaUpdater(null).GetViewCreationSql(sql);
        Assert.NotNull(viewsql);
    }

    [Test, Parallelizable]
    public void VisitDBMax() {
        PostgreInfo info = new();

        Expression<Func<ActiveData, bool>> predicate = d => d.Amount == DB.Max(new[]{d.Amount + 7.8m, -d.Amount}).Decimal;
        OperationPreparator preparator = new();
        CriteriaVisitor visitor = new(EntityDescriptor.Create, preparator, info, false);
        visitor.Visit(predicate);
    }
    
    [Test, Parallelizable]
    public void VisitDBGreatest() {
        PostgreInfo info = new();

        Expression<Func<ActiveData, bool>> predicate = d => d.Amount == DB.Greatest(new[]{d.Amount + 7.8m, -d.Amount}).Decimal;
        OperationPreparator preparator = new();
        CriteriaVisitor visitor = new(EntityDescriptor.Create, preparator, info, false);
        visitor.Visit(predicate);
        Assert.That(preparator.Tokens.Any(t=>t.GetText(info)=="GREATEST("));
    }

    [Test, Parallelizable]
    public void VisitDBLeast() {
        PostgreInfo info = new();

        Expression<Func<ActiveData, bool>> predicate = d => d.Amount == DB.Least(new[]{d.Amount + 7.8m, -d.Amount}).Decimal;
        OperationPreparator preparator = new();
        CriteriaVisitor visitor = new(EntityDescriptor.Create, preparator, info, false);
        visitor.Visit(predicate);
        Assert.That(preparator.Tokens.Any(t=>t.GetText(info)=="LEAST("));
    }
}