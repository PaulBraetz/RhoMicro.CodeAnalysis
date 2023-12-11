namespace RhoMicro.CodeAnalysis.UtilityGenerators.Library;

using Microsoft.CodeAnalysis;

using System;

partial class DiagnosticsAccumulator
{
    internal sealed class DiagnosisFilter<TModel>(IDiagnosticsAccumulator<TModel> decoratedAccumulator, DiagnosticSeverity severity) : IDiagnosticsAccumulator<TModel>
    {
        private readonly IDiagnosticsAccumulator<TModel> _decoratedAccumulator = decoratedAccumulator;
        private readonly HashSet<DiagnosticSeverity> _severities = [severity];

        public IDiagnosticsAccumulator<TModel> AddSeverity(DiagnosticSeverity severity)
        {
            if(_severities.Contains(severity))
            {
                return this;
            }

            var result = new DiagnosisFilter<TModel>(_decoratedAccumulator, severity);
            foreach(var s in _severities)
            {
                _ = result._severities.Add(s);
            }

            return result;
        }

        public Boolean ContainsErrors => _decoratedAccumulator.ContainsErrors;

        public IDiagnosticsAccumulator<TModel> Add(Diagnostic diagnostic) =>
            _severities.Contains(diagnostic.Severity) ?
            _decoratedAccumulator.Add(diagnostic) :
            this;

        public void ReportDiagnostics(Action<Diagnostic> report) => _decoratedAccumulator.ReportDiagnostics(report);
        public IDiagnosticsAccumulator<TModel> Receive(IDiagnosticProvider<TModel> provider, CancellationToken cancellationToken = default)
        {
            _ = _decoratedAccumulator.Receive(provider, cancellationToken);
            return this;
        }
    }
}
