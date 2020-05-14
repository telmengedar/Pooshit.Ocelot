using System;
using System.Linq.Expressions;
using Moq;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Expressions;
using NightlyCode.Database.Entities.Operations.Fields;
using NightlyCode.Database.Entities.Operations.Fields.Sql;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Errors;
using NightlyCode.Database.Info;
using NightlyCode.Database.Tests.Entities;
using NUnit.Framework;

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

            Expression<Func<Keywords, bool>> predicate = (k) => new PropName(typeof(Keywords), property, ignorecase) == new PropName<Keywords>(property, ignorecase);
            new CriteriaVisitor(type => model, preparator, dbinfo.Object).Visit(predicate);

            PreparedOperation operation = preparator.GetOperation(dbclient.Object);
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

            Expression<Func<Keywords, bool>> predicate = (k) => new PropName(typeof(Keywords), property, ignorecase) == new PropName<Keywords>(property, ignorecase);
            Assert.Throws<PropertyNotFoundException>(() => new CriteriaVisitor(type => model, preparator, dbinfo.Object).Visit(predicate));
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
            new CriteriaVisitor(type => model, preparator, dbinfo.Object).Visit(predicate);

            PreparedOperation operation = preparator.GetOperation(dbclient.Object);
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
            Assert.Throws<PropertyNotFoundException>(() => new CriteriaVisitor(type => model, preparator, dbinfo.Object).Visit(predicate));
        }
    }
}