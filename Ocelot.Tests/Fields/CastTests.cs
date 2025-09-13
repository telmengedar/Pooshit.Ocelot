using System;
using System.Linq;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Models;
using Pooshit.Ocelot.Tokens;
using Pooshit.Ocelot.Tokens.Values;

namespace Pooshit.Ocelot.Tests.Fields;

[TestFixture, Parallelizable]
public class CastTests {

    [Test, Parallelizable]
    public void DateCast() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new(dbclient);

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
    public void IntervalToTicks() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new(dbclient);

        entitymanager.UpdateSchema<DateTimeValue>();

        PreparedOperation operation = entitymanager.Insert<DateTimeValue>()
                                                   .Columns(m => m.Time, m => m.Value)
                                                   .Prepare();
        operation.Execute(new DateTime(2025, 01, 01), 0);

        DateTime reference = new(2025, 01, 02);
        var result = entitymanager.Load<DateTimeValue>()
                                  .ExecuteEntities();
        long ticks =
            entitymanager.Load<DateTimeValue>(v => DB.Cast(reference - DB.Property<DateTimeValue>(m => m.Time).DateTime, CastType.Ticks))
                         .ExecuteScalar<long>();

        TimeSpan interval = new(ticks);
        Assert.That(interval, Is.EqualTo(TimeSpan.FromDays(1)));
    }

    [Test, Parallelizable]
    public void DateCastByValue() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new(dbclient);

        entitymanager.UpdateSchema<DateTimeValue>();

        PreparedOperation operation = entitymanager.Insert<DateTimeValue>().Columns(m => m.Time, m => m.Value).Prepare();
        operation.Execute(new DateTime(2020, 10, 01, 01, 00, 00), 2);
        operation.Execute(new DateTime(2020, 10, 01, 05, 00, 00), 5);
        operation.Execute(new DateTime(2020, 10, 02, 03, 00, 00), 8);
            
        Tuple<DateTime, int>[] tuple =
            entitymanager.Load<DateTimeValue>(
                                              v => DB.Cast(v.Time, CastType.Date), 
                                              v => DB.Sum(v.Value))
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
        EntityManager entitymanager = new(dbclient);

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
    public void ExtractWeekByValue() {
        IDBClient dbclient = TestData.CreateDatabaseAccess();
        EntityManager entitymanager = new(dbclient);

        entitymanager.UpdateSchema<DateTimeValue>();

        PreparedOperation operation = entitymanager.Insert<DateTimeValue>().Columns(m => m.Time, m => m.Value).Prepare();
        operation.Execute(new DateTime(2020, 02, 01, 01, 00, 00), 2);
        operation.Execute(new DateTime(2020, 07, 01, 05, 00, 00), 5);
        operation.Execute(new DateTime(2020, 10, 02, 03, 00, 00), 8);
            
        int[] tuple =
            entitymanager.Load<DateTimeValue>(v => DB.Cast(v.Time, CastType.WeekOfYear))
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
        Assert.That(tuple.SequenceEqual([6, 3, 5]));
    }
}