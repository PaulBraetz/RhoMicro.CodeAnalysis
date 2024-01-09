namespace RhoMicro.CodeAnalysis.DslGenerator.Analysis;

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

    public static DiagnosticDescriptor UnexpectedCharacter { get; } =
        Create(1, "Unexpected Character", "Encountered unexpected character.");
    public static DiagnosticDescriptor UnterminatedTerminal { get; } =
        Create(2, "Unterminated Terminal", "Encountered an unterminated terminal (missing closing quote).");
    public static DiagnosticDescriptor UnexpectedToken { get; } =
        Create(3, "Unexpected Token", "Encountered an unexpected token. Expected token of type '{0}'");
}
