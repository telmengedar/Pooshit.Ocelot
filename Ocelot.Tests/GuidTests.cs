using System;
using Microsoft.Data.Sqlite;
using NightlyCode.Database.Tests.Models;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Info;

namespace NightlyCode.Database.Tests {

    [TestFixture, Parallelizable]
    public class GuidTests {

        [Test, Parallelizable]
        public void FindGuids() {
            IEntityManager database = new EntityManager(ClientFactory.Create(new SqliteConnection("Data Source=:memory:"), new SQLiteInfo()));
            database.UpdateSchema<GuidEntity>();

            Guid guid = Guid.NewGuid();
            database.Insert<GuidEntity>().Columns(g => g.Guid, g => g.SomeValue).Values(guid.ToString(), 7).Execute();
            GuidEntity result = database.Load<GuidEntity>().Where(g => g.Guid == DBParameter.Guid).ExecuteEntity<GuidEntity>(guid.ToString());

            Assert.NotNull(result);
            Assert.AreEqual(7, result.SomeValue);
        }
    }
}