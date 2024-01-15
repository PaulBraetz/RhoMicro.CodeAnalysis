namespace RhoMicro.CodeAnalysis.Library;

partial class DiagnosticProvider
{
    internal sealed class Strategy<TModel>(Action<TModel, IDiagnosticsAccumulator<TModel>, CancellationToken> strategy) : IDiagnosticProvider<TModel>
    {
        private readonly Action<TModel, IDiagnosticsAccumulator<TModel>, CancellationToken> _strategy = strategy;

        public void Diagnose(TModel model, IDiagnosticsAccumulator<TModel> accumulator, CancellationToken cancellationToken = default) => 
            _strategy.Invoke(model, accumulator, cancellationToken);
    }
}