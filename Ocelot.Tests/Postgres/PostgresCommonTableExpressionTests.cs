using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Npgsql;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Tests.Models;
using Pooshit.Ocelot.Tests.Operations;
using Pooshit.Ocelot.Tokens;

namespace Pooshit.Ocelot.Tests.Postgres;

[TestFixture, Parallelizable]
public class PostgresCommonTableExpressionTests {
    static (IEntityManager Manager, string Table) CreateRows() {
        string connection = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");
        if(string.IsNullOrEmpty(connection))
            Assert.Inconclusive("POSTGRES_CONNECTION is required for live PostgreSQL acceptance.");
        IEntityManager em = new EntityManager(ClientFactory.Create(() => new NpgsqlConnection(connection), new PostgreInfo(), true));
        string table = "cte_" + Guid.NewGuid().ToString("N");
        em.Model<ValueModel>().Table(table);
        em.DBClient.NonQuery($"CREATE TABLE {table} (\"integer\" integer NOT NULL, \"string\" text)");
        for(int i = 0; i < 10; i++)
            em.Insert<ValueModel>().Columns(v => v.Integer, v => v.String).Execute(i, $"row-{i}");
        return (em, table);
    }

    [Test, Parallelizable]
    public void Prepare_DependentChainWithArrays_ExecutesWithLinearSqlAndParameters() {
        (IEntityManager em, string table) = CreateRows();
        try {
            List<int> lengths = [];
            List<int> parameters = [];

            for(int depth = 2; depth <= 8; depth++) {
                int[] values = [1, 2];
                List<CommonTableExpression> definitions = [em.Load<ValueModel>(v => v.Integer).Where(v => v.Integer.In(values)).AsCte("hop0")];
                for(int i = 1; i < depth; i++) {
                    CommonTableExpression previous = definitions[i - 1];
                    definitions.Add(em.Load<ValueModel>(v => v.Integer)
                        .Where(v => (v.Integer.In(previous) || v.Integer.In(previous)) && v.Integer.In(values)).AsCte($"hop{i}"));
                }
                CommonTableExpression last = definitions[depth - 1];

                PreparedLoadOperation<ValueModel> prepared = em.Load<ValueModel>(v => v.Integer).With(definitions.ToArray())
                    .Where(v => v.Integer.In(last) || v.Integer.In(last)).Union(last).Prepare(false);

                Assert.That(prepared.ConstantParameters.Length, Is.EqualTo(depth));
                Assert.That(prepared.ConstantParameters.All(p => p is int[] array && array.SequenceEqual(values)), Is.True);
                Assert.That(prepared.ExecuteSet<int>().OrderBy(i => i), Is.EqualTo(new[] { 1, 1, 2, 2 }));

                parameters.Add(prepared.ConstantParameters.Length);
                lengths.Add(Regex.Replace(Regex.Replace(prepared.CommandText, @"@\d+", "@p"), @"hop\d+", "hop").Length);
            }

            Assert.That(parameters.Zip(parameters.Skip(1), (a, b) => b - a).Distinct(), Is.EqualTo(new[] { 1 }));
            Assert.That(lengths.Zip(lengths.Skip(1), (a, b) => b - a).Distinct().Count(), Is.EqualTo(1));
        }
        finally {
            em.DBClient.NonQuery($"DROP TABLE {table}");
        }
    }

    [TestCase(false), TestCase(true), Parallelizable]
    public async Task ExecutePagedAsync_CteArraysAndBodyLimit_ReturnsOuterPageAndWindow(bool preparedEntry) {
        (IEntityManager em, string table) = CreateRows();
        try {
            int[] values = [0, 1, 2, 3, 4, 5, 6, 7];
            ISqlToken literal = new CteLiteralToken("' ( FROM LIMIT OFFSET '' quoted' IS NOT NULL");
            CommonTableExpression seed = em.Load<ValueModel>(v => v.Integer)
                .Where(v => literal.Bool && v.Integer.In(values)).OrderBy(v => v.Integer).Limit(6).AsCte("limit");
            CommonTableExpression next = em.Load<ValueModel>(v => v.Integer).Where(v => v.Integer.In(seed) || v.Integer.In(seed)).AsCte("next");
            LoadOperation<ValueModel> outer = em.Load<ValueModel>(v => v.Integer).With(seed, next).From(next).OrderBy(v => v.Integer).Limit(1).Offset(4);

            PreparedLoadOperation<ValueModel> prepared = outer.Prepare(false);

            Assert.That(prepared.ConstantParameters.Length, Is.EqualTo(4));
            Assert.That(prepared.ConstantParameters[0], Is.EqualTo(values));

            WindowResult<ValueModel, long> page = preparedEntry
                ? await prepared.ExecutePagedAsync(2, 1)
                : await outer.ExecutePagedAsync(2, 1);

            Assert.That(await page.WindowValue, Is.EqualTo(6));

            List<int> items = [];
            await foreach(ValueModel item in page.Items)
                items.Add(item.Integer);

            Assert.That(items, Is.EqualTo(new[] { 1, 2 }));

            WindowResult<ValueModel, long> window = await em.Load<ValueModel>(v => v.Integer).With(seed, next).From(next)
                .ExecuteWindowedAsync<long>(DB.SumOver(DB.Property<ValueModel>(v => v.Integer)));

            Assert.That(await window.WindowValue, Is.EqualTo(15));

            List<int> windowed = [];
            await foreach(ValueModel item in window.Items)
                windowed.Add(item.Integer);

            Assert.That(windowed.OrderBy(i => i), Is.EqualTo(Enumerable.Range(0, 6)));

            WindowReader<long> reader = await prepared.ExecuteWindowedReaderAsync<long>(DB.CountOver());
            using(reader.Reader) {
                Assert.That(reader.Reader.Read(), Is.True);
                Assert.That(await reader.WindowValue, Is.EqualTo(6));
            }
        }
        finally {
            em.DBClient.NonQuery($"DROP TABLE {table}");
        }
    }
}
