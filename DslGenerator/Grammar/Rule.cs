#if DSL_GENERATOR
namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;
#else
#pragma warning disable
#nullable enable
namespace RhoMicro.CodeAnalysis.DslGenerator.Generated.Grammar;
#endif

using System.Diagnostics;
using System.Text.RegularExpressions;

#if DSL_GENERATOR
[IncludeFile]
#endif
[DebuggerDisplay("{ToDisplayString()}")]
abstract partial record Rule : SyntaxNode
{
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed record Alternative(Rule Left, Rule Right) : Rule
    {
        public override String ToDisplayString() => $"{Left.ToDisplayString()} / {Right.ToDisplayString()}";
        public override String ToMetaString() => $"new {nameof(Rule)}.{nameof(Alternative)}({nameof(Left)}: {Left.ToMetaString()},{nameof(Right)}: {Right.ToMetaString()})";
    }
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed record Concatenation(Rule Left, Rule Right) : Rule
    {
        public override String ToDisplayString() => $"{Left.ToDisplayString()} {Right.ToDisplayString()}";
        public override String ToMetaString() => $"new {nameof(Rule)}.{nameof(Concatenation)}({nameof(Left)}: {Left.ToMetaString()},{nameof(Right)}: {Right.ToMetaString()})";
    }
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed record SpecificRepetition(Int32 Count, Rule Rule) : Rule
    {
        public override String ToDisplayString() => $"{Count}{Rule.ToDisplayString()}";
        public override String ToMetaString() => $"new {nameof(Rule)}.{nameof(SpecificRepetition)}({nameof(Count)}: {Count},{nameof(Rule)}: {Rule.ToMetaString()})";
    }
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed record VariableRepetition(Rule Rule) : Rule
    {
        public override String ToDisplayString() => $"*{Rule.ToDisplayString()}";
        public override String ToMetaString() => $"new {nameof(Rule)}.{nameof(VariableRepetition)}({nameof(Rule)}: {Rule.ToMetaString()})";
    }
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed record Grouping(Rule Rule) : Rule
    {
        public override String ToDisplayString() => $"({Rule.ToDisplayString()})";
        public override String ToMetaString() => $"new {nameof(Rule)}.{nameof(Grouping)}({nameof(Rule)}: {Rule.ToMetaString()})";
    }
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed record OptionalGrouping(Rule Rule) : Rule
    {
        public override String ToDisplayString() => $"[{Rule.ToDisplayString()}]";
        public override String ToMetaString() => $"new {nameof(Rule)}.{nameof(OptionalGrouping)}({nameof(Rule)}: {Rule.ToMetaString()})";
    }
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed partial record Terminal(String Value) : Rule
    {
        public override String ToDisplayString() => $"\"{Value}\"";
        private static readonly Regex _quotePattern = new("(\"*)", RegexOptions.Compiled);
        private readonly Lazy<String> _metaString = new(() =>
        {
            var rawQuotes = String.Concat(Enumerable.Repeat('"',
                _quotePattern.Matches(Value)
                    .Cast<Match>()
                    .Select(m => m.Length)
                    .Append(2)
                    .Max() + 1));
            return $"new {nameof(Rule)}.{nameof(Terminal)}({nameof(Value)}: \n{rawQuotes}\n{Value}\n{rawQuotes})";
        });
        public override String ToMetaString() => _metaString.Value;
        public Boolean Equals(Terminal other) => other is not null && Value == other.Value;
        public override Int32 GetHashCode() => Value.GetHashCode();
    }
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed partial record Reference(Name Name) : Rule
    {
        public override String ToDisplayString() => Name.ToDisplayString();
        public override String ToMetaString() => $"new {nameof(Rule)}.{nameof(Reference)}({nameof(Name)}: {Name.ToMetaString()})";
    }
}
