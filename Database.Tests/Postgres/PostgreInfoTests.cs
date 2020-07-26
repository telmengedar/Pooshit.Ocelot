using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Clients.Tables;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations;
using NightlyCode.Database.Entities.Operations.Fields;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Entities.Schema;
using NightlyCode.Database.Fields;
using NightlyCode.Database.Info;
using NightlyCode.Database.Info.Postgre;
using NightlyCode.Database.Tests.Entities;
using NUnit.Framework;

namespace NightlyCode.Database.Tests.Postgres {

    [TestFixture, Parallelizable]
    public class PostgreInfoTests {

        [Test, Parallelizable]
        public void GetView() {
            DataTableColumns viewcolumns = new DataTableColumns() {
                ["schemaname"] = 0,
                ["viewname"] = 1,
                ["viewowner"] = 2,
                ["definition"] = 3
            };

            PostgreInfo info = new PostgreInfo();
            Mock<IDBClient> client = new Mock<IDBClient>();
            client.SetupGet(c => c.DBInfo).Returns(info);
            client.Setup(c => c.Query(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<IEnumerable<object>>())).Returns(new DataTable(
                viewcolumns,
                new[] {
                    new DataRow(new object[] {"public", "testview", "postgres", "SELECT whatever FROM something"}, viewcolumns)
                }
            ));

            ViewDescriptor descriptor = info.GetSchema(client.Object, "testview") as ViewDescriptor;

            Assert.NotNull(descriptor);
            Assert.AreEqual("testview", descriptor.Name);
            Assert.AreEqual("SELECT whatever FROM something", descriptor.SQL);
        }

        [Test, Parallelizable]
        public void GetSchema() {
            DataTableColumns columncolumns = new DataTableColumns() {
                ["table_catalog"] = 0,
                ["table_schema"] = 1,
                ["table_name"] = 2,
                ["column_name"] = 3,
                ["data_type"] = 4,
                ["is_nullable"] = 5,
                ["column_default"] = 6
            };

            DataTableColumns indexcolumns = new DataTableColumns() {
                ["schemaname"] = 0,
                ["tablename"] = 1,
                ["indexname"] = 2,
                ["indexdef"] = 3
            };

            PostgreInfo info = new PostgreInfo();

            Mock<IDBClient> client = new Mock<IDBClient>();
            client.SetupGet(c => c.DBInfo).Returns(info);
            client.Setup(c => c.Query(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<IEnumerable<object>>())).Returns<Transaction, string, IEnumerable<object>>((tr, text, pr) => {
                if(text.Contains(" information_schema.columns ")) {
                    return new DataTable(
                        columncolumns,
                        new[] {
                            new DataRow(new object[] {"xx.io", "public", "companyaddress", "id", "bigint", false, "nextval('company_id_seq'::regclass)"}, columncolumns),
                            new DataRow(new object[] {"xx.io", "public", "companyaddress", "companyid", "bigint", false, DBNull.Value}, columncolumns),
                            new DataRow(new object[] {"xx.io", "public", "companyaddress", "country", "character varying", true, DBNull.Value}, columncolumns),
                            new DataRow(new object[] {"xx.io", "public", "companyaddress", "state", "character varying", true, DBNull.Value}, columncolumns),
                            new DataRow(new object[] {"xx.io", "public", "companyaddress", "city", "character varying", true, DBNull.Value}, columncolumns),
                            new DataRow(new object[] {"xx.io", "public", "companyaddress", "postalcode", "character varying", true, DBNull.Value}, columncolumns),
                            new DataRow(new object[] {"xx.io", "public", "companyaddress", "line", "character varying", true, DBNull.Value}, columncolumns)
                        });
                }

                if(text.Contains(" pg_indexes ")) {
                    return new DataTable(
                        indexcolumns,
                        new[] {
                            new DataRow(new object[] {"public", "companyaddress", "companyaddress_pkey", "CREATE UNIQUE INDEX companyaddress_pkey ON public.companyaddress USING btree (id)"}, indexcolumns),
                            new DataRow(new object[] {"public", "companyaddress", "companyaddress_country_state_city_postalcode_line_key", "CREATE UNIQUE INDEX companyaddress_country_state_city_postalcode_line_key ON public.companyaddress USING btree (country, state, city, postalcode, line)"}, indexcolumns),
                            new DataRow(new object[] {"public", "companyaddress", "idx_companyaddress_company", "CREATE INDEX idx_companyaddress_company ON public.companyaddress USING btree (companyid)"}, indexcolumns),
                        });
                }

                return new DataTable(null, new DataRow[0]);
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


            PreparedLoadValuesOperation loadop = new LoadValuesOperation<PgView>(client.Object, type => new EntityDescriptor("test"), v=>DBFunction.Count()).Limit(7).Prepare();

            Assert.AreEqual("SELECT count( * ) FROM test LIMIT 7", loadop.CommandText);
        }

        [Test, Parallelizable]
        public void OffsetStatement() {
            PostgreInfo dbinfo = new PostgreInfo();
            Mock<IDBClient> client = new Mock<IDBClient>();
            client.SetupGet(c => c.DBInfo).Returns(dbinfo);


            PreparedLoadValuesOperation loadop = new LoadValuesOperation<PgView>(client.Object, type => new EntityDescriptor("test"), v=>DBFunction.Count()).Offset(3).Prepare();

            Assert.AreEqual("SELECT count( * ) FROM test OFFSET 3", loadop.CommandText);
        }

        [Test, Parallelizable]
        public void LimitAndOffsetStatement() {
            PostgreInfo dbinfo = new PostgreInfo();
            Mock<IDBClient> client = new Mock<IDBClient>();
            client.SetupGet(c => c.DBInfo).Returns(dbinfo);


            PreparedLoadValuesOperation loadop = new LoadValuesOperation<PgView>(client.Object, type => new EntityDescriptor("test"), v=>DBFunction.Count()).Limit(7).Offset(3).Prepare();

            Assert.AreEqual("SELECT count( * ) FROM test LIMIT 7 OFFSET 3", loadop.CommandText);
        }

        [Test, Parallelizable]
        public void NoUpdateForSameTypes() {
            DataTableColumns columncolumns = new DataTableColumns() {
                ["table_catalog"] = 0,
                ["table_schema"] = 1,
                ["table_name"] = 2,
                ["column_name"] = 3,
                ["data_type"] = 4,
                ["is_nullable"] = 5,
                ["column_default"] = 6
            };

            DataTableColumns indexcolumns = new DataTableColumns() {
                ["schemaname"] = 0,
                ["tablename"] = 1,
                ["indexname"] = 2,
                ["indexdef"] = 3
            };

            PostgreInfo info = new PostgreInfo();

            Mock<IDBClient> client = new Mock<IDBClient>();
            client.SetupGet(c => c.DBInfo).Returns(info);
            client.Setup(c => c.Query(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<IEnumerable<object>>())).Returns<Transaction, string, IEnumerable<object>>((tr, text, pr) => {
                if(text.Contains(" information_schema.columns ")) {
                    return new DataTable(
                        columncolumns,
                        new[] {
                            new DataRow(new object[] {"xx.io", "public", "companyaddress", "id", "bigint", "NO", "nextval('company_id_seq'::regclass)"}, columncolumns),
                            new DataRow(new object[] {"xx.io", "public", "companyaddress", "companyid", "bigint", "NO", DBNull.Value}, columncolumns),
                            new DataRow(new object[] {"xx.io", "public", "companyaddress", "country", "character varying", "YES", DBNull.Value}, columncolumns),
                            new DataRow(new object[] {"xx.io", "public", "companyaddress", "state", "character varying", "YES", DBNull.Value}, columncolumns),
                            new DataRow(new object[] {"xx.io", "public", "companyaddress", "city", "character varying", "YES", DBNull.Value}, columncolumns),
                            new DataRow(new object[] {"xx.io", "public", "companyaddress", "postalcode", "character varying", "YES", DBNull.Value}, columncolumns),
                            new DataRow(new object[] {"xx.io", "public", "companyaddress", "line", "character varying", "YES", DBNull.Value}, columncolumns)
                        });
                }

                if(text.Contains(" pg_indexes ")) {
                    return new DataTable(
                        indexcolumns,
                        new[] {
                            new DataRow(new object[] {"public", "companyaddress", "companyaddress_pkey", "CREATE UNIQUE INDEX companyaddress_pkey ON public.companyaddress USING btree (id)"}, indexcolumns),
                            new DataRow(new object[] {"public", "companyaddress", "companyaddress_country_state_city_postalcode_line_key", "CREATE UNIQUE INDEX companyaddress_country_state_city_postalcode_line_key ON public.companyaddress USING btree (country, state, city, postalcode, line)"}, indexcolumns),
                            new DataRow(new object[] {"public", "companyaddress", "idx_companyaddress_company", "CREATE INDEX idx_companyaddress_company ON public.companyaddress USING btree (companyid)"}, indexcolumns),
                        });
                }

                return new DataTable(null, new DataRow[0]);
            });
            client.Setup(c => c.NonQuery(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<object[]>())).Returns<Transaction, string, object[]>((tr, text, pr) => {
                if(text.StartsWith("ALTER") || text.StartsWith("DROP") || text.StartsWith("CREATE") || text.StartsWith("INSERT"))
                    Assert.Fail();
                return 0;
            });
            client.Setup(c => c.Scalar(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<object[]>())).Returns(1L);
            SchemaUpdater updater = new SchemaUpdater(new EntityDescriptorCache());
            updater.Update<CompanyAddress>(client.Object);
        }

        [Test, Parallelizable]
        public void NoUpdateForSameTypes2() {
            DataTableColumns columncolumns = new DataTableColumns() {
                ["table_catalog"] = 0,
                ["table_schema"] = 1,
                ["table_name"] = 2,
                ["column_name"] = 3,
                ["data_type"] = 4,
                ["is_nullable"] = 5,
                ["column_default"] = 6
            };

            DataTableColumns indexcolumns = new DataTableColumns() {
                ["schemaname"] = 0,
                ["tablename"] = 1,
                ["indexname"] = 2,
                ["indexdef"] = 3
            };

            PostgreInfo info = new PostgreInfo();

            Mock<IDBClient> client = new Mock<IDBClient>();
            client.SetupGet(c => c.DBInfo).Returns(info);
            client.Setup(c => c.Query(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<IEnumerable<object>>())).Returns<Transaction, string, IEnumerable<object>>((tr, text, pr) => {
                if(text.Contains(" information_schema.columns ")) {
                    return new DataTable(
                        columncolumns,
                        new[] {
                            new DataRow(new object[] {"xx.io", "public", "company", "id", "bigint", "NO", "nextval('company_id_seq1'::regclass)"}, columncolumns),
                            new DataRow(new object[] {"xx.io", "public", "company", "name", "character varying", "YES", DBNull.Value}, columncolumns),
                            new DataRow(new object[] {"xx.io", "public", "company", "url", "character varying", "YES", DBNull.Value}, columncolumns)
                        });
                }

                if(text.Contains(" pg_indexes ")) {
                    return new DataTable(
                        indexcolumns,
                        new[] {
                            new DataRow(new object[] {"public", "company", "company_pkey1", "CREATE UNIQUE INDEX company_pkey1 ON public.company USING btree (id)"}, indexcolumns),
                            new DataRow(new object[] {"public", "company", "company_name_key1", "CREATE UNIQUE INDEX company_name_key1 ON public.company USING btree (name)"}, indexcolumns),
                            new DataRow(new object[] {"public", "company", "company_url_key1", "CREATE UNIQUE INDEX company_url_key1 ON public.company USING btree (url)"}, indexcolumns),
                            new DataRow(new object[] {"public", "company", "idx_company_name", "CREATE INDEX idx_company_name ON public.company USING btree (name)"}, indexcolumns),
                            new DataRow(new object[] {"public", "company", "idx_company_url", "CREATE INDEX idx_company_url ON public.company USING btree (url)"}, indexcolumns),
                        });
                }

                return new DataTable(null, new DataRow[0]);
            });
            client.Setup(c => c.NonQuery(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<object[]>())).Returns<Transaction, string, object[]>((tr, text, pr) => {
                if(text.StartsWith("ALTER") || text.StartsWith("DROP") || text.StartsWith("CREATE") || text.StartsWith("INSERT"))
                    Assert.Fail();
                return 0;
            });
            client.Setup(c => c.Scalar(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<object[]>())).Returns(1L);
            SchemaUpdater updater = new SchemaUpdater(new EntityDescriptorCache());
            updater.Update<Company>(client.Object);
        }

        [Test, Parallelizable]
        public void ExtractViewSql() {

            string sql;
            using(StreamReader reader = new StreamReader(typeof(PostgreInfoTests).Assembly.GetManifestResourceStream("NightlyCode.Database.Tests.Resources.postgresview.sql")))
                sql = reader.ReadToEnd();

            string viewsql = new SchemaUpdater(null).GetViewCreationSql(sql);
            Assert.NotNull(viewsql);
        }
    }
}