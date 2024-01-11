namespace RhoMicro.CodeAnalysis.DslGenerator.Analysis;
using System;

#if DSL_GENERATOR
[IncludeFile]
#endif
static class DiagnosticDescriptors
{
    private static DiagnosticDescriptor Create(
        Int32 id,
        String title,
        String message) =>
        new(id, title, message);

    public static DiagnosticDescriptor UnexpectedCharacter { get; } =
        Create(1, "Unexpected Character", "Encountered unexpected character.");
    public static DiagnosticDescriptor UnterminatedTerminal { get; } =
        Create(2, "Unterminated Terminal", "Encountered an unterminated terminal (missing closing quote).");
    public static DiagnosticDescriptor UnexpectedToken { get; } =
        Create(3, "Unexpected Token", "Encountered an unexpected '{0}' token. {1}");
    public static DiagnosticDescriptor InvalidRangeToken { get; } =
        Create(4, "Invalid Range", "Encountered a range token. Ranges must be surrounded by terminals of one character.");
}
