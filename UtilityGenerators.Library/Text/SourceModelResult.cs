namespace RhoMicro.CodeAnalysis.Library.Text;

using Microsoft.CodeAnalysis;

using System;
using System.Collections.Immutable;

readonly record struct SourceModelResult(String HintName, String SourceText, ImmutableArray<Diagnostic> Diagnostics)
{
    private static readonly ImmutableArray<Diagnostic> _emptyDiagnostics = ImmutableArray.Create<Diagnostic>();
    public static SourceModelResult Create(String hint, String sourceText)
    {
        var hintName = $"{hint}_{Guid.NewGuid().ToString().Replace('-', '_')}.g.cs";
        var result = new SourceModelResult(hintName, sourceText, _emptyDiagnostics);

        return result;
    }

    public void AddToContext(SourceProductionContext context)
    {
        context.AddSource(HintName, SourceText);
        for(var i = 0; i < Diagnostics.Length; i++)
        {
            context.ReportDiagnostic(Diagnostics[i]);
        }
    }
    public static void AddToContext(SourceProductionContext context, SourceModelResult result) => result.AddToContext(context);
}
