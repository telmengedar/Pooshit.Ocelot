using System;
using System.Linq.Expressions;
using System.Reflection;
using Moq;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Expressions;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Tests.Entities;
using Pooshit.Ocelot.Tokens;

namespace Pooshit.Ocelot.Tests;

[TestFixture, Parallelizable]
public class FunctionTests {
	
	[Test, Parallelizable]
	public void CallCustomFunction() {
		DBInfo dbInfo = new SQLiteInfo();
		Mock<IDBClient> dbclient = new();
		dbclient.SetupGet(c => c.DBInfo).Returns(dbInfo);

		OperationPreparator preparator = new();
		EntityDescriptor model = EntityDescriptor.Create(typeof(Keywords));

		Expression<Func<Keywords, object>> predicate = k => DB.CustomFunction("test");
		CriteriaVisitor.GetAssignmentText(predicate, type => model, dbInfo, preparator);

		PreparedOperation operation = preparator.GetOperation(dbclient.Object, false);
		Assert.That(operation.CommandText, Is.EqualTo("test ( )"));
	}

	[Test, Parallelizable]
	public void CallCustomFunctionWithTypeComparision() {
		DBInfo dbInfo = new SQLiteInfo();
		Mock<IDBClient> dbclient = new();
		dbclient.SetupGet(c => c.DBInfo).Returns(dbInfo);

		OperationPreparator preparator = new();
		EntityDescriptor model = EntityDescriptor.Create(typeof(Keywords));

		Expression<Func<Keywords, bool>> predicate = k => 7==DB.CustomFunction("test").Type<int>();
		CriteriaVisitor.GetAssignmentText(predicate, type => model, dbInfo, preparator);

		PreparedOperation operation = preparator.GetOperation(dbclient.Object, false);
		Assert.That(operation.CommandText, Is.EqualTo("@1 = test ( )"));
	}
	
	[Test, Parallelizable]
	public void CallCustomFunctionWithTypeComparisionAndArguments() {
		DBInfo dbInfo = new SQLiteInfo();
		Mock<IDBClient> dbclient = new();
		dbclient.SetupGet(c => c.DBInfo).Returns(dbInfo);

		OperationPreparator preparator = new();
		EntityDescriptor model = EntityDescriptor.Create(typeof(Keywords));

		Expression<Func<Keywords, bool>> predicate = k => 7==DB.CustomFunction("test", DB.Property<Keywords>(w=>w.Class), DB.Constant(7)).Type<int>();
		CriteriaVisitor.GetAssignmentText(predicate, type => model, dbInfo, preparator);

		PreparedOperation operation = preparator.GetOperation(dbclient.Object, false);
		Assert.That(operation.CommandText, Is.EqualTo("@1 = test ( [class] , @2 )"));
	}

}