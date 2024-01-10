#if DSL_GENERATOR
namespace RhoMicro.CodeAnalysis.DslGenerator.Parsing;
#else
#pragma warning disable
#nullable enable
namespace RhoMicro.CodeAnalysis.DslGenerator.Generated.Parsing;
#endif

#if DSL_GENERATOR
using RhoMicro.CodeAnalysis.DslGenerator.Analysis;
using RhoMicro.CodeAnalysis.DslGenerator.Grammar;
#else
using RhoMicro.CodeAnalysis.DslGenerator.Generated.Analysis;
using RhoMicro.CodeAnalysis.DslGenerator.Generated.Grammar;
#endif

#if DSL_GENERATOR
[IncludeFile]
#endif
readonly record struct ParseResult(RuleList RuleList, DiagnosticsCollection Diagnostics)
{
    public Boolean Equals(ParseResult other) => RuleList.Equals(other.RuleList);
    public override Int32 GetHashCode() => RuleList.GetHashCode();
}
