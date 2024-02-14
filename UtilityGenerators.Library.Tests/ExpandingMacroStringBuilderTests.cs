#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace RhoMicro.CodeAnalysis.Library.Tests;

using System;

public partial class ExpandingMacroStringBuilderTests
{
    [UnionType<Char, String>]
    [UnionType<Int32>(Alias = "Macro")]
    [UnionTypeSettings(Layout = LayoutSetting.Auto)]
    public readonly partial struct ValueOrPlaceholder;

    public readonly struct Unit;

    public static Object[][] Data =>
        [
            [
                new ValueOrPlaceholder[] { "Hello", ',', " World", '!' },
                Array.Empty<IMacroExpansion<Int32>>(),
                "Hello, World!"
            ],
            [
                new ValueOrPlaceholder[] { "Hello", 0, 1, '!' },
                new[]
                {
                    MacroExpansion.Create(0, (b,_)=>b.Append(',')),
                    MacroExpansion.Create(1, (b,_)=>b.Append(" World"))
                },
                "Hello, World!"
            ],
            [
                new ValueOrPlaceholder[] { "Hello", 0, '!' },
                new[]
                {
                    MacroExpansion.Create(0, (b,t)=>b.Append(',').AppendMacro(1, t)),
                    MacroExpansion.Create(1, (b,_)=>b.Append(" World"))
                },
                "Hello, World!"
            ],
            [
                new ValueOrPlaceholder[] { "He", -1, "o", 0, '!' },
                new[]
                {
                    MacroExpansion.Create(0, (b,t)=>b.Append(',').AppendMacro(1, t)),
                    MacroExpansion.Create(1, (b,_)=>b.Append(" World")),
                    MacroExpansion.Create(-1, (b,_)=>b.Append("ll"))
                },
                "Hello, World!"
            ],
            [
                new ValueOrPlaceholder[] { "He", 0 },
                new[]
                {
                    MacroExpansion.Create(2, (b,t)=>b.Append('l').Append('d').AppendMacro(3, t)),
                    MacroExpansion.Create(3, (b,_)=>b.Append('!')),
                    MacroExpansion.Create(1, (b,_)=>b.Append("o, ")),
                    MacroExpansion.Create(0, (b,t)=>b.Append("ll").AppendMacro(1,t).Append("Wor").AppendMacro(2, t))
                },
                "Hello, World!"
            ],
            [
                new ValueOrPlaceholder[] { "He", 0 },
                new[]
                {
                    MacroExpansion.Create(2, (b,t)=>b.AppendMacro(4,t).Append('d').AppendMacro(3, t)),
                    MacroExpansion.Create(3, (b,_)=>b.Append('!')),
                    MacroExpansion.Create(1, (b,_)=>b.Append("o, ")),
                    MacroExpansion.Create(4, (b,_)=>b.Append('l')),
                    MacroExpansion.Create(0, (b,t)=>b.AppendMacro(4,t).AppendMacro(4,t).AppendMacro(1,t).Append("Wor").AppendMacro(2, t))
                },
                "Hello, World!"
            ]
        ];

    [Theory]
    [MemberData(nameof(Data))]
    internal void ExpandsCorrectly(
        IEnumerable<ValueOrPlaceholder> valuesOrPlaceholders,
        IEnumerable<IMacroExpansion<Int32>> providers,
        String expected)
    {
        var actual = CreateAndInitializeBuilder(valuesOrPlaceholders, providers).Build(default);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(Data))]
    internal void SupportsRepeatableBuilds(
        IEnumerable<ValueOrPlaceholder> valuesOrPlaceholders,
        IEnumerable<IMacroExpansion<Int32>> providers,
        String _)
    {
        var builder = CreateAndInitializeBuilder(valuesOrPlaceholders, providers);
        var expected = builder.Build();
        var actual = builder.Build();
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(Data))]
    internal void ExpansionDoesNotConsumeBuilderQueue(
        IEnumerable<ValueOrPlaceholder> valuesOrPlaceholders,
        IEnumerable<IMacroExpansion<Int32>> providers,
        String expected)
    {
        var builder = CreateAndInitializeBuilder(valuesOrPlaceholders, Array.Empty<IMacroExpansion<Int32>>());
        foreach(var provider in providers)
        {
            _ = builder.Receive(provider.Macro, (b, t) =>
            {
                provider.Expand(b, t);
                try
                {
                    _ = b.Build(t);
                } catch(UnexpandedMacrosException<Int32>)
                { }
            });
        }

        var actual = builder.Build();
        Assert.Equal(expected, actual);
    }

    public static Object[][] RecursiveData =>
        [
            [
                new ValueOrPlaceholder[] { "Hello", 0, "World" },
                new[]
                {
                    MacroExpansion.Create(0, (b,t)=>b.Append(',').AppendMacro(0, t))
                }
            ],
            [
                new ValueOrPlaceholder[] { "Hello", 0, "World" },
                new[]
                {
                    MacroExpansion.Create(1, static (b,t)=>b.AppendMacro(2, t)),
                    MacroExpansion.Create(0, static (b,t)=>b.Append(',').AppendMacro(1, t)),
                    MacroExpansion.Create(2, static (b,t)=>b.AppendMacro(0, t))
                }
            ]
        ];
    [Theory]
    [MemberData(nameof(RecursiveData))]
    internal void ThrowsInfiniteRecusion(IEnumerable<ValueOrPlaceholder> valuesOrPlaceholders, IEnumerable<IMacroExpansion<Int32>> providers) =>
        Assert.Throws<InfinitelyRecursingExpansionException<Int32>>(() => CreateAndInitializeBuilder(valuesOrPlaceholders, providers));

    [Fact]
    internal void ThrowsNIE()
    {
        var ex = Assert.Throws<NotImplementedException>(static () =>
        {
            var builder = ExpandingMacroStringBuilder.Create<Int32>(); //()-
            builder = builder.Append("He"); //(He)-
            builder = builder.AppendMacro(1); //(He)-[1]
            builder = builder.Append("o, "); //(He)-[1]><["o, "]
            builder = builder.AppendMacro(0);  //(He)-[1]><["o, "]><[0]
            builder = builder.Receive(
                0,
                static (b, t) => b.Append("World").Receive(1, (b, t) => b.Append("ll"), t)); //(Hello, World)-
            builder = builder.AppendLine('!'); //(Hello, World!)-

            var s = builder.Build();
        });

        Assert.Equal("Providing macro expansions inside of macro expansions is not implemented yet.", ex.Message);
    }

    [Fact]
    internal void DoesNotThrowIOE()
    {
        var builder = ExpandingMacroStringBuilder.Create<Int32>(); //()-
        builder = builder.Append("He"); //(He)-
        builder = builder.AppendMacro(0); //(He)-[0]
        builder = builder.Append("o, "); //(He)-[0]><["o, "]
        builder = builder.AppendMacro(0);  //(He)-[0]><["o, "]><[0]
        builder = builder.Receive(0, static (b, t) => b.Append("World")); //(HeWorldo, World)-
        builder = builder.AppendLine('!'); //(HeWorldo, World!)-

        var s = builder.Build();
    }

    private static IExpandingMacroStringBuilder<Int32> CreateAndInitializeBuilder(IEnumerable<ValueOrPlaceholder> valuesOrPlaceholders, IEnumerable<IMacroExpansion<Int32>> providers)
    {
        var builder = ExpandingMacroStringBuilder.Create<Int32>();

        foreach(var valueOrPlaceholder in valuesOrPlaceholders)
        {
            valueOrPlaceholder.Switch(
                onChar: c => builder.Append(c),
                onString: s => builder.Append(s),
                onMacro: p => builder.AppendMacro(p));
        }

        foreach(var provider in providers)
        {
            builder = builder.Receive(provider);
        }

        return builder;
    }
}
