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

namespace Pooshit.Ocelot.Tests.Fields.Sql {
    
    [TestFixture, Parallelizable]
    public class PropertyTests {
        
        [Test, Parallelizable]
        public void ResolveExistingProperties() {
            Mock<IDBInfo> dbinfo = new();
            dbinfo.Setup(i => i.MaskColumn(It.IsAny<string>())).Returns<string>(c => c);
            Mock<IDBClient> dbclient = new();
            dbclient.SetupGet(c => c.DBInfo).Returns(dbinfo.Object);

            OperationPreparator preparator = new();
            EntityDescriptor model = EntityDescriptor.Create(typeof(Keywords));

            PropertyInfo property = typeof(Keywords).GetProperty("Class");
            Expression<Func<Keywords, bool>> predicate = k => DB.Property(property, null) == DB.Property(property, null);
            CriteriaVisitor.GetCriteriaText(predicate, type => model, dbinfo.Object, preparator);

            PreparedOperation operation = preparator.GetOperation(dbclient.Object, false);
            Assert.AreEqual("keywords.class = keywords.class", operation.CommandText);
        }

    }
}