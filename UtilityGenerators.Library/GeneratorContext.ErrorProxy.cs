namespace RhoMicro.CodeAnalysis.UtilityGenerators.Library;

using System;
using System.Collections.Generic;

partial class GeneratorContext
{
    /// <summary>
    /// Proxies operations on an expanding macro string builder against error diagnostics. 
    /// Operations will only be passed through if no errors have been diagnosed 
    /// to the diagnostics accumulator provided.
    /// </summary>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <typeparam name="TModel">The type of model whose diagnostics to query.</typeparam>
    /// <param name="proxied">The expanding macro string builder to proxy.</param>
    /// <param name="diagnostics">The diagnostics accumulator to query for errors when intercepting operations.</param>
    internal sealed class ErrorProxy<TMacro, TModel>(
        IExpandingMacroStringBuilder<TMacro> proxied,
        IDiagnosticsAccumulator<TModel> diagnostics) :
        IExpandingMacroStringBuilder<TMacro>,
        IEquatable<ErrorProxy<TMacro, TModel>?>
    {
        private readonly IExpandingMacroStringBuilder<TMacro> _proxied = proxied;
        private readonly IDiagnosticsAccumulator<TModel> _diagnostics = diagnostics;

        /// <inheritdoc/>
        public String Build(CancellationToken cancellationToken = default) => _proxied.Build(cancellationToken);
        /// <inheritdoc/>
        public IExpandingMacroStringBuilder<TMacro> Append(String value)
        {
            if(!_diagnostics.ContainsErrors)
            {
                _ = _proxied.Append(value);
            }

            return this;
        }
        /// <inheritdoc/>
        public IExpandingMacroStringBuilder<TMacro> Append(Char value)
        {
            if(!_diagnostics.ContainsErrors)
            {
                _ = _proxied.Append(value);
            }

            return this;
        }
        /// <inheritdoc/>
        public IExpandingMacroStringBuilder<TMacro> AppendMacro(TMacro macro, CancellationToken cancellationToken = default)
        {
            if(!_diagnostics.ContainsErrors)
            {
                _ = _proxied.AppendMacro(macro, cancellationToken);
            }

            return this;
        }
        /// <inheritdoc/>
        public IExpandingMacroStringBuilder<TMacro> Receive(IMacroExpansion<TMacro> provider, CancellationToken cancellationToken = default)
        {
            if(!_diagnostics.ContainsErrors)
            {
                _ = _proxied.Receive(provider, cancellationToken);
            }

            return this;
        }
        /// <inheritdoc/>
        public override Boolean Equals(Object? obj) =>
            Equals(obj as ErrorProxy<TMacro, TModel>);
        /// <inheritdoc/>
        public Boolean Equals(ErrorProxy<TMacro, TModel>? other) =>
            other is not null && EqualityComparer<IExpandingMacroStringBuilder<TMacro>>.Default.Equals(_proxied, other._proxied);
        /// <inheritdoc/>
        public override Int32 GetHashCode() =>
            -305811549 + EqualityComparer<IExpandingMacroStringBuilder<TMacro>>.Default.GetHashCode(_proxied);
    }
}
