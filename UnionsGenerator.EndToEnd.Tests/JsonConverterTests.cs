namespace RhoMicro.CodeAnalysis.UnionsGenerator.EndToEnd.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public partial class JsonConverterTests
{
    [UnionType<DateTime, Double, String, List<String>>]
    readonly partial struct Union;

    [Fact]
    public void SerializesDateTimeUnion()
    {
        var expected = DateTime.Parse("01/10/2009 7:34");
        Union u = expected;
        var serialized = JsonSerializer.Serialize(u);
        var deserialized = JsonSerializer.Deserialize<Union>(serialized);
        Assert.True(deserialized.IsDateTime);
        Assert.Equal(expected, deserialized.AsDateTime);
    }
    [Fact]
    public void SerializesDoubleUnion()
    {
        var expected = 32d;
        Union u = expected;
        var serialized = JsonSerializer.Serialize(u);
        var deserialized = JsonSerializer.Deserialize<Union>(serialized);
        Assert.True(deserialized.IsDouble);
        Assert.Equal(expected, deserialized.AsDouble);
    }
    [Fact]
    public void SerializesStringUnion()
    {
        var expected = "Hello, World!";
        Union u = expected;
        var serialized = JsonSerializer.Serialize(u);
        var deserialized = JsonSerializer.Deserialize<Union>(serialized);
        Assert.True(deserialized.IsString);
        Assert.Equal(expected, deserialized.AsString);
    }
    [Fact]
    public void SerializesListUnion()
    {
        List<String> expected = ["Hell", "o, ", "World", "!"];
        Union u = expected;
        var serialized = JsonSerializer.Serialize(u);
        var deserialized = JsonSerializer.Deserialize<Union>(serialized);
        Assert.True(deserialized.IsList_of_String);
        Assert.True(expected.SequenceEqual(deserialized.AsList_of_String!));
    }
}
