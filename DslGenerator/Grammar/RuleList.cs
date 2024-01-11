namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;

using System.Diagnostics;
using System.Linq;
using System.Text;

#if DSL_GENERATOR
[IncludeFile]
#endif
[DebuggerDisplay("{ToDisplayString()}")]
record RuleList(IReadOnlyList<RuleDefinition> Definitions) : SyntaxNode
{
    public override String ToString() => base.ToString();
    public virtual Boolean Equals(RuleList other) => other.Definitions.SequenceEqual(Definitions);
    public override Int32 GetHashCode() => Definitions.Aggregate(997021164, (hc, def) => hc * -1521134295 + def.GetHashCode());
    public override void AppendDisplayStringTo(StringBuilder builder) =>
        _ = Definitions.Aggregate(builder, (sb, r) => sb.AppendDisplayString(r).Append('\n'));
    protected override void AppendCtorArgs(StringBuilder builder) => AppendCtorArg(builder, nameof(Definitions), Definitions);
}
