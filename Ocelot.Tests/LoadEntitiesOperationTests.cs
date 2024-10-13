﻿using System;
using NightlyCode.Database.Tests.Data;
using NightlyCode.Database.Tests.Models;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Expressions;
using Pooshit.Ocelot.Fields;

namespace NightlyCode.Database.Tests;

[TestFixture, Parallelizable]
public class LoadEntitiesOperationTests {
        
    [Test, Parallelizable]
    public void SpecifyAlias() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new(dbclient);

        PreparedLoadOperation operation = entitymanager.Load<Option>()
                                                       .Alias("o666")
                                                       .Where((o) => o.Id == Guid.Empty)
                                                       .Prepare();

        Assert.AreEqual("SELECT o666.[id] , o666.[name] , o666.[description] , o666.[type] , o666.[min] , o666.[max] , o666.[mandatory] FROM option AS o666 WHERE o666.[id] = @1", operation.CommandText);
    }

    [Test, Parallelizable]
    public void AliasWithSubQueries() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new(dbclient);
        entitymanager.UpdateSchema<Option>();

        LoadOperation<Option> subquery1 = entitymanager.Load<Option>(o => o.Id).Alias("sq1");
        LoadOperation<Option> subquery2 = entitymanager.Load<Option>(o => o.Id).Alias("sq2").Where(o=>o.Id.In(subquery1));
        LoadOperation<Option> subquery3 = entitymanager.Load<Option>(o => o.Id).Alias("sq3").Where(o => o.Id.In(subquery2) || o.Id.In(subquery1));

        PreparedLoadOperation operation = entitymanager.Load<Option>()
                                                       .Alias("o666")
                                                       .Where((o) => o.Id.In(subquery3) && o.Id.In(subquery2) && o.Id.In(subquery3))
                                                       .Prepare();

        operation.Execute();
    }

    [Test, Parallelizable]
    public void AliasWithSubQueryJoin() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new(dbclient);
        entitymanager.UpdateSchema<Option>();
        entitymanager.UpdateSchema<GuidEntity>();

        LoadOperation<GuidEntity> subquery1 = entitymanager.Load<GuidEntity>(o => o.Guid)
                                                           .Alias("sq1")
                                                           .Join<Option>((o1, o2) => o1.Guid == o2.Id, "sq2")
                                                           .Join<Option>((o1, o2) => o1.Guid == o2.Id, "sq3");

        LoadOperation<GuidEntity> subquery2 = entitymanager.Load<GuidEntity>(o => o.Guid)
                                                           .Alias("sq4")
                                                           .Join<Option>((o1, o2) => o1.Guid == o2.Id, "sq5")
                                                           .Join<Option>((o1, o2) => o1.Guid == o2.Id, "sq6");

        PredicateExpression<Option> predicate = null;
        predicate &= o => o.Id.In(subquery1);
        predicate &= o => o.Id.In(subquery2);

        PreparedLoadOperation operation = entitymanager.Load<Option>()
                                                       .Alias("o666")
                                                       .Where(predicate?.Content)
                                                       .Prepare();

        operation.Execute();
    }

    [Test, Parallelizable]
    public void Fields() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new(dbclient);
        entitymanager.UpdateSchema<Option>();
        entitymanager.UpdateSchema<GuidEntity>();

        LoadOperation<GuidEntity> operation = entitymanager.Load<GuidEntity>()
                                                           .Alias("sq1")
                                                           .Join<Option>((o1, o2) => o1.Guid == o2.Id, "sq2")
                                                           .Fields(o=>o.Guid)
                                                           .Fields<Option>("sq2", o=>o.Name, o=>o.Description);

        string text = operation.Prepare().CommandText;
        Console.WriteLine(text);
    }

}