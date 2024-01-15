namespace RhoMicro.CodeAnalysis.Library;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using System;
using System.Collections.Generic;

partial class ExpandingMacroStringBuilder
{
    /// <summary>
    /// Formats the build output of the decorated expanding macro string builder.
    /// </summary>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <param name="builder">The expanding macro string builder to decorate.</param>
    /// <param name="options">The options to use when formatting generated source text.</param>
    internal sealed class CSharpFormatter<TMacro>(
        IExpandingMacroStringBuilder<TMacro> builder,
        CSharpBuilderFormattingOptions options) :
        IExpandingMacroStringBuilder<TMacro>,
        IEquatable<CSharpFormatter<TMacro>?>
    {
        private readonly IExpandingMacroStringBuilder<TMacro> _decoratedBuilder = builder;
        private readonly CSharpBuilderFormattingOptions _options = options;

        /// <summary>
        /// Creates a new formatting decorator decorating the same builder, but using the provided options.
        /// </summary>
        /// <param name="newOptions">The options to be used by the new formatting decorator.</param>
        /// <returns>A new formatting decorator.</returns>
        public CSharpFormatter<TMacro> WithOptions(CSharpBuilderFormattingOptions newOptions) =>
            new(_decoratedBuilder, newOptions);

        /// <inheritdoc/>
        public IExpandingMacroStringBuilder<TMacro> Append(String value)
        {
            _ = _decoratedBuilder.Append(value);

            return this;
        }

        /// <inheritdoc/>
        public IExpandingMacroStringBuilder<TMacro> Append(Char value)
        {
            _ = _decoratedBuilder.Append(value);

            return this;
        }

        /// <inheritdoc/>
        public IExpandingMacroStringBuilder<TMacro> AppendMacro(TMacro macro, CancellationToken cancellationToken = default)
        {
            _ = _decoratedBuilder.AppendMacro(macro, cancellationToken);

            return this;
        }

        /// <inheritdoc/>
        public IExpandingMacroStringBuilder<TMacro> Receive(IMacroExpansion<TMacro> writer, CancellationToken cancellationToken = default)
        {
            _ = _decoratedBuilder.Receive(writer, cancellationToken);

            return this;
        }

        /// <inheritdoc/>
        public String Build(CancellationToken cancellationToken = default) =>
            CSharpSyntaxTree.ParseText(
                text: _decoratedBuilder.Build(cancellationToken),
                options: _options.ParseOptions,
                cancellationToken: cancellationToken)
                .GetRoot(cancellationToken)
                .NormalizeWhitespace(
                    indentation: _options.Indentation,
                    eol: _options.EndOfLine,
                    elasticTrivia: _options.ElasticTrivia)
                .SyntaxTree
                .GetText(cancellationToken)
                .ToString();
        /// <inheritdoc/>
        public override String ToString() => _decoratedBuilder.ToString();
        /// <inheritdoc/>
        public override Boolean Equals(Object? obj) => Equals(obj as CSharpFormatter<TMacro>);
        /// <inheritdoc/>
        public Boolean Equals(CSharpFormatter<TMacro>? other) =>
            other is not null &&
            EqualityComparer<IExpandingMacroStringBuilder<TMacro>>.Default.Equals(_decoratedBuilder, other._decoratedBuilder);
        /// <inheritdoc/>
        public override Int32 GetHashCode() => 1679842278 + EqualityComparer<IExpandingMacroStringBuilder<TMacro>>.Default.GetHashCode(_decoratedBuilder);
    }
}
