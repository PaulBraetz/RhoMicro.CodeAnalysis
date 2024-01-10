#if DSL_GENERATOR
namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;
#else
#pragma warning disable
#nullable enable
namespace RhoMicro.CodeAnalysis.DslGenerator.Generated.Grammar;
#endif

#if DSL_GENERATOR
[IncludeFile]
#endif
abstract record SyntaxNode
{
    public abstract String ToDisplayString();
    public abstract String ToMetaString();
}
