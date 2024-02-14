namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Threading;

using RhoMicro.CodeAnalysis.UnionsGenerator.Models.Storage;
using RhoMicro.CodeAnalysis.UnionsGenerator.Utils;

/// <summary>
/// Models a representable type.
/// </summary>
/// <param name="Alias"></param>
/// <param name="Options"></param>
/// <param name="Storage"></param>
/// <param name="Groups"></param>
/// <param name="Signature"></param>
/// <param name="Factory"></param>
/// <param name="StrategyContainer"></param>
/// <param name="OmitConversionOperators"></param>
sealed record RepresentableTypeModel(
    String Alias,
    UnionTypeOptions Options,
    StorageOption Storage,
    EquatableList<String> Groups,
    TypeSignatureModel Signature,
    Boolean OmitConversionOperators,
    FactoryModel Factory,
    StaticallyEquatableContainer<StorageStrategy> StrategyContainer) :
    PartialRepresentableTypeModel(Alias, Options, Storage, Groups, Signature, OmitConversionOperators)
{
    /// <summary>
    /// Creates a new representable type model.
    /// </summary>
    /// <param name="partialModel"></param>
    /// <param name="factory"></param>
    /// <param name="strategy"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static RepresentableTypeModel Create(
        PartialRepresentableTypeModel partialModel,
        FactoryModel factory,
        StorageStrategy strategy,
        CancellationToken cancellationToken)
    {
        Throw.ArgumentNull(partialModel, nameof(partialModel));

        var container = new StaticallyEquatableContainer<StorageStrategy>(strategy, true);

        cancellationToken.ThrowIfCancellationRequested();
        var result = new RepresentableTypeModel(
            partialModel.Alias,
            partialModel.Options,
            partialModel.Storage,
            partialModel.Groups,
            partialModel.Signature,
            partialModel.OmitConversionOperators,
            factory,
            container);

        return result;
    }
    /// <inheritdoc/>
    public override void Receive<TVisitor>(TVisitor visitor)
        => visitor.Visit(this);
}
