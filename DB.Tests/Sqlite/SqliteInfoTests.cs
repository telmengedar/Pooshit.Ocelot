using System.Collections.Generic;
using NightlyCode.DB.Entities.Schema;
using NightlyCode.DB.Info;
using NUnit.Framework;

namespace NightlyCode.DB.Tests.Sqlite {

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
    }
}