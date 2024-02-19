namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Collections.Immutable;
using System.Threading;

using Microsoft.CodeAnalysis;

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
/// <param name="StorageStrategy"></param>
/// <param name="OmitConversionOperators"></param>
/// <param name="IsBaseClassToUnionType"></param>
sealed record RepresentableTypeModel(
    String Alias,
    UnionTypeOptions Options,
    StorageOption Storage,
    EquatableList<String> Groups,
    TypeSignatureModel Signature,
    Boolean OmitConversionOperators,
    Boolean IsBaseClassToUnionType,
    FactoryModel Factory,
    EquatedData<StorageStrategy> StorageStrategy) :
    PartialRepresentableTypeModel(Alias, Options, Storage, Groups, Signature, OmitConversionOperators, IsBaseClassToUnionType)
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

        cancellationToken.ThrowIfCancellationRequested();
        var result = new RepresentableTypeModel(
            partialModel.Alias,
            partialModel.Options,
            partialModel.Storage,
            partialModel.Groups,
            partialModel.Signature,
            partialModel.OmitConversionOperators,
            partialModel.IsBaseClassToUnionType,
            factory,
            strategy);

        return result;
    }
    /// <inheritdoc/>
    public override void Receive<TVisitor>(TVisitor visitor)
        => visitor.Visit(this);
}
