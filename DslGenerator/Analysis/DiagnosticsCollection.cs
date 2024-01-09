namespace RhoMicro.CodeAnalysis.DslGenerator.Analysis;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.DslGenerator.Lexing;

using System;
using System.Collections;
using System.Collections.Generic;

sealed class DiagnosticsCollection : IReadOnlyList<Diagnostic>
{
    private readonly List<Diagnostic> _diagnostics = [];

    public void ReportToContext(SourceProductionContext context)
    {
        foreach(var diagnostic in _diagnostics)
        {
            context.ReportDiagnostic(diagnostic);
        }
    }
    public void Add(DiagnosticDescriptor descriptor, Location location, params Object[] messageArgs) =>
        _diagnostics.Add(Diagnostic.Create(descriptor, location, messageArgs));

    public Diagnostic this[Int32 index] => ((IReadOnlyList<Diagnostic>)_diagnostics)[index];

    public Int32 Count => ((IReadOnlyCollection<Diagnostic>)_diagnostics).Count;

    public IEnumerator<Diagnostic> GetEnumerator() => ((IEnumerable<Diagnostic>)_diagnostics).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_diagnostics).GetEnumerator();
}