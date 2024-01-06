namespace RhoMicro.CodeAnalysis.DslGenerator.Lexing;

using Microsoft.CodeAnalysis;

using System;
using System.Collections;
using System.Collections.Generic;

sealed class DiagnosticsCollection : IReadOnlyList<Diagnostic>
{
    private readonly List<Diagnostic> _diagnostics = [];

    public void Add(DiagnosticDescriptor descriptor, Location location, Lexeme lexeme)
    {
        var lineSpan = location.GetLineSpan();
        var line = lineSpan.StartLinePosition.Line;
        var character = lineSpan.Span.Start.Character;

        _diagnostics.Add(Diagnostic.Create(descriptor, location, line, character, lexeme));
    }

    public Diagnostic this[Int32 index] => ((IReadOnlyList<Diagnostic>)_diagnostics)[index];

    public Int32 Count => ((IReadOnlyCollection<Diagnostic>)_diagnostics).Count;

    public IEnumerator<Diagnostic> GetEnumerator() => ((IEnumerable<Diagnostic>)_diagnostics).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_diagnostics).GetEnumerator();
}