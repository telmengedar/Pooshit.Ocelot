using System.IO;
using NightlyCode.Database.Info.Postgre;
using NUnit.Framework;

namespace NightlyCode.Database.Tests.Postgres {
    
    [TestFixture, Parallelizable]
    public class PostgresHelperTests {

        [Test, Parallelizable]
        public void TestCreateStatementProcessing() {
            using StreamReader statementReader=new StreamReader(typeof(PostgresHelperTests).Assembly.GetManifestResourceStream("NightlyCode.Database.Tests.Data.createstatement_postgres.txt"));
            string statement = statementReader.ReadToEnd();
            using StreamReader expectedReader=new StreamReader(typeof(PostgresHelperTests).Assembly.GetManifestResourceStream("NightlyCode.Database.Tests.Data.createstatement_postgres_processed.txt"));
            string expected = expectedReader.ReadToEnd();

            Assert.AreEqual(expected, statement.ProcessCreateStatement());
        }
    }
}