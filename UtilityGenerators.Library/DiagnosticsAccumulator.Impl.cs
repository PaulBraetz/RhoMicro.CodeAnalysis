namespace RhoMicro.CodeAnalysis.UtilityGenerators.Library;

using Microsoft.CodeAnalysis;

using System;

partial class DiagnosticsAccumulator
{
    internal sealed class Impl<TModel>(TModel model) :
    IDiagnosticsAccumulator<TModel>
    {
        private readonly List<Diagnostic> _diagnostics = [];
        private readonly TModel _model = model;

        public Boolean ContainsErrors { get; private set; }

        public IDiagnosticsAccumulator<TModel> Add(Diagnostic diagnostic)
        {
            _diagnostics.Add(diagnostic);

            if(diagnostic.Severity == DiagnosticSeverity.Error)
            {
                ContainsErrors = true;
            }

            return this;
        }

        public IDiagnosticsAccumulator<TModel> Receive(IDiagnosticProvider<TModel> provider, CancellationToken cancellationToken = default)
        {
            provider.Diagnose(_model, this, cancellationToken);
            return this;
        }
        public void ReportDiagnostics(Action<Diagnostic> report)
        {
            foreach(var diagnostic in _diagnostics)
            {
                report.Invoke(diagnostic);
            }
        }
    }
}
