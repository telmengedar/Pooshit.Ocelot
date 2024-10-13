using System.Numerics;
using Pooshit.Ocelot.CustomTypes;

namespace NightlyCode.Database.Tests.Entities; 

public class BigIntData {
    public BigInteger Data { get; set; }

    public Range<BigInteger> Range { get; set; }
}