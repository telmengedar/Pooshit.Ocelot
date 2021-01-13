using NightlyCode.Database.Entities;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Tests.Data;
using NightlyCode.Database.Tests.Models;
using NightlyCode.Database.Tokens;
using NUnit.Framework;

namespace NightlyCode.Database.Tests {
    
    [TestFixture, Parallelizable]
    public class TokenTests {

        [Test, Parallelizable]
        public void CountSpecificValues() {
            IEntityManager database = TestData.CreateEntityManager();
            database.UpdateSchema<ValueModel>();

            PreparedOperation insert = database.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Prepare();

            for (int i = 0; i < 16; ++i)
                insert.Execute(i, 0.0f, 0.0);

            long count = database.Load<ValueModel>(DB.Count(DB.If(DB.Predicate<ValueModel>(v => v.Integer > 3 && v.Integer < 8), DB.Constant(1)))).ExecuteScalar<long>();
            Assert.AreEqual(4, count);
        }
        
        [Test, Parallelizable]
        public void CountSpecificValuesInExpression() {
            IEntityManager database = TestData.CreateEntityManager();
            database.UpdateSchema<ValueModel>();

            PreparedOperation insert = database.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Prepare();

            for (int i = 0; i < 16; ++i)
                insert.Execute(i, 0.0f, 0.0);

            long count = database.Load<ValueModel>(v=>Xpr.Count(Xpr.If(Xpr.Predicate(v.Integer > 3 && v.Integer < 8), Xpr.Constant(1)))).ExecuteScalar<long>();
            Assert.AreEqual(4, count);
        }
        
        [Test, Parallelizable]
        public void CountSpecificValuesInSimplifiedExpression() {
            IEntityManager database = TestData.CreateEntityManager();
            database.UpdateSchema<ValueModel>();

            PreparedOperation insert = database.Insert<ValueModel>().Columns(v => v.Integer, v => v.Single, v => v.Double).Prepare();

            for (int i = 0; i < 16; ++i)
                insert.Execute(i, 0.0f, 0.0);

            long count = database.Load<ValueModel>(v=>Xpr.Count(Xpr.If(v.Integer > 3 && v.Integer < 8, 1))).ExecuteScalar<long>();
            Assert.AreEqual(4, count);
        }

    }
}