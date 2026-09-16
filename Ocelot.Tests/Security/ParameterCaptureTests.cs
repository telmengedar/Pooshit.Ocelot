using Moq;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Statements;
using Pooshit.Ocelot.Tests.Data;

namespace Pooshit.Ocelot.Tests.Security;

[TestFixture, Parallelizable]
public class ParameterCaptureTests {

    [Test, Parallelizable, Description("SQLiteInfo.GenerateCreateStatement binds the table name as a parameter, not a string-formatted literal")]
    public void SqliteGenerateCreateStatementBindsTableAsParameter() {
        SQLiteInfo dbInfo = new();
        string capturedText = null;
        object[] capturedParams = null;
        Mock<IDBClient> client = new();
        client.Setup(c => c.DBInfo).Returns(dbInfo);
        client.Setup(c => c.ScalarAsync(It.IsAny<string>(), It.IsAny<object[]>()))
              .Callback<string, object[]>((text, parameters) => {
                  capturedText = text;
                  capturedParams = parameters;
              })
              .ReturnsAsync((object)"CREATE TABLE victim (id INTEGER)");

        dbInfo.GenerateCreateStatement(client.Object, "victim").GetAwaiter().GetResult();

        Assert.That(capturedText, Does.Contain("@1"));
        Assert.That(capturedText, Does.Not.Contain("'victim'"));
        Assert.That(capturedParams, Does.Contain("victim"));
    }

    [Test, Parallelizable, Description("PostgreInfo.GenerateCreateStatement binds the table name as a parameter, not a string.Format literal")]
    public void PostgresGenerateCreateStatementBindsTableAsParameter() {
        PostgreInfo dbInfo = new();
        string capturedText = null;
        object[] capturedParams = null;
        Mock<IDBClient> client = new();
        client.Setup(c => c.DBInfo).Returns(dbInfo);
        client.Setup(c => c.ScalarAsync(It.IsAny<string>(), It.IsAny<object[]>()))
              .Callback<string, object[]>((text, parameters) => {
                  capturedText = text;
                  capturedParams = parameters;
              })
              .ReturnsAsync((object)"CREATE TABLE victim (id bigint);");

        dbInfo.GenerateCreateStatement(client.Object, "victim").GetAwaiter().GetResult();

        Assert.That(capturedText, Does.Contain("relname = @1"));
        Assert.That(capturedText, Does.Not.Contain("'{0}'"));
        Assert.That(capturedText, Does.Not.Contain("'victim'"));
        Assert.That(capturedParams, Does.Contain("victim"));
    }

    [Test, Parallelizable, Description("SQLiteInfo.Truncate reset-identity binds the table name as a parameter on the self-managed-transaction branch")]
    public void SqliteTruncateResetIdentityBindsTableAsParameter() {
        SQLiteInfo dbInfo = new();

        string capturedNoTransaction = null;
        object[] paramsNoTransaction = null;
        Mock<IDBClient> clientWithoutTransaction = new();
        clientWithoutTransaction.Setup(c => c.DBInfo).Returns(dbInfo);
        clientWithoutTransaction.Setup(c => c.Transaction()).Returns(TestData.CreateDatabaseAccess().Transaction());
        clientWithoutTransaction.Setup(c => c.NonQueryAsync(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<object[]>()))
                                 .Callback<Transaction, string, object[]>((transaction, text, parameters) => {
                                     if(text.Contains("sqlite_sequence")) {
                                         capturedNoTransaction = text;
                                         paramsNoTransaction = parameters;
                                     }
                                 })
                                 .ReturnsAsync(1);

        dbInfo.Truncate(clientWithoutTransaction.Object, "victim", new() { ResetIdentity = true }).GetAwaiter().GetResult();

        Assert.That(capturedNoTransaction, Does.Contain("[name] = @1"));
        Assert.That(capturedNoTransaction, Does.Not.Contain("'victim'"));
        Assert.That(paramsNoTransaction, Does.Contain("victim"));
    }

    [Test, Parallelizable, Description("SQLiteInfo.Truncate reset-identity binds the table name as a parameter on the caller-transaction branch")]
    public void SqliteTruncateResetIdentityBindsTableAsParameterWithCallerTransaction() {
        SQLiteInfo dbInfo = new();

        string capturedWithTransaction = null;
        object[] paramsWithTransaction = null;
        Mock<IDBClient> clientWithTransaction = new();
        clientWithTransaction.Setup(c => c.DBInfo).Returns(dbInfo);
        clientWithTransaction.Setup(c => c.NonQueryAsync(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<object[]>()))
                              .Callback<Transaction, string, object[]>((transaction, text, parameters) => {
                                  if(text.Contains("sqlite_sequence")) {
                                      capturedWithTransaction = text;
                                      paramsWithTransaction = parameters;
                                  }
                              })
                              .ReturnsAsync(1);

        TruncateOptions options = new() { ResetIdentity = true, Transaction = TestData.CreateDatabaseAccess().Transaction() };
        dbInfo.Truncate(clientWithTransaction.Object, "victim", options).GetAwaiter().GetResult();

        Assert.That(capturedWithTransaction, Does.Contain("[name] = @1"));
        Assert.That(capturedWithTransaction, Does.Not.Contain("'victim'"));
        Assert.That(paramsWithTransaction, Does.Contain("victim"));
    }
}
