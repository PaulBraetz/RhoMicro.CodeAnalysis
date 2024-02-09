namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis;
using RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using System;
using System.Diagnostics;

[DebuggerDisplay("{Names.SimpleTypeName}")]
internal sealed class RepresentableTypeModel(
    UnionTypeBaseAttribute attribute,
    ITypeSymbol target,
    String commentRef,
    RepresentableTypeNature nature,
    RepresentableTypeNames names,
    StorageStrategy storage)
{
    public FactoryModel Factory { get; set; }

    public readonly UnionTypeBaseAttribute Attribute = attribute;
    public readonly ITypeSymbol Target = target;
    public readonly String DocCommentRef = commentRef;
    public readonly RepresentableTypeNature Nature = nature;
    public readonly RepresentableTypeNames Names = names;
    public readonly StorageStrategy Storage = storage;

    public String GetCorrespondingTag(TargetDataModel model) => $"{model.TagTypeName}.{Names.SafeAlias}";

    public static RepresentableTypeModel Create(
        UnionTypeBaseAttribute attribute,
        ITypeSymbol target)
    {
        var commentRef =
            //only commented out because of breaking rewrite changes
            //attribute.RepresentableTypeIsTypeParameter ?
            //$"<typeparamref name=\"{attribute.RepresentableTypeSymbol!.Name}\"/>" :
            $"<see cref=\"{attribute.RepresentableTypeSymbol?.ToFullOpenString().Replace('<', '{').Replace('>', '}')}\"/>";
        var names = RepresentableTypeNames.Create(attribute);
        var nature = RepresentableTypeNatureFactory.Create(attribute);

        var storageStrategy = StorageStrategy.Create(
            names.SafeAlias,
            names.FullTypeName,
            attribute.Storage,
            nature,
            targetIsGeneric: target is INamedTypeSymbol { IsGenericType: true });

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
