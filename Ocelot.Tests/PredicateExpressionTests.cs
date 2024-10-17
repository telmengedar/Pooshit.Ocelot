using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Expressions;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Expressions;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Models;

namespace Pooshit.Ocelot.Tests {

    [TestFixture, Parallelizable]
    public class PredicateExpressionTests {

        [Test, Parallelizable]
        public void ListOfOrs() {
            OperationPreparator preparator = new OperationPreparator();
            CriteriaVisitor visitor = new CriteriaVisitor(EntityDescriptor.Create, preparator, new SQLiteInfo(), true);

            PredicateExpression<ValueModel> predicate = null;
            for(int i = 0; i < 6; i += 2) {
                int i2 = i + 1;
                predicate |= m => m.Integer == i && m.Integer < i2;
            }

            visitor.Visit(predicate);

            IDBClient client = TestData.CreateDatabaseAccess();
            PreparedOperation operation = preparator.GetOperation(client, false);
            Assert.AreEqual("[integer] = @1 AND [integer] < @2 OR [integer] = @3 AND [integer] < @4 OR [integer] = @5 AND [integer] < @6", operation.CommandText);
        }

        [Test, Parallelizable]
        public void ListOfAnds() {
            OperationPreparator preparator = new OperationPreparator();
            CriteriaVisitor visitor = new CriteriaVisitor(EntityDescriptor.Create, preparator, new SQLiteInfo(), true);

            PredicateExpression<ValueModel> predicate = null;
            for(int i = 0; i < 6; i += 2) {
                int i2 = i + 1;
                predicate &= m => m.Integer == i || m.Integer < i2;
            }

            visitor.Visit(predicate);

            IDBClient client = TestData.CreateDatabaseAccess();
            PreparedOperation operation = preparator.GetOperation(client, false);
            Assert.AreEqual("( [integer] = @1 OR [integer] < @2 ) AND ( [integer] = @3 OR [integer] < @4 ) AND ( [integer] = @5 OR [integer] < @6 )", operation.CommandText);
        }

    }
}