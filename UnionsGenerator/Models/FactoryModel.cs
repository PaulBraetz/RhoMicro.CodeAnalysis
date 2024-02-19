namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator.Transformation.Visitors;
using RhoMicro.CodeAnalysis.UnionsGenerator.Utils;

sealed record FactoryModel(
    Boolean RequiresGeneration,
    String Name,
    TypeSignatureModel Parameter,
    EquatedData<ImmutableArray<Location>> Locations) : IModel<FactoryModel>
{
    public static EquatableList<FactoryModel> Create(INamedTypeSymbol targetSymbol, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = targetSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(Qualifications.IsUnionTypeFactorySymbol)
            .Select(m => CreateCustom(m.Name, m.Parameters[0].Type, cancellationToken))
            .ToEquatableList(cancellationToken);

        return result;
    }

    private static FactoryModel CreateCustom(String name, ITypeSymbol parameterType, ImmutableArray<Location> location, CancellationToken cancellationToken) =>
        new(RequiresGeneration: false, name, TypeSignatureModel.Create(parameterType, cancellationToken), location);
    public static FactoryModel CreateCustom(String name, ITypeSymbol parameterType, CancellationToken cancellationToken) =>
        CreateCustom(name, parameterType, ImmutableArray<Location>.Empty, cancellationToken);
    public static FactoryModel CreateCustom(IMethodSymbol parameterType, CancellationToken cancellationToken) =>
        CreateCustom(parameterType.Name, parameterType.ContainingType ?? throw new ArgumentException($"Containing type of {nameof(parameterType)} must not be null.", nameof(parameterType)), parameterType.Locations, cancellationToken);
    public static FactoryModel CreateGenerated(PartialRepresentableTypeModel model) =>
        new(RequiresGeneration: true, $"CreateFrom{model.Alias}", model.Signature, ImmutableArray<Location>.Empty);
    public void Receive<TVisitor>(TVisitor visitor)
        where TVisitor : IVisitor<FactoryModel>
        => visitor.Visit(this);
}
