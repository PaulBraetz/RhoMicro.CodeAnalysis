namespace RhoMicro.CodeAnalysis;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

partial class UnionTypeAttribute: IEquatable<UnionTypeAttribute>
{
    public UnionTypeAttribute(Object representableTypeSymbolContainer) =>
        _representableTypeSymbolContainer = representableTypeSymbolContainer;

    internal RepresentableTypeModel ExtractData(INamedTypeSymbol target) =>
        RepresentableTypeModel.Create(this, target);

    public override Boolean Equals(Object? obj) => Equals(obj as UnionTypeAttribute);
    public Boolean Equals(UnionTypeAttribute? other)
    {
        var result = other is not null &&
            Alias == other.Alias &&
            Options == other.Options &&
            (RepresentableTypeIsGenericParameter ?
            GenericRepresentableTypeName == other.GenericRepresentableTypeName :
            SymbolEqualityComparer.Default.Equals(RepresentableTypeSymbol, other.RepresentableTypeSymbol));

        return result;
    }

    public override Int32 GetHashCode()
    {
        var hashCode = 1581354465;
        hashCode = hashCode * -1521134295 + EqualityComparer<String?>.Default.GetHashCode(Alias);
        hashCode = hashCode * -1521134295 + Options.GetHashCode();
        hashCode = RepresentableTypeIsGenericParameter ?
            hashCode * -1521134295 + EqualityComparer<String?>.Default.GetHashCode(GenericRepresentableTypeName) :
            hashCode * -1521134295 + SymbolEqualityComparer.Default.GetHashCode(RepresentableTypeSymbol);

        return hashCode;
    }
}
