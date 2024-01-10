#if DSL_GENERATOR
namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;
#else
#pragma warning disable
#nullable enable
namespace RhoMicro.CodeAnalysis.DslGenerator.Generated.Grammar;
#endif

using System.Diagnostics;

#if DSL_GENERATOR
[IncludeFile]
#endif
[DebuggerDisplay("{ToDisplayString()}")]
abstract partial record RuleDefinition(Name Name, Rule Rule) : SyntaxNode
{
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed record New : RuleDefinition
    {
        public New(Name name, Rule rule) : base(name, rule) { }
        public override String ToDisplayString() => $"{Name.ToDisplayString()} = {Rule.ToDisplayString()};";
    }
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed record Incremental : RuleDefinition
    {
        public Incremental(Name name, Rule rule) : base(name, rule) { }

        public override String ToDisplayString() => $"{Name.ToDisplayString()} /= {Rule.ToDisplayString()};";
    }
    public override String ToMetaString() => $"new {nameof(RuleDefinition)}.{GetType().Name}(name: {Name.ToMetaString()}, rule: {Rule.ToMetaString()})";
}
