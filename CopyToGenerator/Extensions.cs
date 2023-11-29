namespace RhoMicro.CodeAnalysis.CopyToGenerator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

static class Extensions
{
    public static Boolean IsCopyable(this ITypeSymbol symbol) =>
        symbol.IsMarked() ||
        symbol.TryGetListAssignableItemType(out _);
    public static Boolean IsMarked(this ITypeSymbol symbol) =>
        symbol.GetAttributes().Any(a =>
                    a.AttributeClass != null &&
                    a.AttributeClass.MetadataName == typeof(GenerateCopyToAttribute).Name &&
                    a.AttributeClass.ContainingNamespace.ToDisplayString() == typeof(GenerateCopyToAttribute).Namespace);
    public static Boolean TryGetListAssignableItemType(this ITypeSymbol symbol, out ITypeSymbol? itemType)
    {
        var result = symbol is INamedTypeSymbol named &&
                isMatchingType(named) &&
                named.TypeArguments.Length == 1 &&
                Extensions.IsCopyable(named.TypeArguments[0]);

        itemType = result ?
            (symbol as INamedTypeSymbol)!.TypeArguments[0] :
            null;

        return result;

        static Boolean isMatchingType(INamedTypeSymbol named) =>
            isType(named, typeof(IEnumerable<>)) ||
            isType(named, typeof(ICollection<>)) ||
            isType(named, typeof(IReadOnlyCollection<>)) ||
            isType(named, typeof(IList<>)) ||
            isType(named, typeof(IReadOnlyList<>));

        static Boolean isType(INamedTypeSymbol named, Type type)
        {
            return named.Name == type.Name.Substring(0, type.Name.Length - 2) &&
                   named.IsGenericType &&
                   named.ContainingNamespace.ToDisplayString() == type.Namespace;
        }
    }
}