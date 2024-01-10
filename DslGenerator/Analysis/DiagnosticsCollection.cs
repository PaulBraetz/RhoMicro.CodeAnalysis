#if DSL_GENERATOR
namespace RhoMicro.CodeAnalysis.DslGenerator.Analysis;
#else
#pragma warning disable
#nullable enable
namespace RhoMicro.CodeAnalysis.DslGenerator.Generated.Analysis;
#endif

#if DSL_GENERATOR
using RhoMicro.CodeAnalysis.DslGenerator.Lexing;
#else
using RhoMicro.CodeAnalysis.DslGenerator.Generated.Lexing;
#endif

using System;
using System.Collections;
using System.Collections.Generic;

#if DSL_GENERATOR
[IncludeFile]
#endif
sealed class DiagnosticsCollection : IReadOnlyList<Diagnostic>
{
    private readonly List<Diagnostic> _diagnostics = [];

#if DSL_GENERATOR
    public void ReportToContext(Microsoft.CodeAnalysis.SourceProductionContext context)
    {
        foreach(var diagnostic in _diagnostics)
        {
            var msDiagnostic = diagnostic.ToMsDiagnostic();
            context.ReportDiagnostic(msDiagnostic);
        }
    }
#endif
    public void Add(DiagnosticDescriptor descriptor, Location location, params Object[] messageArgs) =>
        _diagnostics.Add(new Diagnostic(descriptor, location, messageArgs));
    public void Add(DiagnosticsCollection diagnostics) => _diagnostics.AddRange(diagnostics);

    public Diagnostic this[Int32 index] => ((IReadOnlyList<Diagnostic>)_diagnostics)[index];

    public Int32 Count => ((IReadOnlyCollection<Diagnostic>)_diagnostics).Count;

    public IEnumerator<Diagnostic> GetEnumerator() => ((IEnumerable<Diagnostic>)_diagnostics).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_diagnostics).GetEnumerator();
}