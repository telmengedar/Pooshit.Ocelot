using System;
using System.Linq.Expressions;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Expressions;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Expressions;
using NightlyCode.Database.Fields;
using NightlyCode.Database.Info;
using NightlyCode.Database.Tests.Data;
using NightlyCode.Database.Tests.Models;
using NUnit.Framework;

namespace NightlyCode.Database.Tests {

    [TestFixture, Parallelizable]
    public class CriteriaVisitorTests {

        [Test, Parallelizable]
        public void NoParanthesis() {
            OperationPreparator preparator=new OperationPreparator();
            CriteriaVisitor visitor = new CriteriaVisitor(EntityDescriptor.Create, preparator, new SQLiteInfo());

            Expression<Func<ValueModel, bool>> predicate = m => m.String == "Hello" && m.Integer == 3 || m.Integer == 0;
            visitor.Visit(predicate);

            IDBClient client = TestData.CreateDatabaseAccess();
            PreparedOperation operation = preparator.GetOperation(client);
            Assert.AreEqual("[string] = @1 AND [integer] = @2 OR [integer] = @3", operation.CommandText);
        }
        
        [Test, Parallelizable]
        public void NecessaryParanthesis() {
            OperationPreparator preparator=new OperationPreparator();
            CriteriaVisitor visitor = new CriteriaVisitor(EntityDescriptor.Create, preparator, new SQLiteInfo());

            Expression<Func<ValueModel, bool>> predicate = m => m.String == "Hello" && (m.Integer == 3 || m.Integer == 0);
            visitor.Visit(predicate);

            IDBClient client = TestData.CreateDatabaseAccess();
            PreparedOperation operation = preparator.GetOperation(client);
            Assert.AreEqual("[string] = @1 AND ( [integer] = @2 OR [integer] = @3 )", operation.CommandText);
        }

        [Test, Parallelizable]
        public void ComplexParanthesis() {
            OperationPreparator preparator=new OperationPreparator();
            CriteriaVisitor visitor = new CriteriaVisitor(EntityDescriptor.Create, preparator, new SQLiteInfo());

            Expression<Func<ValueModel, bool>> predicate = m => (m.Integer!=2 && (m.Integer==4 || m.Integer==5)) || (m.Integer==2 && m.Integer==8);
            visitor.Visit(predicate);

            IDBClient client = TestData.CreateDatabaseAccess();
            PreparedOperation operation = preparator.GetOperation(client);
            Assert.AreEqual("[integer] <> @1 AND ( [integer] = @2 OR [integer] = @3 ) OR [integer] = @4 AND [integer] = @5", operation.CommandText);
        }

        // (c.BrokerId != 2 && (c.Type == ClickType.Magic || c.Type == ClickType.Visit)) || (c.BrokerId == 2 && c.Type == ClickType.Broker))

    }
}