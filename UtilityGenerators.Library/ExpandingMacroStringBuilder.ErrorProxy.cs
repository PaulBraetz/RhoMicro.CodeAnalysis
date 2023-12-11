namespace RhoMicro.CodeAnalysis.UtilityGenerators.Library;

using System;

partial class ExpandingMacroStringBuilder
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
        /// <summary>
        /// Creates a new proxy for the builder provided, if it is not already an error proxy using the diagnostics provided.
        /// </summary>
        /// <param name="builder">The builder to conditionally proxy.</param>
        /// <param name="diagnostics">The diagnostics based on which to intercept appendices.</param>
        /// <returns>
        /// The new proxy if the builder provided is not already an error proxy using the diagnostics 
        /// provided; otherwise, a reference to the builder, for chaining of further method calls.
        /// </returns>
        public static IExpandingMacroStringBuilder<TMacro> Apply(IExpandingMacroStringBuilder<TMacro> builder, IDiagnosticsAccumulator<TModel> diagnostics)
        {
            if(builder is not ErrorProxy<TMacro, TModel> proxy || 
               !EqualityComparer<IDiagnosticsAccumulator<TModel>>.Default.Equals(proxy._diagnostics, diagnostics))
            {
                proxy = new ErrorProxy<TMacro, TModel>(builder, diagnostics);
            }

            return proxy;
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
