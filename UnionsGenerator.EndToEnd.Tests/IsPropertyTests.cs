#pragma warning disable IDE0059 // Unnecessary assignment of a value
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace RhoMicro.CodeAnalysis.UnionsGenerator.EndToEnd.Tests;

using System;

public partial class IsPropertyTests
{
    [UnionType<Int32>(Alias = "Int")]
    [UnionType<List<String>>]
    partial class Union<[UnionType] T>;

    [Fact]
    public void IsIntWhenRepresentingInt32()
    {
        Union<Byte> u = 32;
        Assert.True(u.IsInt);
    }
    [Fact]
    public void IsNotIntWhenRepresentingList()
    {
        Union<Byte> u = new List<String>();
        Assert.False(u.IsInt);
    }
    [Fact]
    public void IsNotIntWhenRepresentingByte()
    {
        Union<Byte> u = (Byte)32;
        Assert.False(u.IsInt);
    }

    [Fact]
    public void IsNotListWhenRepresentingInt32()
    {
        Union<Byte> u = 32;
        Assert.False(u.IsList_of_String);
    }
    [Fact]
    public void IsListWhenRepresentingList()
    {
        Union<Byte> u = new List<String>();
        Assert.True(u.IsList_of_String);
    }
    [Fact]
    public void IsNotListWhenRepresentingByte()
    {
        Union<Byte> u = (Byte)32;
        Assert.False(u.IsList_of_String);
    }

    [Fact]
    public void IsNotByteWhenRepresentingInt32()
    {
        Union<Byte> u = 32;
        Assert.False(u.IsT);
    }
    [Fact]
    public void IsNotByteWhenRepresentingList()
    {
        Union<Byte> u = new List<String>();
        Assert.False(u.IsT);
    }
    [Fact]
    public void IsByteWhenRepresentingByte()
    {
        Union<Byte> u = (Byte)32;
        Assert.True(u.IsT);
    }
}
