using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Pooshit.Ocelot.Entities.Schema;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Schemas;

namespace Pooshit.Ocelot.Tests.Sqlite {

    [TestFixture]
    public class SqliteInfoTests {
        readonly SQLiteInfo info = new SQLiteInfo();

        static IEnumerable<string> Definitions
        {
            get
            {
                yield return "CREATE TABLE systemuser ('password' BLOB, 'key' BLOB, 'userid' INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, 'accountname' VARCHAR, 'domain' VARCHAR, 'encryption' INTEGER NOT NULL, 'automaticpasswordchange' BOOLEAN NOT NULL, 'lastpasswordchange' TIMESTAMP, locked BOOLEAN NOT NULL DEFAULT false, UNIQUE ('accountname','domain'))";
                yield return "CREATE TABLE formdata ('userid' INTEGER NOT NULL, 'url' TEXT, 'data' TEXT, behavior INTEGER NOT NULL DEFAULT 0)";
                yield return "CREATE TABLE epartscredentials ('userid' INTEGER NOT NULL, 'brand' INTEGER NOT NULL, 'dealer' TEXT, 'username' TEXT, 'password' BLOB, 'lastrefresh' TIMESTAMP NOT NULL)";
                yield return "CREATE TABLE epartscredentialchangedata ('userid' INTEGER NOT NULL, 'brand' INTEGER NOT NULL, 'password' BLOB)";
                yield return "CREATE TABLE timetrackingevent ('event' INTEGER NOT NULL DEFAULT Login, 'userid' INTEGER NOT NULL DEFAULT 0, 'timestamp' TIMESTAMP NOT NULL DEFAULT '01/01/0001 00:00:00', 'host' TEXT)";
                yield return "CREATE TABLE systemsetting ('key' TEXT UNIQUE, 'value' TEXT)";
            }
        }

        [Test]
        public void TestTableAnalyse([ValueSource(nameof(Definitions))]string sql) {
            TableDescriptor descriptor = new TableDescriptor("test");
            info.AnalyseTableSql(descriptor, sql);
        }

        [Test]
        public void AnalyzeGeneratedPrimaryKey() {
            string sql;
            using (StreamReader reader = new StreamReader(GetType().Assembly.GetManifestResourceStream("NightlyCode.Ocelot.Tests.Resources.GeneratedPrimaryKey.txt")))
                sql = reader.ReadToEnd();

            TableDescriptor descriptor = new TableDescriptor("video");
            info.AnalyseTableSql(descriptor, sql);

            ColumnDescriptor column = descriptor.Columns.FirstOrDefault(c => c.Name == "id");
            Assert.NotNull(column);
            Assert.IsTrue(column.PrimaryKey);
            Assert.IsTrue(column.AutoIncrement);
        }

        /*[Test]
         // order of properties not always the same so test fails sometimes
        public async Task GenerateCreateStatement() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.UpdateSchema<Company>();

            string statement=await entitymanager.DBClient.DBInfo.GenerateCreateStatement(entitymanager.DBClient, "company");
            Assert.AreEqual("CREATE TABLE company ( [id] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL , [name] TEXT UNIQUE , [url] TEXT UNIQUE )", statement);
        }*/
    }
}