using System;
using Microsoft.Data.Sqlite;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities;
using NightlyCode.Database.Fields;
using NightlyCode.Database.Info;
using NightlyCode.Database.Tests.Models;
using NUnit.Framework;

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