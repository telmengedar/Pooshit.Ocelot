using System;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Expressions;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Info;
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
		Assert.AreEqual("[keywords].[class] = [keywords].[class]", operation.CommandText);

	}
}