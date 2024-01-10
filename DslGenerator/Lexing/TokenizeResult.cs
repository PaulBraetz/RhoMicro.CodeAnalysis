#if DSL_GENERATOR
namespace RhoMicro.CodeAnalysis.DslGenerator.Lexing;
#else
#pragma warning disable
#nullable enable
namespace RhoMicro.CodeAnalysis.DslGenerator.Generated.Lexing;
#endif

#if DSL_GENERATOR
using RhoMicro.CodeAnalysis.DslGenerator.Analysis;
#else
using RhoMicro.CodeAnalysis.DslGenerator.Generated.Analysis;
#endif

using System.Collections.Immutable;
using System.Xml.Linq;

#if DSL_GENERATOR
[IncludeFile]
#endif
readonly record struct TokenizeResult(IReadOnlyList<Token> Tokens, DiagnosticsCollection Diagnostics)
{
    public Boolean Equals(TokenizeResult other) => Tokens.SequenceEqual(other.Tokens);
    public override Int32 GetHashCode() => Tokens.Aggregate(997021164, (hc, t) => hc * -1521134295 + t.GetHashCode());
}
