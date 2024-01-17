namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;

#if DSL_GENERATOR
[IncludeFile]
#endif
static class Utils
{
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Boolean IsValidNameChar(Char c) =>
        c is >= 'a' and <= 'z' or
        >= 'A' and <= 'Z' or
        '_';
}