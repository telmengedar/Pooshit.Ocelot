using NUnit.Framework;
using Pooshit.Ocelot.Errors;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Tests.Security;

[TestFixture, Parallelizable]
public class IdentifierGuardTests {

    [TestCase("a")]
    [TestCase("_a")]
    [TestCase("A1")]
    [TestCase("__total")]
    [TestCase("sq1")]
    [TestCase("o666")]
    [TestCase("t")]
    [Parallelizable]
    public void SimpleAcceptsEveryInRepoShape(string value) {
        Assert.That(IdentifierGuard.Simple(value, "column"), Is.EqualTo(value));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("a b")]
    [TestCase("a;b")]
    [TestCase("a]")]
    [TestCase("a\"")]
    [TestCase("a`")]
    [TestCase("1a")]
    [TestCase("a.b")]
    [TestCase("a-b")]
    [TestCase("ä")]
    [TestCase("a\0")]
    [Parallelizable]
    public void SimpleRejects(string value) {
        Assert.Throws<InvalidIdentifierException>(() => IdentifierGuard.Simple(value, "column"));
    }

    [TestCase("information_schema.columns")]
    [TestCase("pg_views")]
    [TestCase("a.b.c")]
    [Parallelizable]
    public void QualifiedAcceptsDotted(string value) {
        Assert.That(IdentifierGuard.Qualified(value, "table"), Is.EqualTo(value));
    }

    [TestCase(".a")]
    [TestCase("a.")]
    [TestCase("a..b")]
    [TestCase("a. b")]
    [TestCase("a;b.c")]
    [Parallelizable]
    public void QualifiedRejects(string value) {
        Assert.Throws<InvalidIdentifierException>(() => IdentifierGuard.Qualified(value, "table"));
    }

    [Test, Parallelizable, Description("a quoted default with a quote, backslash or control character breaks out of the literal")]
    public void QuotedDefaultRejectsQuoteBackslashControl() {
        Assert.Throws<InvalidIdentifierException>(() => IdentifierGuard.DefaultLiteral("a'b", true, "default value"));
        Assert.Throws<InvalidIdentifierException>(() => IdentifierGuard.DefaultLiteral("a\\b", true, "default value"));
        Assert.Throws<InvalidIdentifierException>(() => IdentifierGuard.DefaultLiteral("a\nb", true, "default value"));
    }

    [TestCase("a b")]
    [TestCase("0;DROP")]
    [TestCase("")]
    [Parallelizable]
    public void BareDefaultRejectsNonToken(string text) {
        Assert.Throws<InvalidIdentifierException>(() => IdentifierGuard.DefaultLiteral(text, false, "default value"));
    }

    [TestCase("")]
    [TestCase("none")]
    [TestCase("0")]
    [TestCase("-1.5")]
    [TestCase("true")]
    [Parallelizable]
    public void DefaultAcceptsPlainValues(string text) {
        Assert.That(IdentifierGuard.DefaultLiteral(text, true, "default value"), Is.EqualTo(text));
    }

    [Test, Parallelizable, Description("the exception carries the offending value and its role")]
    public void ExceptionCarriesValueAndRole() {
        InvalidIdentifierException exception = Assert.Throws<InvalidIdentifierException>(() => IdentifierGuard.Simple("a;b", "column"));
        Assert.That(exception.Value, Is.EqualTo("a;b"));
        Assert.That(exception.Role, Is.EqualTo("column"));
    }

    [Test, Parallelizable, Description("InvalidIdentifierException derives from ArgumentException so existing ArgumentException handling maps it")]
    public void ExceptionIsArgumentException() {
        Assert.That(new InvalidIdentifierException("a;b", "column"), Is.InstanceOf<System.ArgumentException>());
    }

    [Test, Parallelizable, Description("a hint is appended as a final sentence; Role is unchanged")]
    public void ExceptionAppendsHintWhenGiven() {
        InvalidIdentifierException exception = new("a;b", "column", "use DataField.Raw(...) to emit a sql expression verbatim");
        Assert.That(exception.Message, Does.Contain("(use DataField.Raw(...) to emit a sql expression verbatim)"));
        Assert.That(exception.Role, Is.EqualTo("column"));
    }

    [Test, Parallelizable, Description("no hint means the message ends after the grammar clause, with no trailing sentence")]
    public void ExceptionOmitsHintWhenNull() {
        InvalidIdentifierException exception = new("a;b", "column");
        Assert.That(exception.Message, Is.EqualTo("'a;b' is not a valid column, expected an unquoted identifier matching [A-Za-z_][A-Za-z0-9_]* (optionally dot-separated)"));
    }

    [TestCase("TEXT")]
    [TestCase("INTEGER")]
    [TestCase("FLOAT")]
    [TestCase("BOOLEAN")]
    [TestCase("BLOB")]
    [TestCase("DECIMAL")]
    [TestCase("VARCHAR(255)")]
    [TestCase("DECIMAL(10,2)")]
    [TestCase("DECIMAL(10, 2)")]
    [TestCase("DOUBLE PRECISION")]
    [TestCase("character varying")]
    [TestCase("timestamp without time zone")]
    [TestCase("int4range")]
    [TestCase("real[]")]
    [TestCase("real[10]")]
    [TestCase("UNSIGNED BIG INT")]
    [TestCase(null)]
    [TestCase("")]
    [Parallelizable]
    public void TypeTokenAcceptsSqlTypeShapes(string value) {
        Assert.That(IdentifierGuard.TypeToken(value, "column type"), Is.EqualTo(value));
    }

    [TestCase("TEXT); CREATE TABLE pwned_type(x); --")]
    [TestCase("TEXT;")]
    [TestCase("TEXT --")]
    [TestCase("TEXT, x TEXT")]
    [TestCase("TEXT)")]
    [TestCase("VARCHAR(255) DEFAULT 'x'")]
    [TestCase("VARCHAR(a)")]
    [TestCase("TEXT  INTEGER")]
    [TestCase(" TEXT")]
    [TestCase("real[a]")]
    [Parallelizable]
    public void TypeTokenRejects(string value) {
        Assert.Throws<InvalidIdentifierException>(() => IdentifierGuard.TypeToken(value, "column type"));
    }
}
