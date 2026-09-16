using NUnit.Framework;
using Pooshit.Ocelot.Clients;
using Pooshit.Ocelot.Entities;
using Pooshit.Ocelot.Entities.Operations.Tables;
using Pooshit.Ocelot.Errors;
using Pooshit.Ocelot.Tests.Data;

namespace Pooshit.Ocelot.Tests.Security;

[TestFixture, Parallelizable]
public class DataFieldHatchTests {

    [Test, Parallelizable, Description("DataField.Raw renders its argument verbatim, unmasked")]
    public void RawDataFieldStillRendersExpression() {
        IDBClient client = TestData.CreateDatabaseAccess();
        EntityManager em = new(client);
        em.CreateTable("victim").Column("first").Execute();

        string sql = em.LoadData("victim").Columns(DataField.Raw("COUNT(*)")).Prepare().CommandText;
        Assert.That(sql, Does.Contain("SELECT COUNT(*) FROM"));
    }

    [Test, Parallelizable, Description("new DataField(expression) throws and the message names DataField.Raw")]
    public void DataFieldConstructorIsColumnAndRejectsExpression() {
        InvalidIdentifierException exception = Assert.Throws<InvalidIdentifierException>(() => new DataField("COUNT(*)"));
        Assert.That(exception.Message, Does.Contain("DataField.Raw"));
    }

    [Test, Parallelizable, Description("a validated column field cannot be flipped to raw after construction")]
    public void DataFieldIsColumnIsNotSettable() {
        DataField field = new("first");
        Assert.That(field.IsColumn, Is.True);
        Assert.That(field.GetType().GetProperty(nameof(DataField.IsColumn)).CanWrite, Is.False);
    }
}
