namespace RhoMicro.CodeAnalysis.DslGenerator.Lexing;

using Microsoft.CodeAnalysis;

using System;

static class DiagnosticDescriptors
{
    private const String _category = "RhoMicro.CodeAnalysis.DslGenerator";

    private static DiagnosticDescriptor Create(
        Int32 id,
        String title,
        String message,
        DiagnosticSeverity severity = DiagnosticSeverity.Error,
        Boolean isEnabledByDefault = true) =>
        new($"DSLG{id:0000}", title, message, _category, severity, isEnabledByDefault);

    public static DiagnosticDescriptor UnexpectedCharacter =
        Create(1, "Unexpected Token", "Encountered unexpected token at {0}:{1} ({2})");
    public static DiagnosticDescriptor UnterminatedTerminal=
        Create(2, "Unterminated Terminal", "Encountered an unterminated terminal (missing closing quote) at {0}:{1} ({2})");
}
