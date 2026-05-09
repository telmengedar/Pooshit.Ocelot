using System.Threading.Tasks;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Models;
using Pooshit.Ocelot.Tokens;

namespace Pooshit.Ocelot.Tests.Fields;

/// <summary>
/// Unit tests for <see cref="FieldMapper{TModel}.EntityFromCurrentRow"/> and its relationship
/// to the existing <see cref="FieldMapper{TModel}.EntityFromReader"/> method.
/// </summary>
[TestFixture, Parallelizable]
public class EntityFromCurrentRowTests {

    static FieldMapper<ValueModel> CreateValueMapper() {
        return new FieldMapper<ValueModel>(
            new FieldMapping<ValueModel, int>("integer", DB.Property<ValueModel>(v => v.Integer), (e, v) => e.Integer = v),
            new FieldMapping<ValueModel, string>("string", DB.Property<ValueModel>(v => v.String), (e, v) => e.String = v)
        );
    }

    // -------------------------------------------------------------------------
    // Positioned-on-row materialization works
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public async Task EntityFromCurrentRow_PositionedOnRow_MaterializesCorrectly() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager em = new(dbclient);
        em.UpdateSchema<ValueModel>();
        await em.Insert<ValueModel>().Columns(v => v.Integer, v => v.String).ExecuteAsync(42, "hello");

        FieldMapper<ValueModel> mapper = CreateValueMapper();

        using Reader reader = await em.Load<ValueModel>(
                DB.Property<ValueModel>(v => v.Integer),
                DB.Property<ValueModel>(v => v.String))
            .ExecuteReaderAsync();

        bool hasRow = await reader.ReadAsync();
        Assert.IsTrue(hasRow, "Reader should have one row");

        ValueModel entity = mapper.EntityFromCurrentRow(reader, "integer", "string");
        Assert.AreEqual(42, entity.Integer);
        Assert.AreEqual("hello", entity.String);
    }

    // -------------------------------------------------------------------------
    // EntityFromCurrentRow does NOT advance the reader
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public async Task EntityFromCurrentRow_DoesNotAdvanceReader() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager em = new(dbclient);
        em.UpdateSchema<ValueModel>();
        await em.Insert<ValueModel>().Columns(v => v.Integer).ExecuteAsync(1);
        await em.Insert<ValueModel>().Columns(v => v.Integer).ExecuteAsync(2);

        FieldMapper<ValueModel> mapper = new(
            new FieldMapping<ValueModel, int>("integer", DB.Property<ValueModel>(v => v.Integer), (e, v) => e.Integer = v)
        );

        using Reader reader = await em.Load<ValueModel>(DB.Property<ValueModel>(v => v.Integer)).ExecuteReaderAsync();

        // Advance to row 1 manually
        bool first = await reader.ReadAsync();
        Assert.IsTrue(first);
        ValueModel entity1 = mapper.EntityFromCurrentRow(reader, "integer");

        // Advance to row 2 manually — EntityFromCurrentRow should not have consumed it
        bool second = await reader.ReadAsync();
        Assert.IsTrue(second, "Row 2 must still be available after EntityFromCurrentRow");
        ValueModel entity2 = mapper.EntityFromCurrentRow(reader, "integer");

        Assert.AreNotEqual(entity1.Integer, entity2.Integer, "Two distinct rows should have been materialized");

        bool third = await reader.ReadAsync();
        Assert.IsFalse(third, "No more rows should be available after row 2");
    }

    // -------------------------------------------------------------------------
    // EntityFromReader behaviour is identical (delegates to EntityFromCurrentRow)
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public async Task EntityFromReader_BehaviorIdenticalToReadThenCurrentRow() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager em = new(dbclient);
        em.UpdateSchema<ValueModel>();
        await em.Insert<ValueModel>().Columns(v => v.Integer, v => v.String).ExecuteAsync(7, "test");

        FieldMapper<ValueModel> mapper = CreateValueMapper();

        // Each reader is scoped to its own block — SQLite's single-connection semaphore
        // is held by an open reader and only released on dispose, so two `using Reader` in the
        // same method body would deadlock.
        ValueModel fromReader;
        using (Reader reader1 = await em.Load<ValueModel>(
                       DB.Property<ValueModel>(v => v.Integer),
                       DB.Property<ValueModel>(v => v.String))
                   .ExecuteReaderAsync()) {
            fromReader = await mapper.EntityFromReader(reader1, "integer", "string");
        }

        ValueModel fromCurrentRow;
        using (Reader reader2 = await em.Load<ValueModel>(
                       DB.Property<ValueModel>(v => v.Integer),
                       DB.Property<ValueModel>(v => v.String))
                   .ExecuteReaderAsync()) {
            await reader2.ReadAsync();
            fromCurrentRow = mapper.EntityFromCurrentRow(reader2, "integer", "string");
        }

        Assert.AreEqual(fromReader.Integer, fromCurrentRow.Integer);
        Assert.AreEqual(fromReader.String, fromCurrentRow.String);
    }

    // -------------------------------------------------------------------------
    // EntityFromReader on empty stream returns default
    // -------------------------------------------------------------------------

    [Test, Parallelizable]
    public async Task EntityFromReader_EmptyStream_ReturnsDefault() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager em = new(dbclient);
        em.UpdateSchema<ValueModel>();

        FieldMapper<ValueModel> mapper = CreateValueMapper();

        using Reader reader = await em.Load<ValueModel>(
                DB.Property<ValueModel>(v => v.Integer),
                DB.Property<ValueModel>(v => v.String))
            .ExecuteReaderAsync();

        ValueModel result = await mapper.EntityFromReader(reader, "integer", "string");
        Assert.IsNull(result, "EntityFromReader on empty stream should return default (null for a class)");
    }
}
