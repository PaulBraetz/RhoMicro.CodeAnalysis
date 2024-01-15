namespace RhoMicro.CodeAnalysis.Library;

using System;

static partial class ExpandingMacroStringBuilder
{
    /// <summary>
    /// Decorates an expanding macro string builder with operators for ease of use.
    /// </summary>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <typeparam name="TSelf">The implementing type.</typeparam>
    public abstract class OperatorsDecoratorBase<TMacro, TSelf> : IExpandingMacroStringBuilder<TMacro>, IEquatable<OperatorsDecoratorBase<TMacro, TSelf>?> where TSelf : OperatorsDecoratorBase<TMacro, TSelf>
    {
        /// <summary>
        /// Intializes a new instance.
        /// </summary>
        /// <param name="decorated">The builder to decorate with operators.</param>
        /// <param name="cancellationToken">The cancellation token to ambiently pass to calls to the decorated builder.</param>
        protected OperatorsDecoratorBase(IExpandingMacroStringBuilder<TMacro> decorated, CancellationToken cancellationToken)
        {
            _decorated = decorated;
            CancellationToken = cancellationToken;
        }

        /// <summary>
        /// The decorated builder.
        /// </summary>
        private readonly IExpandingMacroStringBuilder<TMacro> _decorated;
        /// <summary>
        /// Gets the cancellation token to ambiently pass to calls to the decorated builder.
        /// </summary>
        public CancellationToken CancellationToken { get; }

        protected abstract TSelf GetSelf();
        /// <inheritdoc cref="IExpandingMacroStringBuilder{TMacro}.Append(String)"/>
        public TSelf Append(String value)
        {
            _ = _decorated.Append(value);
            return GetSelf();
        }
        /// <summary>
        /// Applies a mthod to the builder.
        /// </summary>
        /// <param name="method">The method to apply.</param>
        /// <returns>A reference to the builder, for chaining of further method calls.</returns>
        public TSelf Apply(Action<TSelf> method)
        {
            method.Invoke(GetSelf());
            return GetSelf();
        }
        /// <inheritdoc cref="IExpandingMacroStringBuilder{TMacro}.Append(Char)"/>
        public TSelf Append(Char value)
        {
            _ = _decorated.Append(value);
            return GetSelf();
        }
        /// <inheritdoc cref="IExpandingMacroStringBuilder{TMacro}.AppendMacro(TMacro, CancellationToken)"/>
        public TSelf AppendMacro(TMacro macro, CancellationToken cancellationToken)
        {
            _ = _decorated.AppendMacro(macro, cancellationToken);
            return GetSelf();
        }
        /// <inheritdoc cref="IExpandingMacroStringBuilder{TMacro}.Receive(IMacroExpansion{TMacro}, CancellationToken)"/>
        public TSelf Receive(IMacroExpansion<TMacro> provider, CancellationToken cancellationToken)
        {
            _ = _decorated.Receive(provider, cancellationToken);
            return GetSelf();
        }

        /// <inheritdoc/>
        IExpandingMacroStringBuilder<TMacro> IExpandingMacroStringBuilder<TMacro>.Append(String value) => Append(value);
        /// <inheritdoc/>
        IExpandingMacroStringBuilder<TMacro> IExpandingMacroStringBuilder<TMacro>.Append(Char value) => Append(value);
        /// <inheritdoc/>
        IExpandingMacroStringBuilder<TMacro> IExpandingMacroStringBuilder<TMacro>.AppendMacro(TMacro macro, CancellationToken cancellationToken) => AppendMacro(macro, cancellationToken);
        /// <inheritdoc/>
        IExpandingMacroStringBuilder<TMacro> IExpandingMacroStringBuilder<TMacro>.Receive(IMacroExpansion<TMacro> provider, CancellationToken cancellationToken) => Receive(provider, cancellationToken);
        /// <inheritdoc/>
        String IExpandingMacroStringBuilder<TMacro>.Build(CancellationToken cancellationToken) =>
            _decorated.Build(cancellationToken);

        /// <inheritdoc/>
        public String Build() => ((IExpandingMacroStringBuilder<TMacro>)this).Build(CancellationToken);

        /// <inheritdoc/>
        public override String ToString() => _decorated.ToString();
        /// <inheritdoc/>
        public override Boolean Equals(Object? obj) => Equals(obj as OperatorsDecoratorBase<TMacro, TSelf>);
        /// <inheritdoc/>
        public Boolean Equals(OperatorsDecoratorBase<TMacro, TSelf>? other) => other is not null && EqualityComparer<IExpandingMacroStringBuilder<TMacro>>.Default.Equals(_decorated, other._decorated);
        /// <inheritdoc/>
        public override Int32 GetHashCode() => 47476743 + EqualityComparer<IExpandingMacroStringBuilder<TMacro>>.Default.GetHashCode(_decorated);

        /// <summary>
        /// Appends a string to the builder wrapped by the decorator provided.
        /// </summary>
        /// <param name="decorator">The decorator wrapping a builder.</param>
        /// <param name="value">The string to append to the builder wrapped by the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static TSelf operator *(OperatorsDecoratorBase<TMacro, TSelf> decorator, String value) =>
            decorator.Append(value);
        /// <summary>
        /// Appends a character to the builder wrapped by the decorator provided.
        /// </summary>
        /// <param name="decorator">The decorator wrapping a builder.</param>
        /// <param name="value">The character to append to the builder wrapped by the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static TSelf operator *(OperatorsDecoratorBase<TMacro, TSelf> decorator, Char value) =>
            decorator.Append(value);
        /// <summary>
        /// Appends a macro to the builder wrapped by the decorator provided.
        /// </summary>
        /// <param name="decorator">The decorator wrapping a builder.</param>
        /// <param name="macro">The macro to append to the builder wrapped by the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static TSelf operator *(OperatorsDecoratorBase<TMacro, TSelf> decorator, TMacro macro) =>
            decorator.AppendMacro(macro, decorator.CancellationToken);
        /// <summary>
        /// Registers a macro expansion to the builder wrapped by the decorator provided.
        /// </summary>
        /// <param name="decorator">The decorator wrapping a builder.</param>
        /// <param name="expansion">The expansion to be received by the builder wrapped by the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static TSelf operator *(OperatorsDecoratorBase<TMacro, TSelf> decorator, IMacroExpansion<TMacro> expansion) =>
            decorator.Receive(expansion, decorator.CancellationToken);
        /// <summary>
        /// Applies a method to a decorator.
        /// </summary>
        /// <param name="decorator">The decorator to apply a method to.</param>
        /// <param name="action">The method to apply to the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static TSelf operator *(OperatorsDecoratorBase<TMacro, TSelf> decorator, Action<OperatorsDecoratorBase<TMacro, TSelf>> action)
        {
            action.Invoke(decorator);
            return decorator.GetSelf();
        }
        /// <summary>
        /// Applies a sequence of methods to a decorator.
        /// </summary>
        /// <param name="decorator">The decorator to apply a method to.</param>
        /// <param name="actions">The methods to apply to the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static TSelf operator *(OperatorsDecoratorBase<TMacro, TSelf> decorator, IEnumerable<Action<OperatorsDecoratorBase<TMacro, TSelf>>> actions)
        {
            foreach(var a in actions)
            {
                a.Invoke(decorator);
            }

            return decorator.GetSelf();
        }

        /// <summary>
        /// Appends a string, preceded by a new line, to the builder wrapped by the decorator provided.
        /// </summary>
        /// <param name="decorator">The decorator wrapping a builder.</param>
        /// <param name="value">The string to append to the builder wrapped by the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static TSelf operator /(OperatorsDecoratorBase<TMacro, TSelf> decorator, String value)
        {
            _ = decorator.AppendLine().Append(value);
            return decorator.GetSelf();
        }
        /// <summary>
        /// Appends a character, preceded by a new line, to the builder wrapped by the decorator provided.
        /// </summary>
        /// <param name="decorator">The decorator wrapping a builder.</param>
        /// <param name="value">The character to append to the builder wrapped by the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static TSelf operator /(OperatorsDecoratorBase<TMacro, TSelf> decorator, Char value)
        {
            _ = decorator.AppendLine().Append(value);
            return decorator.GetSelf();
        }
        /// <summary>
        /// Appends a macro, preceded by a new line, to the builder wrapped by the decorator provided.
        /// </summary>
        /// <param name="decorator">The decorator wrapping a builder.</param>
        /// <param name="macro">The macro to append to the builder wrapped by the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static TSelf operator /(OperatorsDecoratorBase<TMacro, TSelf> decorator, TMacro macro)
        {
            _ = decorator.AppendLine().AppendMacro(macro);
            return decorator.GetSelf();
        }
        /// <summary>
        /// Applies a method to a decorator, appending a new line before applying the method.
        /// </summary>
        /// <param name="decorator">The decorator to apply a method to.</param>
        /// <param name="action">The method to apply to the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static TSelf operator /(OperatorsDecoratorBase<TMacro, TSelf> decorator, Action<OperatorsDecoratorBase<TMacro, TSelf>> action)
        {
            _ = decorator.AppendLine();
            action.Invoke(decorator);
            return decorator.GetSelf();
        }
        /// <summary>
        /// Applies a sequence of methods to a decorator, appending a new line before applying a method.
        /// </summary>
        /// <param name="decorator">The decorator to apply a method to.</param>
        /// <param name="actions">The methods to apply to the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static TSelf operator /(OperatorsDecoratorBase<TMacro, TSelf> decorator, IEnumerable<Action<OperatorsDecoratorBase<TMacro, TSelf>>> actions)
        {
            foreach(var a in actions)
            {
                _ = decorator.AppendLine();
                a.Invoke(decorator);
            }

            return decorator.GetSelf();
        }

        /// <summary>
        /// Appends a string, followed by a new line, to the builder wrapped by the decorator provided.
        /// </summary>
        /// <param name="decorator">The decorator wrapping a builder.</param>
        /// <param name="value">The string to append to the builder wrapped by the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static TSelf operator %(OperatorsDecoratorBase<TMacro, TSelf> decorator, String value)
        {
            _ = decorator.AppendLine(value);
            return decorator.GetSelf();
        }
        /// <summary>
        /// Appends a character, followed by a new line, to the builder wrapped by the decorator provided.
        /// </summary>
        /// <param name="decorator">The decorator wrapping a builder.</param>
        /// <param name="value">The character to append to the builder wrapped by the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static TSelf operator %(OperatorsDecoratorBase<TMacro, TSelf> decorator, Char value)
        {
            _ = decorator.AppendLine(value);
            return decorator.GetSelf();
        }
        /// <summary>
        /// Appends a macro, followed by a new line, to the builder wrapped by the decorator provided.
        /// </summary>
        /// <param name="decorator">The decorator wrapping a builder.</param>
        /// <param name="macro">The macro to append to the builder wrapped by the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static TSelf operator %(OperatorsDecoratorBase<TMacro, TSelf> decorator, TMacro macro)
        {
            _ = decorator.AppendMacro(macro, decorator.CancellationToken).AppendLine();
            return decorator.GetSelf();
        }
        /// <summary>
        /// Applies a method to a decorator, appending a new line after applying the method.
        /// </summary>
        /// <param name="decorator">The decorator to apply a method to.</param>
        /// <param name="action">The method to apply to the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static TSelf operator %(OperatorsDecoratorBase<TMacro, TSelf> decorator, Action<OperatorsDecoratorBase<TMacro, TSelf>> action)
        {
            action.Invoke(decorator);
            _ = decorator.AppendLine();
            return decorator.GetSelf();
        }
        /// <summary>
        /// Applies a sequence of methods to a decorator, appending a new line after applying a method.
        /// </summary>
        /// <param name="decorator">The decorator to apply a method to.</param>
        /// <param name="actions">The methods to apply to the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static TSelf operator %(OperatorsDecoratorBase<TMacro, TSelf> decorator, IEnumerable<Action<OperatorsDecoratorBase<TMacro, TSelf>>> actions)
        {
            foreach(var a in actions)
            {
                a.Invoke(decorator);
                _ = decorator.AppendLine();
            }

            return decorator.GetSelf();
        }
    }
}
