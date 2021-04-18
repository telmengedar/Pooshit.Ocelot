using System;
using System.Linq;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Tests.Data;
using NightlyCode.Database.Tests.Models;
using NightlyCode.Database.Tokens;
using NightlyCode.Database.Tokens.Values;
using NUnit.Framework;

namespace NightlyCode.Database.Tests.Fields {
    
    [TestFixture, Parallelizable]
    public class CastTests {

        [Test, Parallelizable]
        public void DateCast() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<DateTimeValue>();

            PreparedOperation operation = entitymanager.Insert<DateTimeValue>().Columns(m => m.Time, m => m.Value).Prepare();
            operation.Execute(new DateTime(2020, 10, 01, 01, 00, 00), 2);
            operation.Execute(new DateTime(2020, 10, 01, 05, 00, 00), 5);
            operation.Execute(new DateTime(2020, 10, 02, 03, 00, 00), 8);
            
            Tuple<DateTime, int>[] tuple =
                entitymanager.Load<DateTimeValue>(
                        v => DB.Cast(DB.Property<DateTimeValue>(m => m.Time), CastType.Date), 
                        v => DB.Sum(DB.Property<DateTimeValue>(m => m.Value)))
                    .GroupBy(DB.Cast(DB.Property<DateTimeValue>(m => m.Time), CastType.Date))
                    .ExecuteTypes(r => new Tuple<DateTime, int>(r.GetValue<DateTime>(0), r.GetValue<int>(1)))
                    .ToArray();

            Assert.AreEqual(2, tuple.Length);
            Assert.AreEqual(new DateTime(2020,10,01), tuple[0].Item1);
            Assert.AreEqual(7, tuple[0].Item2);
            Assert.AreEqual(new DateTime(2020,10,02), tuple[1].Item1);
            Assert.AreEqual(8, tuple[1].Item2);
        }
        
        [Test, Parallelizable]
        public void ExtractWeek() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<DateTimeValue>();

            PreparedOperation operation = entitymanager.Insert<DateTimeValue>().Columns(m => m.Time, m => m.Value).Prepare();
            operation.Execute(new DateTime(2020, 02, 01, 01, 00, 00), 2);
            operation.Execute(new DateTime(2020, 07, 01, 05, 00, 00), 5);
            operation.Execute(new DateTime(2020, 10, 02, 03, 00, 00), 8);
            
            int[] tuple =
                entitymanager.Load<DateTimeValue>(v => DB.Cast(DB.Property<DateTimeValue>(m => m.Time), CastType.WeekOfYear))
                    .ExecuteSet<int>()
                    .ToArray();

            Assert.AreEqual(3, tuple.Length);
            Assert.That(tuple.SequenceEqual(new[] {4, 26, 39}));
        }

        [Test, Parallelizable]
        public void ExtractDayOfWeek() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<DateTimeValue>();

            PreparedOperation operation = entitymanager.Insert<DateTimeValue>().Columns(m => m.Time, m => m.Value).Prepare();
            operation.Execute(new DateTime(2020, 02, 01, 01, 00, 00), 2);
            operation.Execute(new DateTime(2020, 07, 01, 05, 00, 00), 5);
            operation.Execute(new DateTime(2020, 10, 02, 03, 00, 00), 8);
            
            int[] tuple =
                entitymanager.Load<DateTimeValue>(v => DB.Cast(DB.Property<DateTimeValue>(m => m.Time), CastType.DayOfWeek))
                    .ExecuteSet<int>()
                    .ToArray();

            Assert.AreEqual(3, tuple.Length);
            Assert.That(tuple.SequenceEqual(new[] {6, 3, 5}));
        }

        [Test, Parallelizable]
        public void ExtractDayOfWeekUsingAlias() {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.UpdateSchema<DateTimeValue>();

            PreparedOperation operation = entitymanager.Insert<DateTimeValue>().Columns(m => m.Time, m => m.Value).Prepare();
            operation.Execute(new DateTime(2020, 02, 01, 01, 00, 00), 2);
            operation.Execute(new DateTime(2020, 07, 01, 05, 00, 00), 5);
            operation.Execute(new DateTime(2020, 10, 02, 03, 00, 00), 8);
            
            int[] tuple =
                entitymanager.Load<DateTimeValue>(v => DB.Cast(DB.Property<DateTimeValue>(m => m.Time, "dtv"), CastType.DayOfWeek))
                    .Alias("dtv")
                    .ExecuteSet<int>()
                    .ToArray();

            Assert.AreEqual(3, tuple.Length);
            Assert.That(tuple.SequenceEqual(new[] {6, 3, 5}));
        }
    }
}