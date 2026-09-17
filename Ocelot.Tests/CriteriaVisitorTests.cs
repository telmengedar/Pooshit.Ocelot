using System;
using System.Collections.Generic;
using System.Linq;
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
using Pooshit.Ocelot.Tokens;

namespace Pooshit.Ocelot.Tests;

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
    public void NecessaryParenthesisOperatorPrecedence() {
        OperationPreparator preparator = new();
        CriteriaVisitor visitor = new(EntityDescriptor.Create, preparator, new SQLiteInfo(), true);

        Expression<Func<ValueModel, object>> predicate = m => m.Double / ((m.Double - -m.Double) * m.Double);
        visitor.Visit(predicate);

        IDBClient client = TestData.CreateDatabaseAccess();
        PreparedOperation operation = preparator.GetOperation(client, false);
        Assert.AreEqual("[double] / ( ( [double] - - [double] ) * [double] )", operation.CommandText);
    }

    [Test, Parallelizable]
    public void ValueCallEnclosedInParanthesis() {
        OperationPreparator preparator = new();
        CriteriaVisitor visitor = new(EntityDescriptor.Create, preparator, new SQLiteInfo(), true);

        Expression<Func<ValueModel, object>> predicate = m => DB.Value<ValueModel>(vm => vm.Double - vm.Double).Double * m.Double;
        visitor.Visit(predicate);

        IDBClient client = TestData.CreateDatabaseAccess();
        PreparedOperation operation = preparator.GetOperation(client, false);
        Assert.AreEqual("( [double] - [double] ) * [double]", operation.CommandText);
    }

    [Test, Parallelizable]
    public void ComplementForInteger() {
        OperationPreparator preparator = new();
        CriteriaVisitor visitor = new(EntityDescriptor.Create, preparator, new SQLiteInfo(), true);

        Expression<Func<ValueModel, object>> predicate = m => DB.Value<ValueModel>(vm => ~vm.Integer);
        visitor.Visit(predicate);

        IDBClient client = TestData.CreateDatabaseAccess();
        PreparedOperation operation = preparator.GetOperation(client, false);
        Assert.AreEqual("( ~ [integer] )", operation.CommandText);
    }

    [Test, Parallelizable]
    public void NotForBoolean() {
        OperationPreparator preparator = new();
        CriteriaVisitor visitor = new(EntityDescriptor.Create, preparator, new SQLiteInfo(), true);

        Expression<Func<ValueModel, object>> predicate = m => DB.Value<ValueModel>(vm => !(vm.Integer==vm.Integer));
        visitor.Visit(predicate);

        IDBClient client = TestData.CreateDatabaseAccess();
        PreparedOperation operation = preparator.GetOperation(client, false);
        Assert.AreEqual("( NOT [integer] = [integer] )", operation.CommandText);
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

    [Test, Parallelizable]
    public void TestPerimeter() {
        IDBInfo dbInfo = new PostgreInfo();

        OperationPreparator preparator=new();
        CriteriaVisitor visitor = new(EntityDescriptor.Create, preparator, dbInfo, true);

        double lat1=0.0;
        double lat2=0.0;
        double lon1=1.0;
        double lon2=1.0;
        double pi = Math.PI;
        double perimeter = 50;
        Expression<Func<ValueModel, bool>> predicate = m => Math.Acos(Math.Sin(pi * lat1 / 180.0) * Math.Sin(pi * lat2 / 180.0) + Math.Cos(pi * lat1 / 180.0) * Math.Cos(pi * lat2 / 180.0) * Math.Cos(pi * lon2 / 180.0 - pi * lon1 / 180.0)) * 6371 > perimeter;
        visitor.Visit(predicate);

        Mock<IDBClient> client = new();
        client.SetupGet(s => s.DBInfo).Returns(dbInfo);
            
        PreparedOperation operation = preparator.GetOperation(client.Object, false);
        Assert.AreEqual("ACOS( ( SIN( ( @1 * @2 ) / @3 ) * SIN( ( @4 * @5 ) / @6 ) + COS( ( @7 * @8 ) / @9 ) * COS( ( @10 * @11 ) / @12 ) * COS( ( ( @13 * @14 ) / @15 - ( @16 * @17 ) / @18 ) ) ) ) * @19 > @20", operation.CommandText);
    }

    [Test, Parallelizable]
    public void TestContainsWithArrayRendersInOnSqlite() {
        OperationPreparator preparator = new();
        CriteriaVisitor visitor = new(EntityDescriptor.Create, preparator, new SQLiteInfo(), true);

        int[] array = { 2, 3 };
        Expression<Func<ValueModel, bool>> predicate = m => array.Contains(m.Integer);
        visitor.Visit(predicate);

        IDBClient client = TestData.CreateDatabaseAccess();
        PreparedOperation operation = preparator.GetOperation(client, false);
        Assert.That(operation.CommandText, Is.EqualTo("[integer] IN( @1 , @2 )"));
    }

    [Test, Parallelizable]
    public void TestNotContainsWithArrayRendersNotInOnSqlite() {
        OperationPreparator preparator = new();
        CriteriaVisitor visitor = new(EntityDescriptor.Create, preparator, new SQLiteInfo(), true);

        int[] array = { 2, 3 };
        Expression<Func<ValueModel, bool>> predicate = m => !array.Contains(m.Integer);
        visitor.Visit(predicate);

        IDBClient client = TestData.CreateDatabaseAccess();
        PreparedOperation operation = preparator.GetOperation(client, false);
        Assert.That(operation.CommandText, Is.EqualTo("NOT [integer] IN( @1 , @2 )"));
    }

    [Test, Parallelizable]
    public void TestContainsWithArrayRendersAnyOnPostgres() {
        IDBInfo dbInfo = new PostgreInfo();

        OperationPreparator preparator = new();
        CriteriaVisitor visitor = new(EntityDescriptor.Create, preparator, dbInfo, true);

        int[] array = { 2, 3 };
        Expression<Func<ValueModel, bool>> predicate = m => array.Contains(m.Integer);
        visitor.Visit(predicate);

        Mock<IDBClient> client = new();
        client.SetupGet(s => s.DBInfo).Returns(dbInfo);

        PreparedOperation operation = preparator.GetOperation(client.Object, false);
        Assert.That(operation.CommandText, Is.EqualTo("\"integer\" = ANY( @1 )"));
    }

    [Test, Parallelizable]
    public void TestNotContainsWithArrayRendersNotAnyOnPostgres() {
        IDBInfo dbInfo = new PostgreInfo();

        OperationPreparator preparator = new();
        CriteriaVisitor visitor = new(EntityDescriptor.Create, preparator, dbInfo, true);

        int[] array = { 2, 3 };
        Expression<Func<ValueModel, bool>> predicate = m => !array.Contains(m.Integer);
        visitor.Visit(predicate);

        Mock<IDBClient> client = new();
        client.SetupGet(s => s.DBInfo).Returns(dbInfo);

        PreparedOperation operation = preparator.GetOperation(client.Object, false);
        Assert.That(operation.CommandText, Is.EqualTo("NOT \"integer\" = ANY( @1 )"));
    }

    [Test, Parallelizable]
    public void TestContainsWithParameterArrayRendersArrayPlaceholderOnSqlite() {
        OperationPreparator preparator = new();
        CriteriaVisitor visitor = new(EntityDescriptor.Create, preparator, new SQLiteInfo(), true);

        Expression<Func<ValueModel, bool>> predicate = m => DBParameter<int[]>.Value.Contains(m.Integer);
        visitor.Visit(predicate);

        IDBClient client = TestData.CreateDatabaseAccess();
        PreparedOperation operation = preparator.GetOperation(client, false);
        Assert.That(operation.CommandText, Is.EqualTo("[integer] IN( [0] )"));
    }

    [Test, Parallelizable]
    public void TestContainsWithParameterArrayRendersAnyOnPostgres() {
        IDBInfo dbInfo = new PostgreInfo();

        OperationPreparator preparator = new();
        CriteriaVisitor visitor = new(EntityDescriptor.Create, preparator, dbInfo, true);

        Expression<Func<ValueModel, bool>> predicate = m => DBParameter<int[]>.Value.Contains(m.Integer);
        visitor.Visit(predicate);

        Mock<IDBClient> client = new();
        client.SetupGet(s => s.DBInfo).Returns(dbInfo);

        PreparedOperation operation = preparator.GetOperation(client.Object, false);
        Assert.That(operation.CommandText, Is.EqualTo("\"integer\" = ANY( @1 )"));
    }

    [Test, Parallelizable]
    public void TestContainsWithInlineArrayLiteralRendersInOnSqlite() {
        OperationPreparator preparator = new();
        CriteriaVisitor visitor = new(EntityDescriptor.Create, preparator, new SQLiteInfo(), true);

        Expression<Func<ValueModel, bool>> predicate = m => new[] { 2, 3 }.Contains(m.Integer);
        visitor.Visit(predicate);

        IDBClient client = TestData.CreateDatabaseAccess();
        PreparedOperation operation = preparator.GetOperation(client, false);
        Assert.That(operation.CommandText, Is.EqualTo("[integer] IN( @1 , @2 )"));
    }

    [Test, Parallelizable]
    public void TestContainsWithEnumArrayRendersInOnSqlite() {
        OperationPreparator preparator = new();
        CriteriaVisitor visitor = new(EntityDescriptor.Create, preparator, new SQLiteInfo(), true);

        TestEnum[] enums = { TestEnum.Crazy, TestEnum.Insane };
        Expression<Func<EnumEntity, bool>> predicate = e => enums.Contains(e.Enum);
        visitor.Visit(predicate);

        IDBClient client = TestData.CreateDatabaseAccess();
        PreparedOperation operation = preparator.GetOperation(client, false);
        Assert.That(operation.CommandText, Is.EqualTo("[enum] IN( @1 , @2 )"));
    }

    [Test, Parallelizable]
    public void TestContainsWithEnumArrayRendersAnyOnPostgres() {
        IDBInfo dbInfo = new PostgreInfo();

        OperationPreparator preparator = new();
        CriteriaVisitor visitor = new(EntityDescriptor.Create, preparator, dbInfo, true);

        TestEnum[] enums = { TestEnum.Crazy, TestEnum.Insane };
        Expression<Func<EnumEntity, bool>> predicate = e => enums.Contains(e.Enum);
        visitor.Visit(predicate);

        Mock<IDBClient> client = new();
        client.SetupGet(s => s.DBInfo).Returns(dbInfo);

        PreparedOperation operation = preparator.GetOperation(client.Object, false);
        Assert.That(operation.CommandText, Is.EqualTo("\"enum\" = ANY( @1 )"));
    }

    [Test, Parallelizable]
    public void TestContainsDoesNotStripNonSpanWrapperCall() {
        OperationPreparator preparator = new();
        CriteriaVisitor visitor = new(EntityDescriptor.Create, preparator, new SQLiteInfo(), true);

        TestEnum[] enums = { TestEnum.Crazy, TestEnum.Insane };
        Expression<Func<EnumEntity, bool>> predicate = e => OnlyFirst(enums).Contains(e.Enum);
        visitor.Visit(predicate);

        IDBClient client = TestData.CreateDatabaseAccess();
        PreparedOperation operation = preparator.GetOperation(client, false);
        Assert.That(operation.CommandText, Is.EqualTo("[enum] IN( @1 )"));
    }

    [Test, Parallelizable]
    public void TestContainsWithComparerOverloadThrows() {
        OperationPreparator preparator = new();
        CriteriaVisitor visitor = new(EntityDescriptor.Create, preparator, new SQLiteInfo(), true);

        int[] ints = { 2, 3 };
        Expression<Func<ValueModel, bool>> predicate = m => ints.Contains(m.Integer, EqualityComparer<int>.Default);

        NotImplementedException exception = Assert.Throws<NotImplementedException>(() => visitor.Visit(predicate));
        Assert.That(exception.Message, Does.Contain("Contains[Int32]"));
        Assert.That(exception.Message, Does.Contain("IEqualityComparer"));
    }

    [Test, Parallelizable]
    public void TestInWithArrayRendersInOnSqlite() {
        OperationPreparator preparator = new();
        CriteriaVisitor visitor = new(EntityDescriptor.Create, preparator, new SQLiteInfo(), true);

        int[] intArray = { 2, 3 };
        Expression<Func<ValueModel, bool>> predicate = m => m.Integer.In(intArray);
        visitor.Visit(predicate);

        IDBClient client = TestData.CreateDatabaseAccess();
        PreparedOperation operation = preparator.GetOperation(client, false);
        Assert.That(operation.CommandText, Is.EqualTo("[integer] IN( @1 , @2 )"));
    }

    static TestEnum[] OnlyFirst(TestEnum[] source) => new[] { source[0] };
}