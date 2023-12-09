namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis;
using RhoMicro.CodeAnalysis.UnionsGenerator.Generators;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System;
using System.Diagnostics;

[DebuggerDisplay("{Names.SimpleTypeName}")]
sealed class RepresentableTypeModel(
    UnionTypeAttribute attribute,
    INamedTypeSymbol target,
    String commentRef,
    RepresentableTypeNature nature,
    RepresentableTypeNames names,
    StorageStrategy storage)
{
    public readonly UnionTypeAttribute Attribute = attribute;
    public readonly INamedTypeSymbol Target = target;
    public readonly String DocCommentRef = commentRef;
    public readonly RepresentableTypeNature Nature = nature;
    public readonly RepresentableTypeNames Names = names;
    public readonly StorageStrategy Storage = storage;
    public readonly String CorrespondingTag = $"Tag.{names.SafeAlias}";

    public static RepresentableTypeModel Create(
        UnionTypeAttribute attribute,
        INamedTypeSymbol target)
    {
        var commentRef =
            attribute.RepresentableTypeIsGenericParameter ?
            $"<typeparamref name=\"{attribute.GenericRepresentableTypeName}\"/>" :
            $"<see cref=\"{attribute.RepresentableTypeSymbol?.ToFullOpenString().Replace('<', '{').Replace('>', '}')}\"/>";
        var names = RepresentableTypeNames.Create(attribute);
        var nature = RepresentableTypeNatureFactory.Create(attribute, target);

        var storageStrategy = StorageStrategy.Create(
            names.SafeAlias,
            names.FullTypeName,
            attribute.Storage,
            nature,
            target.IsGenericType);

        var result = new RepresentableTypeModel(
            attribute,
            target,
            commentRef,
            nature,
            names,
            storageStrategy);

        return result;
    }
}
