namespace RhoMicro.CodeAnalysis.UnionsGenerator.Utils;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Runtime.CompilerServices;
using System.Threading;

internal static class Qualifications
{
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