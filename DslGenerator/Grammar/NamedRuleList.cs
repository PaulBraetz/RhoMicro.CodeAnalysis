namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

#if DSL_GENERATOR
[IncludeFile]
#endif
[DebuggerDisplay("{ToDisplayString()}")]
record NamedRuleList(Name Name, IReadOnlyList<RuleDefinition> Definitions) : RuleList(Definitions)
{
    public override String ToString() => base.ToString();
    public virtual Boolean Equals(NamedRuleList other) => Name.Equals(other.Name) && base.Equals(other);
    public override Int32 GetHashCode() => base.GetHashCode() * -1521134295 + Name.GetHashCode();
    public override void AppendDisplayStringTo(StringBuilder builder) => base.AppendDisplayStringTo(builder.AppendDisplayString(Name).AppendLine(";"));
    protected override void AppendCtorArgs(StringBuilder builder) => AppendCtorArg(AppendCtorArg(builder, nameof(Name), Name).Append(", "), nameof(Definitions), Definitions);
}
