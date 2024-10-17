using System;
using Pooshit.Ocelot.Entities.Attributes;

namespace Pooshit.Ocelot.Tests.Models {
    public class TestEntityWithoutAnySpecifications {

        /// <summary>
        /// ctor
        /// </summary>
        public TestEntityWithoutAnySpecifications() {}

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="column1"></param>
        /// <param name="integerValue"></param>
        /// <param name="something"></param>
        public TestEntityWithoutAnySpecifications(string column1, int integerValue, double something) {
            Column1 = column1;
            IntegerValue = integerValue;
            Something = something;
        }

        [Unique]
        public string Column1 { get; set; }

        [Index("key")]
        public int IntegerValue { get; set; }

        [Index("key")]
        public double Something { get; set; }

        public bool BooleanValue { get; set; }

        [PrimaryKey]
        [AutoIncrement]
        public long ThePrimaryKey { get; set; }

        protected bool Equals(TestEntityWithoutAnySpecifications other) {
            return string.Equals(Column1, other.Column1) && IntegerValue == other.IntegerValue && Math.Abs(Something-other.Something)<0.001 && ThePrimaryKey == other.ThePrimaryKey;
        }

        public override bool Equals(object obj) {
            if(ReferenceEquals(null, obj)) return false;
            if(ReferenceEquals(this, obj)) return true;
            if(obj.GetType() != GetType()) return false;
            return Equals((TestEntityWithoutAnySpecifications)obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = (Column1 != null ? Column1.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IntegerValue;
                hashCode = (hashCode * 397) ^ Something.GetHashCode();
                hashCode = (hashCode * 397) ^ ThePrimaryKey.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() {
            return string.Format("{0}, {1}, {2}, {3}", ThePrimaryKey, Column1, IntegerValue, Something);
        }
    }

    public class OtherTestEntity {
        [PrimaryKey]
        [AutoIncrement]
        public long PrimaryKey { get; set; }

        public string SomeColumn { get; set; }
    }
}