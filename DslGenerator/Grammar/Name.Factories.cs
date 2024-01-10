#if DSL_GENERATOR
namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;
#else
#pragma warning disable
#nullable enable
namespace RhoMicro.CodeAnalysis.DslGenerator.Generated.Grammar;
#endif

#if DSL_GENERATOR
using RhoMicro.CodeAnalysis.DslGenerator.Lexing;
#else
using RhoMicro.CodeAnalysis.DslGenerator.Generated.Lexing;
#endif

#if DSL_GENERATOR
[IncludeFile]
#endif
partial record Name
{
    public Name(Token token) : this(token.Lexeme) { }
    public Name(Lexeme lexeme) : this(lexeme.ToString() ?? String.Empty) { }
}
