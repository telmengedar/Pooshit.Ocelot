using Moq;
using NUnit.Framework;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Tests;

[TestFixture, Parallelizable]
public class VectorAttributeTests {

    [Test, Parallelizable]
    public void ExternalDialect_WithoutOwnImplementation_HasNoVectorType() {
        Mock<IDBInfo> external = new() { CallBase = true };

        Assert.That(external.Object.GetVectorType(3), Is.Null);
    }
}
