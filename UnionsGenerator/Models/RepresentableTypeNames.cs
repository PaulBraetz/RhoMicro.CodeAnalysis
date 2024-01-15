namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis.CSharp;

using RhoMicro.CodeAnalysis;
using RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using System;

sealed class RepresentableTypeNames(
    String fullTypeName,
    String openTypeName,
    String simpleTypeName,
    String safeAlias,
    String typeStringName)
{
    public readonly String FullTypeName = fullTypeName;
    public readonly String OpenTypeName = openTypeName;
    public readonly String SimpleTypeName = simpleTypeName;
    public readonly String SafeAlias = safeAlias;
    public readonly String TypeStringName = typeStringName;
    public readonly String AsPropertyName = $"As{safeAlias}";
    public readonly String IsPropertyName = $"Is{safeAlias}";
    public readonly String GeneratedFactoryName = $"CreateFrom{safeAlias}";

    public static RepresentableTypeNames Create(UnionTypeAttribute attribute)
    {
        var openTypeName =
            (attribute.RepresentableTypeIsTypeParameter ?
            attribute.RepresentableTypeSymbol!.Name :
            attribute.RepresentableTypeSymbol!.ToMinimalOpenString()) ??
            String.Empty;
        var fullTypeName =
            (attribute.RepresentableTypeIsTypeParameter ?
            attribute.RepresentableTypeSymbol!.Name :
            attribute.RepresentableTypeSymbol?.ToFullOpenString()) ??
            String.Empty;
        var simpleTypeName =
            (attribute.RepresentableTypeIsTypeParameter ?
            attribute.RepresentableTypeSymbol!.Name :
            attribute.RepresentableTypeSymbol?.Name) ??
            String.Empty;
        var safeAlias = attribute.Alias != null && SyntaxFacts.IsValidIdentifier(attribute.Alias) ?
                attribute.Alias :
                (attribute.RepresentableTypeIsTypeParameter ?
                attribute.RepresentableTypeSymbol!.Name :
                attribute.RepresentableTypeSymbol?.ToIdentifierCompatString()) ??
                simpleTypeName;
        var typeStringName =
            (attribute.RepresentableTypeIsTypeParameter ?
            attribute.RepresentableTypeSymbol!.Name :
            attribute.RepresentableTypeSymbol?.ToTypeString()) ??
            String.Empty;

        var result = new RepresentableTypeNames(fullTypeName, openTypeName, simpleTypeName, safeAlias, typeStringName);

        return result;
    }
}