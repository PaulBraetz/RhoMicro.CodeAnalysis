namespace RhoMicro.CodeAnalysis.UnionsGenerator._Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator._Transformation.Visitors;

internal readonly record struct FactoryModel(
    Boolean RequiresGeneration,
    String Name,
    TypeSignatureModel Parameter) : IModel<FactoryModel>
{
    public static FactoryModel CreateCustom(String name, ITypeSymbol symbol, CancellationToken cancellationToken) =>
        new(RequiresGeneration: false, name, TypeSignatureModel.Create(symbol, cancellationToken));
    public static FactoryModel CreateGenerated(PartialRepresentableTypeModel model) =>
        new(RequiresGeneration: true, $"CreateFrom{model.Alias}", model.Signature);
    public void Receive<TVisitor>(TVisitor visitor)
        where TVisitor : IVisitor<FactoryModel>
        => visitor.Visit(this);
}
