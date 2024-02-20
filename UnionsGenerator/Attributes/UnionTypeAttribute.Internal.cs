namespace RhoMicro.CodeAnalysis;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

partial class AliasedUnionTypeBaseAttribute
{
    internal PartialRepresentableTypeModel GetPartialModel(TypeOrTypeParameterType representableType, INamedTypeSymbol unionType, CancellationToken ct)
    {
        var result = PartialRepresentableTypeModel.Create(
            Alias,
            FactoryName,
            Options,
            Storage,
            new(Groups),
            representableType,
            unionType,
            ct);

        return result;
    }
}
