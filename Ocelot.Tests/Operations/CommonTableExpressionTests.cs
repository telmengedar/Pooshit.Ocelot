using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Errors;
using Pooshit.Ocelot.Fields;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Models;
using Pooshit.Ocelot.Tokens;

namespace Pooshit.Ocelot.Tests.Operations;

[TestFixture, Parallelizable]
public class CommonTableExpressionTests {
    static IEntityManager CreateRows() {
        IEntityManager em = TestData.CreateEntityManager();
        em.UpdateSchema<ValueModel>();
        em.InsertEntities<ValueModel>().Execute(Enumerable.Range(0, 10).Select(i => new ValueModel(i, s: $"row-{i}")).ToArray());
        return em;
    }

    static IEntityManager CreateRenderer(IDBInfo info) {
        Mock<IDBClient> client = new();
        client.SetupGet(c => c.DBInfo).Returns(info);
        return new EntityManager(client.Object);
    }

    [Test, Parallelizable]
    public void With_OrderedDefinitionsInAndUnionAll_ReturnsExpectedMultiset() {
        IEntityManager em = CreateRows();
        CommonTableExpression seed = em.Load<ValueModel>(v => v.Integer).Where(v => v.Integer == 1)
            .Union(em.Load<ValueModel>(v => v.Integer).Where(v => v.Integer == 2)).AsCte("seed");
        CommonTableExpression next = em.Load<ValueModel>(v => v.Integer)
            .Where(v => v.Integer.In(seed) || v.Integer.In(seed)).Union(seed).AsCte("next");
        LoadOperation<ValueModel> outer = em.Load<ValueModel>(v => v.Integer).With(seed).With(next).From(next).Union(seed);

        PreparedLoadOperation<ValueModel> prepared = outer.Prepare(false);

        Assert.That(prepared.CommandText.IndexOf("[seed] AS", StringComparison.Ordinal), Is.LessThan(prepared.CommandText.IndexOf("[next] AS", StringComparison.Ordinal)));
        Assert.That(prepared.ConstantParameters, Is.EqualTo(new object[] { 1, 2 }));
        Assert.That(prepared.ExecuteSet<int>().OrderBy(i => i), Is.EqualTo(new[] { 1, 1, 1, 2, 2, 2 }));
    }

    [Test, Parallelizable]
    public void Prepare_RepeatedReferences_RendersAndBindsBodyOncePerStatement() {
        IEntityManager em = CreateRows();
        LoadOperation<ValueModel> body = em.Load<ValueModel>(v => v.Integer).Where(v => v.String == "row-3");
        CommonTableExpression seed = body.AsCte("seed");
        LoadOperation<ValueModel> outer = em.Load<ValueModel>(v => v.Integer).With(seed)
            .Where(v => v.Integer.In(seed) || v.Integer.In(seed)).Union(seed);

        PreparedLoadOperation<ValueModel> first = outer.Prepare(false);

        Assert.That(Regex.Matches(first.CommandText, @"\[string\]").Count, Is.EqualTo(1));
        Assert.That(Regex.Matches(first.CommandText, @"SELECT \* FROM \[seed\]").Count, Is.EqualTo(3));
        Assert.That(first.ConstantParameters, Is.EqualTo(new object[] { "row-3" }));
        Assert.That(first.ExecuteSet<int>(), Is.EqualTo(new[] { 3, 3 }));

        body.Where(v => v.String == "row-4");

        PreparedLoadOperation<ValueModel> second = outer.Prepare(false);

        Assert.That(second.ConstantParameters, Is.EqualTo(new object[] { "row-4" }));
        Assert.That(second.ExecuteSet<int>(), Is.EqualTo(new[] { 4, 4 }));
        Assert.That(em.Load<ValueModel>(v => v.Integer).With(seed).From(seed).ExecuteSet<int>(), Is.EqualTo(new[] { 4 }));
    }

    [TestCase(false), TestCase(true), Parallelizable]
    public void Prepare_ChainWithCollections_GrowsLinearly(bool postgres) {
        IEntityManager em = postgres ? CreateRenderer(new PostgreInfo()) : CreateRows();
        List<int> lengths = [];
        List<int> counts = [];

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
                .Where(v => v.Integer.In(last) || v.Integer.In(last)).Prepare(false);

            if(!postgres)
                Assert.That(prepared.ExecuteSet<int>(), Is.EqualTo(new[] { 1, 2 }));

            counts.Add(prepared.ConstantParameters.Length);
            lengths.Add(Regex.Replace(Regex.Replace(prepared.CommandText, @"@\d+", "@p"), @"hop\d+", "hop").Length);

            Assert.That(prepared.ConstantParameters.Length, Is.EqualTo(depth * (postgres ? 1 : 2)));
            if(postgres)
                Assert.That(prepared.ConstantParameters.All(p => p is int[]), Is.True);
        }

        Assert.That(counts.Zip(counts.Skip(1), (a, b) => b - a).Distinct().Count(), Is.EqualTo(1));
        Assert.That(lengths.Zip(lengths.Skip(1), (a, b) => b - a).Distinct().Count(), Is.EqualTo(1));
    }

    [Test, Parallelizable]
    public void With_TypedUntypedAndJoinedCopies_PreservesDefinitions() {
        IEntityManager em = CreateRows();
        CommonTableExpression seed = em.Load<ValueModel>(v => v.Integer).Where(v => v.Integer == 3).AsCte("seed");
        LoadOperation<ValueModel, ValueModel> joined = em.Load<ValueModel>(v => v.Integer).With(seed)
            .Join<ValueModel>((left, right) => left.Integer == right.Integer, "other").Where((left, right) => left.Integer.In(seed));

        Assert.That(joined.ExecuteSet<int>(), Is.EqualTo(new[] { 3 }));

        LoadOperation untyped = (LoadOperation)em.Load(DB.Column("t", "integer")).With(seed)
            .Join<ValueModel>(right => DB.Column("t", "integer").Int32 == DB.Column("other", "integer").Int32, "other")
            .Where(() => DB.Column("t", "integer").Int32.In(seed));

        Assert.That(untyped.From(seed).ExecuteSet<int>(), Is.EqualTo(new[] { 3 }));

        LoadOperation copied = (LoadOperation)em.Load(DB.Column("t", "integer")).With(seed)
            .Join(seed, () => DB.Column("t", "integer").Int32 == DB.Column("other", "integer").Int32, "other");

        Assert.That(copied.From(seed).ExecuteSet<int>(), Is.EqualTo(new[] { 3 }));

        ILoadOperation projection = em.Load(DB.Column("integer")).With(seed).From(seed).Union(seed);

        Assert.That(projection.ExecuteSet<int>(), Is.EqualTo(new[] { 3, 3 }));

        ILoadOperation untypedBody = em.Load(DB.Constant(7));
        CommonTableExpression constant = untypedBody.AsCte("untyped");

        Assert.That(em.Load(DB.All).With(constant).From(constant).ExecuteSet<int>(), Is.EqualTo(new[] { 7 }));
    }

    [TestCase(null), TestCase(""), TestCase("a.b"), TestCase("bad; DROP TABLE value"), TestCase("bad name"), Parallelizable]
    public void AsCte_InvalidName_ThrowsBeforeExecution(string name) {
        IEntityManager em = CreateRenderer(new SQLiteInfo());

        Assert.Throws<InvalidIdentifierException>(() => em.Load<ValueModel>().AsCte(name));
        Assert.Throws<InvalidIdentifierException>(() => em.Load(DB.Constant(1)).AsCte(name));
    }

    [TestCase(false), TestCase(true), Parallelizable]
    public void With_DuplicateName_RejectsWholeAppend(bool untyped) {
        IEntityManager em = CreateRows();
        CommonTableExpression seed = em.Load<ValueModel>(v => v.Integer).Where(v => v.Integer == 3).AsCte("seed");
        CommonTableExpression duplicate = em.Load<ValueModel>(v => v.Integer).AsCte("SEED");
        CommonTableExpression extra = em.Load<ValueModel>(v => v.Integer).AsCte("extra");
        if(untyped) {
            LoadOperation load = em.Load(DB.Column("integer")).With(seed).From(seed);

            Assert.Throws<ArgumentException>(() => load.With(extra, duplicate));
            Assert.That(load.Prepare().CommandText, Does.Not.Contain("[extra] AS"));
            Assert.Throws<ArgumentException>(() => load.With(seed));
            Assert.That(load.ExecuteSet<int>(), Is.EqualTo(new[] { 3 }));
        }
        else {
            LoadOperation<ValueModel> load = em.Load<ValueModel>(v => v.Integer).With(seed).From(seed);

            Assert.Throws<ArgumentException>(() => load.With(extra, duplicate));
            Assert.That(load.Prepare(false).CommandText, Does.Not.Contain("[extra] AS"));
            Assert.Throws<ArgumentException>(() => load.With(seed));
            Assert.That(load.ExecuteSet<int>(), Is.EqualTo(new[] { 3 }));
        }
    }

    [TestCase("sqlite"), TestCase("postgres"), TestCase("mysql"), TestCase("mssql"), Parallelizable]
    public void Prepare_AllDialects_QuotesNonrecursiveDefinitions(string dialect) {
        IDBInfo info = dialect switch { "sqlite" => new SQLiteInfo(), "postgres" => new PostgreInfo(), "mysql" => new MySQLInfo(), _ => new MsSqlInfo() };
        IEntityManager em = CreateRenderer(info);
        CommonTableExpression seed = em.Load(DB.Constant(1)).AsCte("limit");

        PreparedLoadOperation prepared = em.Load(DB.All).With(seed).From(seed).Prepare();

        Assert.That(prepared.CommandText, Does.StartWith($"WITH {info.MaskColumn("limit")} AS ( SELECT"));
        Assert.That(prepared.CommandText, Does.Contain($"SELECT * FROM {info.MaskColumn("limit")}"));
        Assert.That(Regex.Matches(prepared.CommandText, "WITH").Count, Is.EqualTo(1));
        Assert.That(prepared.CommandText, Does.Not.Contain("RECURSIVE"));
        Assert.That(prepared.ConstantParameters, Is.EqualTo(new object[] { 1 }));
        Assert.That(em.Load(DB.Constant(2)).With().Prepare().CommandText, Does.StartWith("SELECT"));
    }

    [TestCase(false), TestCase(true), Parallelizable]
    public async Task ExecutePagedAsync_CteBodyLimit_PreservesInnerLimitAndOuterTotal(bool preparedEntry) {
        IEntityManager em = CreateRows();
        CommonTableExpression seed = em.Load<ValueModel>(v => v.Integer).OrderBy(v => v.Integer).Limit(6).AsCte("limit");
        LoadOperation<ValueModel> outer = em.Load<ValueModel>(v => v.Integer).With(seed).From(seed).OrderBy(v => v.Integer).Limit(1).Offset(4);

        WindowResult<ValueModel, long> page = preparedEntry
            ? await outer.Prepare(false).ExecutePagedAsync(2, 1)
            : await outer.ExecutePagedAsync(2, 1);

        Assert.That(await page.WindowValue, Is.EqualTo(6));

        List<int> items = [];
        await foreach(ValueModel item in page.Items)
            items.Add(item.Integer);

        Assert.That(items, Is.EqualTo(new[] { 1, 2 }));
    }

    [TestCase("' ) FROM LIMIT OFFSET '' quoted'"), TestCase("' ( FROM LIMIT OFFSET '' quoted'"), Parallelizable]
    public async Task ExecuteWindowedAsync_CteBodyLiteral_PreservesOuterAggregate(string literal) {
        IEntityManager em = CreateRows();
        CommonTableExpression seed = em.Load<ValueModel>(DB.Property<ValueModel>(v => v.Integer), new CteLiteralToken(literal))
            .OrderBy(v => v.Integer).Limit(6).AsCte("seed");

        PreparedLoadOperation<ValueModel> outer = em.Load<ValueModel>(v => v.Integer).With(seed).From(seed).OrderBy(v => v.Integer).Prepare(false);

        Assert.That(outer.CommandText, Does.Contain(literal));

        WindowResult<ValueModel, long> window = await outer.ExecuteWindowedAsync<long>(DB.SumOver(DB.Property<ValueModel>(v => v.Integer)));

        Assert.That(await window.WindowValue, Is.EqualTo(15));

        List<int> items = [];
        await foreach(ValueModel item in window.Items)
            items.Add(item.Integer);

        Assert.That(items, Is.EqualTo(Enumerable.Range(0, 6)));

        WindowResult<ValueModel, long> page = await outer.ExecutePagedAsync(2, 1);

        Assert.That(await page.WindowValue, Is.EqualTo(6));

        List<int> paged = [];
        await foreach(ValueModel item in page.Items)
            paged.Add(item.Integer);

        Assert.That(paged, Is.EqualTo(new[] { 1, 2 }));

        WindowReader<long> reader = await outer.ExecuteWindowedReaderAsync<long>(DB.CountOver());
        using(reader.Reader) {
            Assert.That(reader.Reader.Read(), Is.True);
            Assert.That(await reader.WindowValue, Is.EqualTo(6));
        }
    }
}
