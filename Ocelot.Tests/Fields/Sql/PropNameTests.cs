using System;
using System.Linq.Expressions;
using Moq;
using NightlyCode.Database.Tests.Entities;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Expressions;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Errors;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Tokens;

namespace NightlyCode.Database.Tests.Fields.Sql {

    [TestFixture, Parallelizable]
    public class PropNameTests {

        [Test, Parallelizable]
        [TestCase("Class", false)]
        [TestCase("class", true)]
        public void ResolveExistingProperties(string property, bool ignorecase) {
            Mock<IDBInfo> dbinfo = new Mock<IDBInfo>();
            dbinfo.Setup(i => i.MaskColumn(It.IsAny<string>())).Returns<string>(c => c);
            Mock<IDBClient> dbclient = new Mock<IDBClient>();
            dbclient.SetupGet(c => c.DBInfo).Returns(dbinfo.Object);

            OperationPreparator preparator = new OperationPreparator();
            EntityDescriptor model = EntityDescriptor.Create(typeof(Keywords));

            Expression<Func<Keywords, bool>> predicate = (k) => DB.Property(typeof(Keywords), property, ignorecase) == DB.Property<Keywords>(property, ignorecase);
            CriteriaVisitor.GetCriteriaText(predicate, type => model, dbinfo.Object, preparator);

            PreparedOperation operation = preparator.GetOperation(dbclient.Object, false);
            Assert.AreEqual("class = class", operation.CommandText);
        }

        [Test, Parallelizable]
        [TestCase("Bull", false)]
        [TestCase("bull", true)]
        public void ThrowOnNonExistingProperty(string property, bool ignorecase) {
            Mock<IDBInfo> dbinfo = new Mock<IDBInfo>();
            dbinfo.Setup(i => i.MaskColumn(It.IsAny<string>())).Returns<string>(c => c);
            Mock<IDBClient> dbclient = new Mock<IDBClient>();
            dbclient.SetupGet(c => c.DBInfo).Returns(dbinfo.Object);

            OperationPreparator preparator = new OperationPreparator();
            EntityDescriptor model = EntityDescriptor.Create(typeof(Keywords));

            Expression<Func<Keywords, bool>> predicate = (k) => DB.Property(typeof(Keywords), property, ignorecase) == DB.Property<Keywords>(property, ignorecase);
            Assert.Throws<PropertyNotFoundException>(() => CriteriaVisitor.GetCriteriaText(predicate, type => model, dbinfo.Object, preparator));
        }

        [Test, Parallelizable]
        [TestCase("Class", false)]
        [TestCase("class", true)]
        public void ResolveExistingPropertiesUsingHelper(string property, bool ignorecase) {
            Mock<IDBInfo> dbinfo = new Mock<IDBInfo>();
            dbinfo.Setup(i => i.MaskColumn(It.IsAny<string>())).Returns<string>(c => c);
            Mock<IDBClient> dbclient = new Mock<IDBClient>();
            dbclient.SetupGet(c => c.DBInfo).Returns(dbinfo.Object);

            OperationPreparator preparator = new OperationPreparator();
            EntityDescriptor model = EntityDescriptor.Create(typeof(Keywords));

            Expression<Func<Keywords, bool>> predicate = (k) => Field.Property(typeof(Keywords), property, ignorecase) == Field.Property<Keywords>(property, ignorecase);
            CriteriaVisitor.GetCriteriaText(predicate,type => model, dbinfo.Object, preparator);

            PreparedOperation operation = preparator.GetOperation(dbclient.Object, false);
            Assert.AreEqual("class = class", operation.CommandText);
        }

        [Test, Parallelizable]
        [TestCase("Bull", false)]
        [TestCase("bull", true)]
        public void ThrowOnNonExistingPropertyUsingHelper(string property, bool ignorecase) {
            Mock<IDBInfo> dbinfo = new Mock<IDBInfo>();
            dbinfo.Setup(i => i.MaskColumn(It.IsAny<string>())).Returns<string>(c => c);
            Mock<IDBClient> dbclient = new Mock<IDBClient>();
            dbclient.SetupGet(c => c.DBInfo).Returns(dbinfo.Object);

            OperationPreparator preparator = new OperationPreparator();
            EntityDescriptor model = EntityDescriptor.Create(typeof(Keywords));

            Expression<Func<Keywords, bool>> predicate = (k) => Field.Property(typeof(Keywords), property, ignorecase) == Field.Property<Keywords>(property, ignorecase);
            Assert.Throws<PropertyNotFoundException>(() => CriteriaVisitor.GetCriteriaText(predicate,type => model, dbinfo.Object, preparator));
        }
    }
}