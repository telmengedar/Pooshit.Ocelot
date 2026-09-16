using NUnit.Framework;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Errors;
using Pooshit.Ocelot.Tests.Data;
using Pooshit.Ocelot.Tests.Security.Models;

namespace Pooshit.Ocelot.Tests.Security;

[TestFixture, Parallelizable]
public class EntityModelGuardTests {

    [Test, Parallelizable, Description("a [Table] attribute carrying an injected name throws on first model use, before any client call")]
    public void EntityWithInjectedTableAttributeThrowsOnModel() {
        EntityManager em = new(TestData.CreateDatabaseAccess());
        Assert.Throws<InvalidIdentifierException>(() => em.Model<InjectedTableEntity>());
    }

    [Test, Parallelizable, Description("a [Column] attribute carrying an injected name throws on first model use")]
    public void EntityWithInjectedColumnAttributeThrowsOnModel() {
        EntityManager em = new(TestData.CreateDatabaseAccess());
        Assert.Throws<InvalidIdentifierException>(() => em.Model<InjectedColumnEntity>());
    }

    [Test, Parallelizable, Description("an [Index] attribute carrying an injected name throws on first model use")]
    public void EntityWithInjectedIndexNameThrowsOnModel() {
        EntityManager em = new(TestData.CreateDatabaseAccess());
        Assert.Throws<InvalidIdentifierException>(() => em.Model<InjectedIndexNameEntity>());
    }

    [Test, Parallelizable, Description("an [Index] attribute carrying an injected type throws on first model use")]
    public void EntityWithInjectedIndexTypeThrowsOnModel() {
        EntityManager em = new(TestData.CreateDatabaseAccess());
        Assert.Throws<InvalidIdentifierException>(() => em.Model<InjectedIndexTypeEntity>());
    }

    [Test, Parallelizable, Description("a [Unique] attribute carrying an injected name throws on first model use")]
    public void EntityWithInjectedUniqueNameThrowsOnModel() {
        EntityManager em = new(TestData.CreateDatabaseAccess());
        Assert.Throws<InvalidIdentifierException>(() => em.Model<InjectedUniqueNameEntity>());
    }

    [Test, Parallelizable, Description("a [DefaultValue] attribute carrying a quote breaks out of the ddl literal and throws on first model use")]
    public void EntityWithQuotedDefaultAttributeThrowsOnModel() {
        EntityManager em = new(TestData.CreateDatabaseAccess());
        Assert.Throws<InvalidIdentifierException>(() => em.Model<InjectedDefaultEntity>());
    }

    [Test, Parallelizable, Description("EntityDescriptorAccess.Table(string) rejects an injected table name at the runtime mutation point")]
    public void ModelTableSetterRejectsInjectedName() {
        EntityManager em = new(TestData.CreateDatabaseAccess());
        Assert.Throws<InvalidIdentifierException>(() => em.Model<RuntimeMutatedEntity>().Table("victim; DROP TABLE x; --"));
    }

    [Test, Parallelizable, Description("EntityDescriptorAccess.Index(name, columns) rejects an injected index name at the runtime mutation point")]
    public void ModelIndexRejectsInjectedName() {
        EntityManager em = new(TestData.CreateDatabaseAccess());
        Assert.Throws<InvalidIdentifierException>(() => em.Model<RuntimeMutatedEntity>().Index("victim; DROP TABLE x; --", e => e.Name));
    }

    [Test, Parallelizable, Description("EntityDescriptorAccess.Column(expr, name) rejects an injected rename target at the runtime mutation point")]
    public void ModelColumnRenameRejectsInjectedName() {
        EntityManager em = new(TestData.CreateDatabaseAccess());
        Assert.Throws<InvalidIdentifierException>(() => em.Model<RuntimeMutatedEntity>().Column(e => e.Name, "victim; DROP TABLE x; --"));
    }

    [Test, Parallelizable, Description("EntityDescriptorAccess.Default(expr, value) rejects a value that breaks out of the ddl literal")]
    public void ModelDefaultRejectsQuoteBreakout() {
        EntityManager em = new(TestData.CreateDatabaseAccess());
        Assert.Throws<InvalidIdentifierException>(() => em.Model<RuntimeMutatedEntity>().Default(e => e.Name, "a'b"));
    }
}
