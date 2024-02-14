#pragma warning disable IDE0059 // Unnecessary assignment of a value
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace RhoMicro.CodeAnalysis.UnionsGenerator.EndToEnd.Tests;

using System;

public partial class AsPropertyTests
{
    [UnionType<Int32>(Alias = "Int")]
    [UnionType<List<String>>]
    partial class Union<[UnionType(Alias = "ValueT")] T>
        where T : struct;

    [Fact]
    public void AsIntWhenRepresentingInt32()
    {
        const Int32 expected = 32;
        Union<Byte> u = expected;
        Assert.Equal(expected, u.AsInt);
    }
    [Fact]
    public void NotAsIntWhenRepresentingList()
    {
        Union<Byte> u = new List<String>();
        Assert.Equal(default, u.AsInt);
    }
    [Fact]
    public void NotAsIntWhenRepresentingByte()
    {
        Union<Byte> u = (Byte)32;
        Assert.Equal(default, u.AsInt);
    }

    [Fact]
    public void NotAsListWhenRepresentingInt32()
    {
        Union<Byte> u = 32;
        Assert.Equal(default, u.AsList_of_String);
    }
    [Fact]
    public void AsListWhenRepresentingList()
    {
        var expected = new List<String>();
        Union<Byte> u = expected;
        Assert.Equal(expected, u.AsList_of_String);
    }
    [Fact]
    public void NotAsListWhenRepresentingByte()
    {
        Union<Byte> u = (Byte)32;
        Assert.Equal(default, u.AsList_of_String);
    }

    [Fact]
    public void NotAsByteWhenRepresentingInt32()
    {
        Union<Byte> u = 32;
        Assert.Equal(default, u.AsValueT);
    }
    [Fact]
    public void NotAsByteWhenRepresentingList()
    {
        Union<Byte> u = new List<String>();
        Assert.Equal(default, u.AsValueT);
    }
    [Fact]
    public void AsByteWhenRepresentingByte()
    {
        const Byte expected = 32;
        Union<Byte> u = expected;
        Assert.Equal(expected, u.AsValueT);
    }
}
