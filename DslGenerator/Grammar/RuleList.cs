#if DSL_GENERATOR
namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;
#else
#pragma warning disable
#nullable enable
namespace RhoMicro.CodeAnalysis.DslGenerator.Generated.Grammar;
#endif

using System.Diagnostics;
using System.Linq;
using System.Text;

#if DSL_GENERATOR
[IncludeFile]
#endif
[DebuggerDisplay("{ToDisplayString()}")]
record RuleList(IReadOnlyList<RuleDefinition> Definitions) : SyntaxNode
{
    public virtual Boolean Equals(RuleList other) => other.Definitions.SequenceEqual(Definitions);
    public override Int32 GetHashCode() => Definitions.Aggregate(997021164, (hc, def) => hc * -1521134295 + def.GetHashCode());
    public override String ToDisplayString() => Definitions.Aggregate(new StringBuilder(), (sb, r) => sb.Append(r.ToDisplayString()).Append('\n')).ToString();
    public override String ToMetaString() => $"new {nameof(RuleList)}({nameof(Definitions)}: [{String.Join(",", Definitions.Select(d => d.ToMetaString()))}])";
    public override String ToString() => ToDisplayString();
}
