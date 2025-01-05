using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Expressions;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Entities.Schema;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Schemas;
using Pooshit.Ocelot.Tests.Models;
using Pooshit.Ocelot.Tokens;

namespace Pooshit.Ocelot.Tests.Sqlite;

[TestFixture]
public class SqliteInfoTests {
    readonly SQLiteInfo info = new();

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
        TableDescriptor descriptor = new("test");
        info.AnalyseTableSql(descriptor, sql);
    }

    [Test]
    public void AnalyzeGeneratedPrimaryKey() {
        string sql;
        using (StreamReader reader = new StreamReader(GetType().Assembly.GetManifestResourceStream("Pooshit.Ocelot.Tests.Resources.GeneratedPrimaryKey.txt")))
            sql = reader.ReadToEnd();

        TableDescriptor descriptor = new TableDescriptor("video");
        info.AnalyseTableSql(descriptor, sql);

        ColumnDescriptor column = descriptor.Columns.FirstOrDefault(c => c.Name == "id");
        Assert.NotNull(column);
        Assert.IsTrue(column.PrimaryKey);
        Assert.IsTrue(column.AutoIncrement);
    }

    [Test, Parallelizable]
    public void VisitDBAny() {
        Expression<Func<ActiveData, bool>> predicate = d => d.Amount == DB.Any(new[]{d.Amount + 7.8m, -d.Amount}).Decimal;
        OperationPreparator preparator = new();
        CriteriaVisitor visitor = new(EntityDescriptor.Create, preparator, info, false);
        visitor.Visit(predicate);
        Assert.That(preparator.Tokens.Any(t=>t.GetText(info)=="MIN("));
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