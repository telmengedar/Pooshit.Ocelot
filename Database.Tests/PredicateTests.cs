using System;
using System.Linq;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities;
using NightlyCode.Database.Expressions;
using NightlyCode.Database.Tests.Data;
using NightlyCode.Database.Tests.Models;
using NUnit.Framework;

namespace NightlyCode.Database.Tests {
    public class PredicateTests {

        [Test, Parallelizable]
        public void SubBlock() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);
            entitymanager.Create<TestEntityWithoutAnySpecifications>();

            entitymanager.Insert<TestEntityWithoutAnySpecifications>().Columns(t => t.Something, t => t.IntegerValue, t=>t.BooleanValue).Values(1.0, 1, true).Execute();
            entitymanager.Insert<TestEntityWithoutAnySpecifications>().Columns(t => t.Something, t => t.IntegerValue, t => t.BooleanValue).Values(2.0, 2, false).Execute();
            entitymanager.Insert<TestEntityWithoutAnySpecifications>().Columns(t => t.Something, t => t.IntegerValue, t => t.BooleanValue).Values(3.0, 1, true).Execute();
            entitymanager.Insert<TestEntityWithoutAnySpecifications>().Columns(t => t.Something, t => t.IntegerValue, t => t.BooleanValue).Values(4.0, 2, false).Execute();
            entitymanager.Insert<TestEntityWithoutAnySpecifications>().Columns(t => t.Something, t => t.IntegerValue, t => t.BooleanValue).Values(5.0, 3, false).Execute();

            PredicateExpression<TestEntityWithoutAnySpecifications> testpredicate = new PredicateExpression<TestEntityWithoutAnySpecifications>(t => t.Something > 2.0);

            PredicateExpression<TestEntityWithoutAnySpecifications> subquery = new PredicateExpression<TestEntityWithoutAnySpecifications>(t => t.IntegerValue == 1);
            subquery |= t => t.IntegerValue == 2;
            testpredicate &= subquery;

            Assert.AreEqual(2, entitymanager.Load<TestEntityWithoutAnySpecifications>().Where(testpredicate).ExecuteEntities<TestEntityWithoutAnySpecifications>().Count());
        }
    }
}