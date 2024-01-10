
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
static class Utils
{
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Boolean IsValidNameChar(Char c) => Char.IsLetter(c) || c == '_';
}