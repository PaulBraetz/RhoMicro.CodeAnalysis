namespace RhoMicro.CodeAnalysis.Library;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using System.Threading;

/// <summary>
/// Incrementally accumulates model diagnostics.
/// </summary>
/// <typeparam name="TModel">The type of model to diagnose.</typeparam>
interface IDiagnosticsAccumulator<TModel>
{
    /// <summary>
    /// Gets a value indicating whether the accumulator currently contains errors.
    /// </summary>
    Boolean ContainsErrors { get; }
    /// <summary>
    /// Adds a diagnostic to the accumulator.
    /// </summary>
    /// <param name="diagnostic">The diagnostic to add.</param>
    /// <returns>A reference to the accumulator, for chaining of further method calls.</returns>
    IDiagnosticsAccumulator<TModel> Add(Diagnostic diagnostic);
    /// <summary>
    /// Reports the accumulated diagnostics to a report handler.
    /// For instance, the handler could be the <see cref="SyntaxNodeAnalysisContext.ReportDiagnostic(Diagnostic)"/> method.
    /// </summary>
    /// <param name="report">The context action to invoke in order to report diagnostics.</param>
    void ReportDiagnostics(Action<Diagnostic> report);
    /// <summary>
    /// Receives a diagnostics provider for generating diagnostics on the model.
    /// </summary>
    /// <param name="provider">The provider to generate diagnostics with.</param>
    /// <param name="cancellationToken">A token signalling the diagnosis should terminate.</param>
    /// <returns>A reference tothe accumulator, for chaining of further method calls.</returns>
    IDiagnosticsAccumulator<TModel> Receive(IDiagnosticProvider<TModel> provider, CancellationToken cancellationToken = default);
}