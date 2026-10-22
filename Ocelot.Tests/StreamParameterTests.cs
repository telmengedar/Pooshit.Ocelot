using System;
using System.Data;
using System.IO;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Moq;
using Moq.Protected;
using Npgsql;
using NUnit.Framework;
using Pooshit.Ocelot.CustomTypes;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Tests.Data;

namespace Pooshit.Ocelot.Tests;

/// <summary>
/// Binary stream parameter binding guards.
/// </summary>
[TestFixture]
public class StreamParameterTests {
    [Test]
    public void ConstructorRejectsNullNegativeLengthAndUnreadableSources() {
        Assert.Throws<ArgumentNullException>(() => new StreamParameter(null, 0));
        using MemoryStream source = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => new StreamParameter(source, -1));
        source.Dispose();
        Assert.Throws<ArgumentException>(() => new StreamParameter(source, 0));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void BindingRejectsMovedPositionMismatchAndDisposedSourcesWithoutReading(bool postgres) {
        IDBInfo info = postgres ? new PostgreInfo() : new SQLiteInfo();
        using IDbCommand command = postgres ? new NpgsqlCommand() : new SqliteCommand();
        using MemoryStream source = new(new byte[] { 1, 2, 3 });
        StreamParameter parameter = new(source, 3);
        source.Position = 1;
        Assert.Throws<ArgumentException>(() => info.CreateParameter(command, parameter));
        Assert.AreEqual(1, source.Position);
        source.Position = 0;
        Assert.Throws<ArgumentException>(() => info.CreateParameter(command, new StreamParameter(source, 2)));
        Assert.AreEqual(0, source.Position);
        source.Dispose();
        Assert.Throws<ArgumentException>(() => info.CreateParameter(command, parameter));
        Assert.AreEqual(0, command.Parameters.Count);
    }

    [Test]
    public void PostgreSqlBindsOriginalStreamAsBinaryWithoutSizeOrReading() {
        using MemoryStream source = new(new byte[] { 9, 0, 255 });
        source.Position = 1;
        using NpgsqlCommand command = new();
        new PostgreInfo().CreateParameter(command, new StreamParameter(source, 2));
        Assert.AreSame(source, command.Parameters[0].Value);
        Assert.AreEqual(DbType.Binary, command.Parameters[0].DbType);
        Assert.AreEqual(0, command.Parameters[0].Size);
        Assert.AreEqual(1, source.Position);
    }

    [Test]
    public void PostgreSqlRejectsNonseekableWithoutRead() {
        using ShortReadStream source = new(new byte[] { 1, 2 });
        using NpgsqlCommand command = new();
        Assert.Throws<NotSupportedException>(() => new PostgreInfo().CreateParameter(command, new StreamParameter(source, 2)));
        Assert.AreEqual(0, source.Reads);
        Assert.IsTrue(source.CanRead);
    }

    [Test]
    public void SqliteMaterializesShortReadsAndRejectsPrematureEof() {
        using ShortReadStream source = new(new byte[] { 0, 255, 3, 4, 5 });
        using SqliteCommand command = new();
        new SQLiteInfo().CreateParameter(command, new StreamParameter(source, 5));
        CollectionAssert.AreEqual(new byte[] { 0, 255, 3, 4, 5 }, (byte[])command.Parameters[0].Value);
        Assert.Greater(source.Reads, 1);
        Assert.IsTrue(source.CanRead);
        using ShortReadStream truncated = new(new byte[] { 1 });
        Assert.Throws<EndOfStreamException>(() => new SQLiteInfo().CreateParameter(command, new StreamParameter(truncated, 2)));
        Assert.IsTrue(truncated.CanRead);
        Assert.AreEqual(1, command.Parameters.Count);
    }

    [Test]
    public void UnsupportedProvidersRejectCarrierWithoutReading() {
        using ShortReadStream source = new(new byte[] { 1 });
        using SqliteCommand command = new();
        Assert.Throws<NotSupportedException>(() => new MsSqlInfo().CreateParameter(command, new StreamParameter(source, 1)));
        Assert.AreEqual(0, source.Reads);
    }

    [Test]
    public async Task PreparedAsyncDisposesCommandAndOwnedConnectionOnlyAfterCompletion() {
        TaskCompletionSource<int> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        bool commandDisposed = false;
        bool connectionDisposed = false;
        Mock<DbCommand> command = new();
        command.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).Returns(completion.Task);
        command.Protected().Setup("Dispose", ItExpr.IsAny<bool>()).Callback<bool>(_ => commandDisposed = true);
        Mock<DbConnection> connection = new();
        connection.SetupGet(c => c.State).Returns(ConnectionState.Open);
        connection.Protected().Setup<DbCommand>("CreateDbCommand").Returns(command.Object);
        connection.Protected().Setup("Dispose", ItExpr.IsAny<bool>()).Callback<bool>(_ => connectionDisposed = true);
        IDBClient client = ClientFactory.Create(() => connection.Object, new PostgreInfo(), true);
        Task<int> execution = client.NonQueryPreparedAsync(null, "INSERT", Array.Empty<object>(), CancellationToken.None);
        bool disposedBeforeCompletion = commandDisposed || connectionDisposed;
        completion.SetResult(7);
        Assert.AreEqual(7, await execution);
        Assert.IsFalse(disposedBeforeCompletion);
        Assert.IsTrue(commandDisposed);
        Assert.IsTrue(connectionDisposed);
    }
}
