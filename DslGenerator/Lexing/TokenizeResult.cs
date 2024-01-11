namespace RhoMicro.CodeAnalysis.DslGenerator.Lexing;

using RhoMicro.CodeAnalysis.DslGenerator.Analysis;

#if DSL_GENERATOR
[IncludeFile]
#endif
readonly record struct TokenizeResult(IReadOnlyList<Token> Tokens, DiagnosticsCollection Diagnostics)
{
    public Boolean Equals(TokenizeResult other) => Tokens.SequenceEqual(other.Tokens);
    public override Int32 GetHashCode() => Tokens.Aggregate(997021164, (hc, t) => hc * -1521134295 + t.GetHashCode());
}
