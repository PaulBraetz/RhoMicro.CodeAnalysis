namespace RhoMicro.CodeAnalysis;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

partial class UnionTypeAttribute : IEquatable<UnionTypeAttribute>
{
    public UnionTypeAttribute(Object representableTypeSymbolContainer) =>
        _representableTypeSymbolContainer = representableTypeSymbolContainer;

    public UnionTypeAttribute WithTypeParameter(ITypeParameterSymbol symbol)
    {
        var result = new UnionTypeAttribute()
        {
            Alias = Alias,
            Options = Options,
            Storage = Storage,
            _representableTypeSymbolContainer = symbol
        };

        return result;
    }

    internal RepresentableTypeModel ExtractData(ITypeSymbol target) => RepresentableTypeModel.Create(this, target);

    public Boolean RepresentableTypeIsTypeParameter => RepresentableTypeSymbol is IParameterSymbol;

    public override Boolean Equals(Object? obj) => Equals(obj as UnionTypeAttribute);
    public Boolean Equals(UnionTypeAttribute? other)
    {
        var result = other is not null &&
            Alias == other.Alias &&
            Options == other.Options &&
            SymbolEqualityComparer.Default.Equals(RepresentableTypeSymbol, other.RepresentableTypeSymbol);

        return result;
    }

    public override Int32 GetHashCode()
    {
        var hashCode = 1581354465;
        hashCode = hashCode * -1521134295 + EqualityComparer<String?>.Default.GetHashCode(Alias);
        hashCode = hashCode * -1521134295 + Options.GetHashCode();
        hashCode = hashCode * -1521134295 + SymbolEqualityComparer.Default.GetHashCode(RepresentableTypeSymbol);

        return hashCode;
    }
}
