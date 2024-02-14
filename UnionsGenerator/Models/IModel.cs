namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using RhoMicro.CodeAnalysis.UnionsGenerator.Transformation.Visitors;

/// <summary>
/// Represents a base model, capable of receiving visitors for Open/Closed-compliant extension of functionality.
/// </summary>
/// <typeparam name="TSelf">The model type itself (CRTP).</typeparam>
public interface IModel<TSelf>
{
    /// <summary>
    /// Receives a visitor.
    /// </summary>
    /// <remarks>
    /// Implementations should simply call the visitors <see cref="IVisitor{T}.Visit(T)"/>
    /// method, where <c>T</c> is <typeparamref name="TSelf"/>.<br/>
    /// For example: <br/>
    /// <code>
    /// class MyModel : IModel&lt;MyModel&gt;
    /// {
    ///     public void Receive&lt;TVisitor&gt;(TVisitor visitor)
    ///     where TVisitor : IVisitor&lt;MyModel&gt;
    ///     => visitor.Receive(this);
    /// }
    /// </code>
    /// </remarks>
    /// <typeparam name="TVisitor">The type of visitor to receive.</typeparam>
    /// <param name="visitor">The visitor to receive.</param>
    void Receive<TVisitor>(TVisitor visitor)
        where TVisitor : IVisitor<TSelf>;
}