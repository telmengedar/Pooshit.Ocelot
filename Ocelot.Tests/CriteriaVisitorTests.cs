using System;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Expressions;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Models;

namespace Pooshit.Ocelot.Tests {

    [TestFixture, Parallelizable]
    public class CriteriaVisitorTests {

        [Test, Parallelizable]
        public void NoParenthesis() {
            OperationPreparator preparator=new();
            CriteriaVisitor visitor = new(EntityDescriptor.Create, preparator, new SQLiteInfo(), true);

            Expression<Func<ValueModel, bool>> predicate = m => m.String == "Hello" && m.Integer == 3 || m.Integer == 0;
            visitor.Visit(predicate);

            IDBClient client = TestData.CreateDatabaseAccess();
            PreparedOperation operation = preparator.GetOperation(client, false);
            Assert.AreEqual("[string] = @1 AND [integer] = @2 OR [integer] = @3", operation.CommandText);
        }
        
        [Test, Parallelizable]
        public void NecessaryParenthesis() {
            OperationPreparator preparator=new();
            CriteriaVisitor visitor = new(EntityDescriptor.Create, preparator, new SQLiteInfo(), true);

            Expression<Func<ValueModel, bool>> predicate = m => m.String == "Hello" && (m.Integer == 3 || m.Integer == 0);
            visitor.Visit(predicate);

            IDBClient client = TestData.CreateDatabaseAccess();
            PreparedOperation operation = preparator.GetOperation(client, false);
            Assert.AreEqual("[string] = @1 AND ( [integer] = @2 OR [integer] = @3 )", operation.CommandText);
        }

        [Test, Parallelizable]
        public void ComplexParenthesis() {
            OperationPreparator preparator=new();
            CriteriaVisitor visitor = new(EntityDescriptor.Create, preparator, new SQLiteInfo(), true);

            Expression<Func<ValueModel, bool>> predicate = m => (m.Integer!=2 && (m.Integer==4 || m.Integer==5)) || (m.Integer==2 && m.Integer==8);
            visitor.Visit(predicate);

            IDBClient client = TestData.CreateDatabaseAccess();
            PreparedOperation operation = preparator.GetOperation(client, false);
            Assert.AreEqual("[integer] <> @1 AND ( [integer] = @2 OR [integer] = @3 ) OR [integer] = @4 AND [integer] = @5", operation.CommandText);
        }

        [Test, Parallelizable]
        public void TestInWithArray() {
            IDBInfo dbInfo = new PostgreInfo();

            OperationPreparator preparator=new();
            CriteriaVisitor visitor = new(EntityDescriptor.Create, preparator, dbInfo, true);

            int[] intArray = { 1, 2, 3, 4, 5 };
            Expression<Func<ValueModel, bool>> predicate = m => m.Integer.In(intArray);
            visitor.Visit(predicate);

            Mock<IDBClient> client = new();
            client.SetupGet(s => s.DBInfo).Returns(dbInfo);
            
            PreparedOperation operation = preparator.GetOperation(client.Object, false);
            Assert.AreEqual("\"integer\" = ANY( @1 )", operation.CommandText);
        }
        
        [Test, Parallelizable]
        public void TestNotInWithArray() {
            IDBInfo dbInfo = new PostgreInfo();

            OperationPreparator preparator=new();
            CriteriaVisitor visitor = new(EntityDescriptor.Create, preparator, dbInfo, true);

            int[] intArray = { 1, 2, 3, 4, 5 };
            Expression<Func<ValueModel, bool>> predicate = m => !m.Integer.In(intArray);
            visitor.Visit(predicate);

            Mock<IDBClient> client = new();
            client.SetupGet(s => s.DBInfo).Returns(dbInfo);
            
            PreparedOperation operation = preparator.GetOperation(client.Object, false);
            Assert.AreEqual("NOT \"integer\" = ANY( @1 )", operation.CommandText);
        }

    }
}