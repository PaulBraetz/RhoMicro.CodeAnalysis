namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using RhoMicro.CodeAnalysis.UnionsGenerator.Transformation.Storage;
using RhoMicro.CodeAnalysis.UnionsGenerator.Transformation.Visitors;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models.Storage;
using RhoMicro.CodeAnalysis.UnionsGenerator.Utils;

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
sealed record UnionTypeModel(
    TypeSignatureModel Signature,
    EquatableList<RepresentableTypeModel> RepresentableTypes,
    EquatableList<RelationModel> Relations,
    SettingsModel Settings,
    Boolean IsGenericType,
    String ScopedDataTypeName,
    GroupsModel Groups,
    StaticallyEquatableContainer<StrategySourceHost> StrategyHostContainer) : IModel<UnionTypeModel>
{
    /// <inheritdoc/>
    public void Receive<TVisitor>(TVisitor visitor)
        where TVisitor : IVisitor<UnionTypeModel>
        => visitor.Visit(this);

    /// <summary>
    /// Creates a new union type model.
    /// </summary>
    /// <param name="signature"></param>
    /// <param name="partialRepresentableTypes"></param>
    /// <param name="factories"></param>
    /// <param name="relations"></param>
    /// <param name="settings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static UnionTypeModel Create(
        TypeSignatureModel signature,
        EquatableList<PartialRepresentableTypeModel> partialRepresentableTypes,
        EquatableList<FactoryModel> factories,
        EquatableList<RelationModel> relations,
        SettingsModel settings,
        CancellationToken cancellationToken)
    {
        Throw.ArgumentNull(signature, nameof(signature));
        Throw.ArgumentNull(partialRepresentableTypes, nameof(partialRepresentableTypes));
        Throw.ArgumentNull(factories, nameof(factories));
        Throw.ArgumentNull(relations, nameof(relations));
        Throw.ArgumentNull(settings, nameof(settings));

        if(partialRepresentableTypes.Count < 1)
            throw new ArgumentException($"{nameof(partialRepresentableTypes)} must contain at least one representable type model.", nameof(partialRepresentableTypes));

        cancellationToken.ThrowIfCancellationRequested();

        var isGenericType = signature.TypeArgs.Count > 0;
        var conversionFunctionsTypeName = $"{signature.Names.FullIdentifierOrHintName}_ScopedData{signature.Names.TypeArgsString}";

        var factoryMap = factories.ToDictionary(f => f.Parameter);
        var representableTypes = partialRepresentableTypes
            .Select(p =>
            (
                partial: p,
                factory: factoryMap.TryGetValue(p.Signature, out var f)
                    ? f
                    : FactoryModel.CreateGenerated(p),
                strategy: StorageStrategy.Create(settings, isGenericType, p)
            )).Select(t => RepresentableTypeModel.Create(t.partial, t.factory, t.strategy, cancellationToken))
            .ToEquatableList(cancellationToken);

        var groups = GroupsModel.Create(representableTypes);

        var hostContainer = new StaticallyEquatableContainer<StrategySourceHost>(
            new(
                settings,
                signature,
                isGenericType,
                representableTypes), 
            true);

        for(var i = 0; i < representableTypes.Count; i++)
        {
            representableTypes[i].StrategyContainer.Value.Visit(hostContainer.Value);
        }

        var result = new UnionTypeModel(
            signature,
            representableTypes,
            relations,
            settings,
            isGenericType,
            conversionFunctionsTypeName,
            groups,
            hostContainer);

        return result;
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
