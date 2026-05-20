using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Errors;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Models;
using Pooshit.Ocelot.Tokens;
using DataTable = Pooshit.Ocelot.Clients.Tables.DataTable;

namespace Pooshit.Ocelot.Tests;

[TestFixture, Parallelizable]
public class FieldMapperTests {

    [Test, Parallelizable]
    public async Task TestFullEntity() {
        IEntityManager entityManager = TestData.CreateEntityManager();
        entityManager.UpdateSchema<ValueModel>();
        FieldMapper<ValueModel> mapper = new(
                                             new FieldMapping<ValueModel, int>("integer", DB.Property<ValueModel>(v => v.Integer), (model, i) => model.Integer = i),
                                             new FieldMapping<ValueModel, float>("single", DB.Property<ValueModel>(v => v.Single), (model, i) => model.Single = i),
                                             new FieldMapping<ValueModel, double>("double", DB.Property<ValueModel>(v => v.Double), (model, i) => model.Double = i),
                                             new FieldMapping<ValueModel, string>("string", DB.Property<ValueModel>(v => v.String), (model, i) => model.String = i),
                                             new FieldMapping<ValueModel, DateTime?>("ndatetime", DB.Property<ValueModel>(v => v.NDatetime), (model, i) => model.NDatetime = i),
                                             new FieldMapping<ValueModel, byte[]>("blob", DB.Property<ValueModel>(v => v.Blob), (model, i) => model.Blob = i)
                                            );

        await entityManager.Insert<ValueModel>()
                           .Columns(v => v.Integer, v => v.Single, v => v.Double, v => v.String, v => v.NDatetime, v => v.Blob)
                           .Values(1, 4.5f, 7.8, "haha", new DateTime(2024, 10, 17), new byte[] { 1, 2, 3 })
                           .ExecuteAsync();
        await entityManager.Insert<ValueModel>()
                           .Columns(v => v.Integer, v => v.Single, v => v.Double, v => v.String, v => v.NDatetime, v => v.Blob)
                           .Values(7, 5.4f, 8.8, "hihi", null, null)
                           .ExecuteAsync();

        DataTable table = await entityManager.Load<ValueModel>(mapper.DbFields.ToArray())
                                             .ExecuteAsync();


        ValueModel[] models = mapper.EntitiesFromTable(table).ToArray();
        Assert.AreEqual(2, models.Length);
    }
    
    [Test, Parallelizable]
    public async Task TestSelectedFieldsEntity() {
        IEntityManager entityManager = TestData.CreateEntityManager();
        entityManager.UpdateSchema<ValueModel>();
        FieldMapper<ValueModel> mapper = new(
                                             new FieldMapping<ValueModel, int>("integer", DB.Property<ValueModel>(v => v.Integer), (model, i) => model.Integer = i),
                                             new FieldMapping<ValueModel, float>("single", DB.Property<ValueModel>(v => v.Single), (model, i) => model.Single = i),
                                             new FieldMapping<ValueModel, double>("double", DB.Property<ValueModel>(v => v.Double), (model, i) => model.Double = i),
                                             new FieldMapping<ValueModel, string>("string", DB.Property<ValueModel>(v => v.String), (model, i) => model.String = i),
                                             new FieldMapping<ValueModel, DateTime?>("ndatetime", DB.Property<ValueModel>(v => v.NDatetime), (model, i) => model.NDatetime = i),
                                             new FieldMapping<ValueModel, byte[]>("blob", DB.Property<ValueModel>(v => v.Blob), (model, i) => model.Blob = i)
                                            );

        await entityManager.Insert<ValueModel>()
                           .Columns(v => v.Integer, v => v.Single, v => v.Double, v => v.String, v => v.NDatetime, v => v.Blob)
                           .Values(1, 4.5f, 7.8, "haha", new DateTime(2024, 10, 17), new byte[] { 1, 2, 3 })
                           .ExecuteAsync();
        await entityManager.Insert<ValueModel>()
                           .Columns(v => v.Integer, v => v.Single, v => v.Double, v => v.String, v => v.NDatetime, v => v.Blob)
                           .Values(7, 5.4f, 8.8, "hihi", null, null)
                           .ExecuteAsync();

        DataTable table = await entityManager.Load<ValueModel>(mapper.DbFieldsFromNames("single", "string", "blob").ToArray())
                                             .ExecuteAsync();


        ValueModel[] models = mapper.EntitiesFromTable(table, "single", "string", "blob").ToArray();
        Assert.AreEqual(2, models.Length);
        Assert.AreEqual(0, models[0].Integer);
        Assert.AreEqual(0, models[1].Integer);
        Assert.AreEqual(4.5f, models[0].Single);
        Assert.AreEqual(5.4f, models[1].Single);
        Assert.AreEqual("haha", models[0].String);
        Assert.AreEqual("hihi", models[1].String);
    }

    [Test, Parallelizable]
    public void TestUnknownFieldName_ThrowsUnknownFieldException() {
        FieldMapper<ValueModel> mapper = new(
            new FieldMapping<ValueModel, int>("integer", DB.Property<ValueModel>(v => v.Integer), (model, i) => model.Integer = i),
            new FieldMapping<ValueModel, string>("string", DB.Property<ValueModel>(v => v.String), (model, i) => model.String = i)
        );

        UnknownFieldException ex = Assert.Throws<UnknownFieldException>(() => mapper.DbFieldsFromNames("doesnotexist").ToArray());
        Assert.AreEqual("doesnotexist", ex.FieldName);
        Assert.That(ex.AvailableNames, Does.Contain("integer"));
        Assert.That(ex.AvailableNames, Does.Contain("string"));
    }

    [Test, Parallelizable]
    public void TestUnknownFieldName_ExceptionIsAlsoKeyNotFoundException() {
        FieldMapper<ValueModel> mapper = new(
            new FieldMapping<ValueModel, int>("integer", DB.Property<ValueModel>(v => v.Integer), (model, i) => model.Integer = i),
            new FieldMapping<ValueModel, string>("string", DB.Property<ValueModel>(v => v.String), (model, i) => model.String = i)
        );

        UnknownFieldException ex = Assert.Throws<UnknownFieldException>(() => mapper.DbFieldsFromNames("doesnotexist").ToArray());
        Assert.That(ex, Is.InstanceOf<KeyNotFoundException>());
    }

    [Test, Parallelizable]
    public void TestUnknownFieldName_KnownNamesUnaffected() {
        FieldMapper<ValueModel> mapper = new(
            new FieldMapping<ValueModel, int>("integer", DB.Property<ValueModel>(v => v.Integer), (model, i) => model.Integer = i),
            new FieldMapping<ValueModel, string>("string", DB.Property<ValueModel>(v => v.String), (model, i) => model.String = i)
        );

        IDBField[] fields = mapper.DbFieldsFromNames("integer", "string").ToArray();
        Assert.AreEqual(2, fields.Length);
    }

    [Test, Parallelizable]
    public void TestUnknownFieldName_IndexerThrowsUnknownFieldException() {
        FieldMapper<ValueModel> mapper = new(
            new FieldMapping<ValueModel, int>("integer", DB.Property<ValueModel>(v => v.Integer), (model, i) => model.Integer = i),
            new FieldMapping<ValueModel, string>("string", DB.Property<ValueModel>(v => v.String), (model, i) => model.String = i)
        );

        UnknownFieldException ex = Assert.Throws<UnknownFieldException>(() => { _ = mapper["doesnotexist"]; });
        Assert.AreEqual("doesnotexist", ex.FieldName);
        Assert.That(ex.AvailableNames, Does.Contain("integer"));
        Assert.That(ex.AvailableNames, Does.Contain("string"));
    }

    [Test, Parallelizable]
    public void TestKnownFieldName_CaseInsensitiveMatch_ForwardCached() {
        FieldMapping<ValueModel, int> intMapping = new("integer", DB.Property<ValueModel>(v => v.Integer), (model, i) => model.Integer = i);
        FieldMapper<ValueModel> mapper = new(intMapping);

        FieldMapping<ValueModel> firstLookup = mapper["Integer"];
        FieldMapping<ValueModel> secondLookup = mapper["Integer"];

        Assert.AreSame(intMapping, firstLookup);
        Assert.AreSame(firstLookup, secondLookup);
    }
}