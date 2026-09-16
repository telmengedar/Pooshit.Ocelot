using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Entities;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Tests.Security.Models;

namespace Pooshit.Ocelot.Tests.Security;

[TestFixture, Parallelizable]
public class EntityColumnRenderingTests {

    [Test, Parallelizable, Description("UpdateEntitiesOperation renders columns through MaskColumn as [integer]=")]
    public void UpdateEntitiesRendersColumnsThroughMaskColumn() {
        string captured = null;
        Mock<IDBClient> client = new();
        client.Setup(c => c.DBInfo).Returns(new SQLiteInfo());
        client.Setup(c => c.NonQuery(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<IEnumerable<object>>()))
              .Callback<Transaction, string, IEnumerable<object>>((transaction, commandtext, parameters) => captured = commandtext)
              .Returns(1);

        UpdateEntitiesOperation<PkIntegerEntity> operation = new(client.Object, EntityDescriptor.Create);
        operation.Execute(new PkIntegerEntity { Id = 1, Integer = 5 });

        Assert.That(captured, Does.Contain("[integer]="));
        Assert.That(captured, Does.Not.Contain("\"integer\"="));
    }

    [Test, Parallelizable, Description("InsertEntitiesOperation renders columns through MaskColumn as [integer]")]
    public void InsertEntitiesRendersColumnsThroughMaskColumn() {
        string captured = null;
        Mock<IDBClient> client = new();
        client.Setup(c => c.DBInfo).Returns(new SQLiteInfo());
        client.Setup(c => c.NonQuery(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<IEnumerable<object>>()))
              .Callback<Transaction, string, IEnumerable<object>>((transaction, commandtext, parameters) => captured = commandtext)
              .Returns(1);

        EntityManager em = new(client.Object);
        InsertEntitiesOperation<RenderEntity> operation = new(em, EntityDescriptor.Create);
        operation.Execute(new RenderEntity { Id = 1, Integer = 5 });

        Assert.That(captured, Does.Contain("[integer]"));
        Assert.That(captured, Does.Not.Contain("\"integer\""));
    }

    [Test, Parallelizable, Description("DeleteEntitiesOperation renders the primary key column through MaskColumn as [id]")]
    public void DeleteEntitiesRendersColumnsThroughMaskColumn() {
        string captured = null;
        Mock<IDBClient> client = new();
        client.Setup(c => c.DBInfo).Returns(new SQLiteInfo());
        client.Setup(c => c.NonQuery(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<IEnumerable<object>>()))
              .Callback<Transaction, string, IEnumerable<object>>((transaction, commandtext, parameters) => captured = commandtext)
              .Returns(1);
        client.Setup(c => c.NonQuery(It.IsAny<Transaction>(), It.IsAny<string>(), It.IsAny<object[]>()))
              .Callback<Transaction, string, object[]>((transaction, commandtext, parameters) => captured = commandtext)
              .Returns(1);

        DeleteEntitiesOperation<RenderEntity> operation = new(client.Object, EntityDescriptor.Create);
        operation.Execute(new RenderEntity { Id = 1, Integer = 5 });

        Assert.That(captured, Does.Contain("[id] IN"));
        Assert.That(captured, Does.Not.Contain("\"id\""));
    }
}
