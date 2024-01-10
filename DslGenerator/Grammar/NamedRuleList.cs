#if DSL_GENERATOR
namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;
#else
#pragma warning disable
#nullable enable
namespace RhoMicro.CodeAnalysis.DslGenerator.Generated.Grammar;
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;

#if DSL_GENERATOR
[IncludeFile]
#endif
[DebuggerDisplay("{ToDisplayString()}")]
sealed record NamedRuleList(Name Name, IReadOnlyList<RuleDefinition> Definitions) : RuleList(Definitions)
{
    public Boolean Equals(NamedRuleList other) => Name.Equals(other.Name) && base.Equals(other);
    public override Int32 GetHashCode() => base.GetHashCode() * -1521134295 + Name.GetHashCode();
    public override String ToDisplayString() => $"{Name.ToDisplayString()};\n{base.ToDisplayString()}";
    public override String ToMetaString() => $"new {nameof(NamedRuleList)}({nameof(Name)}: {Name.ToMetaString()},{nameof(Definitions)}: [{String.Join(",", Definitions.Select(d => d.ToMetaString()))}])";
    public override String ToString() => ToDisplayString();
}
