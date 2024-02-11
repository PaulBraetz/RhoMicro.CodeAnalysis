namespace RhoMicro.CodeAnalysis.UnionsGenerator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;

internal static class Util
{
    public static Boolean IsGeneratorTarget(SyntaxNode node) =>
        node is TypeDeclarationSyntax tds and not RecordDeclarationSyntax &&
        tds.Modifiers.Any(SyntaxKind.PartialKeyword) ||
        node is TypeParameterSyntax;

    public static Boolean IsAnalysisCandidate(SyntaxNode node, SemanticModel semanticModel)
    {
        var symbol = semanticModel.GetDeclaredSymbol(node);

        if(symbol is not INamedTypeSymbol or ITypeParameterSymbol)
            return false;

        var attributes = symbol.GetAttributes();
        foreach(var attribute in attributes)
        {
            if(AliasedUnionTypeBaseAttribute.TryCreate(attribute, out _))
                return true;
            //commented out because of breaking rewrite changes
            //if(RelationAttribute.TryCreate(attribute, out _))
            //    return true;
            if(UnionTypeSettingsAttribute.TryCreate(attribute, out _))
                return true;
        }

        //only commented out because of breaking rewrite changes
        var result = symbol is INamedTypeSymbol; //named &&
        //named.GetMembers().SelectMany(m => m.GetAttributes()).OfUnionFactoryAttribute().Any();

        return result;
    }
}
