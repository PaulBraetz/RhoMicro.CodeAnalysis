#if DSL_GENERATOR
namespace RhoMicro.CodeAnalysis.DslGenerator.Analysis;
#else
#pragma warning disable
#nullable enable
namespace RhoMicro.CodeAnalysis.DslGenerator.Generated.Analysis;
#endif

using System;

#if DSL_GENERATOR
using RhoMicro.CodeAnalysis.DslGenerator.Lexing;
#else
using RhoMicro.CodeAnalysis.DslGenerator.Generated.Lexing;
#endif

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
}
