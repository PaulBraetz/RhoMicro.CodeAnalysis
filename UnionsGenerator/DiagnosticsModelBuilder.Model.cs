namespace RhoMicro.CodeAnalysis.UnionsGenerator;

using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics;

sealed partial class DiagnosticsModelBuilder
{
    public sealed class Model
    {
        public Model(IEnumerable<Diagnostic> diagnostics)
        {
            _diagnostics = diagnostics;
            IsError = diagnostics.Any(Extensions.IsError);
        }
        private Model(Boolean isError)
        {
            _diagnostics = Array.Empty<Diagnostic>();
            IsError = isError;
        }

        public readonly static Model Error = new(isError: true);
        public readonly static Model NoError = new(isError: false);

        public readonly Boolean IsError;
        private readonly IEnumerable<Diagnostic> _diagnostics;

        public void AddToContext(SyntaxNodeAnalysisContext context)
        {
            foreach(var diagnostic in _diagnostics)
            {
                context.ReportDiagnostic(diagnostic);
            }
        }
        public void AddToContext(SourceProductionContext context)
        {
            foreach(var diagnostic in _diagnostics)
            {
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
