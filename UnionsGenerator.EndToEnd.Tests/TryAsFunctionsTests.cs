#pragma warning disable IDE0059 // Unnecessary assignment of a value
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace RhoMicro.CodeAnalysis.UnionsGenerator.EndToEnd.Tests;

using System;

public partial class TryAsFunctionsTests
{
    [UnionType<Int32>(Alias = "Int")]
    [UnionType<List<String>>]
    partial class Union<[UnionType] T>;

    [Fact]
    public void IsIntWhenRepresentingInt32()
    {
        const Int32 expected = 32;
        Union<Byte> u = expected;
        Assert.True(u.TryAsInt(out var actual));
        Assert.Equal(expected, actual);
    }
    [Fact]
    public void IsNotIntWhenRepresentingList()
    {
        const Int32 expected = 0;
        Union<Byte> u = new List<String>();
        Assert.False(u.TryAsInt(out var actual));
        Assert.Equal(expected, actual);
    }
    [Fact]
    public void IsNotIntWhenRepresentingByte()
    {
        const Int32 expected = 0;
        Union<Byte> u = (Byte)32;
        Assert.False(u.TryAsInt(out var actual));
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsNotListWhenRepresentingInt32()
    {
        List<String>? expected = null;
        Union<Byte> u = 32;
        Assert.False(u.TryAsList_of_String(out var actual));
        Assert.Equal(expected, actual);
    }
    [Fact]
    public void IsListWhenRepresentingList()
    {
        var expected = new List<String>();
        Union<Byte> u = expected;
        Assert.True(u.TryAsList_of_String(out var actual));
        Assert.Equal(expected, actual);
    }
    [Fact]
    public void IsNotListWhenRepresentingByte()
    {
        List<String>? expected = null;
        Union<Byte> u = (Byte)32;
        Assert.False(u.TryAsList_of_String(out var actual));
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsNotByteWhenRepresentingInt32()
    {
        Byte expected = 0;
        Union<Byte> u = 32;
        Assert.False(u.TryAsT(out var actual));
        Assert.Equal(expected, actual);
    }
    [Fact]
    public void IsNotByteWhenRepresentingList()
    {
        Byte expected = 0;
        Union<Byte> u = new List<String>();
        Assert.False(u.TryAsT(out var actual));
        Assert.Equal(expected, actual);
    }
    [Fact]
    public void IsByteWhenRepresentingByte()
    {
        Byte expected = 32;
        Union<Byte> u = expected;
        Assert.True(u.TryAsT(out var actual));
        Assert.Equal(expected, actual);
    }
}