namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using RhoMicro.CodeAnalysis.UnionsGenerator.Transformation.Storage;
using RhoMicro.CodeAnalysis.UnionsGenerator.Transformation.Visitors;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models.Storage;
using RhoMicro.CodeAnalysis.UnionsGenerator.Utils;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

/// <summary>
/// Represents a user-defined union type.
/// </summary>
/// <param name="Signature"></param>
/// <param name="RepresentableTypes"></param>
/// <param name="Relations"></param>
/// <param name="Settings"></param>
/// <param name="IsGenericType"></param>
/// <param name="ScopedDataTypeName"></param>
/// <param name="Groups"></param>
/// <param name="StrategyHostContainer"></param>
/// <param name="IsEqualsRequired"></param>
/// <param name="Locations"></param>
sealed record UnionTypeModel(
    TypeSignatureModel Signature,
    EquatableList<RepresentableTypeModel> RepresentableTypes,
    EquatableList<RelationModel> Relations,
    SettingsModel Settings,
    Boolean IsGenericType,
    String ScopedDataTypeName,
    GroupsModel Groups,
    EquatedData<StrategySourceHost> StrategyHostContainer,
    Boolean IsEqualsRequired,
    EquatedData<ImmutableArray<Location>> Locations) : IModel<UnionTypeModel>
{
    public static UnionTypeModel Create(INamedTypeSymbol unionType, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var signature = TypeSignatureModel.Create(unionType, cancellationToken);
        var relations = RelationModel.Create(unionType, cancellationToken);
        var settings = SettingsModel.Create(unionType, cancellationToken);
        var factories = FactoryModel.Create(unionType, cancellationToken);
        var isEqualsRequired = PartialUnionTypeModel.IsEqualsRequiredForTarget(unionType);
        var partials = PartialUnionTypeModel.Create(unionType, cancellationToken)
            .Select(m => m.RepresentableType)
            .ToEquatableList(cancellationToken);
        var locations = unionType.Locations
            .Where(l => l.IsInSource && !( l.SourceTree?.FilePath.EndsWith(".g.cs") ?? true ))
            .ToImmutableArray();

        var result = Create(
            signature,
            partials,
            factories,
            relations,
            settings,
            isEqualsRequired,
            locations,
            cancellationToken);

        return result;
    }
    /// <inheritdoc/>
    public void Receive<TVisitor>(TVisitor visitor)
        where TVisitor : IVisitor<UnionTypeModel>
        => visitor.Visit(this);
    public static UnionTypeModel Create(
        TypeSignatureModel signature,
        EquatableList<PartialRepresentableTypeModel> partialRepresentableTypes,
        EquatableList<FactoryModel> factories,
        EquatableList<RelationModel> relations,
        SettingsModel settings,
        Boolean isEqualsRequired,
        CancellationToken cancellationToken) =>
        Create(
        signature,
        partialRepresentableTypes,
        factories,
        relations,
        settings,
        isEqualsRequired,
        ImmutableArray<Location>.Empty,
        cancellationToken);

    /// <summary>
    /// Creates a new union type model.
    /// </summary>
    /// <param name="signature"></param>
    /// <param name="partialRepresentableTypes"></param>
    /// <param name="factories"></param>
    /// <param name="relations"></param>
    /// <param name="settings"></param>
    /// <param name="isEqualsRequired"></param>
    /// <param name="locations"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static UnionTypeModel Create(
        TypeSignatureModel signature,
        EquatableList<PartialRepresentableTypeModel> partialRepresentableTypes,
        EquatableList<FactoryModel> factories,
        EquatableList<RelationModel> relations,
        SettingsModel settings,
        Boolean isEqualsRequired,
        ImmutableArray<Location> locations,
        CancellationToken cancellationToken)
    {
        Throw.ArgumentNull(signature, nameof(signature));
        Throw.ArgumentNull(partialRepresentableTypes, nameof(partialRepresentableTypes));
        Throw.ArgumentNull(factories, nameof(factories));
        Throw.ArgumentNull(relations, nameof(relations));
        Throw.ArgumentNull(settings, nameof(settings));

        cancellationToken.ThrowIfCancellationRequested();

        var isGenericType = signature.TypeArgs.Count > 0;
        var conversionFunctionsTypeName = $"{signature.Names.FullIdentifierOrHintName}_ScopedData{signature.Names.TypeArgsString}";

        var factoryMap = factories.ToDictionary(f => f.Parameter);
        var representableTypes = GetRepresentableTypes(partialRepresentableTypes, settings, isGenericType, factoryMap, cancellationToken);
        var groups = GroupsModel.Create(representableTypes);
        var hostContainer = CreateHostContainerAndReceiveStrategies(signature, settings, isGenericType, representableTypes);

        var result = new UnionTypeModel(
            signature,
            representableTypes,
            relations,
            settings,
            isGenericType,
            conversionFunctionsTypeName,
            groups,
            hostContainer,
            isEqualsRequired,
            locations);

        return result;
    }

    private static EquatableList<RepresentableTypeModel> GetRepresentableTypes(EquatableList<PartialRepresentableTypeModel> partialRepresentableTypes, SettingsModel settings, Boolean isGenericType, Dictionary<TypeSignatureModel, FactoryModel> factoryMap, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = partialRepresentableTypes
            .Select(p =>
            (
                partial: p,
                factory: factoryMap.TryGetValue(p.Signature, out var f)
                    ? f
                    : FactoryModel.CreateGenerated(p),
                strategy: StorageStrategy.Create(settings, isGenericType, p)
            )).Select(t => RepresentableTypeModel.Create(t.partial, t.factory, t.strategy, cancellationToken))
            .ToEquatableList(cancellationToken);

        return result;
    }

    private static StrategySourceHost CreateHostContainerAndReceiveStrategies(TypeSignatureModel signature, SettingsModel settings, Boolean isGenericType, EquatableList<RepresentableTypeModel> representableTypes)
    {
        var hostContainer = new StrategySourceHost(
                settings,
                signature,
                isGenericType,
                representableTypes);

        for(var i = 0; i < representableTypes.Count; i++)
        {
            representableTypes[i].StorageStrategy.Value.Visit(hostContainer);
        }

        return hostContainer;
    }

    public String GetSpecificAccessibility(TypeSignatureModel _)
    {
        var accessibility = Settings.ConstructorAccessibility;

        if(accessibility == ConstructorAccessibilitySetting.PublicIfInconvertible
            //TODO: consider omissions
            /*&& OperatorOmissions.AllOmissions.Contains(representableType)*/)
        {
            accessibility = ConstructorAccessibilitySetting.Public;
        }

        var result = accessibility == ConstructorAccessibilitySetting.Public ?
            "public" :
            "private";

        return result;
    }
}
