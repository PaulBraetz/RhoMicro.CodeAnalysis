namespace RhoMicro.CodeAnalysis.UtilityGenerators.Library;

using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Net.Http.Headers;

partial class GeneratorContext
{
    internal sealed class Impl<TMacro, TModel>(
    IExpandingMacroStringBuilder<TMacro> builder,
    IDiagnosticsAccumulator<TModel> diagnostics,
    TModel model) :
        IEquatable<Impl<TMacro, TModel>?>, IGeneratorContext<TMacro, TModel>
    {
        /// <summary>
        /// The underlying expanding macro string builder.
        /// </summary>
        private IExpandingMacroStringBuilder<TMacro> _sourceText = builder;
        /// <summary>
        /// The underlying diagnostics accumulator.
        /// </summary>
        private IDiagnosticsAccumulator<TModel> _diagnostics = diagnostics;
        /// <inheritdoc/>
        public TModel Model { get; } = model;

        public IGeneratorContext<TMacro, TModel> ApplyToSource(
            Func<IExpandingMacroStringBuilder<TMacro>, TModel, CancellationToken, IExpandingMacroStringBuilder<TMacro>> build,
            CancellationToken cancellationToken)
        {
            _sourceText = build.Invoke(_sourceText, Model, cancellationToken);
            return this;
        }
        public IGeneratorContext<TMacro, TModel> ApplyToSource(
            Action<IExpandingMacroStringBuilder<TMacro>, TModel, CancellationToken> build,
            CancellationToken cancellationToken)
        {
            build.Invoke(_sourceText, Model, cancellationToken);
            return this;
        }
        public IGeneratorContext<TMacro, TModel> ApplyToDiagnostics(
            Func<IDiagnosticsAccumulator<TModel>, TModel, CancellationToken, IDiagnosticsAccumulator<TModel>> diagnose,
            CancellationToken cancellationToken)
        {
            _diagnostics = diagnose.Invoke(_diagnostics, Model, cancellationToken);
            return this;
        }
        public IGeneratorContext<TMacro, TModel> ApplyToDiagnostics(
            Action<IDiagnosticsAccumulator<TModel>, TModel, CancellationToken> diagnose,
            CancellationToken cancellationToken)
        {
            diagnose.Invoke(_diagnostics, Model, cancellationToken);
            return this;
        }

        /// <inheritdoc/>
        public IGeneratorContext<TMacro, TModel> Receive<TProvider>(TProvider provider)
            where TProvider : IMacroExpansion<TMacro>, IDiagnosticProvider<TModel>
        {
            _ = _diagnostics.Receive(provider);
            _ = _sourceText.Receive(provider);
            return this;
        }
        /// <inheritdoc/>
        public IGeneratorContext<TMacro, TModel> Receive<TProvider>()
            where TProvider : IMacroExpansion<TMacro>, IDiagnosticProvider<TModel>, new() =>
            Receive(new TProvider());

        /// <inheritdoc/>
        public GeneratorContextBuildResult<TModel> BuildSource(CancellationToken cancellationToken)
        {
            String sourceText;
            var diagnostics = DiagnosticsAccumulator.Create(Model);
            _diagnostics.ReportDiagnostics(d => diagnostics.Add(d));

            try
            {
                sourceText = _sourceText.Build(cancellationToken);
            } catch(Exception ex)
            {
                _ = _diagnostics.Add(
                    Diagnostic.Create(
                        BuildErrorDiagnosticDescriptor,
                        Location.None,
                        ex));
                sourceText = String.Empty;
            }

            var result = new GeneratorContextBuildResult<TModel>(sourceText, diagnostics);

            return result;
        }

        /// <inheritdoc/>
        public override Boolean Equals(Object? obj) => Equals(obj as Impl<TMacro, TModel>);
        /// <inheritdoc/>
        public Boolean Equals(Impl<TMacro, TModel>? other) => other is not null && EqualityComparer<TModel>.Default.Equals(Model, other.Model);
        /// <inheritdoc/>
        public override Int32 GetHashCode() => -623947254 + EqualityComparer<TModel>.Default.GetHashCode(Model);
    }
}
