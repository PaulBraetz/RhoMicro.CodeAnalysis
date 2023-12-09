namespace RhoMicro.CodeAnalysis.Common.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public partial class ReplaceableQueueTests
{
    [UnionType(typeof(String), Alias = "String")]
    [UnionType(typeof(Int32), Alias = "Macro")]
    public readonly partial struct StringOrMacro;

    public static Object[][] Data { get; } = [
        [
            new StringOrMacro[] { "Hello, World!" },
            new Dictionary<Int32, String[]>() { },
            "Hello, World!"],
        [
            new StringOrMacro[] { 0 },
            new Dictionary<Int32, String[]>()
            {
                {0, new[]{"Hello, World!" } }
            },
            "Hello, World!"],
        [
            new StringOrMacro[] { 0 },
            new Dictionary<Int32, String[]>()
            {
                {0, new[]{"Hello",", ","World!" } }
            },
            "Hello, World!"],
        [
            new StringOrMacro[] { 0, 1 },
            new Dictionary<Int32, String[]>()
            {
                {0, new[]{"Hello" } },
                {1, new[]{ ", ", "World!" } }
            },
            "Hello, World!"],
        [
            new StringOrMacro[] { 0, ", ", 1 },
            new Dictionary<Int32, String[]>()
            {
                {0, new[]{"Hello" } },
                {1, new[]{ "World!" } }
            },
            "Hello, World!"],
        [
            Array.Empty<StringOrMacro>(),
            new Dictionary<Int32, String[]>()
            {
                {0, new[]{"Hello, World!" } }
            },
            ""],
        [
            new StringOrMacro[] { "Foobar. ", 0 },
            new Dictionary<Int32, String[]>()
            {
                {0, new[]{"Hello, World!" } }
            },
            "Foobar. Hello, World!"],
        [
            new StringOrMacro[] { 0, " Foobar." },
            new Dictionary<Int32, String[]>()
            {
                {0, new[]{"Hello, World!" } }
            },
            "Hello, World! Foobar."],
        [
            new StringOrMacro[] { "Lorem ipsum ", 0, ", consectetur ", 1, "elit, sed do eiusmod tempor ", 2, " dolore magna aliqua." },
            new Dictionary<Int32, String[]>()
            {
                {0, new[]{ "dolor ","sit ","amet" } },
                {1, new[]{ "adipiscing " } },
                {2, new[]{ "incididunt", " ut labore", " et" } }
            },
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."]];

    [Theory]
    [MemberData(nameof(Data))]
    public void ReplacesCorrectly(StringOrMacro[] items, Dictionary<Int32, String[]> replacements, String expected)
    {
        _ = items ?? throw new ArgumentNullException(nameof(items));
        _ = replacements ?? throw new ArgumentNullException(nameof(replacements));

        var queue = new ExpandingMacroStringBuilder.Queue<String, Int32>();
        foreach(var item in items)
        {
            item.Switch(m => queue.EnqueueMacro(m), s => queue.EnqueueValue(s));
        }

        foreach(var replacement in replacements)
        {
            _ = queue.Expand(replacement.Key, q => replacement.Value.Aggregate(q, static (q, v) => q.EnqueueValue(v)));
        }

        var values = queue.DequeueValues();
        var actualBuilder = new StringBuilder();
        foreach(var value in values)
        {
            _ = actualBuilder.Append(value);
        }

        var actual = actualBuilder.ToString();

        Assert.Equal(expected, actual);
    }
    [Theory]
    [InlineData(new Object[] { new String[] { "Hello", ", ", "World", "!" } })]
    [InlineData(new Object[] { new String[] { "1", "2", "3", "4" } })]
    [InlineData(new Object[] { new String[] { "This ", "is", " a", " longer ", "sequence", " of ", "strings" } })]
    public void AppendsValues(String[] values)
    {
        _ = values ?? throw new ArgumentNullException(nameof(values));

        var queue = new ExpandingMacroStringBuilder.Queue<String, Int32>();
        foreach(var value in values)
        {
            _ = queue.EnqueueValue(value);
        }

        Assert.True(values.SequenceEqual(queue.DequeueValues()));
    }
}
