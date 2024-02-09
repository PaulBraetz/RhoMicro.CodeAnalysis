namespace RhoMicro.CodeAnalysis;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator._Models;

internal partial class UnionTypeBaseAttribute
{
    private UnionTypeBaseAttribute(Object representableTypeSymbolContainer) =>
        _representableTypeSymbolContainer = representableTypeSymbolContainer;
    public PartialRepresentableTypeModel GetPartialModel(TypeOrTypeParameterType representableType, INamedTypeSymbol unionType, CancellationToken ct)
    {
        var result = PartialRepresentableTypeModel.Create(Alias, Options, Storage, new(Groups), representableType, unionType, ct);

        return result;
    }

    public const String GenericMetadataName = NonGenericMetadataName + "`1";
    public const String NonGenericMetadataName = "RhoMicro.CodeAnalysis.UnionTypeAttribute";
}
