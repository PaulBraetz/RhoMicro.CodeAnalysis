namespace RhoMicro.CodeAnalysis;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

internal partial class AliasedUnionTypeBaseAttribute
{
    public PartialRepresentableTypeModel GetPartialModel(TypeOrTypeParameterType representableType, INamedTypeSymbol unionType, CancellationToken ct)
    {
        var result = PartialRepresentableTypeModel.Create(Alias, Options, Storage, new(Groups), representableType, unionType, ct);

        return result;
    }
}
