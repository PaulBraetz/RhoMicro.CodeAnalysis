#if DSL_GENERATOR
namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;
#else
#pragma warning disable
#nullable enable
namespace RhoMicro.CodeAnalysis.DslGenerator.Generated.Grammar;
#endif

#nullable enable
#if DSL_GENERATOR
using RhoMicro.CodeAnalysis.DslGenerator.Lexing;
#else
using RhoMicro.CodeAnalysis.DslGenerator.Generated.Lexing;
#endif

#if DSL_GENERATOR
[IncludeFile]
#endif
partial record Rule
{
    partial record Terminal
    {
        public static Terminal Create(Lexeme lexeme) => new(lexeme.ToString() ?? String.Empty);
    }
}