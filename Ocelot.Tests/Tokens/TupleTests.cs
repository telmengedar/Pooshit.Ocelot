using System;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Expressions;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Models;
using Pooshit.Ocelot.Tokens;

namespace Pooshit.Ocelot.Tests.Tokens;

[TestFixture, Parallelizable]
public class TupleTests {
	IDBClient CreateClient() {
		DBInfo dbInfo = new SQLiteInfo();
		Mock<IDBClient> dbclient = new();
		dbclient.SetupGet(c => c.DBInfo).Returns(dbInfo);
		return dbclient.Object;
	}

	[Test, Parallelizable]
	public void GenerateCommandText() {
		IDBClient client = CreateClient();

		OperationPreparator preparator = new();

		Expression<Func<object, bool>> predicate = v => DB.Tuple(1, 2, 3) == DB.Tuple(2, 3, 4);
		CriteriaVisitor.GetCriteriaText(predicate, EntityDescriptor.Create, client.DBInfo, preparator);

		PreparedOperation operation = preparator.GetOperation(client, false);
		Assert.That(operation.CommandText, Is.EqualTo("( @1 , @2 , @3 ) = ( @4 , @5 , @6 )"));
		Assert.That(operation.ConstantParameters, Is.EqualTo(new object[] {1, 2, 3, 2, 3, 4}));
	}

	[Test, Parallelizable]
	public void GenerateCommandTextWithMember() {
		IDBClient client = CreateClient();

		OperationPreparator preparator = new();

		Expression<Func<ValueModel, bool>> predicate = m => DB.Tuple(m.Integer, m.Single) == DB.Tuple(3, 4.0f);
		CriteriaVisitor.GetCriteriaText(predicate, EntityDescriptor.Create, client.DBInfo, preparator);

		PreparedOperation operation = preparator.GetOperation(client, false);
		Assert.That(operation.CommandText, Is.EqualTo("( [integer] , [single] ) = ( @1 , @2 )"));
	}

	[Test, Parallelizable]
	public void GenerateCommandTextWithField() {
		IDBClient client = CreateClient();

		OperationPreparator preparator = new();

		Expression<Func<object, bool>> predicate = v => DB.Tuple(DB.Field("integer"), DB.Field("single")) == DB.Tuple(3, 4.0f);
		CriteriaVisitor.GetCriteriaText(predicate, EntityDescriptor.Create, client.DBInfo, preparator);

		PreparedOperation operation = preparator.GetOperation(client, false);
		Assert.That(operation.CommandText, Is.EqualTo("( integer , single ) = ( @1 , @2 )"));
	}

	[Test, Parallelizable]
	public void FilterEntitiesByTuplePredicate() {
		IDBClient dbclient = TestData.CreateDatabaseAccess();
		EntityManager entitymanager = new(dbclient);
		entitymanager.UpdateSchema<ValueModel>();
		entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single).Values(3, 4.0f).Execute();
		entitymanager.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single).Values(5, 6.0f).Execute();

		ValueModel[] result = entitymanager.Load<ValueModel>()
		                                    .Where(m => DB.Tuple(m.Integer, m.Single) == DB.Tuple(3, 4.0f))
		                                    .ExecuteEntities()
		                                    .ToArray();

		Assert.That(result.Length, Is.EqualTo(1));
		Assert.That(result[0].Integer, Is.EqualTo(3));
	}
}
