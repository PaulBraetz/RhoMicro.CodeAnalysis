namespace RhoMicro.CodeAnalysis.UnionsGenerator;

using System.Threading;

readonly struct ModelIntegrationContext<TModel>(
    DiagnosticsModelBuilder diagnostics,
    SourceModelBuilder source,
    CancellationToken cancellationToken,
    TModel model)
{
    public readonly DiagnosticsModelBuilder Diagnostics = diagnostics;
    public readonly SourceModelBuilder Source = source;
    public readonly CancellationToken CancellationToken = cancellationToken;
    public readonly TModel Model = model;
}