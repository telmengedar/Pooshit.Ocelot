using System.Threading.Tasks;
using NightlyCode.Database.Tests.Data;
using NightlyCode.Database.Tests.Entities;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Schemas;

namespace NightlyCode.Database.Tests; 

[TestFixture, Parallelizable]
public class DataTypeTests {
    
    [Test, Parallelizable]
    public async Task BigInteger() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new(dbclient);

        BigIntData entity = new() {
            Data = System.Numerics.BigInteger.Parse("281470698565120")
        };
        SchemaService schemaService = new(dbclient);
        await schemaService.CreateOrUpdateSchema<BigIntData>();

        PreparedOperation insertoperation = entitymanager.Insert<BigIntData>()
            .Columns(d => d.Data)
            .Prepare();

        await insertoperation.ExecuteAsync(entity.Data);

        BigIntData loadedData = await entitymanager.Load<BigIntData>()
            .ExecuteEntityAsync();

        Assert.NotNull(loadedData);
        Assert.AreEqual(entity.Data, loadedData.Data);
    }

}