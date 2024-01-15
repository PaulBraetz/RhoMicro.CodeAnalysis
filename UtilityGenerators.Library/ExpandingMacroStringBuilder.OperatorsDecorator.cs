namespace RhoMicro.CodeAnalysis.Library;

using System;

static partial class ExpandingMacroStringBuilder
{
    /// <summary>
    /// Decorates an expanding macro string builder with operators for ease of use.
    /// </summary>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    public sealed partial class OperatorsDecorator<TMacro> : OperatorsDecoratorBase<TMacro, OperatorsDecorator<TMacro>>
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
    public sealed partial class OperatorsDecorator<TMacro, TModel> : OperatorsDecoratorBase<TMacro, OperatorsDecorator<TMacro, TModel>>
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
        /// Applies a method to a decorator.
        /// </summary>
        /// <param name="decorator">The decorator to apply a method to.</param>
        /// <param name="action">The method to apply to the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static OperatorsDecorator<TMacro, TModel> operator *(OperatorsDecorator<TMacro, TModel> decorator, Action<OperatorsDecorator<TMacro, TModel>> action)
        {
            action.Invoke(decorator);
            return decorator;
        }
        /// <summary>
        /// Applies a sequence of methods to a decorator.
        /// </summary>
        /// <param name="decorator">The decorator to apply a method to.</param>
        /// <param name="actions">The methods to apply to the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static OperatorsDecorator<TMacro, TModel> operator *(OperatorsDecorator<TMacro, TModel> decorator, IEnumerable<Action<OperatorsDecorator<TMacro, TModel>>> actions)
        {
            foreach(var a in actions)
            {
                a.Invoke(decorator);
            }

            return decorator.GetSelf();
        }
        /// <summary>
        /// Applies a method to a decorator, appending a new line before applying the method.
        /// </summary>
        /// <param name="decorator">The decorator to apply a method to.</param>
        /// <param name="action">The method to apply to the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static OperatorsDecorator<TMacro, TModel> operator /(OperatorsDecorator<TMacro, TModel> decorator, Action<OperatorsDecorator<TMacro, TModel>> action)
        {
            _ = decorator.AppendLine();
            action.Invoke(decorator);
            return decorator;
        }
        /// <summary>
        /// Applies a sequence of methods to a decorator, appending a new line before applying a method.
        /// </summary>
        /// <param name="decorator">The decorator to apply a method to.</param>
        /// <param name="actions">The methods to apply to the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static OperatorsDecorator<TMacro, TModel> operator /(OperatorsDecorator<TMacro, TModel> decorator, IEnumerable<Action<OperatorsDecorator<TMacro, TModel>>> actions)
        {
            foreach(var a in actions)
            {
                _ = decorator.AppendLine();
                a.Invoke(decorator);
            }

            return decorator.GetSelf();
        }
        /// <summary>
        /// Applies a method to a decorator, appending a new line after applying the method.
        /// </summary>
        /// <param name="decorator">The decorator to apply a method to.</param>
        /// <param name="action">The method to apply to the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static OperatorsDecorator<TMacro, TModel> operator %(OperatorsDecorator<TMacro, TModel> decorator, Action<OperatorsDecorator<TMacro, TModel>> action)
        {
            action.Invoke(decorator);
            _ = decorator.AppendLine();
            return decorator;
        }
        /// <summary>
        /// Applies a sequence of methods to a decorator, appending a new line after applying a method.
        /// </summary>
        /// <param name="decorator">The decorator to apply a method to.</param>
        /// <param name="actions">The methods to apply to the decorator.</param>
        /// <returns>A reference to the decorator.</returns>
        public static OperatorsDecorator<TMacro, TModel> operator %(OperatorsDecorator<TMacro, TModel> decorator, IEnumerable<Action<OperatorsDecorator<TMacro, TModel>>> actions)
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