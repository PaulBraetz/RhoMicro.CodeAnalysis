namespace RhoMicro.CodeAnalysis.UnionsGenerator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Text;

static class Util
{
    public static Boolean IsSyntaxCandidate(SyntaxNode node) =>
        node is TypeDeclarationSyntax tds and not RecordDeclarationSyntax &&
        tds.Modifiers.Any(SyntaxKind.PartialKeyword);
    public static Boolean IsSymbolCandidate(SyntaxNode node, SemanticModel semanticModel) =>
        IsSyntaxCandidate(node) &&
        semanticModel.GetDeclaredSymbol(node) is INamedTypeSymbol named &&
        named.GetAttributes().Any(a =>
            a.AttributeClass!.MetadataName == RelationAttribute.MetadataName ||
            a.AttributeClass.MetadataName == UnionTypeAttribute.MetadataName ||
            a.AttributeClass.MetadataName == UnionTypeSettingsAttribute.MetadataName);
}
