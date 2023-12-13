namespace RhoMicro.CodeAnalysis.UtilityGenerators.Library;

using System;

static partial class ExpandingMacroStringBuilder
{
    /// <summary>
    /// Decorates an expanding macro string builder with operators for ease of use.
    /// </summary>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <typeparam name="TSelf">The implementing type.</typeparam>
    public abstract class OperatorsDecoratorBase<TMacro, TSelf> : IExpandingMacroStringBuilder<TMacro>
        where TSelf : OperatorsDecoratorBase<TMacro, TSelf>
    {
        /// <summary>
        /// Intializes a new instance.
        /// </summary>
        /// <param name="decorated">The builder to decorate with operators.</param>
        /// <param name="cancellationToken">The cancellation token to ambiently pass to calls to the decorated builder.</param>
        protected OperatorsDecoratorBase(IExpandingMacroStringBuilder<TMacro> decorated, CancellationToken cancellationToken)
        {
            Decorated = decorated;
            CancellationToken = cancellationToken;
        }

        /// <summary>
        /// Gets the decorated builder.
        /// </summary>
        public IExpandingMacroStringBuilder<TMacro> Decorated { get; }
        /// <summary>
        /// Gets the cancellation token to ambiently pass to calls to the decorated builder.
        /// </summary>
        public CancellationToken CancellationToken { get; }

        protected abstract TSelf GetSelf();
        private TSelf Append(String value)
        {
            _ = Decorated.Append(value);
            return GetSelf();
        }
        private TSelf Append(Char value)
        {
            _ = Decorated.Append(value);
            return GetSelf();
        }
        private TSelf AppendMacro(TMacro macro, CancellationToken cancellationToken)
        {
            _ = Decorated.AppendMacro(macro, cancellationToken);
            return GetSelf();
        }
        private TSelf Receive(IMacroExpansion<TMacro> provider, CancellationToken cancellationToken)
        {
            _ = Decorated.Receive(provider, cancellationToken);
            return GetSelf();
        }

        IExpandingMacroStringBuilder<TMacro> IExpandingMacroStringBuilder<TMacro>.Append(String value) => Append(value);
        IExpandingMacroStringBuilder<TMacro> IExpandingMacroStringBuilder<TMacro>.Append(Char value) => Append(value);
        IExpandingMacroStringBuilder<TMacro> IExpandingMacroStringBuilder<TMacro>.AppendMacro(TMacro macro, CancellationToken cancellationToken) => AppendMacro(macro, cancellationToken);
        IExpandingMacroStringBuilder<TMacro> IExpandingMacroStringBuilder<TMacro>.Receive(IMacroExpansion<TMacro> provider, CancellationToken cancellationToken) => Receive(provider, cancellationToken);
        String IExpandingMacroStringBuilder<TMacro>.Build(CancellationToken cancellationToken) =>
            Decorated.Build(cancellationToken);

        /// <summary>
        /// Appends a string to the builder wrapped by the decorator provided.
        /// </summary>
        /// <param name="decorator">The decorator wrapping a builder.</param>
        /// <param name="value">The string to append to the builder wrapped by the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static TSelf operator +(OperatorsDecoratorBase<TMacro, TSelf> decorator, String value) =>
            decorator.Append(value);
        /// <summary>
        /// Appends a character to the builder wrapped by the decorator provided.
        /// </summary>
        /// <param name="decorator">The decorator wrapping a builder.</param>
        /// <param name="value">The character to append to the builder wrapped by the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static TSelf operator +(OperatorsDecoratorBase<TMacro, TSelf> decorator, Char value) =>
            decorator.Append(value);
        /// <summary>
        /// Appends an appendix to the builder wrapped by the decorator provided.
        /// </summary>
        /// <param name="decorator">The decorator wrapping a builder.</param>
        /// <param name="value">The appendix to append to the builder wrapped by the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static TSelf operator +(OperatorsDecoratorBase<TMacro, TSelf> decorator, Appendix<TMacro> value)
        {
            _ = decorator.Decorated.Append(value, decorator.CancellationToken);
            return decorator.GetSelf();
        }
        /// <summary>
        /// Appends a macro to the builder wrapped by the decorator provided.
        /// </summary>
        /// <param name="decorator">The decorator wrapping a builder.</param>
        /// <param name="macro">The macro to append to the builder wrapped by the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static TSelf operator +(OperatorsDecoratorBase<TMacro, TSelf> decorator, TMacro macro) =>
            decorator.AppendMacro(macro, decorator.CancellationToken);
        /// <summary>
        /// Registers a macro expansion to the builder wrapped by the decorator provided.
        /// </summary>
        /// <param name="decorator">The decorator wrapping a builder.</param>
        /// <param name="expansion">The expansion to be received by the builder wrapped by the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static TSelf operator +(OperatorsDecoratorBase<TMacro, TSelf> decorator, IMacroExpansion<TMacro> expansion) =>
            decorator.Receive(expansion, decorator.CancellationToken);

        /// <summary>
        /// Appends a string, followed by a newline, to the builder wrapped by the decorator provided.
        /// </summary>
        /// <param name="decorator">The decorator wrapping a builder.</param>
        /// <param name="value">The string to append to the builder wrapped by the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static TSelf operator -(OperatorsDecoratorBase<TMacro, TSelf> decorator, String value)
        {
            _ = decorator.Decorated.AppendLine(value);
            return decorator.GetSelf();
        }
        /// <summary>
        /// Appends a character, followed by a newline, to the builder wrapped by the decorator provided.
        /// </summary>
        /// <param name="decorator">The decorator wrapping a builder.</param>
        /// <param name="value">The character to append to the builder wrapped by the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static TSelf operator -(OperatorsDecoratorBase<TMacro, TSelf> decorator, Char value)
        {
            _ = decorator.Decorated.AppendLine(value);
            return decorator.GetSelf();
        }
        /// <summary>
        /// Appends an appendix, followed by a new line, to the builder wrapped by the decorator provided.
        /// </summary>
        /// <param name="decorator">The decorator wrapping a builder.</param>
        /// <param name="value">The appendix to append to the builder wrapped by the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static TSelf operator -(OperatorsDecoratorBase<TMacro, TSelf> decorator, Appendix<TMacro> value)
        {
            _ = decorator.Decorated.Append(value, decorator.CancellationToken).AppendLine();
            return decorator.GetSelf();
        }
        /// <summary>
        /// Applies a method to a decorator.
        /// </summary>
        /// <param name="decorator">The decorator to apply a method to.</param>
        /// <param name="action">The method to apply to the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static TSelf operator +(OperatorsDecoratorBase<TMacro, TSelf> decorator, Action<OperatorsDecoratorBase<TMacro, TSelf>> action)
        {
            action.Invoke(decorator);
            return decorator.GetSelf();
        }
    }
}
