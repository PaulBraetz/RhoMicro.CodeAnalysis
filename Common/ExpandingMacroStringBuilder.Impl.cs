namespace RhoMicro.CodeAnalysis.Common;

using System;
using System.Text;

partial class ExpandingMacroStringBuilder
{
    /// <summary>
    /// Default implementation of <see cref="IExpandingMacroStringBuilder{TMacro}"/>.
    /// </summary>
    /// <typeparam name="TMacro">The type of macro to support expansion for.</typeparam>
    internal sealed partial class Impl<TMacro> : IExpandingMacroStringBuilder<TMacro>
    {
        private Impl(
            Boolean isRoot,
            Queue<StringOrChar, TMacro> template,
            ExpansionObserver expansionObserver,
            Dictionary<TMacro, IMacroExpansion<TMacro>> expansionMap,
            StringBuilder builder)
        {
            _isRoot = isRoot;
            _template = template;
            _expansionObserver = expansionObserver;
            _expansionMap = expansionMap;
            _builder = builder;
        }

        private readonly Queue<StringOrChar, TMacro> _template;
        private readonly Dictionary<TMacro, IMacroExpansion<TMacro>> _expansionMap;
        private readonly ExpansionObserver _expansionObserver;
        private readonly StringBuilder _builder;
        private readonly Boolean _isRoot;

        /// <summary>
        /// Creates a new instance of the builder.
        /// </summary>
        /// <returns>A new builder instance.</returns>
        public static IExpandingMacroStringBuilder<TMacro> Create() =>
            new Impl<TMacro>(
                isRoot: true,
                template: new(),
                expansionObserver: ExpansionObserver.Create(),
                expansionMap: [],
                builder: new());
        private Impl<TMacro> Derive(Queue<StringOrChar, TMacro> template) =>
            new(
                isRoot: false,
                template: template,
                expansionObserver: _expansionObserver.Derive(),
                expansionMap: _expansionMap,
                builder: new());
        private Impl<TMacro> Clone()
        {
            return new(
                isRoot: true,
                template: _template.Clone(),
                expansionObserver: _expansionObserver.Clone(),
                expansionMap: _expansionMap,
                builder: _builder);
        }

        /// <inheritdoc/>
        public String Build(CancellationToken cancellationToken = default)
        {
            var clone = Clone();
            clone.Expand(cancellationToken);
            clone.Reduce(cancellationToken);
            if(clone._template.ContainsMacros)
            {
                var macros = _template.GetMacros();
                throw new UnexpandedMacrosException<TMacro>(macros);
            } else
            {
                return clone._builder.ToString();
            }
        }
        /// <inheritdoc/>
        public IExpandingMacroStringBuilder<TMacro> Append(String value)
        {
            _ = _template.EnqueueValue(value);
            Reduce(default);
            return this;
        }
        /// <inheritdoc/>
        public IExpandingMacroStringBuilder<TMacro> Append(Char value)
        {
            _ = _template.EnqueueValue(value);
            Reduce(default);
            return this;
        }
        /// <inheritdoc/>
        public IExpandingMacroStringBuilder<TMacro> AppendMacro(TMacro macro, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _ = _template.EnqueueMacro(macro);
            if(_expansionMap.TryGetValue(macro, out var expansion))
            {
                Expand(expansion, cancellationToken);
                Reduce(cancellationToken);
            }

            return this;
        }
        /// <inheritdoc/>
        public IExpandingMacroStringBuilder<TMacro> Receive(IMacroExpansion<TMacro> provider, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if(!_isRoot)
            {
                //TODO
                throw new NotImplementedException("Providing macro expansions inside of macro expansions is not implemented yet.");
            }

            _expansionMap[provider.Macro] = provider;
            Expand(provider, cancellationToken);
            Reduce(cancellationToken);

            return this;
        }

        private void Expand(CancellationToken cancellationToken)
        {
            var expansions = _template.UnexpandedMacros()
                .Select(m => (HasExpansion: _expansionMap.TryGetValue(m, out var e), Expansion: e))
                .Where(t => t.HasExpansion);

            foreach(var (_, expansion) in expansions)
            {
                Expand(expansion, cancellationToken);
            }
        }
        private void Expand(IMacroExpansion<TMacro> expansion, CancellationToken cancellationToken)
        {
            _expansionObserver.NotifyExpansionStart(expansion.Macro);
            try
            {
                ExpansionObserver derivedObserver;
                do
                {
                    derivedObserver = _expansionObserver.Derive();
                    _ = _template.Expand(expansion.Macro, q => expansion.Expand(Derive(template: q), cancellationToken));
                } while(derivedObserver.Expanded);
            } finally
            {
                _expansionObserver.NotifyExpansionEnd(expansion.Macro);
            }
        }

        private void Reduce(CancellationToken cancellationToken)
        {
            if(!_isRoot)
            {
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var values = _template.DequeueValues();
            foreach(var value in values)
            {
                cancellationToken.ThrowIfCancellationRequested();

                _ = value.IsChar ?
                    _builder.Append(value.Char) :
                    _builder.Append(value.String);
            }
        }

        /// <inheritdoc/>
        public override String ToString() => $"({(_builder.Length > 0 ? _builder.ToString() : String.Empty)})-{_template}";
    }
}
