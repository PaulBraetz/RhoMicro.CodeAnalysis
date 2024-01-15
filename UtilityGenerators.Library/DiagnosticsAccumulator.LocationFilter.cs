namespace RhoMicro.CodeAnalysis.Library;

using Microsoft.CodeAnalysis;

using System;

static partial class DiagnosticsAccumulator
{
    sealed class LocationFilter<TModel>(IDiagnosticsAccumulator<TModel> proxied) : IDiagnosticsAccumulator<TModel>
    {
        private readonly IDiagnosticsAccumulator<TModel> _proxied = proxied;

        public Boolean ContainsErrors => _proxied.ContainsErrors;

        public IDiagnosticsAccumulator<TModel> Add(Diagnostic diagnostic)
        {
            _ = _proxied.Add(diagnostic);
            return this;
        }

        public void ReportDiagnostics(Action<Diagnostic> report) => _proxied.ReportDiagnostics(d =>
        {
            if(d.Location == Location.None)
                report.Invoke(d);
        });
        public IDiagnosticsAccumulator<TModel> Receive(IDiagnosticProvider<TModel> provider, CancellationToken cancellationToken = default)
        {
            _ = _proxied.Receive(provider, cancellationToken);
            return this;
        }
    }
}
