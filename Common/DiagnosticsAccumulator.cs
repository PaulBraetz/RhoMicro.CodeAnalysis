namespace RhoMicro.CodeAnalysis.Common;

using Microsoft.CodeAnalysis;

/// <summary>
/// Contains factory and extension methods for diagnostic accumulators.
/// </summary>
static partial class DiagnosticsAccumulator
{
    /// <summary>
    /// Creates a new diagnostic accumulator.
    /// </summary>
    /// <typeparam name="TModel">The type of model to diagnose.</typeparam>
    /// <param name="model">The model to diagnose.</param>
    /// <returns>A new diagnostic accumulator.</returns>
    public static IDiagnosticsAccumulator<TModel> Create<TModel>(TModel model) =>
        new Impl<TModel>(model);

    /// <summary>
    /// Applies a filter to the diagnostic accumulator that discards all hidden diagnostics.
    /// This will prevent diagnostics from being added to the accumulator if they have a severity of <see cref="DiagnosticSeverity.Hidden"/>.
    /// </summary>
    /// <typeparam name="TModel">The type of model to diagnose.</typeparam>
    /// <param name="accumulator">The accumulator to apply the filter to.</param>
    /// <returns>A reference to the filtered accumulator, for chaining of further method calls.</returns>
    public static IDiagnosticsAccumulator<TModel> DiagnoseNonHiddenSeverities<TModel>(this IDiagnosticsAccumulator<TModel> accumulator) =>
        accumulator
        .DiagnoseSeverity(DiagnosticSeverity.Info)
        .DiagnoseSeverity(DiagnosticSeverity.Warning)
        .DiagnoseSeverity(DiagnosticSeverity.Error);
    /// <summary>
    /// Applies a filter to the diagnostic accumulator that discards all diagnostics not registered to be diagnosed.
    /// This will prevent diagnostics from being added to the accumulator if they do not have a registered severity.
    /// Calls to this filter may be chained for registering a set of diagnosable severities.
    /// </summary>
    /// <typeparam name="TModel">The type of model to diagnose.</typeparam>
    /// <param name="accumulator">The accumulator to apply the filter to.</param>
    /// <param name="severity">The diagnostics severity to register as diagnosable.</param>
    /// <returns>A reference to the filtered accumulator, for chaining of further method calls.</returns>
    public static IDiagnosticsAccumulator<TModel> DiagnoseSeverity<TModel>(this IDiagnosticsAccumulator<TModel> accumulator, DiagnosticSeverity severity) =>
        accumulator is DiagnosisFilter<TModel> decorator ?
            decorator.AddSeverity(severity) :
            new DiagnosisFilter<TModel>(accumulator, severity);

    /// <summary>
    /// Receives diagnostics providers for generating diagnostics on the model.
    /// </summary>
    /// <typeparam name="TModel">The type of model to diagnose.</typeparam>
    /// <param name="providers">The providers to generate diagnostics with.</param>
    /// <param name="accumulator">The accumulator to receive the providers.</param>
    /// <param name="cancellationToken">A token signalling the diagnosis should terminate.</param>
    /// <returns>A reference tothe accumulator, for chaining of further method calls.</returns>
    public static IDiagnosticsAccumulator<TModel> Receive<TModel>(this IDiagnosticsAccumulator<TModel> accumulator, IEnumerable<IDiagnosticProvider<TModel>> providers, CancellationToken cancellationToken = default) =>
        providers.Aggregate(accumulator, (d, p) => d.Receive(p, cancellationToken));

    /// <summary>
    /// Applies a filter to the diagnostic accumulator that reports only non hidden diagnostics.
    /// This will cause diagnostics with the severity <see cref="DiagnosticSeverity.Hidden"/> not to be reported, however they will still be added to the accumulator.
    /// </summary>
    /// <typeparam name="TModel">The type of model to diagnose.</typeparam>
    /// <param name="accumulator">The accumulator to apply the filter to.</param>
    /// <returns>A reference to the filtered accumulator, for chaining of further method calls.</returns>
    public static IDiagnosticsAccumulator<TModel> ReportNonHiddenSeverities<TModel>(this IDiagnosticsAccumulator<TModel> accumulator) =>
        accumulator
        .ReportSeverity(DiagnosticSeverity.Info)
        .ReportSeverity(DiagnosticSeverity.Warning)
        .ReportSeverity(DiagnosticSeverity.Error);
    /// <summary>
    /// Applies a filter to the diagnostic accumulator that reports only diagnostics with registered severities.
    /// This will cause diagnostics without a registered severity not to be reported, however they will still be added to the accumulator.
    /// Calls to this filter may be chained for registering a set of reportable severities.
    /// </summary>
    /// <typeparam name="TModel">The type of model to diagnose.</typeparam>
    /// <param name="accumulator">The accumulator to apply the filter to.</param>
    /// <param name="severity">The diagnostics severity to register as reportable.</param>
    /// <returns>A reference to the filtered accumulator, for chaining of further method calls.</returns>
    public static IDiagnosticsAccumulator<TModel> ReportSeverity<TModel>(this IDiagnosticsAccumulator<TModel> accumulator, DiagnosticSeverity severity) =>
        accumulator is ReportFilter<TModel> decorator ?
            decorator.AddSeverity(severity) :
            new ReportFilter<TModel>(accumulator, severity);
}
