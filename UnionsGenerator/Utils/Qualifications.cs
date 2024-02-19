namespace RhoMicro.CodeAnalysis.UnionsGenerator.Utils;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Runtime.CompilerServices;
using System.Threading;

static class Qualifications
{
    public const String NonGenericFullMetadataName = MetadataNamespace + "." + NonGenericMetadataName;
    public const String NonGenericMetadataName = "UnionTypeAttribute";
    public const String MetadataNamespace = "RhoMicro.CodeAnalysis";
    public const String NonGenericRelationMetadataName = "RelationAttribute";
    public const String FactoryMetadataName = "UnionTypeFactoryAttribute";
    public const String GenericFullMetadataName = MetadataNamespace + "." + GenericMetadataName;
    public const String GenericMetadataName = NonGenericMetadataName + "`";
    private static readonly Dictionary<Int32, String> _genericNames =
        Enumerable.Range(1, MaxRepresentableTypesCount).ToDictionary(i => i, i => $"{GenericFullMetadataName}{i}");
    public static String GetGenericFullMetadataName(Int32 typeParamCount) => _genericNames[typeParamCount];
    public static IEnumerable<String> GenericMetadataNames => _genericNames.Values;

    public const Int32 MaxRepresentableTypesCount = 255; //limit to 255 due to tag type being byte + None tag
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Boolean IsRelationAttribute(this AttributeData data) =>
        data.IsAttributesNamespaceAttribute()
        && ( data.AttributeClass?.MetadataName.StartsWith(NonGenericRelationMetadataName) ?? false );
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Boolean IsAttributesNamespaceAttribute(this AttributeData data) =>
        data.AttributeClass?.ContainingNamespace.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted))
        == MetadataNamespace;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Boolean IsUnionTypeDeclarationAttribute(this AttributeData data) =>
        data.IsAttributesNamespaceAttribute()
        && data.AttributeClass!.MetadataName.StartsWith(GenericMetadataName)
        && data.AttributeClass.TypeArguments.Length < MaxRepresentableTypesCount;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Boolean IsUnionTypeParameterAttribute(this AttributeData data) =>
        data.IsAttributesNamespaceAttribute()
        && data.AttributeClass?.MetadataName == NonGenericMetadataName
        && data.AttributeClass.TypeArguments.Length == 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Boolean IsUnionTypeAttribute(this AttributeData data) => data.IsUnionTypeParameterAttribute() || data.IsUnionTypeDeclarationAttribute();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Boolean IsUnionTypeSettingsAttribute(this AttributeData data) =>
        data.IsAttributesNamespaceAttribute()
        && data.AttributeClass?.MetadataName == typeof(UnionTypeSettingsAttribute).Name;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Boolean IsUnionTypeParameterSyntax(SyntaxNode? node, CancellationToken cancellationToken)
    {
        //checking [UnionType] on type parameter

        cancellationToken.ThrowIfCancellationRequested();
        //only type parameters are valid targets
        if(node is not TypeParameterSyntax tps)
            return false;

        cancellationToken.ThrowIfCancellationRequested();
        //the containing declaration must be a type declaration that is itself a valid target (partial, class/struct etc)
        //TypeParameter<-TypeParameterList<-TypeDeclaration
        return IsUnionTypeDeclarationSyntax(tps.Parent?.Parent, cancellationToken);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Boolean IsUnionTypeDeclarationSyntax(SyntaxNode? node, CancellationToken cancellationToken)
    {
        //checking [UnionType<T>] on type declaration

        cancellationToken.ThrowIfCancellationRequested();
        //only classes & structs are valid targets; records & interfaces are excluded
        if(node is not ClassDeclarationSyntax and not StructDeclarationSyntax)
            return false;

        cancellationToken.ThrowIfCancellationRequested();
        //declaration must be partial
        return ( (TypeDeclarationSyntax)node ).Modifiers.Any(SyntaxKind.PartialKeyword);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Boolean IsUnionTypeFactoryAttribute(AttributeData data) =>
        data.IsAttributesNamespaceAttribute() &&
        data.AttributeClass?.MetadataName == FactoryMetadataName;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Boolean IsUnionTypeFactorySymbol(IMethodSymbol methodSymbol)
    {
        var result = methodSymbol.IsStatic
            && !SymbolEqualityComparer.IncludeNullability.Equals(methodSymbol.ReturnType, methodSymbol.ContainingSymbol)
            && methodSymbol.Parameters.Length == 1
            && methodSymbol.TypeParameters.Length == 0
            && methodSymbol.Parameters[0].GetAttributes().Any(IsUnionTypeFactoryAttribute);

        return result;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Boolean IsUnionTypeFactoryDeclarationSyntax(SyntaxNode? node, CancellationToken cancellationToken)
    {
        //checking [UnionTypeFactory] on static factory methods

        cancellationToken.ThrowIfCancellationRequested();
        //check that target is actually a method
        if(node is not ParameterSyntax paramSyntax)
            return false;

        cancellationToken.ThrowIfCancellationRequested();
        //check target method is static, takes one parameter and is not generic
        return paramSyntax.Parent?.Parent is MethodDeclarationSyntax mds &&
           mds.Modifiers.Any(SyntaxKind.StaticKeyword) &&
           mds.ParameterList.Parameters.Count == 1 &&
           mds.TypeParameterList?.Parameters.Count is 0 or null;
    }
}