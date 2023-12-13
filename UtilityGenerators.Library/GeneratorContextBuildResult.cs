namespace RhoMicro.CodeAnalysis.UtilityGenerators.Library;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents the result of building source text using a generator context.
/// </summary>
/// <typeparam name="TModel">The type of model used.</typeparam>
/// <param name="sourceText">The source text generated.</param>
/// <param name="diagnostics">The diagnostics accumulator used while generating the source text.</param>
readonly struct GeneratorContextBuildResult<TModel>(String sourceText, IDiagnosticsAccumulator<TModel> diagnostics) : IEquatable<GeneratorContextBuildResult<TModel>>
{
    public readonly String SourceText = sourceText;
    private readonly IDiagnosticsAccumulator<TModel> _diagnostics = diagnostics;

    public void AddToContext(SourceProductionContext context, String hintName)
    {
        _diagnostics.ReportDiagnostics(context.ReportDiagnostic);
        context.AddSource(hintName, SourceText);
    }

    public override Boolean Equals(Object? obj) => obj is GeneratorContextBuildResult<TModel> result && Equals(result);
    public Boolean Equals(GeneratorContextBuildResult<TModel> other) => SourceText == other.SourceText;
    public override Int32 GetHashCode() => -1893336041 + EqualityComparer<String>.Default.GetHashCode(SourceText);
    public GeneratorContextBuildResult<TModel> WithSourceText(String sourceText) => new(sourceText, _diagnostics);

    public static Boolean operator ==(GeneratorContextBuildResult<TModel> left, GeneratorContextBuildResult<TModel> right) => left.Equals(right);
    public static Boolean operator !=(GeneratorContextBuildResult<TModel> left, GeneratorContextBuildResult<TModel> right) => !(left == right);
}
