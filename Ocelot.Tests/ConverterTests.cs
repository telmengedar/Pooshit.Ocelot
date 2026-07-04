using System;
using NUnit.Framework;
using Pooshit.Ocelot.Extern;

namespace Pooshit.Ocelot.Tests;

/// <summary>
/// Verifies that the string-to-bool converter in <see cref="Converter"/> produces
/// correct boolean values for all recognised textual forms and rejects unrecognised input.
/// </summary>
[TestFixture, Parallelizable]
public class ConverterTests {

    [Test, Parallelizable]
    public void StringFalse_ReturnsFalse() {
        bool result = Converter.Convert<bool>("false");
        Assert.IsFalse(result);
    }

    [Test, Parallelizable]
    public void StringTrue_ReturnsTrue() {
        bool result = Converter.Convert<bool>("true");
        Assert.IsTrue(result);
    }

    [Test, Parallelizable]
    public void StringFalseMixedCase_ReturnsFalse() {
        bool result = Converter.Convert<bool>("FALSE");
        Assert.IsFalse(result);
    }

    [Test, Parallelizable]
    public void StringTrueMixedCase_ReturnsTrue() {
        bool result = Converter.Convert<bool>("True");
        Assert.IsTrue(result);
    }

    [Test, Parallelizable]
    public void StringOne_ReturnsTrue() {
        bool result = Converter.Convert<bool>("1");
        Assert.IsTrue(result);
    }

    [Test, Parallelizable]
    public void StringZero_ReturnsFalse() {
        bool result = Converter.Convert<bool>("0");
        Assert.IsFalse(result);
    }

    [Test, Parallelizable]
    public void EmptyString_ReturnsFalse() {
        bool result = Converter.Convert<bool>("");
        Assert.IsFalse(result);
    }

    [Test, Parallelizable]
    public void UnrecognisedString_ThrowsFormatException() {
        Assert.Throws<FormatException>(() => Converter.Convert<bool>("maybe"));
    }

    [Test, Parallelizable]
    public void NullableStringTrue_ReturnsTrue() {
        bool? result = Converter.Convert<bool?>("true");
        Assert.IsTrue(result);
    }

    [Test, Parallelizable]
    public void NullableStringFalse_ReturnsFalse() {
        bool? result = Converter.Convert<bool?>("false");
        Assert.IsFalse(result.Value);
    }

    [Test, Parallelizable]
    public void NullableNull_ReturnsNull() {
        bool? result = Converter.Convert<bool?>(null, allownullonvaluetypes: true);
        Assert.IsNull(result);
    }

    [Test, Parallelizable]
    public void StringTrueWithWhitespace_ReturnsTrue() {
        bool result = Converter.Convert<bool>(" true ");
        Assert.IsTrue(result);
    }
}
