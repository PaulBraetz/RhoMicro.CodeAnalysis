namespace RhoMicro.CodeAnalysis.DslGenerator.Parsing;

using RhoMicro.CodeAnalysis.DslGenerator.Analysis;
using RhoMicro.CodeAnalysis.DslGenerator.Grammar;

#if DSL_GENERATOR
[IncludeFile]
#endif
readonly record struct ParseResult(RuleList RuleList, DiagnosticsCollection Diagnostics)
{
    public Boolean Equals(ParseResult other) => RuleList.Equals(other.RuleList);
    public override Int32 GetHashCode() => RuleList.GetHashCode();
}
