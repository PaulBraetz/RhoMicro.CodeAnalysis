namespace RhoMicro.CodeAnalysis.UtilityGenerators.Library;

/// <summary>
/// Contains factory and extension methods for diagnostic providers.
/// </summary>
static partial class DiagnosticProvider
{
    /// <summary>
    /// Creates a new diagnostic provider.
    /// </summary>
    /// <typeparam name="TModel">The type of model to analyze.</typeparam>
    /// <param name="strategy">Th estrategy to utilize when diagnosing models.</param>
    /// <returns>A new diagnostic provider.</returns>
    public static IDiagnosticProvider<TModel> Create<TModel>(Action<TModel, IDiagnosticsAccumulator<TModel>, CancellationToken> strategy) =>
        new Strategy<TModel>(strategy);
    /// <summary>
    /// Creates a new diagnostic provider.
    /// </summary>
    /// <typeparam name="TModel">The type of model to analyze.</typeparam>
    /// <param name="strategy">Th estrategy to utilize when diagnosing models.</param>
    /// <returns>A new diagnostic provider.</returns>
    public static IDiagnosticProvider<TModel> Create<TModel>(Action<TModel, IDiagnosticsAccumulator<TModel>> strategy) =>
        new Strategy<TModel>((model, diagnostics, token) =>
        {
            token.ThrowIfCancellationRequested();
            strategy.Invoke(model, diagnostics);
        });
}