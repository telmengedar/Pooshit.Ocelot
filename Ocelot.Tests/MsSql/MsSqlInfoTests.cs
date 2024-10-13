using Moq;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Info.Postgre;

namespace NightlyCode.Database.Tests.MsSql {

    [TestFixture, Parallelizable]
    public class MsSqlInfoTests {

        [Test, Parallelizable]
        public void LimitStatement() {
            MsSqlInfo dbinfo = new MsSqlInfo();
            Mock<IDBClient> client = new Mock<IDBClient>();
            client.SetupGet(c => c.DBInfo).Returns(dbinfo);


            PreparedLoadOperation loadop = new LoadOperation<PgView>(client.Object, type => new EntityDescriptor("test"), v => DBFunction.Count()).Limit(7).Prepare();

            Assert.AreEqual("SELECT count( * ) FROM test ORDER BY(SELECT NULL) OFFSET @1 ROWS FETCH NEXT @2 ROWS ONLY", loadop.CommandText);
        }

        [Test, Parallelizable]
        public void OffsetStatement() {
            MsSqlInfo dbinfo = new MsSqlInfo();
            Mock<IDBClient> client = new Mock<IDBClient>();
            client.SetupGet(c => c.DBInfo).Returns(dbinfo);


            PreparedLoadOperation loadop = new LoadOperation<PgView>(client.Object, type => new EntityDescriptor("test"), v => DBFunction.Count()).Offset(3).Prepare();

            Assert.AreEqual("SELECT count( * ) FROM test ORDER BY(SELECT NULL) OFFSET @1 ROWS", loadop.CommandText);
        }

        [Test, Parallelizable]
        public void LimitAndOffsetStatement() {
            MsSqlInfo dbinfo = new MsSqlInfo();
            Mock<IDBClient> client = new Mock<IDBClient>();
            client.SetupGet(c => c.DBInfo).Returns(dbinfo);


            PreparedLoadOperation loadop = new LoadOperation<PgView>(client.Object, type => new EntityDescriptor("test"), v => DBFunction.Count()).Limit(7).Offset(3).Prepare();

            Assert.AreEqual("SELECT count( * ) FROM test ORDER BY(SELECT NULL) OFFSET @1 ROWS FETCH NEXT @2 ROWS ONLY", loadop.CommandText);
        }

    }
}