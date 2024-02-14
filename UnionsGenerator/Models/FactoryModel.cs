namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator.Transformation.Visitors;

internal readonly record struct FactoryModel(
    Boolean RequiresGeneration,
    String Name,
    TypeSignatureModel Parameter) : IModel<FactoryModel>
{
    public static FactoryModel CreateCustom(String name, ITypeSymbol symbol, CancellationToken cancellationToken) =>
        new(RequiresGeneration: false, name, TypeSignatureModel.Create(symbol, cancellationToken));
    public static FactoryModel CreateCustom(IMethodSymbol symbol, CancellationToken cancellationToken) =>
        CreateCustom(symbol.Name, symbol.ContainingSymbol as ITypeSymbol ?? throw new ArgumentException($"Containing type of {nameof(symbol)} must be of type {typeof(ITypeSymbol)}.", nameof(symbol)), cancellationToken);
    public static FactoryModel CreateGenerated(PartialRepresentableTypeModel model) =>
        new(RequiresGeneration: true, $"CreateFrom{model.Alias}", model.Signature);
    public void Receive<TVisitor>(TVisitor visitor)
        where TVisitor : IVisitor<FactoryModel>
        => visitor.Visit(this);
}
