#pragma warning disable IDE0059 // Unnecessary assignment of a value
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace RhoMicro.CodeAnalysis.UnionsGenerator.EndToEnd.Tests;

using System;
using System.Numerics;

public partial class RepresentableTypeConversionTests
{
    [UnionType<String>(Alias = "ErrorMessage")]
    readonly partial struct Result<[UnionType(Alias = "Result")] T>;

    [Fact]
    public void IsImplicitlyConvertibleFromString()
    {
        Result<Int32> r = "Hello, World!";
    }
    [Fact]
    public void IsImplicitlyConvertibleFromInt32()
    {
        Result<Int32> r = 32;
    }
    [Fact]
    public void IsImplicitlyConvertibleFromChar()
    {
        Result<Char> r = 'T';
    }
    [Fact]
    public void IsExplicitlyConvertibleToString()
    {
        const String expected = "Hello, World!";
        Result<Int32> r = expected;
        var actual = (String)r;

        Assert.Equal(expected, actual);
    }
    [Fact]
    public void IsNotExplicitlyConvertibleToString()
    {
        Result<Int32> r = 32;
        _ = Assert.Throws<InvalidOperationException>(() => (String)r);
    }
    [Fact]
    public void IsNotExplicitlyConvertibleToInt32()
    {
        Result<Int32> r = "Hello, World!";
        _ = Assert.Throws<InvalidOperationException>(() => (Int32)r);
    }
    [Fact]
    public void IsNotExplicitlyConvertibleToChar()
    {
        Result<Char> r = "Hello, World!";
        _ = Assert.Throws<InvalidOperationException>(() => (Char)r);
    }
    [Fact]
    public void IsExplicitlyConvertibleToInt32()
    {
        const Int32 expected = 32;
        Result<Int32> r = expected;
        var actual = (Int32)r;

        Assert.Equal(expected, actual);
    }
    [Fact]
    public void IsExplicitlyConvertibleToChar()
    {
        const Char expected = 'T';
        Result<Char> r = expected;
        var actual = (Char)r;

        Assert.Equal(expected, actual);
    }
}
