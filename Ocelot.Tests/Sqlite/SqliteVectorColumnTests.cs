using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Schemas;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Entities;

namespace Pooshit.Ocelot.Tests.Sqlite;

[TestFixture, Parallelizable]
public class SqliteVectorColumnTests {

    [Test, Parallelizable]
    public void DeclaredVectorProperty_KeepsArrayMapping() {
        SQLiteInfo info = new();

        TableSchema schema = (TableSchema)new SchemaCreator().Create<VectorEntity>(info);

        Assert.That(info.GetVectorType(3), Is.Null);
        Assert.That(schema.Columns.Single(c => c.Name == "embedding").Type, Is.EqualTo(info.GetDBType(typeof(float[]), -1)));
    }

    [Test, Parallelizable]
    public async Task DeclaredVectorProperty_CreateAndUpdateSchema_Works() {
        IDBClient client = TestData.CreateDatabaseAccess();
        EntityManager em = new(client);

        em.Create<VectorEntity>();
        em.UpdateSchema<VectorEntity>();

        SchemaService service = new(client);
        await service.UpdateSchema<VectorEntity>();
        Assert.That(await service.ExistsSchema<VectorEntity>(), Is.True);
    }

    [Test, Parallelizable]
    public void ExternalDialect_WithoutOwnImplementation_HasNoVectorType() {
        Mock<IDBInfo> external = new() { CallBase = true };

        Assert.That(external.Object.GetVectorType(3), Is.Null);
    }
}
