namespace RhoMicro.CodeAnalysis.UtilityGenerators.Library;

/// <summary>
/// Reports model diagnostics to an accumulator.
/// </summary>
/// <typeparam name="TModel">The type of model to diagnose.</typeparam>
interface IDiagnosticProvider<TModel>
{
    /// <summary>
    /// Diagnoses a model.
    /// </summary>
    /// <param name="model">The model to diagnose.</param>
    /// <param name="accumulator">The accumulator to report diagnostics to.</param>
    /// <param name="cancellationToken">A token signalling the diagnosis should terminate.</param>
    void Diagnose(TModel model, IDiagnosticsAccumulator<TModel> accumulator, CancellationToken cancellationToken = default);
}
