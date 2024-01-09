namespace RhoMicro.CodeAnalysis.DslGenerator.Lexing;

using System.Collections.Immutable;

using RhoMicro.CodeAnalysis.DslGenerator.Analysis;
using RhoMicro.CodeAnalysis.UtilityGenerators;

readonly record struct TokenizeResult(ImmutableArray<Token> Tokens, DiagnosticsCollection Diagnostics)
{
    private static readonly IEqualityComparer<ImmutableArray<Token>> _comparer =
        ImmutableArrayCollectionEqualityComparer<Token>.Default;
    public Boolean Equals(TokenizeResult other) => _comparer.Equals(Tokens, other.Tokens);
    public override Int32 GetHashCode() => _comparer.GetHashCode(Tokens);
}
