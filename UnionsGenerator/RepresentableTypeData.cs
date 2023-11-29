namespace RhoMicro.CodeAnalysis.UnionsGenerator;

using Microsoft.CodeAnalysis;

using System;
using System.Diagnostics;

[DebuggerDisplay("{Names.SimpleTypeName}")]
sealed class RepresentableTypeData(
    UnionTypeAttribute attribute,
    INamedTypeSymbol target,
    String commentRef,
    RepresentableTypeNature nature,
    RepresentableTypeNames names,
    StorageStrategy storage) : IEquatable<RepresentableTypeData?>
{
    public readonly UnionTypeAttribute Attribute = attribute;
    public readonly INamedTypeSymbol Target = target;
    public readonly String DocCommentRef = commentRef;
    public readonly RepresentableTypeNature Nature = nature;
    public readonly RepresentableTypeNames Names = names;
    public readonly StorageStrategy Storage = storage;
    public readonly String CorrespondingTag = $"Tag.{names.SafeAlias}";

    public static RepresentableTypeData Create(
        UnionTypeAttribute attribute,
        INamedTypeSymbol target)
    {
        var commentRef =
            attribute.RepresentableTypeIsGenericParameter ?
            $"<typeparamref name=\"{attribute.GenericRepresentableTypeName}\"/>" :
            $"<see cref=\"{attribute.RepresentableTypeSymbol?.ToOpenString().Replace('<', '{').Replace('>', '}')}\"/>";
        var names = RepresentableTypeNames.Create(attribute);
        var nature = RepresentableTypeNatureFactory.Create(attribute, target);

        var storageStrategy = StorageStrategy.Create(
            names.SafeAlias,
            names.FullTypeName,
            attribute.Storage,
            nature,
            target.IsGenericType);

        var result = new RepresentableTypeData(
            attribute,
            target,
            commentRef,
            nature,
            names,
            storageStrategy);

        return result;
    }

    public override Boolean Equals(Object? obj) => Equals(obj as RepresentableTypeData);
    public Boolean Equals(RepresentableTypeData? other) =>
        other is not null &&
        Attribute.Equals(other.Attribute) &&
        SymbolEqualityComparer.Default.Equals(Target, other.Target);

    public override Int32 GetHashCode()
    {
        var hashCode = 1823267663;
        hashCode = hashCode * -1521134295 + Attribute.GetHashCode();
        hashCode = hashCode * -1521134295 + SymbolEqualityComparer.Default.GetHashCode(Target);
        return hashCode;
    }
}
