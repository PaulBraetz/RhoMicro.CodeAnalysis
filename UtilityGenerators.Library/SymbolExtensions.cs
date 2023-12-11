namespace RhoMicro.CodeAnalysis.UtilityGenerators.Library;

using Microsoft.CodeAnalysis;

using System;

static class SymbolExtensions
{
    /// <summary>
    /// Gets the legible, fully qualified name of a symbol; suitable for use in hint names.
    /// </summary>
    /// <param name="symbol">The symbol whose hint name to get.</param>
    /// <returns>The symbols hint name.</returns>
    public static String ToHintName(this ISymbol symbol)=>
        symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat
                    .WithMiscellaneousOptions(
                    /*
                        get rid of special types

                                10110
                        NAND 00100
                            => 10010

                                10110
                            &! 00100
                            => 10010

                                00100
                            ^ 11111
                            => 11011

                                10110
                            & 11011
                            => 10010
                    */
                    SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions &
                    (SymbolDisplayMiscellaneousOptions.UseSpecialTypes ^ (SymbolDisplayMiscellaneousOptions)Int32.MaxValue))
                    .WithGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeParameters))
                    .Replace("<", "_of_")
                    .Replace('>', '_')
                    .Replace(",", "_and_")
                    .Replace(" ", String.Empty)
                    .Replace('.', '_')
                    .Replace("::", "_")
                    .TrimEnd('_');
}
