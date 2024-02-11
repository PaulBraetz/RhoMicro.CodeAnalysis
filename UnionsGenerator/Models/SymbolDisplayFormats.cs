namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

internal static class SymbolDisplayFormats
{
    public static SymbolDisplayFormat FullNullableNoContainingTypesOrNamespacesVariant { get; } =
        new(SymbolDisplayGlobalNamespaceStyle.Omitted,
            SymbolDisplayTypeQualificationStyle.NameOnly,
            SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeVariance,
            SymbolDisplayMemberOptions.None,
            SymbolDisplayDelegateStyle.NameOnly,
            SymbolDisplayExtensionMethodStyle.Default,
            SymbolDisplayParameterOptions.None,
            SymbolDisplayPropertyStyle.NameOnly,
            SymbolDisplayLocalOptions.None,
            SymbolDisplayKindOptions.None,
            SymbolDisplayMiscellaneousOptions.ExpandNullable |
                SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier |
                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);
    public static SymbolDisplayFormat FullNullableNoContainingTypesOrNamespaces { get; } =
        new(SymbolDisplayGlobalNamespaceStyle.Omitted,
            SymbolDisplayTypeQualificationStyle.NameOnly,
            SymbolDisplayGenericsOptions.IncludeTypeParameters,
            SymbolDisplayMemberOptions.None,
            SymbolDisplayDelegateStyle.NameOnly,
            SymbolDisplayExtensionMethodStyle.Default,
            SymbolDisplayParameterOptions.None,
            SymbolDisplayPropertyStyle.NameOnly,
            SymbolDisplayLocalOptions.None,
            SymbolDisplayKindOptions.None,
            SymbolDisplayMiscellaneousOptions.ExpandNullable | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);
    public static SymbolDisplayFormat FullNullable { get; } =
        SymbolDisplayFormat.FullyQualifiedFormat
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
                    ( SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions &
                    ( SymbolDisplayMiscellaneousOptions.UseSpecialTypes ^ (SymbolDisplayMiscellaneousOptions)Int32.MaxValue ) ) |
                    SymbolDisplayMiscellaneousOptions.ExpandNullable)
                    .WithGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeParameters)
                    .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted);
}
