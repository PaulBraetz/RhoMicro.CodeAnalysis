namespace RhoMicro.CodeAnalysis.UtilityGenerators.Library;

using System;

static partial class ExpandingMacroStringBuilder
{
    /// <summary>
    /// Decorates an expanding macro string builder with operators for ease of use.
    /// </summary>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    public partial class OperatorsDecorator<TMacro> : OperatorsDecoratorBase<TMacro, OperatorsDecorator<TMacro>>
    {
        private OperatorsDecorator(IExpandingMacroStringBuilder<TMacro> decorated, CancellationToken cancellationToken) : base(decorated, cancellationToken)
        {
        }

        /// <summary>
        /// Creates a new decorator or returns the provided builder if it already is a decorator.
        /// </summary>
        /// <param name="decorated">The builder to potentially wrap.</param>
        /// <param name="cancellationToken">The cancellation token to ambiently pass to calls to the decorated builder.</param>
        /// <returns>The potentially new decorator.</returns>
        public static OperatorsDecorator<TMacro> Create(IExpandingMacroStringBuilder<TMacro> decorated, CancellationToken cancellationToken) =>
            decorated is OperatorsDecorator<TMacro> decorator ?
            decorator :
            new(decorated, cancellationToken);
        protected override OperatorsDecorator<TMacro> GetSelf() => this;
    }
    /// <summary>
    /// Decorates an expanding macro string builder with operators for ease of use.
    /// </summary>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <typeparam name="TModel">The type of model to support appendices for.</typeparam>
    public partial class OperatorsDecorator<TMacro, TModel> : OperatorsDecoratorBase<TMacro, OperatorsDecorator<TMacro, TModel>>
    {
        private OperatorsDecorator(IExpandingMacroStringBuilder<TMacro> decorated, CancellationToken cancellationToken) : base(decorated, cancellationToken)
        {
        }

        /// <summary>
        /// Creates a new decorator or returns the provided builder if it already is a decorator.
        /// </summary>
        /// <param name="decorated">The builder to potentially wrap.</param>
        /// <param name="cancellationToken">The cancellation token to ambiently pass to calls to the decorated builder.</param>
        /// <returns>The potentially new decorator.</returns>
        public static OperatorsDecorator<TMacro, TModel> Create(IExpandingMacroStringBuilder<TMacro> decorated, CancellationToken cancellationToken) =>
            decorated is OperatorsDecorator<TMacro, TModel> decorator ?
            decorator :
            new(decorated, cancellationToken);
        protected override OperatorsDecorator<TMacro, TModel> GetSelf() => this;

        /// <summary>
        /// Appends an appendix to the builder wrapped by the decorator provided.
        /// </summary>
        /// <param name="decorator">The decorator wrapping a builder.</param>
        /// <param name="value">
        /// The appendix to append to the builder wrapped by the decorator, as well as the model to provide to it.
        /// </param>
        /// <returns>A reference to the decorator.</returns>
        public static OperatorsDecorator<TMacro, TModel> operator +(OperatorsDecorator<TMacro, TModel> decorator, (Appendix<TMacro, TModel> appendix, TModel model) value)
        {
            _ = decorator.Decorated.Append(value.appendix, value.model, decorator.CancellationToken);
            return decorator;
        }
        /// <summary>
        /// Appends an appendix, followed by a new line, to the builder wrapped by the decorator provided.
        /// </summary>
        /// <param name="decorator">The decorator wrapping a builder.</param>
        /// <param name="value">
        /// The appendix to append to the builder wrapped by the decorator, as well as the model to provide to it.
        /// </param>
        /// <returns>A reference to the decorator.</returns>
        public static OperatorsDecorator<TMacro, TModel> operator -(OperatorsDecorator<TMacro, TModel> decorator, (Appendix<TMacro, TModel> appendix, TModel model) value)
        {
            _ = decorator.Decorated.Append(value.appendix, value.model, decorator.CancellationToken).AppendLine();
            return decorator;
        }
        /// <summary>
        /// Applies a method to a decorator.
        /// </summary>
        /// <param name="decorator">The decorator to apply a method to.</param>
        /// <param name="action">The method to apply to the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static OperatorsDecorator<TMacro, TModel> operator +(OperatorsDecorator<TMacro, TModel> decorator, Action<OperatorsDecorator<TMacro, TModel>> action)
        {
            action.Invoke(decorator);
            return decorator;
        }
    }
}
