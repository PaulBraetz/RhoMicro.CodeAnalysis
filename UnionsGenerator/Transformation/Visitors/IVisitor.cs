namespace RhoMicro.CodeAnalysis.UnionsGenerator.Transformation.Visitors;

using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

/// <summary>
/// Represents a visitor to visit models.
/// </summary>
/// <typeparam name="TModel">The type of model to visit.</typeparam>
public interface IVisitor<TModel>
{
    /// <summary>
    /// Visits a model.
    /// </summary>
    /// <remarks>
    /// Implementations should never call <paramref name="model"/>s own
    /// <see cref="IModel{TSelf}.Receive{TVisitor}(TVisitor)"/> function,
    /// as that will likely induce a <see cref="StackOverflowException"/>.
    /// </remarks>
    /// <param name="model">The model to visit.</param>
    void Visit(TModel model);
}
