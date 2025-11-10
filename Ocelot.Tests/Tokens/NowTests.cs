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
public class NowTests {
	
	IDBClient CreateClient() {
		DBInfo dbInfo = new SQLiteInfo();
		Mock<IDBClient> dbclient = new();
		dbclient.SetupGet(c => c.DBInfo).Returns(dbInfo);
		return dbclient.Object;		
	}
	
	[Test, Parallelizable]
	public void GenerateNow() {
		IDBClient client = CreateClient();

		OperationPreparator preparator = new();

		Expression<Action<object>> predicate = v => DB.Now();
		CriteriaVisitor.GetCriteriaText(predicate, EntityDescriptor.Create, client.DBInfo, preparator);

		PreparedOperation operation = preparator.GetOperation(client, false);
		Assert.AreEqual("now", operation.CommandText);

	}

}